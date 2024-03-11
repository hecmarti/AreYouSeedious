using GGJ.Core;
using UnityEngine;

namespace GGJ.Plants
{
    public class SeedUnlocker : MonoBehaviour
    {

        [SerializeField]
        private PlantItemDefinition seedItemDefinition = default;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.tag.Equals("Player"))
            {
                return;
            }

            GameManager.Instance.Inventory.UnlockSeed(new SeedItem(seedItemDefinition));

            Destroy(gameObject);
        }

    }
}