﻿using Serilog;
using Eterra.Integration.Model;
using Eterra.NetApiExt.Generated.Model.solochain_template_runtime;
using Eterra.NetApiExt.Generated.Model.sp_core.crypto;
using Eterra.NetApiExt.Generated.Model.sp_runtime.multiaddress;
using Eterra.NetApiExt.Generated.Storage;
using Eterra.NetApiExt.Helper;
using Substrate.NetApi;
using Substrate.NetApi.Model.Types;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Numerics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
  /// <summary>
  /// Substrate network
  /// </summary>
  public partial class SubstrateNetwork : Eterra.NetApiExt.Client.BaseClient
  {
    /// <summary>
    /// Decimals
    /// </summary>
    public const long DECIMALS = 1000000000000;

    /// <summary>
    /// Account
    /// </summary>
    public Account Account { get; set; }

    /// <summary>
    /// Substrate network constructor
    /// </summary>
    /// <param name="account"></param>
    /// <param name="networkType"></param>
    /// <param name="url"></param>
    public SubstrateNetwork(Account account, string url) : base(url)
    {
      Account = account;
    }

    /// <summary>
    ///   Expose the underlying SubstrateClientExt so we can hand it to generated storage APIs.
    /// </summary>
    public Eterra.NetApiExt.Generated.SubstrateClientExt ApiClient => this.SubstrateClient;

    #region storage

    /// <summary>
    /// Get owner account informations.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<AccountInfoSharp> GetAccountAsync(CancellationToken token)
        => await GetAccountAsync(Account, token);

    /// <summary>
    /// Get account informations.
    /// </summary>
    /// <param name="account32"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<AccountInfoSharp> GetAccountAsync(Account key, CancellationToken token)
        => await GetAccountAsync(Account.ToAccountId32(), token);

    /// <summary>
    /// Get account informations.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<AccountInfoSharp> GetAccountAsync(AccountId32 key, CancellationToken token)
    {
      if (!IsConnected)
      {
        Log.Warning("Currently not connected to the network!");
        return null;
      }

      if (key == null || key.Value == null)
      {
        Log.Warning("No account reference given as key!");
        return null;
      }

      var result = await SubstrateClient.SystemStorage.Account(key, null, token);
      if (result == null)
      {
        return null;
      }

      return new AccountInfoSharp(result);
    }

    #endregion storage

    #region storage_all_generic

    /// <summary>
    /// Get all storage as dictionary.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="module"></param>
    /// <param name="item"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<Dictionary<T1, T2>> GetAllStorageAsync<T1, T2>(string module, string item, CancellationToken token)
        where T1 : IType, new()
        where T2 : IType, new()
    {
      return await GetAllStorageAsync<T1, T2>(module, item, null, null, token);
    }

    /// <summary>
    /// Get all storage as dictionary.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="module"></param>
    /// <param name="item"></param>
    /// <param name="subKey"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<Dictionary<T1, T2>> GetAllStorageAsync<T1, T2>(string module, string item, byte[] subKey, CancellationToken token)
        where T1 : IType, new()
        where T2 : IType, new()
    {
      return await GetAllStorageAsync<T1, T2>(module, item, null, subKey, token);
    }

    /// <summary>
    /// Get all storage as dictionary.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="module"></param>
    /// <param name="item"></param>
    /// <param name="blockHash"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<Dictionary<T1, T2>> GetAllStorageAsync<T1, T2>(string module, string item, string blockHash, CancellationToken token)
        where T1 : IType, new()
        where T2 : IType, new()
    {
      return await GetAllStorageAsync<T1, T2>(module, item, blockHash, null, token);
    }

    /// <summary>
    /// Get all storage as dictionary.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="module"></param>
    /// <param name="item"></param>
    /// <param name="blockHash"></param>
    /// <param name="subKey"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<Dictionary<T1, T2>> GetAllStorageAsync<T1, T2>(string module, string item, string? blockHash, byte[]? subKey, CancellationToken token)
        where T1 : IType, new()
        where T2 : IType, new()
    {
      byte[]? nextStorageKey = null;
      List<(byte[], T1, T2)> storageEntries;

      var result = new Dictionary<T1, T2>();

      uint batchSize = 1000;

      do
      {
        storageEntries = await GetStoragePagedAsync<T1, T2>(module, item, nextStorageKey, batchSize, blockHash, subKey, token);

        foreach (var storageEntry in storageEntries)
        {
          result.Add(storageEntry.Item2, storageEntry.Item3);

          nextStorageKey = storageEntry.Item1;
        }

        Log.Debug("{0}.{1} storage +{2} records parsed.", module, item, batchSize);
      } while (storageEntries.Any());

      Log.Debug("{0}.{1} storage {2} records parsed.", module, item, result.Count);

      return result;
    }

    /// <summary>
    /// Get all storage paged.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="module"></param>
    /// <param name="item"></param>
    /// <param name="startKey"></param>
    /// <param name="page"></param>
    /// <param name="blockHash"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<List<(byte[], T1, T2)>> GetStoragePagedAsync<T1, T2>(string module, string item, byte[] startKey, uint page, string blockHash, CancellationToken token)
        where T1 : IType, new()
        where T2 : IType, new()
    {
      return await GetStoragePagedAsync<T1, T2>(module, item, startKey, page, blockHash, null, token);
    }

    /// <summary>
    /// Get all storage paged.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="module"></param>
    /// <param name="item"></param>
    /// <param name="startKey"></param>
    /// <param name="page"></param>
    /// <param name="blockHash"></param>
    /// <param name="subKey"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public async Task<List<(byte[], T1, T2)>> GetStoragePagedAsync<T1, T2>(string module, string item, byte[]? startKey, uint page, string? blockHash, byte[]? subKey, CancellationToken token)
        where T1 : IType, new()
        where T2 : IType, new()
    {
      if (!IsConnected)
      {
        return null;
      }

      if (page < 2 || page > 1000)
      {
        throw new NotSupportedException("Page size must be in the range of 2 - 1000");
      }

      var result = new List<(byte[], T1, T2)>();
      var keyBytes = RequestGenerator.GetStorageKeyBytesHash(module, item);

      var extKeyBytes = (byte[])keyBytes.Clone();  // Clone the original array to keep it intact
      if (subKey != null)
      {
        int oldLength = extKeyBytes.Length;
        Array.Resize(ref extKeyBytes, oldLength + subKey.Length);
        Array.Copy(subKey, 0, extKeyBytes, oldLength, subKey.Length);
      }

      var storageKeys = await SubstrateClient.State.GetKeysPagedAsync(extKeyBytes, page, startKey, token);
      if (storageKeys == null || !storageKeys.Any())
      {
        return result;
      }

      var keySize = Utils.Bytes2HexString(keyBytes).Length;

      var storageChangeSets = await SubstrateClient.State.GetQueryStorageAtAsync(storageKeys.Select(p => Utils.HexToByteArray(p.ToString())).ToList(), blockHash, token);
      if (storageChangeSets != null)
      {
        foreach (var storageChangeSet in storageChangeSets.First().Changes)
        {
          var storageKeyString = storageChangeSet[0];

          var keyParam = new T1();
          keyParam.Create(storageKeyString[keySize..]);

          T2 valueParam = default;
          if (storageChangeSet[1] != null)
          {
            valueParam = new T2();
            valueParam.Create(storageChangeSet[1]);
          }

          result.Add((Utils.HexToByteArray(storageKeyString), keyParam, valueParam));
        }
      }

      return result;
    }

    /// <summary>
    /// Get all storage keys as list.
    /// </summary>
    /// <param name="module"></param>
    /// <param name="item"></param>
    /// <param name="blockHash"></param>
    /// <param name="subKey"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<List<byte[]>> GetAllStorageKeysAsync(string module, string item, string blockHash, byte[] subKey, CancellationToken token)
    {
      var result = new List<byte[]>();

      byte[] nextStorageKey = null;
      List<byte[]> storageEntries;
      do
      {
        storageEntries = await GetStorageKeysPagedAsync(module, item, nextStorageKey, 1000, blockHash, subKey, token);

        if (storageEntries != null && storageEntries.Any())
        {
          result.AddRange(storageEntries);
          //nextStorageKey = storageEntries[^1];
          nextStorageKey = storageEntries[storageEntries.Count - 1];
        }
      }
      while (storageEntries != null && storageEntries.Any());

      Log.Debug("{0}.{1} storage key {2} records parsed.", module, item, result.Count);

      return result;
    }

    /// <summary>
    /// Get all storage keys paged.
    /// </summary>
    /// <param name="module"></param>
    /// <param name="item"></param>
    /// <param name="startKey"></param>
    /// <param name="page"></param>
    /// <param name="blockHash"></param>
    /// <param name="subKey"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public async Task<List<byte[]>> GetStorageKeysPagedAsync(string module, string item, byte[]? startKey, uint page, string? blockHash, byte[] subKey, CancellationToken token)
    {
      if (!IsConnected)
      {
        return null;
      }

      if (page < 2 || page > 1000)
      {
        throw new NotSupportedException("Page size must be in the range of 2 - 1000");
      }

      var result = new List<byte[]>();
      var keyBytes = RequestGenerator.GetStorageKeyBytesHash(module, item);

      var extKeyBytes = (byte[])keyBytes.Clone();  // Clone the original array to keep it intact
      if (subKey != null)
      {
        int oldLength = extKeyBytes.Length;
        Array.Resize(ref extKeyBytes, oldLength + subKey.Length);
        Array.Copy(subKey, 0, extKeyBytes, oldLength, subKey.Length);
      }

      var storageKeys = await SubstrateClient.State.GetKeysPagedAsync(extKeyBytes, page, startKey, token);
      if (storageKeys == null || !storageKeys.Any())
      {
        return result;
      }

      return storageKeys.Select(p => Utils.HexToByteArray(p.ToString())).ToList();
    }

    /// <summary>
    /// Get all storage by keys.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="module"></param>
    /// <param name="item"></param>
    /// <param name="keys"></param>
    /// <param name="blockHash"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public async Task<List<(string, T1)>> GetStorageByKeysAsync<T1>(string module, string item, List<string> keys, string? blockHash, CancellationToken token)
        where T1 : IType, new()
    {
      if (!IsConnected)
      {
        return null;
      }

      if (keys.Count > 1000)
      {
        throw new NotSupportedException("Max keys to query is 1000");
      }

      var keyBytesHex = Utils.Bytes2HexString(RequestGenerator.GetStorageKeyBytesHash(module, item));
      var keySize = keyBytesHex.Length;
      var result = new List<(string, T1)>();

      var storageChangeSets = await SubstrateClient.State.GetQueryStorageAtAsync(keys.Select(p => Utils.HexToByteArray(keyBytesHex + p.ToString())).ToList(), blockHash, token);
      if (storageChangeSets != null)
      {
        foreach (var storageChangeSet in storageChangeSets.First().Changes)
        {
          var storageKeyString = storageChangeSet[0];
          var valueParam = new T1();
          if (storageChangeSet[1] != null)
          {
            valueParam.Create(storageChangeSet[1]);
          }
          //result.Add((storageKeyString[keySize..], valueParam));
          result.Add((storageKeyString.Substring(keySize), valueParam));
        }
      }

      return result;
    }

    #endregion storage_all_generic

    #region extrinsics


    /// <summary>
    /// Create game
    /// </summary>
    /// <param name="players"></param>
    /// <param name="concurrentTasks"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<string> CreateGameAsync(AccountId32[] players, int concurrentTasks, CancellationToken token)
    {
      var extrinsicType = "EterraCalls.CreateGame";

      if (!IsConnected || Account == null)
      {
        return null;
      }

      var extrinsic = EterraCalls.CreateGame(new BaseVec<AccountId32>(players.ToArray()));

      return await GenericExtrinsicAsync(Account, extrinsicType, extrinsic, concurrentTasks, token);
    }

    /// <summary>
    /// Create game
    /// </summary>
    /// <param name="players"></param>
    /// <param name="concurrentTasks"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<string> SpinSlotAsync(AccountId32 player, int concurrentTasks, CancellationToken token)
    {
      var extrinsicType = "EterraDailySlotsCalls.Roll";

      if (!IsConnected)
      {
        Debug.LogError("[SpinSlotAsync] Network not connected.");
        return null;
      }

      if (Account == null)
      {
        Debug.LogError("[SpinSlotAsync] Account is null.");
        return null;
      }

      var extrinsic = EterraDailySlotsCalls.Roll();
      var subId = await GenericExtrinsicAsync(Account, extrinsicType, extrinsic, concurrentTasks, token);

      Debug.Log($"[SpinSlotAsync] Submitted extrinsic, subId: {subId}");
      return subId;
    }

    public async Task<string> SetReelWeightsAsync(Account account, BaseVec<BaseTuple<U32, U32>> weights, int concurrentTasks, CancellationToken token)
    {
      var extrinsicType = "EterraDailySlotsCalls.SetReelWeights";

      if (!IsConnected || account == null)
      {
        return null!;
      }

      var reelIndex = new U32(); reelIndex.Create(0); // Assuming index 0
      var extrinsic = EterraDailySlotsCalls.SetReelWeights(reelIndex, weights);

      return await GenericExtrinsicAsync(account, extrinsicType, extrinsic, concurrentTasks, token);
    }

    /// <summary>
    /// Remark
    /// </summary>
    /// <param name="remark"></param>
    /// <param name="concurrentTasks"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<string> RemarksAsync(BaseVec<U8> remark, int concurrentTasks, CancellationToken token)
    {
      var extrinsicType = "SystemCalls.Remark";

      if (!IsConnected || Account == null)
      {
        return null;
      }

      var extrinsic = SystemCalls.Remark(remark);

      return await GenericExtrinsicAsync(Account, extrinsicType, extrinsic, concurrentTasks, token);
    }

    /// <summary>
    /// Transfer keep alive
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="value"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<string> TransferKeepAliveAsync(AccountId32 dest, BigInteger value, int concurrentTasks, CancellationToken token)
    {
      return await TransferKeepAliveAsync(Account, dest, value, concurrentTasks, token);
    }

    /// <summary>
    /// Transfer keep alive
    /// </summary>
    /// <param name="account"></param>
    /// <param name="dest"></param>
    /// <param name="value"></param>
    /// <param name="concurrentTasks"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<string> TransferKeepAliveAsync(Account account, AccountId32 dest, BigInteger value, int concurrentTasks, CancellationToken token)
    {
      var extrinsicType = "BalancesCalls.TransferKeepAlive";

      if (!IsConnected || account == null)
      {
        return null;
      }

      var multiAddress = new EnumMultiAddress();
      multiAddress.Create(MultiAddress.Id, dest);

      var balance = new BaseCom<U128>();
      balance.Create(value);

      var extrinsic = BalancesCalls.TransferKeepAlive(multiAddress, balance);

      return await GenericExtrinsicAsync(account, extrinsicType, extrinsic, concurrentTasks, token);
    }

    /// <summary>
    /// Submit a sudo extrinsic.
    /// </summary>
    /// <param name="call"></param>
    /// <param name="concurrentTasks"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<string> SudoAsync(Account sudoAccount, EnumRuntimeCall call, int concurrentTasks, CancellationToken token)
    {
      var extrinsicType = "Sudo.Sudo";

      if (!IsConnected || sudoAccount == null || call == null)
      {
        return null;
      }

      var extrinsic = SudoCalls.Sudo(call);

      return await GenericExtrinsicAsync(sudoAccount, extrinsicType, extrinsic, concurrentTasks, token);
    }

    public async Task<Eterra.NetApiExt.Generated.Model.pallet_eterra.types.game.Game> GetGameByIdAsync(
    string gameIdHex,
    string blockHash,
    CancellationToken token)
    {
      // In our storage key construction, we assume that the key is built by concatenating a fixed prefix
      // (the result of Twox128("Eterra") concatenated with Twox128("GameStorage"))
      // with the 64‑hex‑character game ID (i.e. the H256 representing the GameID).
      string prefix = "0x18B52016F052F43043D279E1CF846B35";
      string storageKey = prefix + gameIdHex;

      // Use the underlying SubstrateClient to fetch storage.
      return await SubstrateClient.GetStorageAsync<Eterra.NetApiExt.Generated.Model.pallet_eterra.types.game.Game>(
          storageKey, blockHash, token);
    }

    #endregion extrinsics

    // Block monitoring implementation
    private uint _lastMonitoredBlock = 0;
    public event Action<uint> NewBlockDetected;

    public async Task StartMonitoringBlocksAsync(CancellationToken token)
    {
      while (!token.IsCancellationRequested)
      {
        try
        {
          var block = await SubstrateClient.SystemStorage.Number(null, token);
          var currentBlock = block?.Value ?? 0;

          if (currentBlock != _lastMonitoredBlock)
          {
            _lastMonitoredBlock = currentBlock;
            NewBlockDetected?.Invoke(currentBlock);
          }
        }
        catch (Exception ex)
        {
          Log.Warning("Block watcher encountered an error: {0}", ex.Message);
        }

        await Task.Delay(6000, token); // Poll every ~6s
      }
    }

    /// <summary>
    /// Subscribe to new block events with a provided callback.
    /// </summary>
    /// <param name="callback">Action to call when a new block is detected</param>
    public event Action<uint> OnNewBlock
    {
      add
      {
        Debug.Log($"[OnNewBlock] Subscribed: {value.Method.Name}");
        NewBlockDetected += value;
      }
      remove
      {
        Debug.Log($"[OnNewBlock] Unsubscribed: {value.Method.Name}");
        NewBlockDetected -= value;
      }
    }
    /// <summary>
    /// Initializes block monitoring. Call this at application startup to begin block monitoring.
    /// </summary>
    /// <param name="token">Cancellation token to stop monitoring</param>
    public void InitializeMonitoring(CancellationToken token)
    {
      _ = StartMonitoringBlocksAsync(token);
    }
  }
}
