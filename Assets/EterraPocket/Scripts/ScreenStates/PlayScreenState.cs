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
    private VisualElement _instance;       // Keep reference to remove it on Exit

    public int PlayerIndex { get; private set; }

    public PlayScreenState(GameController _flowController)
        : base(_flowController)
    {
      _playScreenUI = Resources.Load<VisualTreeAsset>($"UI/Screens/PlayScreenUI");
      if (_playScreenUI == null)
      {
        Debug.LogError("Failed to load PlayScreenUI asset!");
      }
    }

    public override void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}] EnterState");

      if (_playScreenUI != null)
      {
        _instance = _playScreenUI.Instantiate();
        _instance.style.width = new Length(100, LengthUnit.Percent);
        _instance.style.height = new Length(98, LengthUnit.Percent);

        FlowController.VelContainer.Add(_instance);

        FlowController.ChangeScreenSubState(GameScreen.PlayScreen, GameSubScreen.PlayRolling);
      }
      else
      {
        Debug.LogError("PlayScreenUI asset is null! Unable to instantiate the UI.");
      }
    }

    public override void ExitState()
    {
      Debug.Log($"[{this.GetType().Name}] ExitState");

      if (_instance != null)
      {
        FlowController.VelContainer.Remove(_instance);
        _instance = null;
      }
    }
  }
}