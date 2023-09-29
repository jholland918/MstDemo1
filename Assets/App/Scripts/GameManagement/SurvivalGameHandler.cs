﻿using Assets.App.Scripts.Character;
using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using System.Collections.Generic;
using System.Linq;

namespace Assets.App.Scripts.GameManagement
{
    internal class SurvivalGameHandler : BaseGameHandler
    {
        private Logger _log;

        private List<int> _deadCharacters = new();

        protected void Awake()
        {
            base.Awake();
            _log = Mst.Create.Logger("SurvivalGameHandler");
        }

        protected override void OnCharacterAlive(PlayerCharacterVitals characterVitals)
        {
            _log.Debug("*** OnCharacterAlive ***");
        }

        protected override void OnCharacterDie(PlayerCharacterVitals characterVitals)
        {
            _log.Debug("*** OnCharacterAlive ***");
            _deadCharacters.Add(characterVitals.NetworkObject.OwnerId);
            var aliveCharacters = PlayerCharacters.Where(kvp => !_deadCharacters.Contains(kvp.Key));
            if (aliveCharacters.Count() == 1)
            {
                _log.Debug("GAME OVER MAN!!11");
            }
        }
    }
}
