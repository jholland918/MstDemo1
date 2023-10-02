using Assets.App.Scripts.Character;
using MasterServerToolkit.MasterServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.App.Scripts.GameManagement
{
    public class PlayerRegistrationCollection : IEnumerable<PlayerRegistration>
    {
        private readonly Dictionary<string, PlayerRegistration> _players = new Dictionary<string, PlayerRegistration>();

        public IEnumerator<PlayerRegistration> GetEnumerator()
        {
            return _players.Values.GetEnumerator();
        }

        public void Add(RoomPlayer roomPlayer)
        {
            int id = roomPlayer.RoomPeerId;
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

        public void Add(PlayerCharacter character)
        {
            int id = character.NetworkObject.OwnerId;
            throw new NotImplementedException();
        }

        public void Remove(RoomPlayer roomPlayer)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _players.Values.GetEnumerator();
        }
    }
}
