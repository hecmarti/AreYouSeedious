using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;

namespace GGJ.Plants.Effects
{
    public class ScaleGrowEffect : MonoBehaviour, IGrowEffect
    {
        [SerializeField]
        private Transform target;
        [SerializeField]
        private float appearDisplacement;
        [SerializeField]
        private float delay;
        [SerializeField]
        private float animTime;

        private void Awake()
        {
            if (target == null)
            {
                target = transform;
            }
        }

        public void Play()
        {
            Vector3 originalPosition = target.localPosition;
            target.localPosition -= transform.up * appearDisplacement;
            target.localScale = Vector3.zero;

            target.DOScale(Vector3.one, animTime).SetEase(Ease.OutBounce).SetDelay(delay);
            target.DOLocalMove(originalPosition, animTime).SetDelay(delay);
        }
    }
}
