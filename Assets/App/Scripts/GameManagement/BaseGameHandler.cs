using Assets.App.Scripts.Character;
using MasterServerToolkit.MasterServer;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.App.Scripts.GameManagement
{
    internal abstract class BaseGameHandler : MonoBehaviour
    {
        protected GameManager _gameManager;
        protected Dictionary<int, PlayerCharacter> PlayerCharacters => _gameManager.PlayerCharacters;
        private MasterServerToolkit.Logging.Logger _log;

        protected void Awake()
        {
            _log = Mst.Create.Logger("BaseGameHandler");
            _gameManager = GetComponent<GameManager>();

            PlayerCharacterVitals.OnServerCharacterDieEvent += OnCharacterDie;
            PlayerCharacterVitals.OnServerCharacterAliveEvent += OnCharacterAlive;
        }

        protected virtual void OnCharacterAlive(PlayerCharacterVitals characterVitals)
        {
            _log.Debug("*** OnCharacterAlive ***");
        }

        protected virtual void OnCharacterDie(PlayerCharacterVitals characterVitals)
        {
            _log.Debug("*** OnCharacterDie ***");
        }
    }
}
