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
            var aliveCharacters = PlayerCharacters.Where(kvp => !_deadCharacters.Contains(kvp.Key));
            if (aliveCharacters.Count() == 1)
            {
                Dictionary<int, bool> playerResults = new Dictionary<int, bool>();

                foreach (var kvp in PlayerCharacters)
                {
                    bool isWinner = aliveCharacters.Any(kvp2 => kvp2.Key == kvp.Key);
                    playerResults.Add(kvp.Key, isWinner);
                }

                _gameManager.OnGameOver(new GameResults 
                {
                    PlayerResults = playerResults
                });
            }
        }
    }
}
