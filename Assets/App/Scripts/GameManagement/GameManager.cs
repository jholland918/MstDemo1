using Assets.App.Scripts.Character;
using FishNet.Object;
using MasterServerToolkit.MasterServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.App.Scripts.GameManagement
{
    /// <summary>
    /// Manages the state of a multiplayer game
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        public int MatchTimeSeconds = 300;

        public Dictionary<int, RoomPlayer> RoomPlayers;
        public Dictionary<int, PlayerCharacter> PlayerCharacters;

        private RoomController _roomController;
        private LobbyDataPacket _lobbyInfo;
        private bool _isTeamGame;
        private BaseGameHandler _gameHandler;
        private RoomOptions _roomOptions;

        private void Awake()
        {
            Debug.Log("GameManager:Awake");

            _gameHandler = new BaseGameHandler(this);

            RoomPlayers = new Dictionary<int, RoomPlayer>();
            PlayerCharacters = new Dictionary<int, PlayerCharacter>();

            PlayerCharacter.OnServerCharacterSpawnedEvent += PlayerCharacter_OnServerCharacterSpawned;
            PlayerCharacter.OnCharacterDestroyedEvent += PlayerCharacter_OnCharacterDestroyed;
            PlayerCharacter.OnLocalCharacterSpawnedEvent += PlayerCharacter_OnLocalCharacterSpawnedEvent;
            PlayerCharacterVitals.OnServerCharacterDieEvent += OnCharacterDie;
            PlayerCharacterVitals.OnServerCharacterAliveEvent += OnCharacterAlive;
        }

        public static bool IsTeamGame(Dictionary<string, string> lobbyProperties)
        {
            string lobbyFactoryId = lobbyProperties[MstDictKeys.LOBBY_FACTORY_ID];
            bool isTeamGame = lobbyFactoryId == "TwoVsTwo" ? true : false;
            return isTeamGame;
        }

        private void PlayerCharacter_OnLocalCharacterSpawnedEvent(PlayerCharacter playerCharacter)
        {
            PlayerNameTracker.SetName(Mst.Client.Auth.AccountInfo.Username);
        }

        private void OnCharacterDie(PlayerCharacterVitals playerCharacterVitals) => _gameHandler.OnCharacterDie(playerCharacterVitals);

        private void OnCharacterAlive(PlayerCharacterVitals playerCharacterVitals) => _gameHandler.OnCharacterAlive(playerCharacterVitals);

        void Start()
        {
            Debug.Log("GameManager:Start");
            StartCoroutine(nameof(StopGameAfterDelay), MatchTimeSeconds);
        }

        private IEnumerator StopGameAfterDelay(int seconds)
        {
            while (seconds > 0)
            {
                seconds--;
                //Debug.Log($"GameManager:Countdown:{time}");
                yield return new WaitForSeconds(1);
            }
            Debug.Log("GameManager:Countdown Complete!");
            StopGame();
        }

        public void OnGameOver(GameResults gameResults)
        {
            Debug.Log("GameManager:OnGameOver");
            Rpc_NotifyGameResults(gameResults);
            StartCoroutine(nameof(StopGameAfterDelay), 5);
        }

        [ObserversRpc]
        private void Rpc_NotifyGameResults(GameResults gameResults)
        {
            Debug.Log("GameManager:Rpc_NotifyGameResults");
            OnGameResults?.Invoke(gameResults);
        }

        public event Action<GameResults> OnGameResults;

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

                _isTeamGame = GameManager.IsTeamGame(_lobbyInfo.LobbyProperties);
                if (_isTeamGame)
                {
                    foreach (var lobbyMember in _lobbyInfo.Members.Values)
                    {
                        PlayerNameTracker.SetTeam(lobbyMember.Username, lobbyMember.Team);
                    }
                }

                string lobbyFactoryId = _lobbyInfo.LobbyProperties[MstDictKeys.LOBBY_FACTORY_ID];

                Debug.Log($"lobbyFactoryId:{lobbyFactoryId}");

                switch (lobbyFactoryId)
                {
                    case "Survival":
                        Debug.Log("Using Survival");
                        _gameHandler = new SurvivalHandler(this);
                        break;
                    case "OneVsOne":
                        Debug.Log("Using OneVsOne");
                        _gameHandler = new OneVsOneHandler(this);
                        break;
                    case "TwoVsTwo":
                        Debug.Log("Using TwoVsTwo");
                        _gameHandler = new TwoVsTwoHandler(this);
                        break;
                    default:
                        Debug.Log("Using NOTHING!!1");
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
