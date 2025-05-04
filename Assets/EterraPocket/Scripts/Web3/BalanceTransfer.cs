using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using Substrate.NET.Schnorrkel.Keys;
using Substrate.NetApi;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Eterra.NetApiExt.Generated.Storage;  // Import EterraCalls
using Eterra.NetApiExt.Generated.Model.sp_core.crypto;
using Eterra.NetApiExt.Generated.Model.sp_runtime.multiaddress;
using Eterra.NetApiExt.Generated.Model.pallet_eterra.pallet;  // Import EnumCall and Call
using Eterra.NetApiExt.Generated.Model.solochain_template_runtime; // Import EnumRuntimeCallusing Eterra.NetApiExt.Generated.Storage;
using Eterra.NetApiExt.Generated;
using Eterra.NetApiExt.Helper;
using Eterra.Integration.Model;
using System.Numerics;
using Eterra.NetApiExt.Client;

namespace Assets.Scripts
{
  public class BalanceTransfer : Singleton<BalanceTransfer>
  {
    private static string NodeUrl = "ws://127.0.0.1:9944";
    private static SubstrateNetwork client;

    public SubstrateNetwork Client => client;

    #region Account Initialization

    public static MiniSecret MiniSecretAlice => new MiniSecret(
        Utils.HexToByteArray("0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a"),
        ExpandMode.Ed25519);

    public static Account Alice => Account.Build(KeyType.Sr25519, MiniSecretAlice.ExpandToSecret().ToBytes(),
        MiniSecretAlice.GetPair().Public.Key);

    public static MiniSecret MiniSecretBob => new MiniSecret(
        Utils.HexToByteArray("0x398f0c28f98885e046333d4a41c19cee4c37368a9832c6502f6cfd182e2aef89"),
        ExpandMode.Ed25519);

    public static Account Bob => Account.Build(KeyType.Sr25519, MiniSecretBob.ExpandToSecret().ToBytes(),
        MiniSecretBob.GetPair().Public.Key);

    public AccountId32 aliceAccount = Eterra.NetApiExt.Client.BaseClient.Alice.ToAccountId32();
    public AccountId32 bertaAccount = "5DAAnrj7VHTznn2AWBemMuyBwZWs6FNFjdyVXUeYum3PTXFy".ToAccountId32();

    #endregion

    #region Awake and Initialization

    protected override void Awake()
    {
      base.Awake();

      Debug.Log("[BalanceTransfer] Awake called. Initializing client...");
      try
      {
        InitializeClient(NodeUrl);
      }
      catch (Exception ex)
      {
        Debug.LogError($"[BalanceTransfer] Failed to initialize client: {ex.Message}");
      }
    }

    #endregion // Missing directive added here

    #region Client Initialization and Connection

    private static SubstrateNetwork InitializeClient(string nodeUrl)
    {
      if (string.IsNullOrEmpty(nodeUrl))
      {
        Debug.LogError("[BalanceTransfer] Node URL is null or empty.");
        throw new ArgumentException("Node URL cannot be null or empty.");
      }

      Debug.Log("[BalanceTransfer] Initializing client...");
      client = new SubstrateNetwork(Eterra.NetApiExt.Client.BaseClient.Alice, nodeUrl);

      client.ExtrinsicManager.ExtrinsicUpdated += (id, info) =>
      {
        Debug.Log($"[BalanceTransfer] ExtrinsicUpdated: ID={id}, Event={info.TransactionEvent}, Other={info}");
      };

      ConnectClientAsync(client, nodeUrl).ConfigureAwait(false);
      return client;
    }

    private static async Task ConnectClientAsync(SubstrateNetwork client, string nodeUrl)
    {
      if (client == null)
      {
        Debug.LogError("[BalanceTransfer] Client is null during connection.");
        throw new ArgumentNullException(nameof(client), "Client cannot be null during connection.");
      }

      Debug.Log("[BalanceTransfer] Attempting to connect to node...");
      try
      {
        await client.ConnectAsync(true, true, CancellationToken.None);
        Debug.Log($"[BalanceTransfer] Connected to {nodeUrl}: {client.IsConnected}");
      }
      catch (Exception ex)
      {
        Debug.LogError($"[BalanceTransfer] Failed to connect to node: {ex.Message}");
        throw;
      }
    }

    private static async Task DisconnectClientAsync(SubstrateNetwork client)
    {
      if (client == null)
      {
        Debug.LogWarning("[BalanceTransfer] Client is null during disconnection.");
        return;
      }

      Debug.Log("[BalanceTransfer] Attempting to disconnect client...");
      try
      {
        await client.DisconnectAsync();
        Debug.Log("[BalanceTransfer] Disconnected successfully.");
      }
      catch (Exception ex)
      {
        Debug.LogError($"[BalanceTransfer] Failed to disconnect client: {ex.Message}");
      }
    }

    #endregion

    #region Submit Transfer

    public async void ButtonClickTransfer()
    {
      Debug.Log("[BalanceTransfer] ButtonClickTransfer called.");
      if (client == null)
      {
        Debug.LogError("[BalanceTransfer] Client is not initialized.");
        return;
      }

      await SubmitTransfer(client);
    }

    #endregion

    #region Client Validation and Connection

    private static bool ValidateClient(SubstrateNetwork client)
    {
      if (client == null)
      {
        Debug.LogError("[BalanceTransfer] Client is null!");
        return false;
      }

      if (!client.IsConnected)
      {
        Debug.LogError("[BalanceTransfer] Client is not connected!");
        return false;
      }

      return true;
    }

    public static AccountId32 GetRecipientAccount()
    {
      Debug.Log("[BalanceTransfer] Getting recipient account...");
      return BalanceTransfer.Bob.ToAccountId32();
    }

    public async Task SubmitTransfer(SubstrateNetwork client)
    {
      if (!ValidateClient(client))
      {
        Debug.LogError("[BalanceTransfer] Client validation failed.");
        return;
      }

      var bertaAccount = "5DAAnrj7VHTznn2AWBemMuyBwZWs6FNFjdyVXUeYum3PTXFy".ToAccountId32();

      Debug.Log("[BalanceTransfer] Retrieving Berta's account info...");
      var bertaAccountInfo = await client.GetAccountAsync(bertaAccount, CancellationToken.None);

      if (bertaAccountInfo == null)
      {
        Debug.LogWarning("[BalanceTransfer] Berta's account does not exist!");
      }
      else
      {
        Debug.Log($"[BalanceTransfer] Berta's Account Info: Free={bertaAccountInfo.Data.Free}, Frozen={bertaAccountInfo.Data.Frozen}, Reserved={bertaAccountInfo.Data.Reserved}");
      }

      Debug.Log("[BalanceTransfer] Submitting TransferKeepAlive...");
      try
      {
        var subscriptionId = await client.TransferKeepAliveAsync(GetRecipientAccount(), 10000000000, 1, CancellationToken.None);

        if (subscriptionId == null)
        {
          Debug.LogWarning("[BalanceTransfer] TransferKeepAlive subscription failed.");
          return;
        }

        Debug.Log($"[BalanceTransfer] TransferKeepAlive subscriptionId: {subscriptionId}");
      }
      catch (Exception ex)
      {
        Debug.LogError($"[BalanceTransfer] Error during TransferKeepAlive: {ex.Message}");
      }
    }


    public async Task<string?> CreateGameAsync(Account account, Account[] players, int concurrentTasks, CancellationToken token)
    {
      var extrinsicType = "EterraCalls.CreateGame"; // Ensure this matches the correct format

      if (!client.IsConnected || account == null)
      {
        Debug.LogError("[CreateGameAsync] Client is not connected or Account is null.");
        return null;
      }

      // Convert players to AccountId32 array and log each player
      var playerAccountIds = players.Select(p => p.ToAccountId32()).ToArray();
      Debug.Log($"[CreateGameAsync] Caller AccountId32: {BitConverter.ToString(account.ToAccountId32().Encode()).Replace("-", "").ToLower()}");

      foreach (var player in playerAccountIds)
      {
        Debug.Log($"[CreateGameAsync] Player AccountId32: {BitConverter.ToString(player.Encode()).Replace("-", "").ToLower()}");
      }

      // ✅ FIXED: Pass the array directly into CreateGameAsync
      return await CreateGameAsync(playerAccountIds, concurrentTasks, token);
    }

    public async Task<string> CreateGame(SubstrateNetwork client)
    {
      if (!ValidateClient(client))
        return "Client validation failed";

      var aliceAccount = Alice.ToAccountId32();
      var bobAccount = Bob.ToAccountId32();

      if (aliceAccount == null || bobAccount == null)
        return "Invalid account";

      Debug.Log($"[CreateGame] Alice AccountId32: {BitConverter.ToString(aliceAccount.Encode()).Replace("-", "").ToLower()}");
      Debug.Log($"[CreateGame] Bob AccountId32: {BitConverter.ToString(bobAccount.Encode()).Replace("-", "").ToLower()}");

      // ✅ FIXED: Use Account[] instead of List<Account>
      var players = new Account[] { Alice, Bob };

      var accountInfo = await client.GetAccountAsync(aliceAccount, CancellationToken.None);
      if (accountInfo == null)
        return "Account info retrieval failed";

      var subscriptionId = await CreateGameAsync(Alice, players, 1, CancellationToken.None);

      return string.IsNullOrEmpty(subscriptionId) ? "Submission failed" : subscriptionId;
    }

    public async Task<string> CreateGameAsync(AccountId32[] players, int concurrentTasks, CancellationToken token)
    {
      var extrinsicType = "EterraCalls.CreateGame";

      if (!client.IsConnected || client.Account == null)
      {
        return null;
      }

      var extrinsic = EterraCalls.CreateGame(new BaseVec<AccountId32>(players.ToArray()));

      return await client.GenericExtrinsicAsync(client.Account, extrinsicType, extrinsic, concurrentTasks, token);
    }

    #endregion
  }
}