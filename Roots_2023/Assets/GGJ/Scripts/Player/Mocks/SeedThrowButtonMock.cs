using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ.Player.Mock
{
    [RequireComponent(typeof(SeedThrower))]
    public class SeedThrowButtonMock : MonoBehaviour
    {
        [SerializeField]
        private KeyCode throwKey;


        private void Update()
        {
            if (Input.GetKeyDown(throwKey))
            {
                GetComponent<SeedThrower>().ThrowSeed("example", "example");
            }
        }
    }
}