using Assets.App.Scripts.Character;
using MasterServerToolkit.MasterServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.App.Scripts.GameManagement
{
    public struct PlayerRegistration
    {
        public RoomPlayer RoomPlayer;

        public PlayerCharacter PlayerCharacter;

        public LobbyMemberData LobbyMemberData;
    }
}
