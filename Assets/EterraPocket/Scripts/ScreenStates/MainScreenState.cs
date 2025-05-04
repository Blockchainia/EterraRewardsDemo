using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
  public class MainScreenState : GameBaseState
  {

    private Button _btnFaucet;

    public MainScreenState(GameController _flowController)
        : base(_flowController)
    { }

    public override void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}] EnterState");

      // filler is to avoid camera in the ui
      var topFiller = FlowController.VelContainer.Q<VisualElement>("VelTopFiller");
      //topFiller.style.backgroundColor = GameConstant.ColorDark;

      var visualTreeAsset = Resources.Load<VisualTreeAsset>($"UI/Screens/MainScreenUI");
      var instance = visualTreeAsset.Instantiate();
      instance.style.width = new Length(100, LengthUnit.Percent);
      instance.style.height = new Length(98, LengthUnit.Percent);

      var topBound = instance.Q<VisualElement>("TopBound");

      // _lblAccount = topBound.Q<Label>("LblAccount");
      // _lblAddress = topBound.Q<Label>("LblAddress");
      // _lblToken = topBound.Q<Label>("LblToken");

      _btnFaucet = topBound.Query<Button>("BtnFaucet");
      _btnFaucet.RegisterCallback<ClickEvent>(OnFaucetClicked);
      _btnFaucet.SetEnabled(false);

      // var lblNodeUrl = topBound.Q<Label>("LblNodeUrl");
      // lblNodeUrl.text = Network.CurrentNodeType.ToString();
      // _lblNodeVersion = topBound.Q<Label>("LblNodeVersion");
      // _lblConnection = topBound.Q<Label>("LblConnection");
      // _lblBlockNumber = topBound.Q<Label>("LblBlockNumber");

      // add container
      FlowController.VelContainer.Add(instance);

      // load initial sub state
      FlowController.ChangeScreenSubState(GameScreen.MainScreen, GameSubScreen.MainChoose);

      // subscribe to connection changes
      // Network.ConnectionStateChanged += OnConnectionStateChanged;
      // Storage.OnNextBlocknumber += UpdateBlocknumber;

      // connect to substrate node
      // Network.Client.ConnectAsync(true, true, CancellationToken.None);
    }

    public override void ExitState()
    {
      Debug.Log($"[{this.GetType().Name}] ExitState");

      // unsubscribe from event
      // Network.ConnectionStateChanged -= OnConnectionStateChanged;
      // Storage.OnNextBlocknumber -= UpdateBlocknumber;

      // remove container
      FlowController.VelContainer.RemoveAt(1);
    }

    private async void OnFaucetClicked(ClickEvent evt)
    {
      _btnFaucet.SetEnabled(false);

      // var sender = Network.Sudo;
      // var target = Network.Client.Account;

      // var amountToTransfer = new BigInteger(1000 * SubstrateNetwork.DECIMALS);

      // Debug.Log($"[{nameof(NetworkManager)})] Send {amountToTransfer} from {sender.ToAccountId32().ToAddress()} to {target.ToAccountId32().ToAddress()}");

      // var subscriptionId = await Network.Client.TransferKeepAliveAsync(sender, target.ToAccountId32(), amountToTransfer, 1, CancellationToken.None);
      // if (subscriptionId == null)
      // {
      //   Debug.LogError($"[{nameof(NetworkManager)}] Transfer failed");
      //   _btnFaucet.SetEnabled(true);
      //   return;
      // }

      Debug.Log($"Faucet executed!");
      _btnFaucet.SetEnabled(true);

    }
  }
}