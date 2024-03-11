using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGJ.UI
{
    public class StartScreen : MonoBehaviour
    {
        [SerializeField]
        private List<Image> randomImages;

        [SerializeField]
        private bool showInEditor = false;

        private void Awake()
        {
            if (!showInEditor && Application.isEditor)
            {
                gameObject.SetActive(false);
            }

            int randomNumber = Random.Range(0, 3);
            for(int i = 0; i < randomImages.Count; i++)
            {
                randomImages[i].gameObject.SetActive(i == randomNumber);
            }
        }

    }
}