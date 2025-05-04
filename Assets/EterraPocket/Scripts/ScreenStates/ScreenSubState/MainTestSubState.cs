using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
  internal class MainTestSubState : GameBaseState
  {
    public MainScreenState PlayScreenState => ParentState as MainScreenState;

    private readonly System.Random _random = new System.Random();

    private Button _btnTestBalance;

    private Button _btnCreateGame;

    private Button _btnTransfer;
    public MainTestSubState(GameController flowController, GameBaseState parent)
        : base(flowController, parent) { }

    public override void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

      var floatBody = FlowController.VelContainer.Q<VisualElement>("FloatBody");
      floatBody.Clear();

      TemplateContainer scrollViewElement = ElementInstance("UI/Elements/ScrollViewElement");
      floatBody.Add(scrollViewElement);

      var scrollView = scrollViewElement.Q<ScrollView>("ScvElement");

      TemplateContainer elementInstance = ElementInstance("UI/Frames/MainTest");

      _btnTransfer = elementInstance.Q<Button>("BtnTransfer");
      _btnTransfer.SetEnabled(true);
      _btnTransfer.RegisterCallback<ClickEvent>(OnBtnTransferClicked);

      var _btnCreateGame = elementInstance.Q<Button>("BtnCreateGame");
      _btnCreateGame.RegisterCallback<ClickEvent>(OnBtnCreateGameClicked);

      // add element
      scrollView.Add(elementInstance);
    }

    public override void ExitState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
    }


    private void OnBtnPlayClicked(ClickEvent evt)
    {
      Debug.Log($"[{this.GetType().Name}][SUB] OnBtnPlayClicked");
      FlowController.ChangeScreenState(GameScreen.PlayScreen);

    }

    private async void OnBtnTransferClicked(ClickEvent evt)
    {
      Debug.Log($"[{this.GetType().Name}][SUB] OnBtnTransferClicked");

      // Ensure BalanceTransfer instance exists
      if (BalanceTransfer.Instance == null)
      {
        Debug.LogError("[MainTestSubState] BalanceTransfer instance is null!");
        return;
      }

      // Ensure client is initialized and connected
      if (BalanceTransfer.Instance.Client == null || !BalanceTransfer.Instance.Client.IsConnected)
      {
        Debug.LogError("[MainTestSubState] BalanceTransfer client is not connected!");
        return;
      }

      // Call SubmitTransfer asynchronously
      await BalanceTransfer.Instance.SubmitTransfer(BalanceTransfer.Instance.Client);
    }

    private async void OnBtnCreateGameClicked(ClickEvent evt)
    {
      Debug.Log($"[{this.GetType().Name}][SUB] OnBtnCreateGameClicked");
      // Ensure BalanceTransfer instance exists
      if (BalanceTransfer.Instance == null)
      {
        Debug.LogError("[MainTestSubState] BalanceTransfer instance is null!");
        return;
      }

      // Ensure client is initialized and connected
      if (BalanceTransfer.Instance.Client == null || !BalanceTransfer.Instance.Client.IsConnected)
      {
        Debug.LogError("[MainTestSubState] BalanceTransfer client is not connected!");
        return;
      }

      // Call SubmitTransfer asynchronously
      await BalanceTransfer.Instance.CreateGame(BalanceTransfer.Instance.Client);

      // FlowController.ChangeScreenState(GameScreen.PlayScreen);

    }

    private async void OnBtnResetClicked(ClickEvent evt)
    {
      Debug.Log($"[{this.GetType().Name}][SUB] OnBtnResetClicked");

    }

    private void OnBtnExitClicked(ClickEvent evt)
    {
      Debug.Log($"[{this.GetType().Name}][SUB] OnBtnExitClicked");
    }

  }

}