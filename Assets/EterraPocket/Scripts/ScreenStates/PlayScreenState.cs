using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
  public class PlayScreenState : GameBaseState
  {
    private VisualTreeAsset _playScreenUI; // Store the PlayScreen UI asset

    public int PlayerIndex { get; private set; }

    public PlayScreenState(GameController _flowController)
        : base(_flowController)
    {
      // Load assets here to avoid reloading on every state entry
      _playScreenUI = Resources.Load<VisualTreeAsset>($"UI/Screens/PlayScreenUI");
      if (_playScreenUI == null)
      {
        Debug.LogError("Failed to load PlayScreenUI asset!");
      }
    }

    public override void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}] EnterState");

      // Filler to avoid camera in the UI
      // var topFiller = FlowController.VelContainer.Q<VisualElement>("VelTopFiller");

      if (_playScreenUI != null)
      {
        var instance = _playScreenUI.Instantiate();
        instance.style.width = new Length(100, LengthUnit.Percent);
        instance.style.height = new Length(98, LengthUnit.Percent);

        // Add container
        FlowController.VelContainer.Add(instance);

        // Set consistent styling for score labels
        SetScoreLabelTextSize(FlowController.VelContainer, 30);

        // Load initial sub state
        FlowController.ChangeScreenSubState(GameScreen.PlayScreen, GameSubScreen.PlayInit);
      }
      else
      {
        Debug.LogError("PlayScreenUI asset is null! Unable to instantiate the UI.");
      }
    }

    private void SetScoreLabelTextSize(VisualElement root, int fontSize)
    {
      var lblPlayerScore = root.Q<Label>("lblPlayerScore");
      var lblOpponentScore = root.Q<Label>("lblOpponentScore");

      if (lblPlayerScore != null)
      {
        lblPlayerScore.style.fontSize = new Length(fontSize, LengthUnit.Pixel);
        Debug.Log("Player Score Label font size updated.");
      }

      if (lblOpponentScore != null)
      {
        lblOpponentScore.style.fontSize = new Length(fontSize, LengthUnit.Pixel);
        Debug.Log("Opponent Score Label font size updated.");
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