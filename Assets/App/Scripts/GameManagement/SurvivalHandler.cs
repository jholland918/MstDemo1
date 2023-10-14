using Assets.App.Scripts.Character;
using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using System.Collections.Generic;
using System.Linq;

namespace Assets.App.Scripts.GameManagement
{
    internal class SurvivalHandler : BaseGameHandler
    {
        private readonly Logger _log = Mst.Create.Logger(nameof(SurvivalHandler));

        private readonly List<int> _deadCharacters = new();

        public SurvivalHandler(GameManager gameManager)
            : base(gameManager)
        {
        }

        public override void OnCharacterDie(PlayerCharacterVitals characterVitals)
        {
            _log.Debug("*** OnCharacterDie ***");
            _deadCharacters.Add(characterVitals.OwnerId);

            var aliveCharacters = PlayerRegistrations.Characters.Where(p => !_deadCharacters.Contains(p.OwnerId));

            if (aliveCharacters.Count() == 1)
            {
                Dictionary<int, string> playerResults = new();

                foreach (var p in PlayerRegistrations.Characters)
                {
                    bool isWinner = aliveCharacters.Any(a => a.OwnerId == p.OwnerId);
                    playerResults.Add(p.OwnerId, (isWinner ? "You Won!" : "You Lost!"));
                }

                _gameManager.OnGameOver(new GameResults
                {
                    PlayerResults = playerResults
                });
            }
        }
    }
}
