using Assets.App.Scripts.Character;
using MasterServerToolkit.MasterServer;
using System.Collections.Generic;

namespace Assets.App.Scripts.GameManagement
{
    internal class BaseGameHandler
    {
        protected GameManager _gameManager;
        protected PlayerRegistrationCollection PlayerRegistrations => _gameManager.PlayerRegistrations;
        private MasterServerToolkit.Logging.Logger _log;

        public BaseGameHandler(GameManager gameManager)
        {
            _log = Mst.Create.Logger("BaseGameHandler");
            _gameManager = gameManager;
        }

        public virtual void OnCharacterAlive(PlayerCharacterVitals characterVitals)
        {
            _log.Debug("*** OnCharacterAlive ***");
        }

        public virtual void OnCharacterDie(PlayerCharacterVitals characterVitals)
        {
            _log.Debug("*** OnCharacterDie ***");
        }
    }
}
