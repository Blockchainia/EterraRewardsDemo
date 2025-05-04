using Assets.Scripts.ScreenStates;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

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
    //   internal NetworkManager Network => NetworkManager.GetInstance();
    //   internal StorageManager Storage => StorageManager.GetInstance();

    internal readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

    public Vector2 ScrollOffset { get; set; }

    // public CacheData CacheData { get; private set; }

    public VisualElement VelContainer { get; private set; }

    private new void Awake()
    {
      base.Awake();
      //Your code goes here

      //CacheData = new CacheData();
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

      var mainScreenSubStates = new Dictionary<GameSubScreen, IScreenState>
            {
                { GameSubScreen.MainChoose, new MainChooseSubState(this, mainScreen) },
                { GameSubScreen.Play, new MainPlaySubState(this, mainScreen) },
                { GameSubScreen.HistoryInit, new MainPlaySubState(this, mainScreen) },

            };
      _subStateDictionary.Add(GameScreen.MainScreen, mainScreenSubStates);

      var playScreen = new PlayScreenState(this);
      _stateDictionary.Add(GameScreen.PlayScreen, playScreen);

      var playScreenSubStates = new Dictionary<GameSubScreen, IScreenState>
            {
                { GameSubScreen.PlayInit, new PlayInitSubState(this, playScreen) },
                { GameSubScreen.PlaySpinning, new PlaySpinningSubState(this, playScreen) },
                { GameSubScreen.PlayEnabled, new PlayEnabledSubState(this, playScreen) },
                { GameSubScreen.PlayFinished, new PlayFinishedSubState(this, playScreen) },
            };
      _subStateDictionary.Add(GameScreen.PlayScreen, playScreenSubStates);

      var historyScreen = new HistoryScreenState(this);
      _stateDictionary.Add(GameScreen.HistoryScreen, historyScreen);

      var historyScreenSubStates = new Dictionary<GameSubScreen, IScreenState>
            {
                { GameSubScreen.HistoryInit, new HistoryInitSubState(this, historyScreen) },
            };
      _subStateDictionary.Add(GameScreen.HistoryScreen, historyScreenSubStates);
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
