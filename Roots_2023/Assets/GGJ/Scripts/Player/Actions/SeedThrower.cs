using GGJ.Plants;
using GGJ.Player.Aim;
using MoreMountains.CorgiEngine;
using UnityEngine;


namespace GGJ.Player
{
    public class SeedThrower : MonoBehaviour
    {
        [Header("Seed prefab")]
        [SerializeField]
        private float instantiationDistance;

        [Header("Throwing settings")]
        [SerializeField]
        private float throwForce = 15f;

        [SerializeField]
        private SeedThrowAim aim;

        [SerializeField]
        private float randomTorqueMargin;

        private Character character;
        private AudioSource thowSound;

        [SerializeField]
        private float cooldownTime = 0.5f;

        private float lastShootTime;

        private void Start()
        {
            character = GetComponentInParent<Character>();
            thowSound = GetComponent<AudioSource>();
        }

        public void ThrowSeed(string seedId, string plantId)
        {
            if (!CanShootSeed())
            {
                return;
            }

            Vector2 aimDirection = (Vector3)aim.AimDirectionAndForce;
            Vector2 instantiationPosition = (Vector2)transform.position + aimDirection.normalized * instantiationDistance;

            SeedBase seedPrefab = GGJ.Core.GameManager.Instance.PlantsPrefabProvider.GetSeedPrefab(seedId);
            SeedBase seed = Instantiate<SeedBase>(seedPrefab, instantiationPosition, Quaternion.identity);
            seed.PlantId = plantId;

            Rigidbody2D seedRigidbody = seed.GetComponent<Rigidbody2D>();
            float direction = character.IsFacingRight ? 1f : -1f;

            seedRigidbody.AddForce(aimDirection * throwForce, ForceMode2D.Impulse);
            seedRigidbody.AddTorque(Random.Range(-randomTorqueMargin, randomTorqueMargin));

            thowSound.Play();

            lastShootTime = Time.time;
        }

        private bool CanShootSeed()
        {
            return (lastShootTime + cooldownTime) < Time.time;
        }
    }
}