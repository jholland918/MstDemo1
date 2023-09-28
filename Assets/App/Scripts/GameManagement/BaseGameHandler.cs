using Assets.App.Scripts.Character;
using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        protected virtual void PlayerCharacterVitals_OnServerCharacterDieEvent(PlayerCharacterVitals vitals)
        {
            _logger.Debug("*** OnServerCharacterDieEvent ***");
        }
    }
}
