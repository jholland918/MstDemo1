using Assets.App.Scripts.Character;
using MasterServerToolkit.MasterServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.App.Scripts.GameManagement
{
    /// <summary>
    /// Manages the state of a multiplayer game
    /// </summary>
    [RequireComponent(typeof(SurvivalGameHandler))]
    [RequireComponent(typeof(TwoVsTwoGameHandler))]
    [RequireComponent(typeof(OneVsOneGameHandler))]
    public class GameManager : MonoBehaviour
    {
        public int MatchTimeSeconds = 300;

        public Dictionary<int, RoomPlayer> RoomPlayers;
        public Dictionary<int, PlayerCharacter> PlayerCharacters;

        private Dictionary<string, BaseGameHandler> _gameHandlers;
        private RoomController _roomController;
        private LobbyDataPacket _lobbyInfo;
        private BaseGameHandler _gameHandler;
        private RoomOptions _roomOptions;

        private void Awake()
        {
            Debug.Log("GameManager:Awake");

            RoomPlayers = new Dictionary<int, RoomPlayer>();
            PlayerCharacters = new Dictionary<int, PlayerCharacter>();

            _gameHandlers = new Dictionary<string, BaseGameHandler>
            {
                ["Survival"] = GetComponent<SurvivalGameHandler>(),
                ["OneVsOne"] = GetComponent<OneVsOneGameHandler>(),
                ["TwoVsTwo"] = GetComponent<TwoVsTwoGameHandler>()
            };

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

                // Set the active game handler and destroy the rest
                foreach (KeyValuePair<string, BaseGameHandler> kvp in _gameHandlers)
                {
                    if (kvp.Key == lobbyFactoryId)
                    {
                        _gameHandler = kvp.Value;
                    }
                    else
                    {
                        kvp.Value.enabled = false;
                        Destroy(kvp.Value);
                    }
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
