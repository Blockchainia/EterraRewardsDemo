using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
  internal class PlayFinishedSubState : GameBaseState
  {
    private readonly System.Random _random = new System.Random();

    private Button _btnPlay;

    private Button _btnReset;

    public PlayFinishedSubState(GameController flowController, GameBaseState parent)
        : base(flowController, parent) { }

    public override void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

      var root = FlowController.VelContainer.Q<VisualElement>("Screen");

      var lblTitle = root.Q<Label>("LblTitle");
      var slot1 = root.Q<VisualElement>("Slot1");
      var slot2 = root.Q<VisualElement>("Slot2");
      var slot3 = root.Q<VisualElement>("Slot3");
      var slotArmHolder = root.Q<VisualElement>("SlotArmHolder");
      var instantWinDisplay = root.Q<VisualElement>("InstantWinDisplay");
      var rewardHistory = root.Q<VisualElement>("RewardHistory");
      _btnPlay = root.Q<Button>("BtnSpin");

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

    private void OnExitClicked()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] Exit button clicked.");
      FlowController.ChangeScreenState(GameScreen.MainScreen);
      FlowController.ChangeScreenSubState(GameScreen.MainScreen, GameSubScreen.MainChoose);
    }
  }
}