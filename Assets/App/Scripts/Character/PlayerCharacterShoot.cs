using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.App.Scripts.Character
{
    public class PlayerCharacterShoot : PlayerCharacterBehaviour
    {
        /// <summary>
        /// Projectile to spawn.
        /// </summary>
        [Tooltip("Projectile to spawn.")]
        [SerializeField]
        private PredictedProjectile _projectile;

        /// <summary>
        /// Maximum amount of passed time a projectile may have.
        /// This ensures really laggy players won't be able to disrupt
        /// other players by having the projectile speed up beyond
        /// reason on their screens.
        /// </summary>
        private const float MAX_PASSED_TIME = 0.3f;

        /// <summary>
        /// Local client fires weapon.
        /// </summary>
        private void ClientFire()
        {
            Vector3 position = transform.position;
            Vector3 direction = transform.forward;

            /* Spawn locally with 0f passed time.
             * Since this is the firing client
             * they do not need to accelerate/catch up
             * the projectile. */
            SpawnProjectile(position, direction, 0f);
            //Ask server to also fire passing in current Tick.
            ServerFire(position, direction, base.TimeManager.Tick);
        }

        /// <summary>
        /// Spawns a projectile locally.
        /// </summary>
        private void SpawnProjectile(Vector3 position, Vector3 direction, float passedTime)
        {

            PredictedProjectile pp = Instantiate(_projectile, position, Quaternion.identity);
            pp.Initialize(direction, passedTime);
        }

        /// <summary>
        /// Fires on the server.
        /// </summary>
        /// <param name="position">Position to spawn projectile.</param>
        /// <param name="direction">Direction to move projectile.</param>
        /// <param name="tick">Tick when projectile was spawned on client.</param>
        [ServerRpc]
        private void ServerFire(Vector3 position, Vector3 direction, uint tick)
        {
            /* You may want to validate position and direction here.
             * How this is done depends largely upon your game so it
             * won't be covered in this guide. */

            //Get passed time. Note the false for allow negative values.
            float passedTime = (float)base.TimeManager.TimePassed(tick, false);
            /* Cap passed time at half of constant value for the server.
             * In our example max passed time is 300ms, so server value
             * would be max 150ms. This means if it took a client longer
             * than 150ms to send the rpc to the server, the time would
             * be capped to 150ms. This might sound restrictive, but that would
             * mean the client would have roughly a 300ms ping; we do not want
             * to punish other players because a laggy client is firing. */
            passedTime = Mathf.Min(MAX_PASSED_TIME / 2f, passedTime);

            //Spawn on the server.
            SpawnProjectile(position, direction, passedTime);
            //Tell other clients to spawn the projectile.
            ObserversFire(position, direction, tick);
        }

        /// <summary>
        /// Fires on all clients but owner.
        /// </summary>
        [ObserversRpc(ExcludeOwner = true)]
        private void ObserversFire(Vector3 position, Vector3 direction, uint tick)
        {
            //Like on server get the time passed and cap it. Note the false for allow negative values.
            float passedTime = (float)base.TimeManager.TimePassed(tick, false);
            passedTime = Mathf.Min(MAX_PASSED_TIME, passedTime);

            //Spawn the projectile locally.
            SpawnProjectile(position, direction, passedTime);
        }

        /// <summary>
        /// Direction to travel.
        /// </summary>
        private Vector3 _direction;
        private float _passedTime;

        /// <summary>
        /// Distance remaining to catch up. This is calculated from a passed time and move rate.
        /// </summary>
        private float _pasedTime = 0f;
        /// <summary>
        /// In this example the projectile moves at a flat rate of 5f.
        /// </summary>
        private const float MOVE_RATE = 5f;

        /// <summary>
        /// Initializes this projectile.
        /// </summary>
        /// <param name="direction">Direction to travel.</param>
        /// <param name="passedTime">How far in time this projectile is behind te prediction.</param>
        public void Initialize(Vector3 direction, float passedTime)
        {
            _direction = direction;
            _passedTime = passedTime;
        }

        /// <summary>
        /// Move the projectile each frame. This would be called from Update.
        /// </summary>
        private void Move()
        {
            //Frame delta, nothing unusual here.
            float delta = Time.deltaTime;

            //See if to add on additional delta to consume passed time.
            float passedTimeDelta = 0f;
            if (_passedTime > 0f)
            {
                /* Rather than use a flat catch up rate the
                 * extra delta will be based on how much passed time
                 * remains. This means the projectile will accelerate
                 * faster at the beginning and slower at the end.
                 * If a flat rate was used then the projectile
                 * would accelerate at a constant rate, then abruptly
                 * change to normal move rate. This is similar to using
                 * a smooth damp. */

                /* Apply 8% of the step per frame. You can adjust
                 * this number to whatever feels good. */
                float step = (_passedTime * 0.08f);
                _passedTime -= step;

                /* If the remaining time is less than half a delta then
                 * just append it onto the step. The change won't be noticeable. */
                if (_passedTime <= (delta / 2f))
                {
                    step += _passedTime;
                    _passedTime = 0f;
                }
                passedTimeDelta = step;
            }

            //Move the projectile using moverate, delta, and passed time delta.
            transform.position += _direction * (MOVE_RATE * (delta + passedTimeDelta));
        }

        /// <summary>
        /// Handles collision events.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            /* These projectiles are instantiated locally, as in,
             * they are not networked. Because of this there is a very
             * small chance the occasional projectile may not align with
             * 100% accuracy. But, the differences are generally
             * insignifcant and will not affect gameplay. */

            //If client show visual effects, play impact audio.
            if (InstanceFinder.IsClient)
            {
                //Show VFX.
                //Play Audio.
            }

            //If server check to damage hit objects.
            if (InstanceFinder.IsServer)
            {
                //////PlayerShip ps = collision.gameObject.GetComponent<PlayerShip>();

                ///////* If a player ship was hit then remove 50 health.
                ////// * The health value can be synchronized however you like,
                ////// * such as a syncvar. */
                //////if (ps != null)
                //////    ps.Health -= 50f;
            }

            //Destroy projectile (probably pool it instead).
            Destroy(gameObject);
        }
    }
}
