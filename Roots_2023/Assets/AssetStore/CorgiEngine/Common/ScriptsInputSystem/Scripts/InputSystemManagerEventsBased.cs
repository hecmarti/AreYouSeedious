using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Users;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// This is a replacement InputManager if you prefer using Unity's InputSystem over the legacy one, in a multiplayer context.
    /// It requires to be put on an (ideally empty) game object, accompanied by a PlayerInput component, with Actions bound, and Behavior set to SendMessages
    /// Note that it's not the default solution in the engine at the moment, because older versions of Unity don't support it, 
    /// and most people still prefer not using it
    /// You can see an example of how to set it up in the MinimalLevel_InputSystem_Multiplayer demo scene
    /// </summary>
    public class InputSystemManagerEventsBased : InputManager
    {   
        public void OnPrimaryMovement(InputValue value) { _primaryMovement = value.Get<Vector2>(); }
        public void OnSecondaryMovement(InputValue value) { _primaryMovement = value.Get<Vector2>(); }

        public void OnJump(InputValue value) { BindButton(value, JumpButton); }
        public void OnRun(InputValue value) { BindButton(value, RunButton); }
        public void OnDash(InputValue value) { BindButton(value, DashButton); }
        public void OnShoot(InputValue value) { BindButton(value, ShootButton); }
        public void OnSecondaryShoot(InputValue value) { BindButton(value, SecondaryShootButton); }
        public void OnInteract(InputValue value) { BindButton(value, InteractButton); }
        public void OnReload(InputValue value) { BindButton(value, ReloadButton); }
        public void OnPause(InputValue value) { BindButton(value, PauseButton); }
        public void OnSwitchWeapon(InputValue value) { BindButton(value, SwitchWeaponButton); }
        public void OnSwitchCharacter(InputValue value) { BindButton(value, SwitchCharacterButton); }
        public void OnTimeControl(InputValue value) { BindButton(value, TimeControlButton); }
        public void OnSwim(InputValue value) { BindButton(value, SwimButton); }
        public void OnGlide(InputValue value) { BindButton(value, GlideButton); }
        public void OnJetpack(InputValue value) { BindButton(value, JetpackButton); }
        public void OnFly(InputValue value) { BindButton(value, FlyButton); }
        public void OnGrab(InputValue value) { BindButton(value, GrabButton); }
        public void OnThrow(InputValue value) { BindButton(value, ThrowButton); }
        public void OnPush(InputValue value) { BindButton(value, PushButton); }

        protected virtual void BindButton(InputValue inputValue, MMInput.IMButton imButton)
        {
            if (inputValue.isPressed)
            {
                switch (imButton.State.CurrentState)
                {
                    case MMInput.ButtonStates.Off:
                        imButton.State.ChangeState(MMInput.ButtonStates.ButtonDown);
                        break;
                    case MMInput.ButtonStates.ButtonDown:
                        imButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed);
                        break;
                    case MMInput.ButtonStates.ButtonUp:
                        imButton.State.ChangeState(MMInput.ButtonStates.ButtonDown);
                        break;
                }
            }
            else
            {
                switch (imButton.State.CurrentState)
                {
                    case MMInput.ButtonStates.ButtonPressed:
                        imButton.State.ChangeState(MMInput.ButtonStates.ButtonUp);
                        break;
                    case MMInput.ButtonStates.ButtonUp:
                        imButton.State.ChangeState(MMInput.ButtonStates.Off);
                        break;
                    case MMInput.ButtonStates.ButtonDown:
                        imButton.State.ChangeState(MMInput.ButtonStates.ButtonUp);
                        break;
                }
            }
        }

        protected override void Update()
        {
        }
    }
}


