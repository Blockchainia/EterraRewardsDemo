using System.Linq;
using System.Reflection;
using Serilog;
using Substrate.NetApi;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using Substrate.NetApi.Model.Types;
using Eterra.NetApiExt.Generated.Model.sp_core.crypto;
using Eterra.NetApiExt.Generated.Model.pallet_eterra_daily_slots.pallet;
using Eterra.NetApiExt.Generated.Storage;
using Eterra.NetApiExt.Generated.Model.solochain_template_runtime;
using Eterra.NetApiExt.Helper;
using Eterra.Engine.Models;
using Eterra.Engine.Helper;
using Assets.Scripts.ScreenStates;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using Eterra.Engine; // Add this for SubstrateNetwork
using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts;

namespace Assets.Scripts
{
  public enum GameScreen
  {
    StartScreen,
    MainScreen,
    PlayScreen,
    HistoryScreen
  }

  public enum GameSubScreen
  {
    MainChoose,
    Play,
    PlayInit,
    PlaySpinning,
    PlayEnabled,
    PlayFinished,
    HistoryInit,
  }

  public class GameController : ScreenStateMachine<GameScreen, GameSubScreen>
  {
    internal readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();
    public Vector2 ScrollOffset { get; set; }
    public VisualElement VelContainer { get; private set; }

    private SubstrateNetwork _substrate;
    private CancellationTokenSource _cts;

    public SubstrateNetwork Substrate => _substrate;
    public CancellationToken CancellationToken => _cts?.Token ?? CancellationToken.None;

    private new void Awake()
    {
      base.Awake();

      _cts = new CancellationTokenSource();
      _substrate = new SubstrateNetwork(Eterra.NetApiExt.Client.BaseClient.Alice, "ws://127.0.0.1:9944");
      _substrate.ConnectAsync(true, true, _cts.Token).ContinueWith(task =>
      {
        if (task.Exception != null)
          Debug.LogError("Failed to connect to Substrate: " + task.Exception.InnerException?.Message);
      });
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
      var root = GetComponent<UIDocument>().rootVisualElement;

      if (root == null)
      {
        Debug.LogError("UIDocument root is null. Ensure the UI Document is assigned correctly.");
        return;
      }

      VelContainer = root.Q<VisualElement>("VelContainer");
      VelContainer.RemoveAt(1);

      if (VelContainer == null)
      {
        Debug.LogError("VelContainer not found in UI Document. Ensure it exists in the UXML.");
        return;
      }

      ChangeScreenState(GameScreen.StartScreen);
    }

    protected override void InitializeStates()
    {
      _stateDictionary.Add(GameScreen.StartScreen, new StartScreen(this));

      var mainScreen = new MainScreenState(this);
      _stateDictionary.Add(GameScreen.MainScreen, mainScreen);
      _subStateDictionary.Add(GameScreen.MainScreen, new Dictionary<GameSubScreen, IScreenState>
      {
        { GameSubScreen.MainChoose, new MainChooseSubState(this, mainScreen) },
        { GameSubScreen.Play, new MainPlaySubState(this, mainScreen) },
        { GameSubScreen.HistoryInit, new MainPlaySubState(this, mainScreen) },
      });

      var playScreen = new PlayScreenState(this);
      _stateDictionary.Add(GameScreen.PlayScreen, playScreen);
      _subStateDictionary.Add(GameScreen.PlayScreen, new Dictionary<GameSubScreen, IScreenState>
      {
        { GameSubScreen.PlayInit, new PlayInitSubState(this, playScreen) },
        { GameSubScreen.PlaySpinning, new PlaySpinningSubState(this, playScreen) },
        { GameSubScreen.PlayEnabled, new PlayEnabledSubState(this, playScreen) },
        { GameSubScreen.PlayFinished, new PlayFinishedSubState(this, playScreen) },
      });

      var historyScreen = new HistoryScreenState(this);
      _stateDictionary.Add(GameScreen.HistoryScreen, historyScreen);
      _subStateDictionary.Add(GameScreen.HistoryScreen, new Dictionary<GameSubScreen, IScreenState>
      {
        { GameSubScreen.HistoryInit, new HistoryInitSubState(this, historyScreen) },
      });
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
      // Method intentionally left empty.
    }
  }
}
