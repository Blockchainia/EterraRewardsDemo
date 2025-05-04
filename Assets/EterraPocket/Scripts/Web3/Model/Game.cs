using Eterra.NetApiExt.Generated.Model.pallet_eterra.types.game;
using Eterra.NetApiExt.Generated.Model.pallet_eterra.types.card;
using Eterra.NetApiExt.Generated.Model.sp_core.crypto;
using Eterra.NetApiExt.Generated.Types.Base;
using Eterra.NetApiExt.Generated.Model.bounded_collections.bounded_vec;
using Substrate.NetApi.Model.Types.Primitive;
using Substrate.NetApi.Model.Types.Base;
using System;
using System.Linq;
using Eterra.Integration.Helpers;

namespace Eterra.Integration.Model
{
  public class Game
  {
    public byte[] GameId { get; private set; }
    public GameState State { get; private set; }
    public uint LastPlayedBlock { get; private set; }
    public AccountId32[] Players { get; private set; }
    public byte PlayerTurn { get; private set; }
    public byte Round { get; private set; }
    public byte MaxRounds { get; private set; }
    public Board Board { get; private set; }
    public (byte Player1Score, byte Player2Score) Scores { get; private set; }
    public (Color Player1Color, Color Player2Color) PlayerColors { get; private set; }

    /// <summary>
    /// Constructor for Game from GameSharp
    /// </summary>
    public Game(GameSharp gameSharp)
    {
      GameId = gameSharp.GameId;
      State = gameSharp.State;
      LastPlayedBlock = gameSharp.LastPlayedBlock;
      Players = gameSharp.Players;
      PlayerTurn = gameSharp.PlayerTurn;
      Round = gameSharp.Round;
      MaxRounds = gameSharp.MaxRounds;
      Scores = gameSharp.Scores;
      PlayerColors = gameSharp.PlayerColors;
      Board = gameSharp.Board;
    }

    /// <summary>
    /// Initializes the game with default values.
    /// </summary>
    public void Init()
    {
      PlayerTurn = 0;
      Round = 0;
      Scores = (0, 0);
      State = GameState.Matchmaking;
      Board = new Board(new Arr4Arr4BaseOpt());
    }

    /// <summary>
    /// Imports a Game from a GameSharp instance.
    /// </summary>
    public static Game Import(GameSharp gameSharp)
    {
      return new Game(gameSharp);
    }
  }
}