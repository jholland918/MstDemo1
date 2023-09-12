using MasterServerToolkit.Bridges;
using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.UI;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Assets.App.Scripts.UI
{
    public class CreateNewLobbyView : UIView
    {
        [Header("Components"), SerializeField]
        private TMP_InputField roomNameInputField;
        [SerializeField]
        private TMP_Dropdown gameTypeInputDropdown;
        [SerializeField]
        private TMP_Dropdown roomRegionNameInputDropdown;
        [SerializeField]
        private TMP_InputField roomPasswordInputField;

        protected override void Awake()
        {
            base.Awake();

            RoomName = $"Room#{Mst.Helper.CreateFriendlyId()}";

            // Listen to show/hide events
            Mst.Events.AddListener(MstEventKeys.showCreateLobbyView, OnShowCreateLobbyViewEventHandler);
            Mst.Events.AddListener(MstEventKeys.hideCreateLobbyView, OnHideCreateLobbyViewEventHandler);
        }

        private void OnShowCreateLobbyViewEventHandler(EventMessage message)
        {
            Show();
        }

        private void OnHideCreateLobbyViewEventHandler(EventMessage message)
        {
            Hide();
        }

        protected override void OnShow()
        {
            base.OnShow();

            Mst.Client.Matchmaker.GetRegions(regions =>
            {
                roomRegionNameInputDropdown.ClearOptions();
                roomRegionNameInputDropdown.interactable = regions.Count > 0;

                if (regions.Count > 0)
                {
                    roomRegionNameInputDropdown.AddOptions(regions.Select(i =>
                    {
                        return $"<b>{i.Name}</b>, <color=#FF0000FF>Ping: {i.PingTime} ms.</color>";
                    }).ToList());
                }
            });
        }

        public string RoomName
        {
            get
            {
                return roomNameInputField != null ? roomNameInputField.text : string.Empty;
            }

            set
            {
                if (roomNameInputField)
                    roomNameInputField.text = value;
            }
        }

        public string GameType
        {
            get
            {
                return gameTypeInputDropdown != null && gameTypeInputDropdown.options.Count > 0 ? gameTypeInputDropdown.options[gameTypeInputDropdown.value].text : string.Empty;
            }
        }

        public string RegionName
        {
            get
            {
                return roomRegionNameInputDropdown != null && roomRegionNameInputDropdown.options.Count > 0 ? Mst.Client.Matchmaker.Regions[roomRegionNameInputDropdown.value].Name : string.Empty;
            }
        }

        public string Password
        {
            get
            {
                return roomPasswordInputField != null ? roomPasswordInputField.text : string.Empty;
            }

            set
            {
                if (roomPasswordInputField)
                    roomPasswordInputField.text = value;
            }
        }

        public void CreateNewMatch()
        {
            Mst.Events.Invoke(MstEventKeys.showLoadingInfo, "Starting lobby... Please wait!");

            Logs.Debug("Starting lobby... Please wait!");

            Regex roomNameRe = new Regex(@"\s+");

            var options = new MstProperties();
            options.Add(Mst.Args.Names.RoomName, roomNameRe.Replace(RoomName, "_"));
            options.Add(Mst.Args.Names.LobbyId, GameType); // I'm not sure if this is correct. -Jason

            if (!string.IsNullOrEmpty(Password))
                options.Add(Mst.Args.Names.RoomPassword, Password);

            MatchmakingBehaviour.Instance.CreateNewLobby(GameType, options, () =>
            {
                Show();
            });
        }
    }
}