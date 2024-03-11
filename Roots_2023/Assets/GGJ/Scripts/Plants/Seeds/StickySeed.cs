using GGJ.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ.Plants
{
    public class StickySeed : SeedBase, ISeed
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.otherCollider.gameObject.layer == LayerMask.GetMask("Platforms"))
            {
                return;
            }

            PlantBase plantPrefab = GetPlantPrefab();

            TryPlantResponse plantResponse = TryPlant(plantPrefab);
            if (!plantResponse.isPlaceable)
            {
                return;
            }
            Landed(plantPrefab, plantResponse);
        }

        public void Landed(PlantBase plantPrefab, TryPlantResponse tryPlantResponse)
        {
            PlantBase instantiatedPlant = Instantiate(plantPrefab, tryPlantResponse.placePosition, tryPlantResponse.placeRotation);
            instantiatedPlant.Grow();

            Destroy(gameObject);
        }

        private TryPlantResponse TryPlant(PlantBase plantPrefab)
        {
            var rightCheck = CheckPlace(plantPrefab, Vector2.right);
            var leftCheck = CheckPlace(plantPrefab, Vector2.left);

            if (rightCheck.isPlaceable)
            {
                rightCheck.placeRotation = Quaternion.Euler(0f, 0f, -90f);
                return rightCheck;
            }
            else
            {
                leftCheck.placeRotation = Quaternion.Euler(0f, 0f, 90f);
                return leftCheck;
            }
        }

        private TryPlantResponse CheckPlace(PlantBase plantPrefab, Vector2 checkDirection)
        {
            Debug.DrawRay(transform.position, checkDirection * plantPrefab.PlacingRayDistance, Color.red, 3f);
            RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, checkDirection, plantPrefab.PlacingRayDistance, LayerMask.GetMask("Platforms"));

            if (raycastHit.collider != null && (string.IsNullOrEmpty(plantPrefab.TargetTag) || raycastHit.transform.tag == plantPrefab.TargetTag))
            {
                return new TryPlantResponse() { isPlaceable = true, placePosition = raycastHit.point };
            }
            return new TryPlantResponse() { isPlaceable = false };
        }

        private PlantBase GetPlantPrefab()
        {
            return GameManager.Instance.PlantsPrefabProvider.GetPlantPrefab(PlantId);
        }

        public override void LifetimeReached()
        {
            Destroy(gameObject);
        }
    }
}