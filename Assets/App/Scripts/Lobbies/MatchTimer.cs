using MasterServerToolkit.MasterServer;
using System.Collections;
using UnityEngine;

namespace Assets.App.Scripts.Lobbies
{
    public class MatchTimer : MonoBehaviour
    {
        private RoomServerManager _roomManager;
        private RoomController _roomController;

        private void Awake()
        {
            Debug.Log("JMH:001");
            _roomManager = GetComponent<RoomServerManager>();
            _roomManager.OnRoomRegisteredEvent.AddListener(OnRoomRegistered);
        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("JMH:002");
            StartCoroutine("Countdown", 10);
        }

        private IEnumerator Countdown(int time)
        {
            while (time > 0)
            {
                time--;
                Debug.Log($"JMH:006:Countdown:{time}");
                yield return new WaitForSeconds(1);
            }
            Debug.Log("Countdown Complete!");
            _roomController.Destroy();
        }

        private void OnRoomRegistered(RoomController roomController)
        {
            Debug.Log("JMH:003");
            _roomController = roomController;
        }

        // Update is called once per frame
        void Update()
        {
        }
    }

}