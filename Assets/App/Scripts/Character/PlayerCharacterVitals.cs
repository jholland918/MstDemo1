#if FISHNET
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MasterServerToolkit.Networking;
using System;
using TMPro;
using UnityEngine;

namespace Assets.App.Scripts.Character
{
    public delegate void VitalChangeFloatDelegate(short key, float value);

    public class PlayerCharacterVitals : PlayerCharacterBehaviour
    {
        public static event Action<PlayerCharacterVitals> OnServerCharacterDieEvent;
        public static event Action<PlayerCharacterVitals> OnServerCharacterAliveEvent;

        #region INSPECTOR

        [Header("Components"), SerializeField]
        private CharacterController characterController;
        [SerializeField]
        private GameObject dieEffectPrefab;

        #endregion

        /// <summary>
        /// Called when player resurrected
        /// </summary>
        public event Action OnAliveEvent;

        /// <summary>
        /// Called when player dies
        /// </summary>
        public event Action OnDieEvent;

        /// <summary>
        /// Called on client when one of the vital value is changed
        /// </summary>
        public event VitalChangeFloatDelegate OnVitalChangedEvent;

        /// <summary>
        /// Check if character is alive
        /// </summary>
        public bool IsAlive { get; protected set; } = true;

        public override bool IsReady => characterController;

        #region MstDemo1 Changes
        public int Health = 10;

        public void TakeDamage(int hitPoints)
        {
            Health -= hitPoints;
            NotifyVitalChanged((short)PlayerVitalsKey.Health, Health);

            if (Health <= 0)
            {
                NotifyDied();
            }
        }
        #endregion MstDemo1 Changes

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void NotifyVitalChanged(short key, float value)
        {
            if (IsServer)
            {
                Rpc_NotifyVitalChanged(key, value);
            }
        }

        [ObserversRpc]
        private void Rpc_NotifyVitalChanged(short key, float value)
        {
            if (IsOwner)
            {
                OnVitalChangedEvent?.Invoke(key, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void NotifyAlive()
        {
            if (IsServer)
            {
                IsAlive = true;
                Rpc_NotifyAlive();
                OnServerCharacterAliveEvent?.Invoke(this);
            }
        }

        [ObserversRpc]
        private void Rpc_NotifyAlive()
        {
            if (IsOwner)
            {
                IsAlive = true;
                OnAliveEvent?.Invoke();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void NotifyDied()
        {
            if (IsServer)
            {
                characterController.enabled = false;

                IsAlive = false;
                Rpc_NotifyDied();
                OnServerCharacterDieEvent?.Invoke(this);
            }
        }

        [ObserversRpc]
        private void Rpc_NotifyDied()
        {
            if (IsOwner)
            {
                characterController.enabled = false;

                IsAlive = false;
                OnDieEvent?.Invoke();
            }

            if (dieEffectPrefab)
            {
                MstTimer.WaitForSeconds(1f, () =>
                {
                    Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);
                });
            }
        }
    }
}
#endif