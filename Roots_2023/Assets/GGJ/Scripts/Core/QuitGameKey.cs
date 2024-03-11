using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ.Core
{
    public class QuitGameKey : MonoBehaviour
    {
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}