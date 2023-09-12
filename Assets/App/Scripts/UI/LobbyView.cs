using MasterServerToolkit.Bridges;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using MasterServerToolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Assets.App.Scripts.UI
{
    public class LobbyView : UIView
    {
        [Header("Components"), SerializeField]
        private UILable uiLablePrefab;
        [SerializeField]
        private UILable uiColLablePrefab;
        [SerializeField]
        private UIButton uiButtonPrefab;
        [SerializeField]
        private RectTransform listContainer;
        [SerializeField]
        private TMP_Text statusInfoText;

        [SerializeField]
        private UnityEngine.UI.Button startGameButton;

        private static JoinedLobby _lobby;

        public UnityEvent OnStartGameEvent;
        private RoomAccessPacket _currentRoomAccess;

        protected override void Awake()
        {
            base.Awake();

            // Listen to show/hide events
            Mst.Events.AddListener(AppEventKeys.showLobbyView, OnShowLobbyViewEventHandler);
            Mst.Events.AddListener(AppEventKeys.hideLobbyView, OnHideLobbyViewEventHandler);
        }

        protected override void OnDestroy()
        {
            // When starting a game we want to unsubscribe from these events to avoid errors getting generated due to null objects.
            _lobby.OnMemberJoinedEvent -= OnMemberJoined;
            _lobby.OnMemberLeftEvent -= OnMemberLeft;
            _lobby.OnMemberReadyStatusChangedEvent -= OnMemberReadyStatusChanged;

            base.OnDestroy();
        }

        protected override void Start()
        {
            base.Start();

            if (listContainer)
            {
                foreach (Transform t in listContainer)
                {
                    Destroy(t.gameObject);
                }
            }

            if (_lobby != null)
            {
                // If we're here then we're probably returning back from a game to the lobby view...
                ResetLobby();
                DrawPlayersList();
            }
        }

        private void OnShowLobbyViewEventHandler(EventMessage message)
        {
            Mst.Events.Invoke(MstEventKeys.hideCreateLobbyView);
            Mst.Events.Invoke(MstEventKeys.hideLoadingInfo);

            _lobby = message.As<JoinedLobby>();
            _lobby.OnLobbyStateChangeEvent += OnLobbyStateChange;
            _lobby.OnLobbyStatusTextChangeEvent += OnLobbyStatusTextChange;

            ResetLobby();
        }

        private void ResetLobby()
        {
            _lobby.OnMemberJoinedEvent += OnMemberJoined;
            _lobby.OnMemberLeftEvent += OnMemberLeft;
            _lobby.OnMemberReadyStatusChangedEvent += OnMemberReadyStatusChanged;

            bool isMasterUser = _lobby.IsMasterUser(Mst.Client.Auth.AccountInfo.Username);
            startGameButton.gameObject.SetActive(isMasterUser);

            Show();
        }

        private void OnLobbyStatusTextChange(string text)
        {
            statusInfoText.text = $"{statusInfoText.text};{text}";
        }

        private void OnLobbyStateChange(LobbyState state)
        {
            //statusInfoText.text = $"{statusInfoText.text}; LobbyState[{state}]";
            switch (state)
            {
                case LobbyState.FailedToStart:
                    // TODO: Show error message.
                    Debug.Log("LobbyView:OnLobbyStateChange:FailedToStart");
                    break;
                case LobbyState.Preparations:
                    // This is waiting for the game to start (either for the first time or for subsequent times.
                    Debug.Log("LobbyView:OnLobbyStateChange:Preparations");

                    Scene scene = SceneManager.GetActiveScene();

                    // If we're not currently in the Client scene, then we're probably returning from a game/match. So switch it back to the Client scene.
                    if (!scene.name.Equals("Client", StringComparison.OrdinalIgnoreCase))
                    {
                        // When the Client scene loads we have extra logic in Start() to handle re-rendering player list and showing the lobby view...
                        SceneManager.LoadScene("Client");
                    }

                    break;
                case LobbyState.StartingGameServer:
                    // TODO: Show loading text or something...
                    Debug.Log("LobbyView:OnLobbyStateChange:StartingGameServer");
                    break;
                case LobbyState.GameInProgress:
                    // Put player in room
                    Debug.Log("LobbyView:OnLobbyStateChange:GameInProgress");
                    _currentRoomAccess = null;
                    _lobby.GetLobbyRoomAccess((access, error) => 
                    {
                        Debug.Log("JMH:_lobby.GetLobbyRoomAccess");
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            Mst.Events.Invoke(MstEventKeys.showLoadingInfo, $"Get Lobby Room Access error: [{error}]");
                            return;
                        }

                        // Not sure what we're supposed to use this for. Maybe logging when runtime errors occur?
                        _currentRoomAccess = access;
                    });
                    break;
                case LobbyState.GameOver:
                    // TODO: Figure out how to handle this state, if we even have to...
                    Debug.Log("LobbyView:OnLobbyStateChange:GameOver");
                    break;
            }
        }

        private void OnHideLobbyViewEventHandler(EventMessage message)
        {
            Hide();
        }

        protected override void OnShow()
        {
            DrawPlayersList();
        }

        private void ClearPlayersList()
        {
            if (listContainer)
            {
                foreach (Transform tr in listContainer)
                {
                    Destroy(tr.gameObject);
                }
            }
        }

        private void DrawPlayersList()
        {
            ClearPlayersList();

            if (listContainer)
            {
                int index = 0;

                var memberNumberCol = Instantiate(uiColLablePrefab, listContainer, false);
                memberNumberCol.Text = "#";
                memberNumberCol.name = "memberNumberCol";

                var memberNameCol = Instantiate(uiColLablePrefab, listContainer, false);
                memberNameCol.Text = "Player Name";
                memberNameCol.name = "memberNameCol";

                var memberTeamCol = Instantiate(uiColLablePrefab, listContainer, false);
                memberTeamCol.Text = "Team";
                memberTeamCol.name = "memberTeamCol";

                var memberIsReadyCol = Instantiate(uiColLablePrefab, listContainer, false);
                memberIsReadyCol.Text = "Is Ready";
                memberIsReadyCol.name = "memberIsReadyCol";

                LobbyMemberData[] members = _lobby.Members.Values.ToArray();

                foreach (LobbyMemberData member in members)
                {
                    var memberNumberLable = Instantiate(uiLablePrefab, listContainer, false);
                    memberNumberLable.Text = $"{index + 1}";
                    memberNumberLable.name = $"memberNumberLable_{index}";

                    bool isHost = _lobby.IsMasterUser(member.Username);
                    bool isLocalPlayer = Mst.Client.Auth.AccountInfo.Username.Equals(member.Username, StringComparison.OrdinalIgnoreCase);
                    var memberNameLable = Instantiate(uiLablePrefab, listContainer, false);
                    memberNameLable.Text = $"{member.Username}{(isLocalPlayer ? " (You)" : "")}{(isHost ? " (Host)" : "")}";
                    memberNameLable.name = $"memberNameLable_{index}";

                    var memberTeamLable = Instantiate(uiLablePrefab, listContainer, false);
                    memberTeamLable.Text = member.Team;
                    memberTeamLable.name = $"memberTeamLable_{index}";

                    var memberIsReadyLable = Instantiate(uiLablePrefab, listContainer, false);
                    memberIsReadyLable.Text = member.IsReady.ToString();
                    memberIsReadyLable.name = $"memberIsReadyLable_{index}";

                    index++;

                    logger.Info(member);
                }
            }
            else
            {
                logger.Error("Not all components are setup.");
            }
        }

        public void OnReadyClick()
        {
            _lobby.SetReadyStatus(true);
        }

        public void OnUnreadyClick()
        {
            _lobby.SetReadyStatus(false);
        }

        public void OnExitClick()
        {
            _lobby.Leave();
        }

        public void OnStartGame()
        {
            _lobby.StartGame((isSuccessful, error) => 
            { 
                if (!isSuccessful)
                {
                    Debug.LogError($"Lobby Start Game Error: {error}");
                }
            });
        }

        private void OnMemberJoined(LobbyMemberData member)
        {
            DrawPlayersList();
        }

        private void OnMemberLeft(LobbyMemberData member)
        {
            DrawPlayersList();
        }

        private void OnMemberReadyStatusChanged(LobbyMemberData member, bool isReady)
        {
            DrawPlayersList();
        }
    }
}

