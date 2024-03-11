using GGJ.Levels;
using GGJ.Plants;
using GGJ.Player;
using MoreMountains.CorgiEngine;
using System;
using UnityEngine;

namespace GGJ.Core
{

    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private Inventory.Inventory inventory;
        [SerializeField]
        private PlantsPrefabProvider plantsPrefabProvider;

        [SerializeField]
        private EnergySystem energySystem;
        public EnergySystem EnergySystem => energySystem;

        [SerializeField]
        private Levels.LevelManager levelManager;

        public Inventory.Inventory Inventory { get { return inventory; } }
        public PlantsPrefabProvider PlantsPrefabProvider => plantsPrefabProvider;
        public Levels.LevelManager LevelManager => levelManager;

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            Instance = this;
        }

    }

}