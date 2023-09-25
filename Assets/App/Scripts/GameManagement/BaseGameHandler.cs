using MasterServerToolkit.MasterServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.App.Scripts.GameManagement
{
    internal abstract class BaseGameHandler
    {
        private readonly LobbyDataPacket _lobbyInfo;

        protected BaseGameHandler(LobbyDataPacket lobbyInfo) 
        {
            _lobbyInfo = lobbyInfo; 
        }
    }
}
