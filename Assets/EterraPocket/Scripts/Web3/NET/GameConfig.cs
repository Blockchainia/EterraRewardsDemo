using System;
using System.Collections.Generic;
using Eterra.Integration.Model;
using Eterra.NetApiExt.Generated.Model.pallet_eterra.types.card; // Ensure this namespace contains 'Color'
namespace Eterra.Config
{
  public class EterraConfig
  {
    private static EterraConfig _instance;
    private static readonly object _lock = new object();
    private static bool _isInitialized = false;

    public static EterraConfig GetInstance()
    {
      lock (_lock)
      {
        if (_instance == null)
        {
          _instance = new Builder().Build();
        }
        return _instance;
      }
    }

    public static void Initialize(Builder builder)
    {
      lock (_lock)
      {
        if (!_isInitialized)
        {
          _instance = builder.Build();
          _isInitialized = true;
        }
      }
    }

    // Game configuration properties
    public int NumPlayers { get; private set; }
    public int MaxRounds { get; private set; }
    public int BlocksToPlayLimit { get; private set; }

    // Default values
    private const int DefaultNumPlayers = 2;
    private const int DefaultMaxRounds = 10;
    private const int DefaultBlocksToPlayLimit = 5;

    private static readonly Dictionary<Color, byte[]> DefaultPlayerStartResources = new Dictionary<Color, byte[]>
        {
            { Color.Blue, new byte[] { 5, 5, 0, 0, 0, 0, 0 } },
            { Color.Red, new byte[] { 5, 5, 0, 0, 0, 0, 0 } }
        };


    private EterraConfig()
    {
      NumPlayers = DefaultNumPlayers;
      MaxRounds = DefaultMaxRounds;
      BlocksToPlayLimit = DefaultBlocksToPlayLimit;
    }

    public class Builder
    {
      private readonly EterraConfig _config = new EterraConfig();

      public Builder SetNumPlayers(int numPlayers)
      {
        _config.NumPlayers = numPlayers;
        return this;
      }

      public Builder SetMaxRounds(int maxRounds)
      {
        _config.MaxRounds = maxRounds;
        return this;
      }

      public Builder SetBlocksToPlayLimit(int limit)
      {
        _config.BlocksToPlayLimit = limit;
        return this;
      }


      public EterraConfig Build()
      {
        return _config;
      }
    }
  }

  public static class GameConfig
  {
    public static readonly GameSetup[] GAME_RULES = new GameSetup[]
    {
            new GameSetup
            {
                GameMode = "Standard",
                MaxRounds = 10,
            }
    };
  }

  public class GameSetup
  {
    public string GameMode { get; set; }
    public int MaxRounds { get; set; }
  }
}