using Assets.App.Scripts;
using MasterServerToolkit.MasterServer;
using UnityEngine.Events;

namespace MasterServerToolkit.Bridges
{
    public static class MatchmakingBehaviourExtensions
    {
        /// <summary>
        /// Sends request to master server to start new lobby
        /// </summary>
        /// <param name="spawnOptions"></param>
        public static void CreateNewLobby(this MatchmakingBehaviour mmb, string factory, MstProperties spawnOptions, UnityAction failCallback = null)
        {
            // Note: This was copied from CreateNewRoom() and converted for lobby creation, so it's a WIP.
            Mst.Events.Invoke(MstEventKeys.showLoadingInfo, $"Starting {factory} lobby... Please wait!");

            // Can't call mmb.logger from MatchmakingBehaviour because it's protected...
            //mmb.logger.Debug("Starting lobby... Please wait!");
            var logger = Mst.Create.Logger(nameof(MatchmakingBehaviourExtensions));
            logger.Debug($"Starting {factory} lobby... Please wait!");

            // Custom options that will be given to room directly
            var options = new MstProperties();
            options.Add(Mst.Args.Names.StartClientConnection, true);

            Mst.Client.Lobbies.CreateAndJoin(factory, options, (lobby, error) =>
            {
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Mst.Events.Invoke(MstEventKeys.showOkDialogBox, new OkDialogBoxEventMessage($"Create New Lobby failed: {error}", () =>
                    {
                        failCallback?.Invoke();
                    }));

                    return;
                }
                else
                {
                    // TODO: Assuming this means it worked, write code to use the working lobby.

                    // Also, look at MstDictKeys and how to use them:
                    /*
                         public struct MstDictKeys
                        {
                            public const string ROOM_ID = "-roomId";
                            public const string LOBBY_FACTORY_ID = "-lobbyFactory";
                            public const string LOBBY_NAME = "-lobbyName";
                            public const string LOBBY_PASSWORD = "-lobbyPassword";
                            public const string LOBBY_TEAM = "-lobbyTeam";
                    ...
                     */

                    //Mst.Events.Invoke(MstEventKeys.showOkDialogBox, new OkDialogBoxEventMessage("Creating a lobby worked!", () =>
                    //{
                    //    failCallback?.Invoke();
                    //}));

                    Mst.Events.Invoke(MstEventKeys.showLobbyListView, lobby);

                    return;
                }
            });
        }

        public static void JoinLobby(this MatchmakingBehaviour mmb, GameInfoPacket gameInfo)
        {
            var options = new MstProperties();
            options.Add(Mst.Args.Names.LobbyId, gameInfo.Id); //jmh//not sure if this is correct

            Mst.Client.Lobbies.JoinLobby(gameInfo.Id, (lobby, error) =>
            {
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Mst.Events.Invoke(MstEventKeys.showLoadingInfo, $"Join lobby error: [{error}]");
                }
                else
                {
                    Mst.Events.Invoke(AppEventKeys.showLobbyView, lobby);
                    return;
                }
            });
        }
    }

}
