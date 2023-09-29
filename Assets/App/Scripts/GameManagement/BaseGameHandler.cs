using Assets.App.Scripts.Character;
using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using System.Collections.Generic;

namespace Assets.App.Scripts.GameManagement
{
    internal abstract class BaseGameHandler
    {
        protected readonly GameManager _gameManager;
        protected Dictionary<int, PlayerCharacter> PlayerCharacters => _gameManager.PlayerCharacters;
        private Logger _log = Mst.Create.Logger("BaseGameHandler");

        protected BaseGameHandler(GameManager gameManager)
        {
            _gameManager = gameManager;

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
