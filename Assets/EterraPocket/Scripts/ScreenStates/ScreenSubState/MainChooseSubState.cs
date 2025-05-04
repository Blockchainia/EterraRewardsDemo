using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
  internal class MainChooseSubState : GameBaseState
  {
    public MainScreenState PlayScreenState => ParentState as MainScreenState;

    private readonly System.Random _random = new System.Random();

    private Button _btnPlay;

    private Button _btnHistory;

    private Button _btnTrain;
    public MainChooseSubState(GameController flowController, GameBaseState parent)
        : base(flowController, parent) { }

    public override void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

      var floatBody = FlowController.VelContainer.Q<VisualElement>("FloatBody");
      floatBody.Clear();

      TemplateContainer scrollViewElement = ElementInstance("UI/Elements/ScrollViewElement");
      floatBody.Add(scrollViewElement);

      var scrollView = scrollViewElement.Q<ScrollView>("ScvElement");

      TemplateContainer elementInstance = ElementInstance("UI/Frames/PlayMenu");

      _btnPlay = elementInstance.Q<Button>("BtnPlay");
      _btnPlay.SetEnabled(false);
      _btnPlay.RegisterCallback<ClickEvent>(OnBtnPlayClicked);

      _btnTrain = elementInstance.Q<Button>("BtnHistory");
      _btnTrain.SetEnabled(true);
      _btnTrain.RegisterCallback<ClickEvent>(OnBtnHistoryClicked);

      var btnExit = elementInstance.Q<Button>("BtnExit");
      btnExit.RegisterCallback<ClickEvent>(OnBtnExitClicked);

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

    private void OnBtnHistoryClicked(ClickEvent evt)
    {
      Debug.Log($"[{this.GetType().Name}][SUB] OnBtnHistoryClicked");
      FlowController.ChangeScreenState(GameScreen.HistoryScreen);

    }

    private void OnBtnExitClicked(ClickEvent evt)
    {
      Debug.Log($"[{this.GetType().Name}][SUB] OnBtnExitClicked");
    }
  }
}