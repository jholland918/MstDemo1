﻿using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Assets.App.Scripts.Character
{
    public class PredictedProjectile : MonoBehaviour
    {
        /// <summary>
        /// Direction to travel.
        /// </summary>
        private Vector3 _direction;

        /// <summary>
        /// Distance remaining to catch up. This is calculated from a passed time and move rate.
        /// </summary>
        private float _passedTime = 0f;

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

        private void Update()
        {
            Move();
            Destroy(gameObject, 5f);
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

        // https://stackoverflow.com/questions/18281385/oncollisionenter-not-working-in-unity3d
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
                Debug.Log("*** damage hit objects ***");
                PlayerCharacterVitals playerVitals = collision.gameObject.GetComponentInParent<PlayerCharacterVitals>();

                /* If a player was hit then remove 2 health.
                 * The health value can be synchronized however you like,
                 * such as a syncvar. */
                if (playerVitals != null)
                {
                    playerVitals.TakeDamage(2);
                }
            }

            //Destroy projectile (probably pool it instead).
            Destroy(gameObject);
        }
    }
}
