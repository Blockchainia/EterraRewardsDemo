using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
  public class HistoryScreenState : GameBaseState
  {
    private VisualTreeAsset _historyScreenUI; // Store the PlayScreen UI asset

    public HistoryScreenState(GameController _flowController)
        : base(_flowController)
    {
      // Load assets here to avoid reloading on every state entry
      _historyScreenUI = Resources.Load<VisualTreeAsset>($"UI/Screens/HistoryScreenUI");
      if (_historyScreenUI == null)
      {
        Debug.LogError("Failed to load HistoryScreenUI asset!");
      }
    }

    public override void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}] EnterState");

      // Filler to avoid camera in the UI
      // var topFiller = FlowController.VelContainer.Q<VisualElement>("VelTopFiller");

      if (_historyScreenUI != null)
      {
        var instance = _historyScreenUI.Instantiate();
        instance.style.width = new Length(100, LengthUnit.Percent);
        instance.style.height = new Length(98, LengthUnit.Percent);

        // Add container
        FlowController.VelContainer.Add(instance);

        // Load initial sub state
        FlowController.ChangeScreenSubState(GameScreen.HistoryScreen, GameSubScreen.HistoryInit);
      }
      else
      {
        Debug.LogError("HistoryScreenUI asset is null! Unable to instantiate the UI.");
      }
    }

    public override void ExitState()
    {
      Debug.Log($"[{this.GetType().Name}] ExitState");

      // Remove container
      FlowController.VelContainer.RemoveAt(1);
    }
  }
}