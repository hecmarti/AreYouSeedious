using GGJ.Core;
using GGJ.Plants;
using System;
using System.Linq;
using UnityEngine;

namespace GGJ.Plants
{
    public abstract class SeedBase : MonoBehaviour, ISeed
    {
        [SerializeField]
        private string seedId;
        public string SeedId => seedId;
        public string PlantId { get; internal set; }

        [SerializeField]
        private float lifeTime;

        [SerializeField]
        private GameObject landedParticles;

        [SerializeField]
        private GameObject missParticles;

        [SerializeField]
        private float minMissParticlesCollisionSpeed = 1;

        [SerializeField]
        private float maxMissParticlesCollisionSpeed = 10;

        [Range(0.05f, 1)]
        private float minMissParticlesScale = .3f;

        [Range(0.05f, 1)]
        private float missParticlesMinElapseTime = .5f;

        private float currentLifeTime;
        private float lastMissParticlesPlayTime;
        private bool landParticlesShown;

        private void Update()
        {
            currentLifeTime += Time.deltaTime;

            if (currentLifeTime > lifeTime)
            {
                LifetimeReached();
            }
        }

        protected void LandCollision(Collision2D collision)
        {
            if (landParticlesShown)
            {
                return;
            }

            ShowLandedParticles(collision.contacts.First());
        }

        protected void MissCollision(Collision2D collision)
        {
            if (landParticlesShown)
            {
                return;
            }

            CheckShowMissParticles(collision.contacts.First(), collision.relativeVelocity);
        }

        private void CheckShowMissParticles(ContactPoint2D contactPoint2D, Vector2 relativeVelocity)
        {
            if (Time.time < (lastMissParticlesPlayTime + missParticlesMinElapseTime))
            {
                return;
            }

            float impactVelocity = relativeVelocity.magnitude;
            if (relativeVelocity.magnitude < minMissParticlesCollisionSpeed)
            {
                return;
            }

            GameObject missParticlesGameObbject = SpawnParticles(missParticles, contactPoint2D);
            float scale = Mathf.InverseLerp(minMissParticlesCollisionSpeed, maxMissParticlesCollisionSpeed, impactVelocity);
            missParticlesGameObbject.transform.localScale = Vector3.one * Mathf.Lerp(minMissParticlesScale, 1, scale);

            lastMissParticlesPlayTime = Time.time;
        }

        private void ShowLandedParticles(ContactPoint2D contactPoint2D)
        {
            landParticlesShown = true;

            SpawnParticles(landedParticles, contactPoint2D);
        }

        private GameObject SpawnParticles(GameObject particlesPrefab, ContactPoint2D contactPoint2D)
        {
            GameObject particles = Instantiate(particlesPrefab);
            particles.transform.position = contactPoint2D.point;
            particles.transform.eulerAngles = new Vector3(0, 0, Vector2.Angle(contactPoint2D.normal, Vector2.up));
            return particles;
        }

        public abstract void LifetimeReached();
    }


}