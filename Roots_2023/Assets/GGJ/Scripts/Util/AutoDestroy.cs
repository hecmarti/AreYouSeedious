using UnityEngine;

namespace GGJ.Util
{

    public class AutoDestroy : MonoBehaviour
    {

        [SerializeField]
        private float autoDestroySeconds;
        
        void Start()
        {
            Destroy(gameObject, autoDestroySeconds);
        }
    }

}