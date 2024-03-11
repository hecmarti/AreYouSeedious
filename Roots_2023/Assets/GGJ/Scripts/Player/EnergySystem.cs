using UnityEngine;

namespace GGJ.Player
{

    public class EnergySystem : MonoBehaviour, IEnergySystem
    {
        [SerializeField]
        private int currentEnergy = 10;
        public int CurrentEnergy { get => currentEnergy; set => currentEnergy = value; }

        public bool CanUseEnergy(int energy) => energy < CurrentEnergy;
    }

}