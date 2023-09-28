using Assets.App.Scripts.Character;
using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using System.Collections.Generic;

namespace Assets.App.Scripts.GameManagement
{
    internal abstract class BaseGameHandler
    {
        protected readonly LobbyDataPacket _lobbyInfo;
        protected readonly List<PlayerCharacter> PlayerCharacters = new List<PlayerCharacter>();
        private Logger _logger = Mst.Create.Logger("BaseGameHandler");

        protected BaseGameHandler(LobbyDataPacket lobbyInfo) 
        {
            _lobbyInfo = lobbyInfo;

            PlayerCharacterVitals.OnServerCharacterDieEvent += PlayerCharacterVitals_OnServerCharacterDieEvent;
            PlayerCharacterVitals.OnServerCharacterAliveEvent += PlayerCharacterVitals_OnServerCharacterAliveEvent;
        }

        protected virtual void PlayerCharacterVitals_OnServerCharacterAliveEvent(PlayerCharacterVitals characterVitals)
        {
            _logger.Debug("*** OnServerCharacterAliveEvent ***");
        }

        protected virtual void PlayerCharacterVitals_OnServerCharacterDieEvent(PlayerCharacterVitals characterVitals)
        {
            _logger.Debug("*** OnServerCharacterDieEvent ***");
        }
    }
}
