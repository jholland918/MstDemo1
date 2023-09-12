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
            StartCoroutine("Countdown", 10);
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
            _roomController.Destroy();
        }

        private void OnRoomRegistered(RoomController roomController)
        {
            Debug.Log("MatchTimer:OnRoomRegistered");
            _roomController = roomController;
        }
    }
}