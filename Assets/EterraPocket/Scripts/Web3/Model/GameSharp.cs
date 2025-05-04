using Eterra.NetApiExt.Generated.Model.pallet_eterra.types.game;
using Eterra.NetApiExt.Generated.Model.pallet_eterra.types.card;
using Eterra.NetApiExt.Generated.Model.sp_core.crypto;
using Eterra.NetApiExt.Generated.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using Substrate.NetApi.Model.Types.Base;
using System;
using System.Linq;
using Eterra.Integration.Helpers; // ✅ Importing BaseVecHelper

namespace Eterra.Integration.Model
{
  public class GameSharp
  {
    public GameSharp(byte[] gameId, Eterra.NetApiExt.Generated.Model.pallet_eterra.types.game.Game game)
    {
      GameId = gameId;
      State = game.State.Value;
      LastPlayedBlock = game.LastPlayedBlock.Value;

      // ✅ Convert `BoundedVecT5` to `BaseVec<AccountId32>` before calling the helper
      Players = BaseVecHelper.ToArray<AccountId32>(new BaseVec<AccountId32>(game.Players.Value));

      PlayerTurn = game.PlayerTurn.Value;
      Round = game.Round.Value;
      MaxRounds = game.MaxRounds.Value;
      Scores = (((U8)game.Scores.Value[0]).Value, ((U8)game.Scores.Value[1]).Value);
      PlayerColors = ((EnumColor)game.PlayerColors.Value[0], (EnumColor)game.PlayerColors.Value[1]);
      Board = new Board(game.Board);
    }

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
  }
}