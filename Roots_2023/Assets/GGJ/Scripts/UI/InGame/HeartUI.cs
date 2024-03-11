using UnityEngine;
using UnityEngine.UI;

namespace GGJ.UI.InGame
{

    
    public class HeartUI : MonoBehaviour
    {

        [SerializeField]
        private Sprite emptyHeartSprite;

        [SerializeField]
        private Sprite fullHeartSprite;

        [SerializeField]
        private Image heartImage;

        public void SetFull()
        {
            heartImage.sprite = fullHeartSprite;
            heartImage.SetNativeSize();
        }

        public void SetEmpty()
        {
            heartImage.sprite = emptyHeartSprite;
            heartImage.SetNativeSize();
        }

    }

}