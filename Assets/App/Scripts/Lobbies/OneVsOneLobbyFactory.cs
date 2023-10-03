using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.App.Scripts.Lobbies
{
    internal class OneVsOneLobbyFactory : ILobbyFactory
    {
        public string Id => "OneVsOne";
        public static string DefaultName = "OneVsOne Lobby";
        private LobbiesModule _module;

        public OneVsOneLobbyFactory(LobbiesModule module)
        {
            _module = module;
        }

        public ILobby CreateLobby(MstProperties options, IPeer creator)
        {
            var properties = options.ToDictionary();

            // Create the teams
            var teamA = new LobbyTeam("A")
            {
                MaxPlayers = 1,
                MinPlayers = 1
            };
            var teamB = new LobbyTeam("B")
            {
                MaxPlayers = 1,
                MinPlayers = 1
            };

            // Set their colors
            teamA.SetProperty("color", "0000FF");
            teamB.SetProperty("color", "FF0000");

            var config = new LobbyConfig();
            config.PlayAgainEnabled = true; // If this is true, then the lobby state will not go into "LobbyState.GameOver", instead it will go into "LobbyState.Preparations". I'm not sure what I really should use at this time though...

            // Create the lobby
            var lobby = new BaseLobby(_module.NextLobbyId(), new[] { teamA, teamB }, _module, config)
            {
                Name = ExtractLobbyName(properties)
            };

            // Override properties with what user provided
            lobby.SetLobbyProperties(properties);

            // Add control for the game speed
            lobby.AddControl(new LobbyPropertyData()
            {
                Label = "Game Speed",
                Options = new List<string>() { "1x", "2x", "3x" },
                PropertyKey = "speed"
            }, "2x"); // Default option

            // Add control to enable/disable gravity
            lobby.AddControl(new LobbyPropertyData()
            {
                Label = "Gravity",
                Options = new List<string>() { "On", "Off" },
                PropertyKey = "gravity",
            });

            return lobby;
        }

        public static string ExtractLobbyName(Dictionary<string, string> properties)
        {
            return properties.ContainsKey(MstDictKeys.LOBBY_NAME) ? properties[MstDictKeys.LOBBY_NAME] : DefaultName;
        }
    }
}
