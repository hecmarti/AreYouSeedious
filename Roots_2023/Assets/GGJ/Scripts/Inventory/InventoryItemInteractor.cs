using GGJ.Core;
using GGJ.Plants;
using GGJ.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GGJ.Inventory
{
    public class InventoryItemInteractor : MonoBehaviour
    {
        private Inventory inventory;
        private EnergySystem energySystem;

        [Header("Actions")]
        [SerializeField]
        private InputActionAsset inputActionAsset = default;

        [SerializeField]
        private InputActionReference useInventoryItem = default;


        private SeedThrower seedThrower;

        private void Start()
        {
            inventory = GameManager.Instance.Inventory;
            if(inventory.CurrentItem == null)
            {
                inventory.OnCurrentItemUpdated += SubscribeToInputEvents;
            }
            else
            {
                SubscribeToInputEvents(string.Empty);
            }

            energySystem = GameManager.Instance.EnergySystem;
        }

        private void SubscribeToInputEvents(string plantId)
        {
            inventory.OnCurrentItemUpdated -= SubscribeToInputEvents;
            useInventoryItem.action.canceled += OnInventoryUseItem;
        }

        private void OnInventoryUseItem(InputAction.CallbackContext obj)
        {
            Debug.Log("Try use item");

            if (inventory.CurrentItem == null) return;

            var currentItem = inventory.CurrentItem;

            if (!energySystem.CanUseEnergy(currentItem.Cost))
            {
                Debug.LogWarning("Cannot use item. Not enough energy");
                return;
            }

            energySystem.CurrentEnergy -= currentItem.Cost;

            if (currentItem is SeedItem)
            {
                if (seedThrower == null)
                {
                    seedThrower = GameObject.FindObjectOfType<SeedThrower>();
                }

                seedThrower.ThrowSeed(currentItem.SeedId, currentItem.PlantId);
            }
        }

        private void OnEnable() => inputActionAsset.Enable();

        private void OnDisable() => inputActionAsset.Disable();

        private void OnDestroy()
        {
            inventory.OnCurrentItemUpdated -= SubscribeToInputEvents;
            useInventoryItem.action.canceled -= OnInventoryUseItem;
        }
    }
}
