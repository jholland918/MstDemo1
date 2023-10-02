using Assets.App.Scripts.Character;
using MasterServerToolkit.MasterServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.App.Scripts.GameManagement
{
    public class PlayerRegistrationCollection : IEnumerable<PlayerRegistration>
    {
        private readonly Dictionary<string, PlayerRegistration> _players = new Dictionary<string, PlayerRegistration>();
        private readonly Dictionary<int, string> _ids = new();

        public IEnumerator<PlayerRegistration> GetEnumerator()
        {
            return _players.Values.GetEnumerator();
        }

        public void Add(RoomPlayer roomPlayer)
        {
            if (_ids.ContainsKey(roomPlayer.RoomPeerId))
            {
                _ids[roomPlayer.RoomPeerId] = roomPlayer.Username;
            }
            else
            {
                _ids.Add(roomPlayer.RoomPeerId, roomPlayer.Username);
            }

            if (_players.ContainsKey(roomPlayer.Username))
            {
                var player = _players[roomPlayer.Username];
                player.RoomPlayer = roomPlayer;
            }
            else
            {
                _players.Add(roomPlayer.Username, new PlayerRegistration
                {
                    RoomPlayer = roomPlayer,
                });
            }
        }

        public void Add(LobbyMemberData lobbyMember)
        {
            if (_players.ContainsKey(lobbyMember.Username))
            {
                var player = _players[lobbyMember.Username];
                player.LobbyMemberData = lobbyMember;
            }
            else
            {
                _players.Add(lobbyMember.Username, new PlayerRegistration
                {
                    LobbyMemberData = lobbyMember,
                });
            }
        }

        public void Add(PlayerCharacter character)
        {
            PlayerRegistration player;
            int id = character.NetworkObject.OwnerId;

            string username = _ids.ContainsKey(id) ? _ids[id] : null;
            if (_players.ContainsKey(username))
            {
                player = _players[username];
                player.PlayerCharacter = character;
                return;
            }

            // Backup search...
            player = _players.Values.First(p => p.RoomPlayer?.RoomPeerId == id || p.PlayerCharacter?.NetworkObject.OwnerId == id);
            player.PlayerCharacter = character;
        }

        public void Remove(RoomPlayer roomPlayer)
        {
            _players.Remove(roomPlayer.Username);
            _ids.Remove(roomPlayer.RoomPeerId);
        }

        public void Remove(PlayerCharacter character)
        {
            var player = _players[_ids[character.NetworkObject.OwnerId]];
            player.PlayerCharacter = null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _players.Values.GetEnumerator();
        }
    }
}
