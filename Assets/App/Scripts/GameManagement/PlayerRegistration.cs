using Assets.App.Scripts.Character;
using MasterServerToolkit.MasterServer;

namespace Assets.App.Scripts.GameManagement
{
    public class PlayerRegistration
    {
        public int OwnerId
        {
            get
            {
                return PlayerCharacter != null ? PlayerCharacter.OwnerId : 0;
            }
        }

        public int RoomPeerId
        {
            get
            {
                return RoomPlayer != null ? RoomPlayer.RoomPeerId : 0;
            }
        }

        public string Username
        {
            get
            {
                if (LobbyMemberData != null)
                {
                    return LobbyMemberData.Username;
                }

                if (RoomPlayer != null)
                {
                    return RoomPlayer.Username;
                }

                return null;
            }
        }

        public string Team => LobbyMemberData?.Team;

        public RoomPlayer RoomPlayer;

        public PlayerCharacter PlayerCharacter;

        public LobbyMemberData LobbyMemberData;
    }
}
