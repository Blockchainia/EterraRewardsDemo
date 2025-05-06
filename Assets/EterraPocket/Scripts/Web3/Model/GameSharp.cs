using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eterra.NetApiExt.Generated.Model.pallet_eterra_daily_slots.pallet;
using Eterra.NetApiExt.Generated.Model.sp_core.crypto;
using Eterra.NetApiExt.Generated.Model.bounded_collections.bounded_vec;
using Eterra.NetApiExt.Generated.Storage;
using Eterra.Engine.Helper;
using Substrate.NetApi.Model.Types.Base;
using Eterra.Engine.Models;
using UnityEngine;
using Assets.Scripts;
using Eterra.Integration.Helper;


namespace Eterra.Engine.Logic
{
  public static class GameSharp
  {
    public static int CurrentDailyRollsCount { get; private set; } = 0;
    public static RollResult[] CurrentDailyRolls { get; private set; } = Array.Empty<RollResult>();

    public static async Task UpdateRollHistory(SubstrateNetwork substrate, CancellationToken token)
    {
      if (substrate == null || !substrate.IsConnected)
      {
        Debug.LogError("[GameSharp] Substrate client not connected.");
        return;
      }

      try
      {
        var player = Generic.ToAccountId32(substrate.Account);

        var history = await substrate.ApiClient.GetStorageAsync<BoundedVecT8>(
            EterraDailySlotsStorage.RollHistoryParams(player), null, token);

        if (history?.Value != null)
        {
          var rawRolls = BaseVecHelper.ToArray<RollResult>(history.Value);
          var today = DateTime.UtcNow.Date;

          var todayRolls = rawRolls
              .Where(r =>
              {
                  try
                  {
                      var timestampSeconds = ((U64)r.Timestamp).Value;
                      var dateTime = DateTimeOffset.FromUnixTimeSeconds((long)timestampSeconds).UtcDateTime;
                      return dateTime.Date == today;
                  }
                  catch
                  {
                      return false;
                  }
              })
              .ToArray();

          CurrentDailyRolls = todayRolls;
          CurrentDailyRollsCount = todayRolls.Length;

          Debug.Log($"[GameSharp] Updated roll history. Today's rolls: {CurrentDailyRollsCount}");
        }
        else
        {
          Debug.LogWarning("[GameSharp] No roll history found.");
          CurrentDailyRolls = Array.Empty<RollResult>();
          CurrentDailyRollsCount = 0;
        }
      }
      catch (Exception ex)
      {
        Debug.LogError($"[GameSharp] Error updating roll history: {ex}");
      }
    }
  }
}