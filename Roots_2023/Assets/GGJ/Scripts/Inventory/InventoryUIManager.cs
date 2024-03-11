using GGJ.Plants;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GGJ.Inventory
{
    public class InventoryItemUIData
    {
        public float StartingAngle { get; set; }
        public float EndingAngle { get; set; }
        public string PlantId { get; set; }
        public InventoryItemUI Ui { get; set; }
    }

    public class InventoryUIManager : MonoBehaviour
    {
        [SerializeField]
        private Inventory inventory = default;

        [SerializeField]
        private GameObject parent = default;

        [SerializeField]
        private Transform pointingArrow = default;

        [SerializeField]
        private InventoryItemUI inventoryItemPrefab = default;

        private readonly List<InventoryItemUIData> inventoryItemsUI = new List<InventoryItemUIData>();
        private InventoryItemUIData currentAimingItemData = default;

        [SerializeField]
        private Image currentSelectedItemImage = default;

        [SerializeField]
        private InputActionReference mouseAimAction = default;

        [SerializeField]
        private float radius = 1f;

        private bool isUiInitialized = false;

        public bool IsVisible { get; private set; }

        private void Awake()
        {
            inventory.OnItemUnlocked += UnlockItemOnPicked;
            inventory.OnCurrentItemUpdated += CurrentItemUpdated;
            currentSelectedItemImage.gameObject.SetActive(false);

            parent.SetActive(false);
        }

        private void Update()
        {
            CalculateSelectedItem();
        }

        private void CurrentItemUpdated(string plantId)
        {
            var itemUIData = inventoryItemsUI.FirstOrDefault(i => !i.Ui.IsLocked && i.PlantId.Equals(plantId));

            if (currentAimingItemData == null)
            {
                currentAimingItemData = itemUIData;
                currentSelectedItemImage.gameObject.SetActive(true);
                currentSelectedItemImage.sprite = currentAimingItemData.Ui.PlantSprite;
            }
        }

        private void ReleaseMouse(InputAction.CallbackContext obj) => UpdateCurrentAimingItemData();

        private void UpdateCurrentAimingItemData()
        {
            if (!isUiInitialized) return;

            if (currentAimingItemData == null) return;

            currentAimingItemData.Ui.SetSelected(true);
            currentSelectedItemImage.gameObject.SetActive(true);
            currentSelectedItemImage.sprite = currentAimingItemData.Ui.PlantSprite;
            inventory.SetCurrentItem(currentAimingItemData.PlantId);
        }

        private void CalculateSelectedItem()
        {
            if (!isUiInitialized) return;

            var unlockedAimedItem = CalculateAimedItem();

            if (unlockedAimedItem == null) return;

            if (currentAimingItemData != null) currentAimingItemData.Ui.SetSelected(false);

            currentAimingItemData = unlockedAimedItem;

            currentAimingItemData.Ui.SetSelected(true);
        }

        private InventoryItemUIData CalculateAimedItem()
        {
            Vector2 aimActionValue = mouseAimAction.action.ReadValue<Vector2>();

            Vector2 direction;

            if (aimActionValue.magnitude > .05f)
            {
                direction = aimActionValue;
            }
            else
            {
                aimActionValue = Input.mousePosition;
                var screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
                direction = aimActionValue - screenCenter;
            }

            float aimingAngle = -Vector2.SignedAngle(direction, Vector2.up) + 90;

            pointingArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, aimingAngle));

            while (aimingAngle < 0) aimingAngle += 360;
            while (aimingAngle > 360) aimingAngle -= 360;

            var item = inventoryItemsUI.FirstOrDefault(i => !i.Ui.IsLocked && aimingAngle >= i.StartingAngle && aimingAngle <= i.EndingAngle);

            return item;
        }


        public void Show()
        {
            //mouseAimAction.action.performed += CalculateSelectedItem;
            mouseAimAction.action.canceled += ReleaseMouse;
            parent.SetActive(true);
            IsVisible = true;
        }

        public void Hide()
        {
            UpdateCurrentAimingItemData();
            //mouseAimAction.action.performed -= CalculateSelectedItem;
            mouseAimAction.action.canceled -= ReleaseMouse;
            parent.SetActive(false);
            IsVisible = false;
        }

        private void UnlockItemOnPicked(string plantId)
        {
            if (!isUiInitialized) return;

            var unlockedPlantId = inventoryItemsUI.FirstOrDefault(i => i.PlantId.Equals(plantId));

            if (unlockedPlantId == null)
            {
                Debug.LogError($"Cannot unlock {plantId} plant");
                return;
            }

            unlockedPlantId.Ui.SetLocked(false);
        }

        public void InitializeItems(List<PlantItemDefinition> seeds)
        {
            var angle = 360 / seeds.Count;

            var accumulatedAngle = 0;

            foreach (var seed in seeds)
            {
                var direction = new Vector2(Mathf.Cos((accumulatedAngle + angle / 2) * Mathf.Deg2Rad), Mathf.Sin((accumulatedAngle + angle / 2) * Mathf.Deg2Rad));

                //var newPosition = screenCenter + (direction * radius);

                var createdGameobject = Instantiate(inventoryItemPrefab);
                createdGameobject.transform.SetParent(parent.transform);
                createdGameobject.transform.SetAsLastSibling();

                ((RectTransform)createdGameobject.transform).anchoredPosition = direction * radius;

                var newItemUI = new InventoryItemUIData
                {
                    PlantId = seed.PlantId,
                    Ui = createdGameobject,
                    StartingAngle = accumulatedAngle,
                    EndingAngle = accumulatedAngle + angle
                };

                Debug.Log($"Created element START:{newItemUI.StartingAngle}, END: {newItemUI.EndingAngle}");

                newItemUI.Ui.Init(seed);

                inventoryItemsUI.Add(newItemUI);

                accumulatedAngle += angle;
            }

            isUiInitialized = true;
        }

        private void RedrawItems()
        {
            var angle = 360 / inventoryItemsUI.Count;
            var accumulatedAngle = 0;

            foreach (var itemUI in inventoryItemsUI)
            {
                accumulatedAngle += angle;



            }
        }
    }
}
