using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using Eterra.NetApiExt.Client;
using Substrate.NetApi;

namespace Assets.Scripts
{
  public class SubstrateConnectionWatcher : MonoBehaviour
  {
    public static SubstrateConnectionWatcher Instance { get; private set; }

    public bool IsConnected { get; private set; }
    public SubstrateNetwork Client { get; private set; }

    public event Action OnConnected;

    private async void Awake()
    {
      if (Instance != null)
      {
        Destroy(gameObject);
        return;
      }
      Instance = this;
      DontDestroyOnLoad(gameObject);

      await ConnectToNode();
    }

    private async Task ConnectToNode()
    {
      var nodeUrl = "ws://127.0.0.1:9944";
      Client = new SubstrateNetwork(Eterra.NetApiExt.Client.BaseClient.Alice, nodeUrl);

      try
      {
        await Client.ConnectAsync(true, true, CancellationToken.None);
        IsConnected = Client.IsConnected;

        Debug.Log("[SubstrateConnectionWatcher] Connected: " + IsConnected);
        OnConnected?.Invoke(); // 🔔 Notify listeners
      }
      catch (Exception ex)
      {
        Debug.LogError("[SubstrateConnectionWatcher] Connection failed: " + ex.Message);
      }
    }
  }
}