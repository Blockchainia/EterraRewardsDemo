using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using Eterra.Engine.Logic;
using System.Threading;


namespace Assets.Scripts.ScreenStates
{
  internal class PlayInitSubState : GameBaseState
  {
    private VisualElement _slot1;
    private VisualElement _slot2;
    private VisualElement _slot3;
    private VisualElement _slotArmHolder;
    private Label _lblTitle;
    private VisualElement _instantWinDisplay;
    private VisualElement _rewardHistory;
    private Button _btnSpin;

    public PlayInitSubState(GameController flowController, GameBaseState parent)
        : base(flowController, parent) { }

    public override async void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

      await GameSharp.UpdateRollHistory((FlowController as GameController)?.Substrate, CancellationToken.None);

      var root = FlowController.VelContainer.Q<VisualElement>("Screen");

      _lblTitle = root.Q<Label>("LblTitle");
      _slot1 = root.Q<VisualElement>("Slot1");
      _slot2 = root.Q<VisualElement>("Slot2");
      _slot3 = root.Q<VisualElement>("Slot3");
      _slotArmHolder = root.Q<VisualElement>("SlotArmHolder");
      _instantWinDisplay = root.Q<VisualElement>("InstantWinDisplay");
      _rewardHistory = root.Q<VisualElement>("RewardHistory");

      if (_lblTitle != null) Debug.Log("[UI] LblTitle found: " + _lblTitle.text);
      if (_slot1 == null || _slot2 == null || _slot3 == null) Debug.LogError("[UI] One or more Slot elements not found");
      if (_slotArmHolder == null) Debug.LogError("[UI] SlotArmHolder not found");
      if (_instantWinDisplay == null) Debug.LogError("[UI] InstantWinDisplay not found");
      if (_rewardHistory == null) Debug.LogError("[UI] RewardHistory not found");

      _btnSpin = _slotArmHolder.Q<Button>("BtnSpin");
      if (_btnSpin == null)
      {
        Debug.LogError("[UI] BtnSpin not found!");
      }
      else
      {
        int rollsUsed = GameSharp.CurrentDailyRolls?.Length ?? 0;
        int maxRolls = (int)(Eterra.Config.EterraConfig.MaxRollsPerRound?.Value ?? 3);

        if (rollsUsed >= maxRolls)
        {
          _btnSpin.text = "Exit";
          _btnSpin.SetEnabled(true);
          _btnSpin.clicked += () =>
          {
            Debug.Log("[UI] Max rolls reached. Returning to MainChooseSubState.");
            FlowController.ChangeScreenState(GameScreen.MainScreen);
            FlowController.ChangeScreenSubState(GameScreen.MainScreen, GameSubScreen.MainChoose);
          };
        }
        else
        {
          _btnSpin.text = "Spin!";
          _btnSpin.SetEnabled(true);
          _btnSpin.clicked += OnSpinClicked;
        }
      }

      InitializeDisplay();
    }

    private void InitializeDisplay()
    {
      _instantWinDisplay?.Clear();
      _instantWinDisplay?.Add(new Label("Instant Win Display Initialized"));

      _rewardHistory?.Clear();
      _rewardHistory?.Add(new Label("History Panel Initialized"));

      if (_lblTitle != null)
        _lblTitle.text = "🎰 Welcome to the Slot Machine";
    }

    public override void ExitState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
      if (_btnSpin != null)
        _btnSpin.clicked -= OnSpinClicked;
    }

    private void OnSpinClicked()
    {
      Debug.Log("[UI] Spin button clicked. Transitioning to PlaySpinningSubState.");
      _btnSpin.SetEnabled(false);
      FlowController.ChangeScreenSubState(GameScreen.PlayScreen, GameSubScreen.PlaySpinning);
    }
  }
}
