using GGJ.Plants;
using MoreMountains.CorgiEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GGJ.Inventory
{
    public delegate void ItemUnlocked(string plantId);
    public delegate void CurrentItemItemUpdated(string plantId);

    public class Inventory : MonoBehaviour
    {
        private readonly List<ISeedItemDefinition> unlockedItems = new List<ISeedItemDefinition>();

        private ISeedItemDefinition currentItem;
        public ISeedItemDefinition CurrentItem => currentItem;

        public event ItemUnlocked OnItemUnlocked;
        public event CurrentItemItemUpdated OnCurrentItemUpdated;

        [Header("Data")]
        [SerializeField]
        private List<PlantItemDefinition> plantsDefinitions = default;

        [Header("UI")]
        [SerializeField]
        private InventoryUIManager inventoryUI = default;

        public InventoryUIManager UI => inventoryUI;

        [Header("Actions")]
        [SerializeField]
        private InputActionAsset inputActionAsset = default;

        [SerializeField]
        private InputActionReference switchInventory = default;

        private CharacterPause characterPause = default;

        private void Awake()
        {
            Initialize(plantsDefinitions);
        }

        private void Initialize(List<PlantItemDefinition> seedDefinitions)
        {
            inventoryUI.InitializeItems(seedDefinitions);

            switchInventory.action.performed += ShowInventory;
            switchInventory.action.canceled += HideInventory;
        }

        private void OnEnable()
        {
            inputActionAsset.Enable();
        }

        private void ShowInventory(InputAction.CallbackContext obj)
        {
            characterPause = FindObjectOfType<CharacterPause>();
            characterPause.PauseCharacter();
            inventoryUI.Show();
        }

        private void HideInventory(InputAction.CallbackContext obj)
        {
            characterPause.UnPauseCharacter();
            inventoryUI.Hide();
        }

        public void UnlockSeed(ISeedItemDefinition item)
        {
            var alreadyUnlockedItem = unlockedItems.FirstOrDefault(i => i.PlantId.Equals(item.PlantId));

            if (alreadyUnlockedItem != null)
            {
                Debug.Log($"{item.PlantId} already unlocked");
                return;
            }

            unlockedItems.Add(item);

            OnItemUnlocked?.Invoke(item.PlantId);

            if (currentItem == null)
            {
                currentItem = item;
                OnCurrentItemUpdated?.Invoke(item.PlantId);
            }
        }

        public void SetCurrentItem(string plantId)
        {
            var unlockedMatchingItem = unlockedItems.FirstOrDefault(item => item.PlantId.Equals(plantId));

            if(unlockedMatchingItem != null) 
            {
                currentItem = unlockedMatchingItem;
            }
        }

        public ISeedItemDefinition TryGet() => currentItem;

        private void OnDisable()
        {
            inputActionAsset.Disable();
        }

    }
}