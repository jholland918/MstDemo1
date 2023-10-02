using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using MasterServerToolkit.MasterServer;
using System;
using System.Collections.Generic;
using UnityEngine;

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
        /// Collection of user team assignments
        /// </summary>
        private readonly Dictionary<string, string> _userTeams = new Dictionary<string, string>();

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
            string team = string.Empty;
            if (_userTeams.ContainsKey(name))
            {
                team = $"[{_userTeams[name]}] ";
            }

            _playerNames[sender] = $"{team}{name}";
        }

        public static void SetTeam(string username, string team)
        {
            if (_instance._userTeams.ContainsKey(username))
            {
                _instance._userTeams[username] = team;
            }
            else
            {
                _instance._userTeams.Add(username, team);
            }           
        }
    }
}
