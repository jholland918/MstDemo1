using MasterServerToolkit.MasterServer;
using System.Collections;
using UnityEngine;

namespace Assets.App.Scripts.Lobbies
{
    /// <summary>
    /// Controls the time limit for a game match and stops the game when time is up.
    /// </summary>
    public class MatchTimer : MonoBehaviour
    {
        private RoomServerManager _roomManager;
        private RoomController _roomController;

        private void Awake()
        {
            Debug.Log("MatchTimer:Awake");
            _roomManager = GetComponent<RoomServerManager>();
            _roomManager.OnRoomRegisteredEvent.AddListener(OnRoomRegistered);
        }

        void Start()
        {
            Debug.Log("MatchTimer:Start");
            StartCoroutine("Countdown", 30);
        }

        private IEnumerator Countdown(int time)
        {
            while (time > 0)
            {
                time--;
                Debug.Log($"MatchTimer:Countdown:{time}");
                yield return new WaitForSeconds(1);
            }
            Debug.Log("MatchTimer:Countdown Complete!");
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
            Debug.Log("MatchTimer:StopGame");

            // Destroying the room will clean up and reset various states in the game, finally setting the 
            // LobbyState to LobbyState.Preparations, which will trigger an event handler to load the "Client" scene,
            // which will render the LobbyView once again.
            _roomController.Destroy();

            // Closing the connection will terminate/close the Room.exe process if the RoomServerManager "Terminate Room When Disconnected"
            // setting is true. This allows lobby players to keep starting new matches and returning back to the LobbyView as much as they want.
            // If this is not called, the original Room.exe process will not be terminated and new Room.exe processes will be spawned.
            _roomController.Connection.Close();
        }

        private void OnRoomRegistered(RoomController roomController)
        {
            Debug.Log("MatchTimer:OnRoomRegistered");
            _roomController = roomController;
        }
    }
}