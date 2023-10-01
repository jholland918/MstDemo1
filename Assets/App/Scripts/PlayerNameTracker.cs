using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;

// https://www.youtube.com/watch?v=aPJVhLVEexY
// https://github.com/FirstGearGames/FishNet-Examples/blob/main/Assets/Walkthrough/Scripts/PlayerNameTracker.cs
namespace Assets.App.Scripts
{
    /// <summary>
    /// Lets players set their name and synchronizes it to others.
    /// </summary>
    public class PlayerNameTracker : NetworkBehaviour
    {
        /// <summary>
        /// Called when any player changes their name.
        /// </summary>
        public static event Action<NetworkConnection, string> OnNameChange;

        /// <summary>
        /// Collection of each player name for connections.
        /// </summary>
        [SyncObject]
        private readonly SyncDictionary<NetworkConnection, string> _playerNames = new SyncDictionary<NetworkConnection, string>();

        /// <summary>
        /// Singleton instance of this object.
        /// </summary>
        private static PlayerNameTracker _instance;

        private void Awake()
        {
            _instance = this;
            _playerNames.OnChange += _playerNames_OnChange;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            NetworkManager.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
        }

        /// <summary>
        /// Called when a remote client connection state changes.
        /// </summary>
        private void ServerManager_OnRemoteConnectionState(NetworkConnection arg1, RemoteConnectionStateArgs arg2)
        {
            if (arg2.ConnectionState != RemoteConnectionState.Started)
                _playerNames.Remove(arg1);
        }

        /// <summary>
        /// Optional callback when the playerNames collection changes.
        /// </summary>
        private void _playerNames_OnChange(SyncDictionaryOperation op, NetworkConnection key, string value, bool asServer)
        {
            if (op == SyncDictionaryOperation.Add || op == SyncDictionaryOperation.Set)
                OnNameChange?.Invoke(key, value);
        }

        /// <summary> 
        /// Gets a player name. Works on server or client.
        /// </summary>
        public static string GetPlayerName(NetworkConnection conn)
        {
            if (_instance._playerNames.TryGetValue(conn, out string result))
                return result;
            else
                return string.Empty;
        }

        /// <summary>
        /// Lets clients set their name.
        /// </summary>

        [Client]
        public static void SetName(string name)
        {
            _instance.ServerSetName(name);
        }

        /// <summary>
        /// Sets name on server.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sender"></param>
        [ServerRpc(RequireOwnership = false)]
        private void ServerSetName(string name, NetworkConnection sender = null)
        {
            _playerNames[sender] = name;
        }
    }
}
