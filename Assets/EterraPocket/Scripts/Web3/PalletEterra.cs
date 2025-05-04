// using Serilog;
// using Eterra.Integration.Model;
// using Eterra.NetApiExt.Generated.Model.pallet_eterra.pallet;
// using Eterra.NetApiExt.Generated.Model.sp_core.crypto;
// using Eterra.NetApiExt.Generated.Storage;
// using Eterra.NetApiExt.Generated.Types.Base;
// using Eterra.NetApiExt.Client;
// using Eterra.NetApiExt.Generated.Model.pallet_eterra.types.game;
// using Substrate.Integration.Helper;
// using Substrate.NetApi;
// using Substrate.NetApi.Model.Types;
// using Substrate.NetApi.Model.Types.Base;
// using Substrate.NetApi.Model.Types.Primitive;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;

// namespace Substrate.Integration
// {
//   /// <summary>
//   /// Substrate network
//   /// </summary>
//   public partial class SubstrateNetwork : BaseClient
//   {
//     #region storage

//     /// <summary>
//     /// Get game
//     /// </summary>
//     /// <param name="gameId"></param>
//     /// <param name="token"></param>
//     // /// <returns></returns>
//     // public async Task<GameSharp?> GetGameAsync(byte[] gameId, CancellationToken token)
//     // {
//     //   if (!IsConnected)
//     //   {
//     //     Log.Warning("Currently not connected to the network!");
//     //     return null;
//     //   }

//     //   var key = new Eterra.NetApiExt.Generated.Types.Base.Arr32U8();
//     //   key.Create(gameId);

//     //   var result = await SubstrateClient.EterraModuleStorage.GameStorage(key, token);

//     //   if (result == null) return null;

//     //   return new GameSharp(gameId, result);
//     // }

//     /// <summary>
//     /// Get board
//     /// </summary>
//     /// <param name="playerAddress"></param>
//     /// <param name="token"></param>
//     /// <returns></returns>
//     // public async Task<BoardSharp?> GetBoardAsync(string playerAddress, CancellationToken token)
//     // {
//     //   var key = new AccountId32();
//     //   key.Create(Utils.GetPublicKeyFrom(playerAddress));

//     //   var result = await SubstrateClient.EterraModuleStorage.HexBoardStorage(key, token);

//     //   if (result == null) return null;

//     //   return new BoardSharp(result);
//     // }

//     #endregion storage

//     #region call

//     /// <summary>
//     /// Create game
//     /// </summary>
//     /// <param name="account"></param>
//     /// <param name="players"></param>
//     /// <param name="gridSize"></param>
//     /// <param name="concurrentTasks"></param>
//     /// <param name="token"></param>
//     /// <returns></returns>
//     public async Task<string?> CreateGameAsync(Account account, List<Account> players, byte gridSize, int concurrentTasks, CancellationToken token)
//     {
//       var extrinsicType = $"Eterra.CreateGame";

//       var extrinsic = EterraCalls.CreateGame(new BaseVec<AccountId32>(players.Select(p => p.ToAccountId32()).ToArray()));

//       return await GenericExtrinsicAsync(account, extrinsicType, extrinsic, concurrentTasks, token);
//     }

//     /// <summary>
//     /// Play
//     /// </summary>
//     /// <param name="account"></param>
//     /// <param name="placeIndex"></param>
//     /// <param name="buyIndex"></param>
//     /// <param name="payType"></param>
//     /// <param name="concurrentTasks"></param>
//     /// <param name="token"></param>
//     /// <returns></returns>
//     public async Task<string?> PlayAsync(Account account, byte placeIndex, byte buyIndex, int concurrentTasks, CancellationToken token)
//     {
//       var extrinsicType = $"Eterra.Play";

//       var moveStruct = new Move
//       {
//         PlaceIndexX = new U8(placeIndex),
//         PlaceIndexY = new U8(buyIndex),
//       };

//       var extrinsic = EterraCalls.Play(0, moveStruct);

//       return await GenericExtrinsicAsync(account, extrinsicType, extrinsic, concurrentTasks, token);
//     }


//     /// <summary>
//     /// Finish turn
//     /// </summary>
//     /// <param name="account"></param>
//     /// <param name="concurrentTasks"></param>
//     /// <param name="token"></param>
//     /// <returns></returns>
//     public async Task<string?> FinishTurnAsync(Account account, int concurrentTasks, CancellationToken token)
//     {
//       var extrinsicType = $"Eterra.ForceFinishTurn";

//       var extrinsic = EterraCalls.ForceFinishTurn(0);

//       return await GenericExtrinsicAsync(account, extrinsicType, extrinsic, concurrentTasks, token);
//     }



//     #endregion call
//   }
// }