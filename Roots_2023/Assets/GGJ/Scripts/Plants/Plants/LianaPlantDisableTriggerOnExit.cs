using System;
using System.Collections;
using UnityEngine;

namespace GGJ.Plants
{

    [RequireComponent(typeof(BoxCollider2D))]
    public class LianaPlantDisableTriggerOnExit : MonoBehaviour
    {

        [SerializeField]
        private float disableTime = 0.5f;
        private BoxCollider2D gripCollider;

        private void Awake()
        {
            gripCollider = GetComponent<BoxCollider2D>();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            OnUngrip();
        }

        private void OnUngrip()
        {
            gripCollider.enabled = false;

            StartCoroutine(EnableAfter(disableTime));
        }

        private IEnumerator EnableAfter(float disableTime)
        {
            yield return new WaitForSeconds(disableTime);

            gripCollider.enabled = true;
        }
    }

}