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

        // Calls to this Add() method happen first
        public void Add(LobbyMemberData lobbyMember)
        {
            Debug.Log($"Add:LobbyMemberData:Username[{lobbyMember.Username}]");
            _players.Add(lobbyMember.Username, new PlayerRegistration
            {
                LobbyMemberData = lobbyMember,
            });
        }

        // Calls to this Add() method happen second
        public void Add(RoomPlayer roomPlayer)
        {
            Debug.Log($"Add:RoomPlayer: RoomPeerId[{roomPlayer.RoomPeerId}] Username[{roomPlayer.Username}]");

            // This happens before `Add(PlayerCharacter character)`, so we don't need to call ContainsKey() - Just Add()
            _ids.Add(roomPlayer.RoomPeerId, roomPlayer.Username);

            // An entry should have already been created for this username by `Add(LobbyMemberData lobbyMember)`
            var player = _players[roomPlayer.Username];
            player.RoomPlayer = roomPlayer;
        }

        // Calls to this Add() method happen third
        public void Add(PlayerCharacter character)
        {
            Debug.Log($"Add:PlayerCharacter: OwnerId[{character.NetworkObject.OwnerId}]");
            
            // An ID should have already been added by `Add(RoomPlayer roomPlayer)`
            string username = _ids[character.NetworkObject.OwnerId];

            Debug.Log($"Add:PlayerCharacter: Found Username[{username}]");

            // This player registration should have already been added in `Add(LobbyMemberData lobbyMember)`
            var player = _players[username];
            player.PlayerCharacter = character;
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

        public IEnumerator<PlayerRegistration> GetEnumerator()
        {
            return _players.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _players.Values.GetEnumerator();
        }
    }
}
