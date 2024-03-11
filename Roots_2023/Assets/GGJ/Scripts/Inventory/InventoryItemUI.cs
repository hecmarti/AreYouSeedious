using GGJ.Plants;
using UnityEngine;
using UnityEngine.UI;

namespace GGJ.Inventory
{
    public class InventoryItemUI : MonoBehaviour
    {
        [SerializeField]
        private int lockedTransparency = 127;

        [SerializeField]
        private Image plantImage = default;

        [SerializeField]
        private Image background = default;

        [SerializeField]
        private Image selectedOutline = default;

        [SerializeField]
        private GameObject lockedIndicator = default;


        private Sprite plantSprite;
        public Sprite PlantSprite => plantSprite;

        private bool isLocked = true;
        private bool isSelected = false;

        public bool IsLocked => isLocked;
        public bool IsSelected => isSelected;

        private void Awake()
        {
            lockedIndicator.SetActive(true);
            plantImage.gameObject.SetActive(false);
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0.5f);
            selectedOutline.gameObject.SetActive(false);
        }

        public void Init(PlantItemDefinition definition)
        {
            plantImage.sprite = plantSprite = definition.PlantSprite;
        }

        public void SetLocked(bool isLocked)
        {
            this.isLocked = isLocked;
            lockedIndicator.SetActive(isLocked);

            if (!isLocked)
            {
                plantImage.gameObject.SetActive(true);
                background.color = new Color(background.color.r, background.color.g, background.color.b, 1f);
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (isLocked) return;

            this.isSelected = isSelected;

            selectedOutline.gameObject.SetActive(isSelected);
        }
    }
}
