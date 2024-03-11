using UnityEngine;

namespace GGJ.Plants
{
    public class ObjectJumpable : MonoBehaviour, IJumpable
    {
        [SerializeField]
        private Rigidbody2D rigidbody2D;

        [SerializeField]
        private float addedForce = 100f;

        public bool CanJump()
        {
            //PARA CASOS QUE NO TENGA QUE SALTAR
            return true;
        }

        public void JumpOnJumper(float force)
        {
            force *= -addedForce;
            rigidbody2D.AddForce(2f * force * Physics2D.gravity);
        }
    }
}
