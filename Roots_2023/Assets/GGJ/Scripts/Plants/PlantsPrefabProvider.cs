using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GGJ.Plants
{
    public class PlantsPrefabProvider : MonoBehaviour
    {
        [SerializeField]
        private List<PlantBase> plantsPrefabList;
        [SerializeField]
        private List<SeedBase> seedPrefabList;

        public PlantBase GetPlantPrefab(string plantId)
        {
            return plantsPrefabList.FirstOrDefault(plant => plant.PlantId == plantId);
        }

        public SeedBase GetSeedPrefab(string seedId)
        {
            return seedPrefabList.FirstOrDefault(seedPrefab => seedPrefab.SeedId == seedId);
        }
    }
}