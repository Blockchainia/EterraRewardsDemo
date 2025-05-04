using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
  public class StartScreen : GameBaseState
  {

    private VisualElement _velPortrait;

    private Button _btnEnter;

    public StartScreen(GameController _flowController)
     : base(_flowController)
    {
      Debug.Log("StartScreen constructor called.");
    }

    public override void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}] EnterState");

      var visualTreeAsset = Resources.Load<VisualTreeAsset>($"UI/Screens/StartScreenUI");

      if (visualTreeAsset == null)
      {
        Debug.LogError("Failed to load StartScreenUI. Ensure the path is correct and the UXML file is in the Resources folder.");
        return;
      }

      var instance = visualTreeAsset.Instantiate();
      if (instance == null)
      {
        Debug.LogError("Failed to instantiate StartScreenUI.");
        return;
      }

      instance.style.width = new Length(100, LengthUnit.Percent);
      instance.style.height = new Length(98, LengthUnit.Percent);

      _btnEnter = instance.Q<Button>("BtnEnter");
      if (_btnEnter == null)
      {
        Debug.LogError("BtnEnter not found in StartScreenUI.");
        return;
      }

      _btnEnter.RegisterCallback<ClickEvent>(OnEnterClicked);

      if (FlowController.VelContainer == null)
      {
        Debug.LogError("FlowController.VelContainer is null.");
        return;
      }

      FlowController.VelContainer.Add(instance);
    }

    public override void ExitState()
    {
      Debug.Log($"[{this.GetType().Name}] ExitState");

      // Grid.OnSwipeEvent -= OnSwipeEvent;

      FlowController.VelContainer.RemoveAt(1);
    }

    // {
    //   if (direction == Vector3.right)
    //   {
    //     switch (Network.CurrentAccountType)
    //     {
    //       case AccountType.Alice:
    //         Network.SetAccount(AccountType.Bob);
    //         _velPortrait.style.backgroundImage = _portraitBob;
    //         _lblPlayerName.text = AccountType.Bob.ToString();
    //         _lblPlayerName.style.display = DisplayStyle.Flex;
    //         _txfCustomName.style.display = DisplayStyle.None;
    //         break;

    //       case AccountType.Bob:
    //         Network.SetAccount(AccountType.Charlie);
    //         _velPortrait.style.backgroundImage = _portraitCharlie;
    //         _lblPlayerName.text = AccountType.Charlie.ToString();
    //         _lblPlayerName.style.display = DisplayStyle.Flex;
    //         _txfCustomName.style.display = DisplayStyle.None;
    //         break;

    //       case AccountType.Charlie:
    //         Network.SetAccount(AccountType.Dave);
    //         _velPortrait.style.backgroundImage = _portraitDave;
    //         _lblPlayerName.text = AccountType.Dave.ToString();
    //         _lblPlayerName.style.display = DisplayStyle.Flex;
    //         _txfCustomName.style.display = DisplayStyle.None;
    //         break;
    //       case AccountType.Dave:
    //         Network.SetAccount(AccountType.Custom);
    //         _velPortrait.style.backgroundImage = _portraitCustom;
    //         _lblPlayerName.text = AccountType.Custom.ToString();
    //         _lblPlayerName.style.display = DisplayStyle.None;
    //         _txfCustomName.style.display = DisplayStyle.Flex;
    //         break;

    //       case AccountType.Custom:
    //       default:
    //         break;
    //     }
    //   }
    //   else if (direction == Vector3.left)
    //   {
    //     switch (Network.CurrentAccountType)
    //     {
    //       case AccountType.Bob:
    //         Network.SetAccount(AccountType.Alice);
    //         _velPortrait.style.backgroundImage = _portraitAlice;
    //         _lblPlayerName.text = AccountType.Alice.ToString();
    //         _lblPlayerName.style.display = DisplayStyle.Flex;
    //         _txfCustomName.style.display = DisplayStyle.None;
    //         break;

    //       case AccountType.Charlie:
    //         Network.SetAccount(AccountType.Bob);
    //         _velPortrait.style.backgroundImage = _portraitBob;
    //         _lblPlayerName.text = AccountType.Bob.ToString();
    //         _lblPlayerName.style.display = DisplayStyle.Flex;
    //         _txfCustomName.style.display = DisplayStyle.None;
    //         break;

    //       case AccountType.Dave:
    //         Network.SetAccount(AccountType.Charlie);
    //         _velPortrait.style.backgroundImage = _portraitCharlie;
    //         _lblPlayerName.text = AccountType.Charlie.ToString();
    //         _lblPlayerName.style.display = DisplayStyle.Flex;
    //         _txfCustomName.style.display = DisplayStyle.None;
    //         break;

    //       case AccountType.Custom:
    //         Network.SetAccount(AccountType.Dave);
    //         _velPortrait.style.backgroundImage = _portraitDave;
    //         _lblPlayerName.text = AccountType.Dave.ToString();
    //         _lblPlayerName.style.display = DisplayStyle.Flex;
    //         _txfCustomName.style.display = DisplayStyle.None;
    //         break;

    //       case AccountType.Alice:
    //       default:
    //         break;
    //     }
    //   }
    // }


    // private void OnCustomNameChanged(ChangeEvent<string> evt)
    // {
    //   if (string.IsNullOrEmpty(evt.newValue) || evt.newValue.Length < 3 || evt.newValue.Length > 7)
    //   {
    //     _btnEnter.SetEnabled(false);
    //     return;
    //   }

    //   Network.SetAccount(AccountType.Custom, evt.newValue);

    //   _btnEnter.SetEnabled(true);
    // }

    private void OnEnterClicked(ClickEvent evt)
    {
      Debug.Log("Clicked enter button!");

      FlowController.ChangeScreenState(GameScreen.MainScreen);
    }
  }
}