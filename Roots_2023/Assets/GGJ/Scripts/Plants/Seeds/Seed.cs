using GGJ.Core;
using GGJ.Plants;
using System;
using System.Linq;
using UnityEngine;

namespace GGJ.Plants
{
    public class Seed : SeedBase, ISeed
    {
        private bool landed;

        public void Landed(Vector2 plantPosition)
        {
            if (landed)
            {
                return;
            }

            landed = true;

            PlantBase plantPrefab = GetPlantPrefab();
            PlantBase instantiatedPlant = Instantiate(plantPrefab, plantPosition, Quaternion.identity);
            instantiatedPlant.Grow();

            Destroy(gameObject);
        }

        private PlantBase GetPlantPrefab()
        {
            return GameManager.Instance.PlantsPrefabProvider.GetPlantPrefab(PlantId);
        }

        private TryPlantResponse TryPlant(string tag)
        {
            var plantPrefab = GetPlantPrefab();
            Debug.DrawRay(transform.position, plantPrefab.PlacingRayDirection.normalized * plantPrefab.PlacingRayDistance, Color.red, 3f);
            RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, plantPrefab.PlacingRayDirection, plantPrefab.PlacingRayDistance, LayerMask.GetMask("Platforms"));

            if (raycastHit.transform != null && raycastHit.transform.tag == tag)
            {
                return new TryPlantResponse() { isPlaceable = true, placePosition = raycastHit.point };
            }
            return new TryPlantResponse() { isPlaceable = false };
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag != "Fertile")
            {
                MissCollision(collision);
                return;
            }
            TryPlantResponse plantResponse = TryPlant(collision.gameObject.tag);
            if (!plantResponse.isPlaceable)
            {
                MissCollision(collision);
                return;
            }
            Landed(plantResponse.placePosition);
            LandCollision(collision);
        }

        public override void LifetimeReached()
        {
            Destroy(gameObject);
        }
    }
}