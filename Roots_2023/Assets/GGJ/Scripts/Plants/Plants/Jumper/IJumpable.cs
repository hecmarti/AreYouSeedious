using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ.Plants
{
    public interface IJumpable
    {
        bool CanJump();
        void JumpOnJumper(float force);
    }
}
