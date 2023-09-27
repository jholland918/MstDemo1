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

        //protected IEnumerable<RoomPlayer> Players => _roomManager.Players;

        protected BaseGameHandler(LobbyDataPacket lobbyInfo) 
        {
            _lobbyInfo = lobbyInfo;

            //PlayerCharacter.OnServerCharacterSpawnedEvent += PlayerCharacter_OnServerCharacterSpawnedEvent;
            //PlayerCharacter.OnCharacterDestroyedEvent += PlayerCharacter_OnCharacterDestroyedEvent;
        }

        protected virtual void PlayerCharacterVitals_OnAliveEvent(PlayerCharacter playerCharacter)
        {
            _logger.Info($"playerCharacter.OwnerId[{playerCharacter.OwnerId}]");
        }

        protected virtual void PlayerCharacterVitals_OnDieEvent(PlayerCharacter playerCharacter)
        {
            _logger.Info($"playerCharacter.OwnerId[{playerCharacter.OwnerId}]");
        }

        private void PlayerCharacter_OnServerCharacterSpawnedEvent(PlayerCharacter playerCharacter)
        {
            //_logger.Info($"SpawnedEvent::playerCharacter.OwnerId[{playerCharacter.OwnerId}]");

            if (!PlayerCharacters.Contains(playerCharacter))
            {
                PlayerCharacters.Add(playerCharacter);
                var playerCharacterVitals = playerCharacter.GetComponent<PlayerCharacterVitals>();
                playerCharacterVitals.OnDieEvent += () => PlayerCharacterVitals_OnDieEvent(playerCharacter);
                playerCharacterVitals.OnAliveEvent += () => PlayerCharacterVitals_OnAliveEvent(playerCharacter);
            }
        }

        private void PlayerCharacter_OnCharacterDestroyedEvent(PlayerCharacter playerCharacter)
        {
            if (PlayerCharacters.Contains(playerCharacter))
            {
                PlayerCharacters.Remove(playerCharacter);
            }
        }
    }
}
