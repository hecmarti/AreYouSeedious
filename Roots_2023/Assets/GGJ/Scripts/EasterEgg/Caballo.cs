using UnityEngine;

namespace GGJ.EasterEgg
{
    public class Caballo : MonoBehaviour
    {
        public bool moving = false;
        public float speed = 2;

        private void Update()
        {
            if (!moving)
            {
                return;
            }

            transform.position = new Vector3(transform.position.x + speed, transform.position.y, transform.position.z);
        }

        public void SetMoving(bool move)
        {
            moving = move;
        }
    }
}
