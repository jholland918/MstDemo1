#if FISHNET
using UnityEngine;

namespace Assets.App.Scripts.Character
{
    [DisallowMultipleComponent]
    public class PlayerCharacterAvatar : PlayerCharacterBehaviour
    {
        [Header("Components"), SerializeField]
        protected GameObject[] remoteParts;

        public override void OnStartClient()
        {
            base.OnStartClient();
            SetPartsActive(!IsOwner);
        }

        public virtual void SetPartsActive(bool value)
        {
            if (remoteParts != null)
            {
                foreach (var part in remoteParts)
                {
                    part.SetActive(value);
                }
            }
        }
    }
}
#endif