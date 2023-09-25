using MasterServerToolkit.MasterServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.App.Scripts.GameManagement
{
    internal class TwoVsTwoGameHandler : BaseGameHandler
    {
        public TwoVsTwoGameHandler(LobbyDataPacket lobbyInfo) : base(lobbyInfo)
        {
        }
    }
}
