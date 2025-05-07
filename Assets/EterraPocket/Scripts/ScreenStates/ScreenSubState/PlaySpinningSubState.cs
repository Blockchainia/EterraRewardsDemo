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

      Debug.Log(_lblTitle != null ? "LblTitle element found." : "LblTitle element not found!");
      Debug.Log(_slot1 != null ? "Slot1 element found." : "Slot1 element not found!");
      Debug.Log(_slot2 != null ? "Slot2 element found." : "Slot2 element not found!");
      Debug.Log(_slot3 != null ? "Slot3 element found." : "Slot3 element not found!");
      Debug.Log(_slotArmHolder != null ? "SlotArmHolder element found." : "SlotArmHolder element not found!");
      Debug.Log(_instantWinDisplay != null ? "InstantWinDisplay element found." : "InstantWinDisplay element not found!");
      Debug.Log(_rewardHistory != null ? "RewardHistory element found." : "RewardHistory element not found!");
      Debug.Log(_btnSpin != null ? "BtnSpin element found." : "BtnSpin element not found!");

      InitializeGameState();

      _substrate.OnNewBlock += HandleNewBlock;
      Debug.Log("[Spin] Subscribed to OnNewBlock via event subscription.");
    }

    private void InitializeGameState()
    {
      _instantWinDisplay?.Clear();
      _rewardHistory?.Clear();
      if (_lblTitle != null) _lblTitle.text = "Spinning Slots...";
      if (_btnSpin != null)
      {
        _btnSpin.SetEnabled(false);
        _btnSpin.clicked += OnSpinClicked;
      }
    }

    private async void OnSpinClicked()
    {
      Debug.Log("Spin button clicked.");

      if (_substrate == null || !_substrate.IsConnected)
      {
        Debug.LogError("Not connected to Substrate network.");
        return;
      }

      var player = Generic.ToAccountId32(_substrate.Account);
      var token = _cancellationToken;

      try
      {
        var spinSubId = await _substrate.SpinSlotAsync(player, 1, token);
        Debug.Log($"SpinSlotAsync submitted. Subscription ID: {spinSubId}");
        Debug.Log($"[Spin] Submitted extrinsic. Subscription ID: {spinSubId}");

        _substrate.ExtrinsicManager.ExtrinsicUpdated += async (id, info) =>
        {
          Debug.Log($"[Spin] ExtrinsicUpdate triggered. ID: {id}, Status: {info.TransactionEvent}");
          if (id != spinSubId || !(info.IsInBlock || info.IsCompleted)) return;

          Debug.Log("Spin extrinsic finalized or in block.");

          if (info.EventRecords != null)
          {
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

                _pendingRollResult = true;
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
        await Task.Delay(250); // Wait for roll to be stored
        Debug.Log("[Spin] New block detected.");

        await GameSharp.UpdateRollHistory(_substrate, _cancellationToken);
        int rollsUsed = GameSharp.CurrentDailyRolls?.Length ?? 0;
        int maxRolls = (int)(EterraConfig.MaxRollsPerRound?.Value ?? 3);
        Debug.Log($"[Spin] GameSharp updated. Rolls used: {rollsUsed}, Max: {maxRolls}");

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
          if (_btnSpin != null)
          {
            _btnSpin.SetEnabled(rollsUsed < maxRolls);
          }
        });

        if (_pendingRollResult)
        {
          _pendingRollResult = false;
          await FetchAndDisplayLatestRoll();
        }
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"[Spin] Error during OnNewBlock: {ex}");
      }
    }
  }
}