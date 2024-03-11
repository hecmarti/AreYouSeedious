using GGJ.Plants;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ.Player
{
    public class CharacterJumperAction : MonoBehaviour, IJumpable
    {
        [SerializeField]
        private CorgiController _controller;

        private CharacterJump _characterJump;

        [SerializeField]
        private bool PreventJumpIfCharacterIsGrounded = true;

        public bool CanJump()
        {
            if (PreventJumpIfCharacterIsGrounded && (_controller.State.IsGrounded) && (_controller.State.WasGroundedLastFrame))
            {
                return false;
            }

            return true;
        }

        public void JumpOnJumper(float force)
        {
            _controller.SetVerticalForce(Mathf.Sqrt(2f * force * -_controller.Parameters.Gravity));
            _characterJump = _controller.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterJump>();
            if (_characterJump != null)
            {
                _characterJump.SetCanJumpStop(false);
                _characterJump.SetJumpFlags();
            }
        }
    }
}
