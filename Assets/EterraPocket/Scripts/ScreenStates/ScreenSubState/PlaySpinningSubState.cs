using System;
using Eterra.Engine.Models;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Eterra.Engine.Helper; // where Generic lives
using Substrate.NetApi.Model.Rpc; // brings in EventRecord
using Eterra.NetApiExt.Generated.Model.solochain_template_runtime;
using Eterra.NetApiExt.Generated.Model.pallet_eterra_daily_slots.pallet;
using Eterra.NetApiExt.Generated.Model.sp_core.crypto; // for AccountId32
using Assets.Scripts;
using Eterra.Integration.Helper;
using Eterra.NetApiExt.Generated.Model.bounded_collections.bounded_vec;
using Eterra.NetApiExt.Generated.Storage;
using Eterra.NetApiExt.Generated;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using Eterra.Config;
using Eterra.Engine.Logic;

namespace Assets.Scripts.ScreenStates
{
  internal class PlaySpinningSubState : GameBaseState
  {
    private VisualElement _slot1;
    private VisualElement _slot2;
    private VisualElement _slot3;
    private VisualElement _slotArmHolder;
    private Label _lblTitle;
    private VisualElement _instantWinDisplay;
    private VisualElement _rewardHistory;
    private Button _btnSpin;
    private SubstrateNetwork _substrate;
    private CancellationToken _cancellationToken;
    private bool _triggeredInitialSpin = false;
    private bool _pendingRollResult = false;
    private bool _firstBlockHandled = false;

    public PlaySpinningSubState(GameController flowController, GameBaseState parent)
        : base(flowController, parent)
    {

    }

    public override void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

      var root = FlowController.VelContainer.Q<VisualElement>("Screen");

      _substrate = (FlowController as GameController)?.Substrate;
      _cancellationToken = (FlowController as GameController)?.CancellationToken ?? CancellationToken.None;

      _lblTitle = root.Q<Label>("LblTitle");
      _slot1 = root.Q<VisualElement>("Slot1");
      _slot2 = root.Q<VisualElement>("Slot2");
      _slot3 = root.Q<VisualElement>("Slot3");
      _slotArmHolder = root.Q<VisualElement>("SlotArmHolder");
      _instantWinDisplay = root.Q<VisualElement>("InstantWinDisplay");
      _rewardHistory = root.Q<VisualElement>("RewardHistory");
      _btnSpin = root.Q<Button>("BtnSpin");

      InitializeGameState();

      // Register click event using UI Toolkit callback system.
      if (_btnSpin != null)
      {
        _btnSpin.RegisterCallback<ClickEvent>(evt => OnSpinClicked());
        _btnSpin.clicked += () => Debug.Log("[Spin] .clicked fallback event fired");
        _btnSpin.SetEnabled(true);  // Force enable to test
      }

      _substrate.OnNewBlock += HandleNewBlock;
    }

    private void InitializeGameState()
    {
      _instantWinDisplay?.Clear();
      _rewardHistory?.Clear();
      if (_lblTitle != null) _lblTitle.text = "Spinning Slots...";
      if (_btnSpin != null)
      {
        Debug.Log("Spin registered with OnSpinClicked");
      }
    }

    private async void OnSpinClicked()
    {
      Debug.Log("[Spin] OnSpinClicked triggered.");

      if (_pendingRollResult)
      {
        Debug.LogWarning("[Spin] A roll is already pending. Please wait.");
        return;
      }

      if (_substrate == null || !_substrate.IsConnected)
      {
        Debug.LogError("Not connected to Substrate network.");
        return;
      }

      var player = Generic.ToAccountId32(_substrate.Account);
      var token = _cancellationToken;

      try
      {
        _pendingRollResult = true;
        var spinSubId = await _substrate.SpinSlotAsync(player, 1, token);
        Debug.Log($"[Spin] Submitted extrinsic. Subscription ID: {spinSubId}");

        _substrate.ExtrinsicManager.ExtrinsicUpdated += async (id, info) =>
        {
          Debug.Log($"[Spin] ExtrinsicUpdate triggered. ID: {id}, Status: {info.TransactionEvent}");
          if (id != spinSubId || !(info.IsInBlock || info.IsCompleted)) return;

          Debug.Log("Spin extrinsic finalized or in block.");

          if (info.EventRecords != null)
          {
            Debug.Log("[Spin] Checking event records.");
            Debug.Log($"[Spin] Processing {info.EventRecords.Count} event records.");
            foreach (var rec in info.EventRecords)
            {
              Debug.Log($"[Spin] Event type: {rec.Event?.GetType().Name}");
              if (rec.Event is { } runtimeEvent &&
                  runtimeEvent.GetType().Name == typeof(RuntimeEvent).Name)
              {
                var rtField = rec.Event.GetType().GetField("_decodedValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var palletEnumEv = rtField?.GetValue(rec.Event) as Eterra.NetApiExt.Generated.Model.pallet_eterra_daily_slots.pallet.EnumEvent;
                if (palletEnumEv == null || palletEnumEv.Value != Eterra.NetApiExt.Generated.Model.pallet_eterra_daily_slots.pallet.Event.SlotRolled)
                  return;

                Debug.Log($"[Spin] Decoded pallet event: {palletEnumEv.Value}");

                var evField = palletEnumEv.GetType().GetField("_decodedValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var tuple = evField?.GetValue(palletEnumEv) as Substrate.NetApi.Model.Types.Base.BaseTuple<Eterra.NetApiExt.Generated.Model.sp_core.crypto.AccountId32, Substrate.NetApi.Model.Types.Base.BaseVec<Substrate.NetApi.Model.Types.Primitive.U32>>;
                if (tuple?.Value == null || tuple.Value.Length < 2) continue;

                var reelsVec = (Substrate.NetApi.Model.Types.Base.BaseVec<Substrate.NetApi.Model.Types.Primitive.U32>)tuple.Value[1];
                var reels = reelsVec.Value.Select(x => x.Value).ToArray();

                Debug.Log($"[Spin] Final reel results: {string.Join(", ", reels)}");

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                  _slot1.Q<Label>("LblSlot1").text = reels.Length > 0 ? reels[0].ToString() : "?";
                  _slot2.Q<Label>("LblSlot2").text = reels.Length > 1 ? reels[1].ToString() : "?";
                  _slot3.Q<Label>("LblSlot3").text = reels.Length > 2 ? reels[2].ToString() : "?";
                });

                _pendingRollResult = false;
                Debug.Log("[Spin] _pendingRollResult reset to false after extrinsic processed.");
                await FetchAndDisplayLatestRoll();
              }
            }
          }
        };
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"[Spin] Exception during spin handling: {ex}");
      }
    }

    public override void ExitState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] ExitState");

      if (_btnSpin != null)
      {
        _btnSpin.clicked -= OnSpinClicked;
      }
      if (_substrate != null)
      {
        _substrate.OnNewBlock -= HandleNewBlock;
        Debug.Log("[Spin] Unsubscribed from OnNewBlock via event unsubscription.");
      }
    }

    private async void TriggerSlotSpinAsync()
    {
      if (_substrate == null || !_substrate.IsConnected)
      {
        Debug.LogError("[Spin] Substrate not connected.");
        return;
      }

      var player = Generic.ToAccountId32(_substrate.Account);
      var token = _cancellationToken;

      try
      {
        // 🔢 Load GameSharp info (daily rolls, etc.)
        await GameSharp.UpdateRollHistory(_substrate, token);
        int rollsUsed = GameSharp.CurrentDailyRolls != null ? GameSharp.CurrentDailyRolls.Length : 0;
        int maxRolls = (int)(EterraConfig.MaxRollsPerRound?.Value ?? 3);

        Debug.Log($"[Spin] Rolls used today: {rollsUsed} / {maxRolls}");

        if (_btnSpin != null)
        {
          _btnSpin.SetEnabled(rollsUsed < maxRolls);
        }

        // Show most recent result regardless
        // Wait for next block before fetching and displaying roll results
        Debug.Log("[Spin] Waiting for next block before fetching roll results.");
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"[Spin] Failed to initiate spin from EnterState: {ex}");
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
          var summary = WeeklyRollSummary.FromRaw(rawRolls);

          var latestRoll = summary.Rolls
              .OrderByDescending(r => r.Timestamp)
              .FirstOrDefault();

          if (latestRoll?.Result != null && latestRoll.Result.Count >= 3)
          {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
              _slot1.Q<Label>("LblSlot1").text = latestRoll.Result[0].ToString();
              _slot2.Q<Label>("LblSlot2").text = latestRoll.Result[1].ToString();
              _slot3.Q<Label>("LblSlot3").text = latestRoll.Result[2].ToString();
            });

            Debug.Log($"[Spin] Displayed latest roll: {string.Join(", ", latestRoll.Result)}");
          }
          else
          {
            Debug.LogWarning("[Spin] No valid roll result to display.");
          }
        }
        else
        {
          Debug.LogWarning("[Spin] No roll history found. Displaying default placeholders.");
          UnityMainThreadDispatcher.Instance().Enqueue(() =>
          {
            _slot1.Q<Label>("LblSlot1").text = "?";
            _slot2.Q<Label>("LblSlot2").text = "?";
            _slot3.Q<Label>("LblSlot3").text = "?";
          });
        }
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"[Spin] Failed to fetch/display roll history: {ex}");
      }
    }

    private async void HandleNewBlock(uint blockNumber)
    {
      try
      {
        Debug.Log($"[Spin] HandleNewBlock triggered. Block: {blockNumber}");

        if (!_pendingRollResult)
          return;

        var initialCount = GameSharp.CurrentDailyRolls?.Length ?? 0;
        var token = _cancellationToken;
        int maxWaitMs = 5000; // total wait time (5 seconds)
        int intervalMs = 250;
        int waited = 0;

        while (waited < maxWaitMs)
        {
          await GameSharp.UpdateRollHistory(_substrate, token);
          var currentCount = GameSharp.CurrentDailyRolls?.Length ?? 0;

          if (currentCount > initialCount)
          {
            Debug.Log($"[Spin] New roll detected. Count changed: {initialCount} -> {currentCount}");
            break;
          }

          await Task.Delay(intervalMs, token);
          waited += intervalMs;
        }

        int rollsUsed = GameSharp.CurrentDailyRolls?.Length ?? 0;
        int maxRolls = (int)(EterraConfig.MaxRollsPerRound?.Value ?? 3);
        Debug.Log($"[Spin] GameSharp updated. Rolls used: {rollsUsed}, Max: {maxRolls}");

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
          if (_btnSpin != null)
            _btnSpin.SetEnabled(rollsUsed < maxRolls);
        });

        if (rollsUsed >= maxRolls)
        {
          Debug.Log("[Spin] Max daily rolls reached. Transitioning to PlayFinishedSubState.");
          UnityMainThreadDispatcher.Instance().Enqueue(() =>
          {
            FlowController.ChangeScreenSubState(GameScreen.PlayScreen, GameSubScreen.PlayFinished);
          });
          return;
        }

        _pendingRollResult = false;
        await FetchAndDisplayLatestRoll();
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"[Spin] Error during OnNewBlock: {ex}");
      }
    }
  }
}