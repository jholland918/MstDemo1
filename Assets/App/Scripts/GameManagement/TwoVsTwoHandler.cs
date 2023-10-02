using Assets.App.Scripts.Character;
using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using System.Collections.Generic;
using System.Linq;

namespace Assets.App.Scripts.GameManagement
{
    internal class TwoVsTwoHandler : BaseGameHandler
    {
        private readonly Logger _log = Mst.Create.Logger(nameof(TwoVsTwoHandler));

        private readonly List<int> _deadCharacters = new();

        public TwoVsTwoHandler(GameManager gameManager) 
            : base(gameManager)
        {
        }

        public override void OnCharacterDie(PlayerCharacterVitals characterVitals)
        {
            _log.Debug("*** OnCharacterDie ***");
            _deadCharacters.Add(characterVitals.NetworkObject.OwnerId);

            // Get the body counts!
            Dictionary<string, int> dead = new();
            Dictionary<string, int> alive = new();
            foreach (var p in PlayerRegistrations)
            {
                if (!dead.ContainsKey(p.Team))
                {
                    dead.Add(p.Team, 0);
                }

                if (!alive.ContainsKey(p.Team))
                {
                    alive.Add(p.Team, 0);
                }

                if (_deadCharacters.Contains(p.OwnerId))
                {
                    dead[p.Team] += 1;
                }
                else
                {
                    alive[p.Team] += 1;
                }
            }

            if (alive.Values.Sum() == 0)
            {
                // Everyone is dead
                Dictionary<int, string> playerResults = new();
                foreach (var p in PlayerRegistrations)
                {
                    playerResults.Add(p.OwnerId, "Draw!");
                }

                _gameManager.OnGameOver(new GameResults
                {
                    PlayerResults = playerResults
                });
            }
            else if (alive.Count(a => a.Value > 0) == 1)
            {
                // Only one team is left alive
                string winningTeam = alive.Single(a => a.Value > 0).Key;
                Dictionary<int, string> playerResults = new();
                foreach (var p in PlayerRegistrations)
                {
                    string resultMessage = p.Team.Equals(winningTeam) ? "Your team won!" : "Your team lost!";
                    playerResults.Add(p.OwnerId, resultMessage);
                }

                _gameManager.OnGameOver(new GameResults
                {
                    PlayerResults = playerResults
                });
            }
        }
    }
}
