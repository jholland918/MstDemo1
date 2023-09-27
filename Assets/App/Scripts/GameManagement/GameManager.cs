using MasterServerToolkit.Bridges.FishNetworking.Character;
using MasterServerToolkit.MasterServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerCharacter = Assets.App.Scripts.Character.PlayerCharacter;

namespace Assets.App.Scripts.GameManagement
{
    /// <summary>
    /// Manages the state of a multiplayer game
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public int MatchTimeSeconds = 300;

        private RoomController _roomController;
        private LobbyDataPacket _lobbyInfo;
        private BaseGameHandler _gameHandler;
        private RoomOptions _roomOptions;

        protected Dictionary<int, RoomPlayer> RoomPlayers = new();
        protected Dictionary<int, PlayerCharacter> PlayerCharacters = new();

        private void Awake()
        {
            Debug.Log("GameManager:Awake");

            PlayerCharacter.OnServerCharacterSpawnedEvent += PlayerCharacter_OnServerCharacterSpawned;
            PlayerCharacter.OnCharacterDestroyedEvent += PlayerCharacter_OnCharacterDestroyed;
        }

        void Start()
        {
            Debug.Log("GameManager:Start");
            StartCoroutine(nameof(Countdown), MatchTimeSeconds);
        }

        private IEnumerator Countdown(int time)
        {
            while (time > 0)
            {
                time--;
                //Debug.Log($"GameManager:Countdown:{time}");
                yield return new WaitForSeconds(1);
            }
            Debug.Log("GameManager:Countdown Complete!");
            StopGame();
        }

        /// <summary>
        /// Stops the match that was started by a client's call to <see cref="JoinedLobby.StartGame(SuccessCallback)"/>
        /// </summary>
        /// <remarks>
        /// The goal with this lobby system is to return players back to the <see cref="Assets.App.Scripts.UI.LobbyView"/>
        /// once a game/match ends. That way players can play a new game/match when ready.
        /// </remarks>
        private void StopGame()
        {
            Debug.Log("GameManager:StopGame");

            // Destroying the room will clean up and reset various states in the game, finally setting the 
            // LobbyState to LobbyState.Preparations, which will trigger an event handler to load the "Client" scene,
            // which will render the LobbyView once again.
            _roomController.Destroy();

            // Closing the connection will terminate/close the Room.exe process if the RoomServerManager "Terminate Room When Disconnected"
            // setting is true. This allows lobby players to keep starting new matches and returning back to the LobbyView as much as they want.
            // If this is not called, the original Room.exe process will not be terminated and new Room.exe processes will be spawned.
            _roomController.Connection.Close();
        }

        public void RoomServerManager_OnBeforeRoomRegister(RoomOptions roomOptions)
        {
            _roomOptions = roomOptions;
        }

        public void RoomServerManager_OnRoomRegistered(RoomController roomController)
        {
            Debug.Log("GameManager:RoomServerManager_OnRoomRegistered");
            _roomController = roomController;

            Mst.Server.Lobbies.GetLobbyInfo(Mst.Args.LobbyId, (info, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.Log($"GameManager:GetLobbyInfo error: {error}");
                    return;
                }

                _lobbyInfo = info;

                string lobbyFactoryId = _lobbyInfo.LobbyProperties[MstDictKeys.LOBBY_FACTORY_ID];
                switch (lobbyFactoryId)
                {
                    case "Survival":
                        Debug.Log("Setting Survival game handler...");
                        _gameHandler = new SurvivalGameHandler(_lobbyInfo);
                        break;
                    case "TwoVsTwo":
                        Debug.Log("Setting TwoVsTwo game handler...");
                        _gameHandler = new TwoVsTwoGameHandler(_lobbyInfo);
                        break;
                    case "OneVsOne":
                        Debug.Log("Setting OneVsOne game handler...");
                        _gameHandler = new OneVsOneGameHandler(_lobbyInfo);
                        break;
                    default:
                        Debug.Log($"Unhandled lobbyFactoryId [{lobbyFactoryId}]");
                        break;
                }
            });
        }

        public void RoomServerManager_OnPlayerJoinedRoom(RoomPlayer roomPlayer)
        {
            Debug.Log($"roomPlayer.RoomPeerId[{roomPlayer.RoomPeerId}]");

            int id = roomPlayer.RoomPeerId;
            if (RoomPlayers.ContainsKey(id))
            {
                RoomPlayers[id] = roomPlayer;
            }
            else
            {
                RoomPlayers.Add(id, roomPlayer);
            }
        }

        public void RoomServerManager_OnPlayerLeftRoom(RoomPlayer roomPlayer)
        {
            Debug.Log($"roomPlayer.RoomPeerId[{roomPlayer.RoomPeerId}]");

            if (RoomPlayers.ContainsKey(roomPlayer.RoomPeerId))
            {
                RoomPlayers.Remove(roomPlayer.RoomPeerId);
            }
        }

        private void PlayerCharacter_OnServerCharacterSpawned(PlayerCharacter character)
        {
            int id = character.NetworkObject.OwnerId;
            if (PlayerCharacters.ContainsKey(id))
            {
                PlayerCharacters[id] = character;
            }
            else
            {
                PlayerCharacters.Add(id, character);
            }

            var vitals = character.GetComponent<PlayerCharacterVitals>();
            vitals.OnAliveEvent += () => PlayerCharacterVitals_OnAliveEvent(vitals);
            vitals.OnDieEvent += () => PlayerCharacterVitals_OnDieEvent(vitals);
        }

        private void PlayerCharacterVitals_OnDieEvent(PlayerCharacterVitals playerCharacterVitals)
        {
            Debug.Log("PlayerCharacterVitals_OnDieEvent!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        private void PlayerCharacterVitals_OnAliveEvent(PlayerCharacterVitals playerCharacterVitals)
        {
        }

        private void PlayerCharacter_OnCharacterDestroyed(PlayerCharacter character)
        {
            Debug.Log($"OnCharacterDestroyed:OwnerId[{character.NetworkObject.OwnerId}]");
            if (PlayerCharacters.ContainsKey(character.NetworkObject.OwnerId))
            {
                PlayerCharacters.Remove(character.NetworkObject.OwnerId);
            }
        }

        public PlayerCharacter FindPlayerCharacter(RoomPlayer roomPlayer)
        {
            PlayerCharacters.TryGetValue(roomPlayer.RoomPeerId, out PlayerCharacter playerCharacter);
            return playerCharacter;
        }

        public RoomPlayer FindRoomPlayer(PlayerCharacter playerCharacter)
        {
            RoomPlayers.TryGetValue(playerCharacter.NetworkObject.OwnerId, out RoomPlayer roomPlayer);
            return roomPlayer;
        }
    }
}
