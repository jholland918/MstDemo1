using Assets.App.Scripts.Character;
using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using System.Collections.Generic;
using System.Linq;

namespace Assets.App.Scripts.GameManagement
{
    internal class SurvivalHandler : BaseGameHandler
    {
        private Logger _log = Mst.Create.Logger("SurvivalGameHandler");

        private List<int> _deadCharacters = new();

        public SurvivalHandler(GameManager gameManager) 
            : base(gameManager)
        {
        }

        public override void OnCharacterDie(PlayerCharacterVitals characterVitals)
        {
            _log.Debug("*** OnCharacterDie ***");
            _deadCharacters.Add(characterVitals.NetworkObject.OwnerId);

            var aliveCharacters = PlayerRegistrations.Where(pr => !_deadCharacters.Contains(pr.PlayerCharacter.NetworkObject.OwnerId));
            if (aliveCharacters.Count() == 1)
            {
                Dictionary<int, bool> playerResults = new Dictionary<int, bool>();

                foreach (var reg in PlayerRegistrations)
                {
                    bool isWinner = aliveCharacters.Any(ac => ac.PlayerCharacter.NetworkObject.OwnerId == reg.PlayerCharacter.NetworkObject.OwnerId);
                    playerResults.Add(reg.PlayerCharacter.NetworkObject.OwnerId, isWinner);
                }

                _gameManager.OnGameOver(new GameResults 
                {
                    PlayerResults = playerResults
                });
            }
        }
    }
}
