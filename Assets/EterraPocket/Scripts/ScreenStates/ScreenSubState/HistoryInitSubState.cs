using Assets.Scripts.ScreenStates;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Eterra.Engine.Helper;
using Eterra.Engine.Models;
using Substrate.NetApi.Model.Rpc;
using Eterra.NetApiExt.Generated.Model.solochain_template_runtime;
using Eterra.NetApiExt.Generated.Model.pallet_eterra_daily_slots.pallet;
using Eterra.NetApiExt.Generated.Model.sp_core.crypto;
using Assets.Scripts;
using Eterra.Integration.Helper;
using Eterra.NetApiExt.Generated.Model.bounded_collections.bounded_vec;
using Eterra.NetApiExt.Generated.Storage;
using Eterra.NetApiExt.Generated;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using Eterra.Config;
using Eterra.Engine.Logic;

namespace Assets.Scripts
{
  internal class HistoryInitSubState : GameBaseState
  {
    public HistoryScreenState PlayScreenState => ParentState as HistoryScreenState;

    public HistoryInitSubState(GameController flowController, GameBaseState parent)
        : base(flowController, parent) { }

    public override async void EnterState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

      var root = FlowController.VelContainer.Q<VisualElement>("Screen");
      var rewardHistoryContainer = root?.Q<VisualElement>("RewardHistory");

      if (rewardHistoryContainer == null)
      {
        Debug.LogError("[HistoryInit] RewardHistory container not found.");
        return;
      }

      rewardHistoryContainer.Clear();

      var substrate = (FlowController as GameController)?.Substrate;
      var token = (FlowController as GameController)?.CancellationToken ?? default;

      if (substrate == null || !substrate.IsConnected)
      {
        Debug.LogError("[HistoryInit] Substrate not connected.");
        return;
      }

      var player = Generic.ToAccountId32(substrate.Account);

      try
      {
        var history = await substrate.ApiClient.GetStorageAsync<
          Eterra.NetApiExt.Generated.Model.bounded_collections.bounded_vec.BoundedVecT8>(
            Eterra.NetApiExt.Generated.Storage.EterraDailySlotsStorage.RollHistoryParams(player), null, token);

        if (history?.Value == null)
        {
          rewardHistoryContainer.Add(new Label("No roll history found."));
          return;
        }

        var rawRolls = BaseVecHelper
          .ToArray<Eterra.NetApiExt.Generated.Model.pallet_eterra_daily_slots.pallet.RollResult>(history.Value);
        var summary = WeeklyRollSummary.FromRaw(rawRolls);

        var grouped = summary.Rolls
          .GroupBy(r => r.Timestamp.ToLocalTime().Date)
          .OrderByDescending(g => g.Key);

        foreach (var dayGroup in grouped)
        {
          var dayLabel = new Label($"{dayGroup.Key.ToShortDateString()}: {string.Join(" | ", dayGroup.Select(r => string.Join(",", r.Result)))}");
          dayLabel.style.fontSize = 32;
          dayLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
          dayLabel.style.marginTop = 10;
          dayLabel.style.marginBottom = 10;
          rewardHistoryContainer.Add(dayLabel);
        }
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"[HistoryInit] Failed to fetch roll history: {ex}");
      }
    }

    public override void ExitState()
    {
      Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
    }
  }
}