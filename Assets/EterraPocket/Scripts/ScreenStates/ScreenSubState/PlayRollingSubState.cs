using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using Eterra.Engine.Logic;
using Eterra.Engine.Helper;
using Eterra.Engine.Models;
using Eterra.Integration.Helper;
using Eterra.NetApiExt.Client;
using Eterra.NetApiExt.Generated.Model.bounded_collections.bounded_vec;
using Eterra.NetApiExt.Generated.Model.pallet_eterra_daily_slots.pallet;
using Eterra.NetApiExt.Generated.Model.sp_core.crypto;
using Eterra.NetApiExt.Generated.Storage;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using Eterra.NetApiExt.Generated.Model.solochain_template_runtime;

namespace Assets.Scripts.ScreenStates
{
  internal class PlayRollingSubState : GameBaseState
  {
    private enum SpinPhase { Init, Spinning, Completed }
    private SpinPhase _currentPhase = SpinPhase.Init;

    private VisualElement _slot1, _slot2, _slot3, _slotArmHolder, _instantWinDisplay;
    private Label _lblTitle;
    private Button _btnSpin;

    private SubstrateNetwork _substrate;
    private CancellationToken _cancellationToken;
    private bool _pendingRollResult = false;
    private bool _isSpinning = false;
    private bool waitingForFinalRoll = false;

    private ExtrinsicUpdateEvent _extrinsicUpdatedHandler;
    private TaskCompletionSource<bool> _currentSpinCompletion;

    public PlayRollingSubState(GameController flowController, GameBaseState parent)
        : base(flowController, parent) { }

    public override async void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}] EnterState");

      _substrate = (FlowController as GameController)?.Substrate;
      _cancellationToken = (FlowController as GameController)?.CancellationToken ?? CancellationToken.None;

      var root = FlowController.VelContainer.Q<VisualElement>("Screen");
      _lblTitle = root.Q<Label>("LblTitle");
      _slot1 = root.Q<VisualElement>("Slot1");
      _slot2 = root.Q<VisualElement>("Slot2");
      _slot3 = root.Q<VisualElement>("Slot3");
      _slotArmHolder = root.Q<VisualElement>("SlotArmHolder");
      _instantWinDisplay = root.Q<VisualElement>("InstantWinDisplay");
      _btnSpin = _slotArmHolder.Q<Button>("BtnSpin");

      _instantWinDisplay?.Clear();
      _instantWinDisplay?.Add(new Label("Instant Win Display Initialized"));
      if (_lblTitle != null) _lblTitle.text = "Eterra Rewards!";

      await GameSharp.UpdateRollHistory(_substrate, _cancellationToken);

      bool hasRolledToday = GameSharp.CurrentDailyRolls?.Any(roll =>
      {
        var timestamp = roll.Timestamp.Value;
        var rollDate = System.DateTimeOffset.FromUnixTimeSeconds((long)timestamp).UtcDateTime.Date;
        return rollDate == System.DateTime.UtcNow.Date;
      }) ?? false;

      // Check if max rolls reached and immediately redirect if so
      int maxRolls = (int)(Eterra.Config.EterraConfig.MaxRollsPerRound?.Value ?? 3);
      int rollsUsed = GameSharp.CurrentDailyRolls?.Length ?? 0;

      if (rollsUsed >= maxRolls)
      {
        await FetchAndDisplayLatestRoll();
        FlowController.ChangeScreenSubState(GameScreen.PlayScreen, GameSubScreen.PlayFinished);
        return;
      }

      if (hasRolledToday)
      {
        await FetchAndDisplayLatestRoll();
      }

      if (_btnSpin != null)
      {
        Debug.Log("[UI] BtnSpin found and about to be enabled.");
        _btnSpin.text = "Spin!";
        _btnSpin.clicked += OnSpinClicked;
        _btnSpin.SetEnabled(true);
      }

      _substrate.OnNewBlock += HandleNewBlock;
    }

    public override void ExitState()
    {
      Debug.Log($"[{this.GetType().Name}] ExitState");

      if (_btnSpin != null)
      {
        _btnSpin.clicked -= OnSpinClicked;
      }

      if (_substrate != null)
      {
        _substrate.OnNewBlock -= HandleNewBlock;

        if (_extrinsicUpdatedHandler != null)
        {
          _substrate.ExtrinsicManager.ExtrinsicUpdated -= _extrinsicUpdatedHandler;
          _extrinsicUpdatedHandler = null;
        }
      }

      _currentSpinCompletion = null;
    }

    private async void OnSpinClicked()
    {
      Debug.Log("[Spin] OnSpinClicked triggered.");

      if (_isSpinning) return;
      _isSpinning = true;

      try
      {
        if (_pendingRollResult || _currentPhase != SpinPhase.Init)
        {
          Debug.LogWarning("[Spin] Already spinning or invalid state.");
          return;
        }

        if (_currentSpinCompletion != null)
        {
          Debug.LogWarning("[Spin] A spin is already in progress. Blocking duplicate submission.");
          return;
        }

        _btnSpin.SetEnabled(false);
        Debug.Log("[Spin] Spin button disabled, preparing to submit spin extrinsic.");

        _pendingRollResult = true;
        _currentPhase = SpinPhase.Spinning;

        await PerformAtomicSpinAsync();
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"[Spin] Atomic spin failed: {ex}");
        _pendingRollResult = false;
        _currentPhase = SpinPhase.Init;
      }
      finally
      {
        _isSpinning = false;
      }
    }

    private async Task PerformAtomicSpinAsync()
    {
      if (_substrate == null)
      {
        Debug.LogError("[Spin] Substrate reference is null.");
        return;
      }

      if (_substrate.Account == null)
      {
        Debug.LogError("[Spin] Substrate account is null. Aborting spin.");
        _pendingRollResult = false;
        _currentPhase = SpinPhase.Init;
        _btnSpin.text = "Spin";
        _btnSpin.SetEnabled(true);
        return;
      }

      var player = Generic.ToAccountId32(_substrate.Account);

      var blockResponse = await _substrate.ApiClient.Chain.GetBlockAsync(_cancellationToken);
      var currentBlock = blockResponse?.Block?.Header?.Number?.Value ?? 0;
      Debug.Log($"[Spin] Current block before spin submission: {currentBlock}");

      _currentSpinCompletion = null; // Reset before use to avoid stale state
      var spinSubId = await _substrate.SpinSlotAsync(player, 1, _cancellationToken);
      Debug.Log($"[Spin] Received spinSubId from SpinSlotAsync: {spinSubId}");
      if (string.IsNullOrWhiteSpace(spinSubId))
      {
        Debug.LogError("[Spin] SpinSlotAsync returned null or empty sub ID. Aborting spin.");
        _pendingRollResult = false;
        _currentPhase = SpinPhase.Init;
        _btnSpin.text = "Spin";
        _btnSpin.SetEnabled(true);
        return;
      }

      _currentSpinCompletion = new TaskCompletionSource<bool>();

      if (_extrinsicUpdatedHandler != null)
      {
        _substrate.ExtrinsicManager.ExtrinsicUpdated -= _extrinsicUpdatedHandler;
      }

      // uint? finalizedBlock = null;

      _extrinsicUpdatedHandler = async (id, info) =>
      {
          if (_currentSpinCompletion?.Task?.IsCompleted == true)
          {
              Debug.Log("[Spin] Ignoring duplicate update after task completion.");
              return;
          }

          Debug.Log($"[Spin] ExtrinsicUpdate triggered. spinSubId: {spinSubId}, received id: {id}, event: {info.TransactionEvent}");
          if (id != spinSubId || !(info.IsInBlock || info.IsCompleted))
          {
              Debug.Log($"[Spin] Ignoring update. Match: {id == spinSubId}, InBlock: {info.IsInBlock}, Completed: {info.IsCompleted}");
              return;
          }

          Debug.Log("[Spin] Spin extrinsic finalized or in block.");
          Debug.Log("[Spin] Handling finalization. Setting completion task...");

          if (info.EventRecords != null)
          {
              foreach (var rec in info.EventRecords)
              {
                  var runtimeEvent = rec.Event as EnumRuntimeEvent;
                  if (runtimeEvent?.Value == RuntimeEvent.EterraDailySlots &&
                      runtimeEvent.Value2 is EnumEvent ev &&
                      ev.Value == Eterra.NetApiExt.Generated.Model.pallet_eterra_daily_slots.pallet.Event.SlotRolled)
                  {
                      Debug.Log($"[Spin] Found SlotRolled event!");
    
                      // finalizedBlock = info.Index;
                      // Debug.Log($"[Spin] Extrinsic included in block: {finalizedBlock}");

                      var tuple = ev.Value2 as BaseTuple<AccountId32, BaseVec<U32>>;
                      if (tuple?.Value == null || tuple.Value.Length < 2) continue;

                      var reels = tuple.Value[1] as BaseVec<U32>;
                      var reelValues = reels?.Value?.Select(x => x.Value).ToArray();

                      UnityMainThreadDispatcher.Instance().Enqueue(() =>
                      {
                          _slot1.Q<Label>("LblSlot1").text = reelValues.Length > 0 ? reelValues[0].ToString() : "?";
                          _slot2.Q<Label>("LblSlot2").text = reelValues.Length > 1 ? reelValues[1].ToString() : "?";
                          _slot3.Q<Label>("LblSlot3").text = reelValues.Length > 2 ? reelValues[2].ToString() : "?";
                      });

                      _pendingRollResult = false;
                      await WaitForRollHistoryToIncludeNewSpin();
                      await FetchAndDisplayLatestRoll();

                      int rollsUsed = GameSharp.CurrentDailyRolls?.Length ?? 0;
                      int maxRolls = (int)(Eterra.Config.EterraConfig.MaxRollsPerRound?.Value ?? 3);
                      _currentPhase = SpinPhase.Init;

                      // Fetch finalized head and use it for accurate finalized block number
                      var finalizedHash = await _substrate.ApiClient.Chain.GetFinalizedHeadAsync(_cancellationToken);
                      var finalizedBlockData = await _substrate.ApiClient.Chain.GetBlockAsync(finalizedHash, _cancellationToken);
                      uint finalizedBlockNumber = (uint)(finalizedBlockData?.Block?.Header?.Number?.Value ?? 0);

                      Debug.Log($"[Spin] Finalized block determined from Chain API: {finalizedBlockNumber}");

                      // Wait for the next block after finalization
                      await WaitForNextBlockAsync(finalizedBlockNumber);

                      UnityMainThreadDispatcher.Instance().Enqueue(() =>
                      {
                          if (rollsUsed >= maxRolls)
                          {
                              FlowController.ChangeScreenSubState(GameScreen.PlayScreen, GameSubScreen.PlayFinished);
                          }
                          else
                          {
                              _btnSpin.text = "Spin!";
                              _btnSpin.SetEnabled(true);
                          }
                      });

                      _currentSpinCompletion?.TrySetResult(true);
                      _substrate.ExtrinsicManager.ExtrinsicUpdated -= _extrinsicUpdatedHandler;
                      _extrinsicUpdatedHandler = null;
                      _currentSpinCompletion = null;

                      return;
                  }
              }
          }

          Debug.LogWarning("[Spin] SlotRolled event not found in finalized extrinsic.");
      };

      _substrate.ExtrinsicManager.ExtrinsicUpdated += _extrinsicUpdatedHandler;

      Debug.Log("[Spin] ExtrinsicUpdated handler attached.");

      await _currentSpinCompletion.Task;

      // if (finalizedBlock != null)
      // {
      //     await WaitForNextBlockAsync(finalizedBlock ?? 0);
      // }

      async Task WaitForNextBlockAsync(uint startingBlock)
      {
          Debug.Log($"[Spin] Waiting for block after {startingBlock}...");
          var seenNext = false;

          void OnBlock(uint b)
          {
              if (b > startingBlock)
              {
                  seenNext = true;
                  Debug.Log($"[Spin] New block seen after extrinsic block: {b}");
                  _substrate.OnNewBlock -= OnBlock;
              }
          }

          _substrate.OnNewBlock += OnBlock;

          int waited = 0;
          while (!seenNext && waited < 10000)
          {
              await Task.Delay(250, _cancellationToken);
              waited += 250;
          }

          if (!seenNext)
          {
              Debug.LogWarning($"[Spin] Timeout waiting for new block after {startingBlock}");
              _substrate.OnNewBlock -= OnBlock;
          }
      }
    }

    private async Task FetchAndDisplayLatestRoll()
    {
      try
      {
        var player = Generic.ToAccountId32(_substrate.Account);
        var history = await _substrate.ApiClient.GetStorageAsync<BoundedVecT8>(
            EterraDailySlotsStorage.RollHistoryParams(player), null, _cancellationToken);

        if (history?.Value != null)
        {
          var rawRolls = BaseVecHelper.ToArray<RollResult>(history.Value);
          Debug.Log($"[FetchRoll] Raw rolls count: {rawRolls.Length}");
          foreach (var r in rawRolls)
          {
            Debug.Log($"[FetchRoll] Roll timestamp: {r.Timestamp?.Value}");
          }

          var latestRoll = rawRolls.OrderByDescending(r => r.Timestamp?.Value ?? 0).FirstOrDefault();

          if (latestRoll?.Result != null && rawRolls.Length >= 3)
          {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
              var resultArray = BaseVecHelper.ToArray<U32>(latestRoll.Result.Value);
              _slot1.Q<Label>("LblSlot1").text = resultArray.Length > 0 ? resultArray[0].ToString() : "?";
              _slot2.Q<Label>("LblSlot2").text = resultArray.Length > 1 ? resultArray[1].ToString() : "?";
              _slot3.Q<Label>("LblSlot3").text = resultArray.Length > 2 ? resultArray[2].ToString() : "?";
            });
          }
        }
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"[Spin] Error fetching roll: {ex}");
      }
    }

    private async Task WaitForRollHistoryToIncludeNewSpin()
    {
      int initialCount = GameSharp.CurrentDailyRolls?.Length ?? 0;
      int waited = 0;
      int maxWaitMs = 5000;
      int intervalMs = 250;

      while (waited < maxWaitMs)
      {
        await GameSharp.UpdateRollHistory(_substrate, _cancellationToken);
        int currentCount = GameSharp.CurrentDailyRolls?.Length ?? 0;
        if (currentCount > initialCount) break;

        await Task.Delay(intervalMs, _cancellationToken);
        waited += intervalMs;
      }
    }

    private async void HandleNewBlock(uint blockNumber)
    {
      if (_pendingRollResult || _currentPhase != SpinPhase.Spinning) return;

      int maxRolls = (int)((Eterra.Config.EterraConfig.MaxRollsPerRound != null) ? Eterra.Config.EterraConfig.MaxRollsPerRound.Value : 3);
      int rollsUsedFinal = GameSharp.CurrentDailyRolls?.Length ?? 0;
      if (rollsUsedFinal >= maxRolls && !waitingForFinalRoll)
      {
        waitingForFinalRoll = true;
        await FetchAndDisplayLatestRoll();
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
          FlowController.ChangeScreenSubState(GameScreen.PlayScreen, GameSubScreen.PlayFinished);
        });
      }
    }
  }
}