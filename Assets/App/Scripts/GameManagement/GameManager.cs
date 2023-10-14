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

        public PlayerRegistrationCollection PlayerRegistrations = new();

        private RoomController _roomController;
        public LobbyDataPacket LobbyInfo { get; private set; }
        private bool _isTeamGame;
        private RoomServerManager _roomServerManager;
        private BaseGameHandler _gameHandler;
        private RoomOptions _roomOptions;

        private void Awake()
        {
            Debug.Log("GameManager:Awake");

            OnFoundRoomServerManager(FindFirstObjectByType<RoomServerManager>());

            _gameHandler = new BaseGameHandler(this);

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

        private void OnFoundRoomServerManager(RoomServerManager roomServerManager)
        {
            Debug.Log("GameManager:OnFoundRoomServerManager");
            _roomServerManager = roomServerManager;
            _roomServerManager.OnPlayerJoinedRoomEvent.AddListener(RoomServerManager_OnPlayerJoinedRoom);
            _roomServerManager.OnPlayerLeftRoomEvent.AddListener(RoomServerManager_OnPlayerLeftRoom);
            _roomOptions = _roomServerManager.RoomOptions;
            _roomController = _roomServerManager.RoomController;

            PlayerRegistrations.Add(_roomServerManager.Players);

            Mst.Server.Lobbies.GetLobbyInfo(Mst.Args.LobbyId, (info, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.Log($"GameManager:GetLobbyInfo error: {error}");
                    return;
                }

                LobbyInfo = info;

                _isTeamGame = GameManager.IsTeamGame(LobbyInfo.LobbyProperties);

                foreach (LobbyMemberData lobbyMember in LobbyInfo.Members.Values)
                {
                    PlayerRegistrations.Add(lobbyMember);
                    if (_isTeamGame)
                        PlayerNameTracker.SetTeam(lobbyMember.Username, lobbyMember.Team);
                }

                string lobbyFactoryId = LobbyInfo.LobbyProperties[MstDictKeys.LOBBY_FACTORY_ID];

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
            PlayerRegistrations.Add(roomPlayer);
        }

        public void RoomServerManager_OnPlayerLeftRoom(RoomPlayer roomPlayer)
        {
            PlayerRegistrations.Remove(roomPlayer);
        }

        private void PlayerCharacter_OnServerCharacterSpawned(PlayerCharacter character)
        {
            PlayerRegistrations.Add(character);
        }

        private void PlayerCharacter_OnCharacterDestroyed(PlayerCharacter character)
        {
            PlayerRegistrations.Remove(character);
        }
    }
}
