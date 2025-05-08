using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eterra.Engine.Helper;
using Eterra.Engine.Models;
using Eterra.NetApiExt.Generated.Model.bounded_collections.bounded_vec;
using Eterra.NetApiExt.Generated.Model.pallet_eterra_daily_slots.pallet;
using Eterra.NetApiExt.Generated.Storage;
using Eterra.NetApiExt.Helper;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
  internal class PlayFinishedSubState : GameBaseState
  {
    private readonly System.Random _random = new System.Random();

    private Button _btnPlay;

    private Button _btnReset;
    private VisualElement _slot1;
    private VisualElement _slot2;
    private VisualElement _slot3;
    private SubstrateNetwork _substrate;
    private CancellationToken _cancellationToken;

    public PlayFinishedSubState(GameController flowController, GameBaseState parent)
        : base(flowController, parent) { }

    public override void EnterState()
    {
      _substrate = FlowController.Substrate;
      _cancellationToken = FlowController.CancellationToken;
      Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

      var root = FlowController.VelContainer.Q<VisualElement>("Screen");

      var lblTitle = root.Q<Label>("LblTitle");
      _slot1 = root.Q<VisualElement>("Slot1");
      _slot2 = root.Q<VisualElement>("Slot2");
      _slot3 = root.Q<VisualElement>("Slot3");
      var slotArmHolder = root.Q<VisualElement>("SlotArmHolder");
      var instantWinDisplay = root.Q<VisualElement>("InstantWinDisplay");
      var rewardHistory = root.Q<VisualElement>("RewardHistory");
      _btnPlay = root.Q<Button>("BtnSpin");

      FetchAndDisplayLatestRoll();

      if (lblTitle != null)
        lblTitle.text = "You've reached today's limit!";

      if (_btnPlay != null)
      {
        _btnPlay.SetEnabled(false);
        _btnPlay.text = "Exit";
        _btnPlay.SetEnabled(true);
        _btnPlay.clicked += OnExitClicked;
      }

      if (instantWinDisplay != null)
      {
        instantWinDisplay.style.display = DisplayStyle.Flex;
        var winMessage = instantWinDisplay.Q<Label>("LblWinMessage");
        if (winMessage != null)
          winMessage.text = "🎉 Come back tomorrow for more spins! 🎉";
      }
    }

    public override void ExitState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
      if (_btnPlay != null)
        _btnPlay.clicked -= OnExitClicked;
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

    private void OnExitClicked()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] Exit button clicked.");
      FlowController.ChangeScreenState(GameScreen.MainScreen);
      FlowController.ChangeScreenSubState(GameScreen.MainScreen, GameSubScreen.MainChoose);
    }
  }
}