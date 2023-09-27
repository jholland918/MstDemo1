using MasterServerToolkit.MasterServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.App.Scripts.GameManagement
{
    internal class SurvivalGameHandler : BaseGameHandler
    {
        public SurvivalGameHandler(LobbyDataPacket lobbyInfo) 
            : base(lobbyInfo)
        {
        }

        // TODO: End game on last-player-standing
    }
}
