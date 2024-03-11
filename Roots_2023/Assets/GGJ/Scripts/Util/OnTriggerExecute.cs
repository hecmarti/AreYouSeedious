using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace GGJ.Util
{
    public class OnTriggerExecute : MonoBehaviour
    {
        public LayerMask affectingLayers;

        public UnityEvent OnTriggerEnter;
        public UnityEvent OnTriggerExit;
        public UnityEvent OnTriggerEnterFirstTime;

        private bool enteredFirstTime = false;

        [Header("Input dependant Execution")]
        public KeyCode inputKey = KeyCode.C;
        public UnityEvent OnTriggerEnterAndKeyPressed;

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!affectingLayers.MMContains(collision.gameObject.layer))
            {
                return;
            }

            if (Input.GetKeyDown(inputKey))
            {
                OnTriggerEnterAndKeyPressed?.Invoke();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!affectingLayers.MMContains(collision.gameObject.layer))
            {
                return;
            }

            if (!enteredFirstTime)
            {
                OnTriggerEnterFirstTime?.Invoke();
                enteredFirstTime = true;
            }

            OnTriggerEnter?.Invoke();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!affectingLayers.MMContains(collision.gameObject.layer))
            {
                return;
            }

            OnTriggerExit?.Invoke();
        }
    }
}
