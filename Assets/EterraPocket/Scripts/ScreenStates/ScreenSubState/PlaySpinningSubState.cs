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
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

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
      if (_lblTitle != null)
      {
        Debug.Log("LblTitle element found.");
      }
      else
      {
        Debug.LogError("LblTitle element not found!");
      }

      _slot1 = root.Q<VisualElement>("Slot1");
      if (_slot1 != null)
      {
        Debug.Log("Slot1 element found.");
      }
      else
      {
        Debug.LogError("Slot1 element not found!");
      }

      _slot2 = root.Q<VisualElement>("Slot2");
      if (_slot2 != null)
      {
        Debug.Log("Slot2 element found.");
      }
      else
      {
        Debug.LogError("Slot2 element not found!");
      }

      _slot3 = root.Q<VisualElement>("Slot3");
      if (_slot3 != null)
      {
        Debug.Log("Slot3 element found.");
      }
      else
      {
        Debug.LogError("Slot3 element not found!");
      }

      _slotArmHolder = root.Q<VisualElement>("SlotArmHolder");
      if (_slotArmHolder != null)
      {
        Debug.Log("SlotArmHolder element found.");
      }
      else
      {
        Debug.LogError("SlotArmHolder element not found!");
      }

      _instantWinDisplay = root.Q<VisualElement>("InstantWinDisplay");
      if (_instantWinDisplay != null)
      {
        Debug.Log("InstantWinDisplay element found.");
      }
      else
      {
        Debug.LogError("InstantWinDisplay element not found!");
      }

      _rewardHistory = root.Q<VisualElement>("RewardHistory");
      if (_rewardHistory != null)
      {
        Debug.Log("RewardHistory element found.");
      }
      else
      {
        Debug.LogError("RewardHistory element not found!");
      }

      _btnSpin = root.Q<Button>("BtnSpin");
      if (_btnSpin != null)
      {
        Debug.Log("BtnSpin element found.");
      }
      else
      {
        Debug.LogError("BtnSpin element not found!");
      }

      InitializeGameState();
      TriggerSlotSpinAsync();
    }

    private void InitializeGameState()
    {
      if (_instantWinDisplay != null)
      {
        _instantWinDisplay.Clear();
        // Initialize _instantWinDisplay as needed
      }

      if (_rewardHistory != null)
      {
        _rewardHistory.Clear();
        // Initialize _rewardHistory as needed
      }

      if (_lblTitle != null)
      {
        _lblTitle.text = "Spinning Slots...";
      }

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
        var spinSubId = await _substrate.SpinSlotAsync(player, 1, token);
        Debug.Log($"[Spin] Triggered from EnterState. Subscription ID: {spinSubId}");
        await FetchAndDisplayLatestRoll();
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
          Debug.LogWarning("[Spin] No roll history found.");
        }
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"[Spin] Failed to fetch/display roll history: {ex}");
      }
    }
  }
}