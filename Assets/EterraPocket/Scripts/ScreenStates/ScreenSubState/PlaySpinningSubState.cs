using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

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

    public PlaySpinningSubState(GameController flowController, GameBaseState parent)
        : base(flowController, parent)
    {

    }

    public override void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

      var root = FlowController.VelContainer.Q<VisualElement>("Screen");

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

    private void OnSpinClicked()
    {
      Debug.Log("Spin button clicked. Placeholder handler.");
      // Placeholder for spin button click logic
    }

    public override void ExitState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] ExitState");

      if (_btnSpin != null)
      {
        _btnSpin.clicked -= OnSpinClicked;
      }
    }
  }
}