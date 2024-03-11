using DG.Tweening;
using MoreMountains.CorgiEngine;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace GGJ.Enemies
{

    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(CharacterHorizontalMovement))]
    [RequireComponent(typeof(AIWalk))]
    [RequireComponent(typeof(Animator))]
    public class EatPlantAttack : MonoBehaviour
    {

        [SerializeField]
        private bool flipEatPoints = true;

        [SerializeField]
        private float raycastDistance;

        [SerializeField]
        private string aniamtorTriggerName;

        [SerializeField]
        private float eatDelay;
        [SerializeField]
        private float eatAnimTime;

        [SerializeField]
        private Transform swallowPoint;
        private AudioSource audioEffect;
        private Character character;
        private CorgiController corgiController;
        private CharacterHorizontalMovement characterHorizontalMovement;
        private AIWalk aiWalk;

        private Animator animator;
        private Vector3 originalSwallowPoint;
        private Vector3 flippedSwallowPoint;

        private bool isEating;

        private void Awake()
        {
            audioEffect = GetComponent<AudioSource>();
            character = GetComponent<Character>();
            corgiController = GetComponent<CorgiController>();
            characterHorizontalMovement = GetComponent<CharacterHorizontalMovement>();
            aiWalk = GetComponent<AIWalk>();
            
            animator = GetComponent<Animator>();

            if (flipEatPoints)
            {
                originalSwallowPoint = swallowPoint.transform.localPosition;
                flippedSwallowPoint = new Vector3(-swallowPoint.transform.localPosition.x, swallowPoint.transform.localPosition.y, swallowPoint.transform.localPosition.z);
            }
        }

        private void Update()
        {
            if (isEating)
            {
                return;
            }

            swallowPoint.transform.localPosition = character.IsFacingRight ? flippedSwallowPoint : originalSwallowPoint;

            Vector2 direction = character.IsFacingRight ? transform.right : -transform.right;

            Debug.DrawRay(swallowPoint.position, direction * raycastDistance, Color.red);

            RaycastHit2D[] raycastHits = Physics2D.RaycastAll(swallowPoint.position, direction, raycastDistance);

            foreach (var raycastHit in raycastHits)
            {
                if (raycastHit.collider.gameObject.tag == "Plant")
                {
                    Eat(raycastHit.collider.gameObject);
                    break;
                }
            }
        }

        private void Eat(GameObject plant)
        {
            var plantColliders = plant.GetComponentsInChildren<Collider>(true);

            for (int i = plantColliders.Length - 1; i >= 0; i--)
            {
                Destroy(plantColliders[i]);
            }

            StartCoroutine(SwallowCoroutine(plant));
        }

        private IEnumerator SwallowCoroutine(GameObject plant)
        {
            isEating = true;
            corgiController.enabled = false;

            animator.SetTrigger(aniamtorTriggerName);

            audioEffect.Play();

            yield return new WaitForSeconds(eatDelay);

            plant.transform.DOMove(swallowPoint.transform.position, eatAnimTime);
            plant.transform.DOScale(Vector3.one * .5f, eatAnimTime);

            yield return new WaitForSeconds(eatAnimTime);

            Destroy(plant);
            
            corgiController.enabled = true;
            isEating = false;
        }
    }

}