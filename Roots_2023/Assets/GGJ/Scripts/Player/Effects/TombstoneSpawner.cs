using GGJ.Core;
using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ.Player.Effects
{
    public class TombstoneSpawner : MonoBehaviour
    {

        [SerializeField]
        private Health health;

        [SerializeField]
        private float spawnDelay;

        [SerializeField]
        private GameObject[] tombstones;

        [SerializeField]
        private float appearForce;

        private int lastSpawned = -1;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }
            health.OnDeath += OnDeath;
        }

        private void OnDeath()
        {
            Spawn();
        }

        public void Spawn()
        {
            int newTombstoneId;

            do
            {
                newTombstoneId = Random.Range(0, tombstones.Length);
            } while (tombstones.Length > 1 && newTombstoneId == lastSpawned);

            StartCoroutine(Spawn(newTombstoneId));

            lastSpawned = newTombstoneId;
        }

        private IEnumerator Spawn(int newTombstoneId)
        {
            yield return new WaitForSeconds(spawnDelay);
            
            var newTombstone = Instantiate(tombstones[newTombstoneId]);
            var character = GGJ.Core.GameManager.Instance.LevelManager.CurrentLevel.Character;
            newTombstone.transform.position = character.transform.position;

            var rigidbody = newTombstone.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(Vector3.up * appearForce);
        }
    }

}