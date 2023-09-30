using Assets.App.Scripts.Character;
using Assets.App.Scripts.GameManagement;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.UI;
using System;
using TMPro;
using UnityEngine;

namespace Assets.App.Scripts
{
    public class RoomHudView : UIView
    {
        [SerializeField]
        private GameManager _gameManager;

        public TextMeshProUGUI HealthText;
        public TextMeshProUGUI GameResultsText;
        private PlayerCharacter _character;
        private PlayerCharacterVitals _playerCharacterVitals;

        protected override void Awake()
        {
            base.Awake();

            PlayerCharacter.OnLocalCharacterSpawnedEvent += OnLocalCharacterSpawnedEventHandler;
            _gameManager.OnGameResults += OnGameResults;
            GameResultsText.enabled = false;
        }

        private void OnGameResults(GameResults gameResults)
        {
            Debug.Log("RoomHudView:OnGameResults");
            GameResultsText.enabled = true;
            GameResultsText.text = "Draw!";

            if (gameResults.PlayerResults.ContainsKey(_character.NetworkObject.OwnerId))
            {
                Debug.Log("RoomHudView:OnGameResults:ContainsKey");
                bool isWinner = gameResults.PlayerResults[_character.NetworkObject.OwnerId];
                GameResultsText.text = isWinner ? "You Won!" : "You Lost!";
            }
        }

        private void OnLocalCharacterSpawnedEventHandler(PlayerCharacter character)
        {
            var playerCharacterVitals = character.GetComponent<PlayerCharacterVitals>();
            HealthText.text = playerCharacterVitals.Health.ToString();

            // TODO: Experiment when characters get respawned to see if we really need all this logic. Maybe this logic can be simplified.
            if (character == _character)
            {
                // The character objec hasn't changed 
                // So we don't need to do anything at this point unless we want to update the HUD due to respawning.
                return;
            }

            if (_character != null)
            {
                // This is a subsequent time we're assigning event handlers, clean up previous handlers so we don't over subscribe
                _playerCharacterVitals.OnVitalChangedEvent -= OnVitalChangedEventHandler;
                _playerCharacterVitals.OnDieEvent -= OnDieEventHandler;
            }

            // Just do simple initialization here since previous cleanup has been taken care of above.
            _character = character;
            _playerCharacterVitals = playerCharacterVitals;
            _playerCharacterVitals.OnVitalChangedEvent += OnVitalChangedEventHandler;
            _playerCharacterVitals.OnDieEvent += OnDieEventHandler;
        }

        private void OnDieEventHandler()
        {
            HealthText.text = "You died!";
        }

        private void OnVitalChangedEventHandler(short key, float value)
        {
            switch ((PlayerVitalsKey)key)
            {
                case PlayerVitalsKey.Health:
                    HealthText.text = value.ToString();
                    break;
            }
        }

        public void Disconnect()
        {
            Mst.Events.Invoke(MstEventKeys.leaveRoom);
        }

        public void ShowPlayersList()
        {
            if (Mst.Client.Rooms.HasAccess)
                Mst.Events.Invoke(MstEventKeys.showPlayersListView, Mst.Client.Rooms.ReceivedAccess.RoomId);
        }
    }
}
