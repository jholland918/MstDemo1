using Assets.App.Scripts.Character;
using MasterServerToolkit.MasterServer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.App.Scripts.GameManagement
{
    public class PlayerRegistrationCollection
    {
        public readonly List<RoomPlayer> RoomPlayers = new();
        public readonly List<PlayerCharacter> Characters = new();
        public readonly List<LobbyMemberData> LobbyMembers = new();

        public void Add(LobbyMemberData lobbyMember)
        {
            Debug.Log($"Add:LobbyMemberData:Username[{lobbyMember.Username}]");
            if (!LobbyMembers.Contains(lobbyMember))
            {
                LobbyMembers.Add(lobbyMember);
            }
        }

        public void Add(IEnumerable<RoomPlayer> roomPlayers)
        {
            foreach(RoomPlayer roomPlayer in roomPlayers)
            {
                Add(roomPlayer);
            }
        }

        public void Add(RoomPlayer roomPlayer)
        {
            Debug.Log($"Add:RoomPlayer: RoomPeerId[{roomPlayer.RoomPeerId}] Username[{roomPlayer.Username}]");
            if (!RoomPlayers.Contains(roomPlayer))
            {
                RoomPlayers.Add(roomPlayer);
            }
        }

        public void Add(PlayerCharacter character)
        {
            Debug.Log($"Add:PlayerCharacter: OwnerId[{character.OwnerId}]");
            if (!Characters.Contains(character))
            {
                Characters.Add(character);
            }
        }

        public void Remove(RoomPlayer roomPlayer)
        {
            // Hmmm, Instead of removing players, should they have a property named Active and update it to false instead?
            Debug.Log("Remove:RoomPlayer");
            //_players.Remove(roomPlayer.Username);
            //_ids.Remove(roomPlayer.RoomPeerId);
        }

        public void Remove(PlayerCharacter character)
        {
            Debug.Log("Remove:PlayerCharacter");
            //var player = _players[_ids[character.NetworkObject.OwnerId]];
            //player.PlayerCharacter = null;
        }

        internal string GetTeam(PlayerCharacter character)
        {
            var roomPlayer = RoomPlayers.SingleOrDefault(rp => rp.RoomPeerId == character.OwnerId);
            if (roomPlayer == null)
            {
                return null;
            }

            var lobbyMember = LobbyMembers.SingleOrDefault(lm => lm.Username == roomPlayer.Username);
            if (lobbyMember == null)
            {
                return null;
            }

            return lobbyMember.Team;
        }
    }
}
