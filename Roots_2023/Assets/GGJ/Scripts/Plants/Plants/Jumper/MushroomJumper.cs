using UnityEngine;

namespace GGJ.Plants
{
    public class MushroomJumper : MonoBehaviour
    {
        [SerializeField]
        private LayerMask layerThatCanJumpOn;

        [SerializeField]
        public Animator animator;

        [SerializeField]
        private bool canBeActivatedFromBelow;

        [SerializeField]
        private float JumpPlatformBoost = 40;

        private AudioSource soundEffect;

        private void Awake()
        {
            soundEffect = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((GetComponent<Collider2D>().transform.position.y < this.transform.position.y) && !canBeActivatedFromBelow)
            {
                return;
            }

            var jumpable = collision.gameObject.GetComponent<IJumpable>();
            if(jumpable == null || !jumpable.CanJump())
            {
                return;
            }

            jumpable.JumpOnJumper(JumpPlatformBoost);

            soundEffect.Play();
            animator.SetTrigger("JumpOn");
        }
    }
}
