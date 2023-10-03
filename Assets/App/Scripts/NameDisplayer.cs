using FishNet.Connection;
using FishNet.Object;
using TMPro;
using UnityEngine;

namespace Assets.App.Scripts
{
    public class NameDisplayer : NetworkBehaviour
    {
        private int _maxNameLength = 12;

        [SerializeField]
        private TextMeshPro _text;

        public override void OnStartClient()
        {
            base.OnStartClient();
            SetName();
            PlayerNameTracker.OnNameChange += PlayerNameTracker_OnNameChange;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            PlayerNameTracker.OnNameChange -= PlayerNameTracker_OnNameChange;
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            SetName();
        }


        private void PlayerNameTracker_OnNameChange(NetworkConnection arg1, string arg2)
        {
            if (arg1 != Owner)
                return;

            SetName();
        }


        /// <summary>
        /// Sets Text to the name for this objects owner.
        /// </summary>
        private void SetName()
        {
            string result = null;
            //Owner does nto exist.
            if (Owner.IsValid)
                result = PlayerNameTracker.GetPlayerName(Owner);

            if (string.IsNullOrEmpty(result))
                result = "Unset";

            _text.text = result.Substring(0, System.Math.Min(result.Length, _maxNameLength));
        }
    }
}
