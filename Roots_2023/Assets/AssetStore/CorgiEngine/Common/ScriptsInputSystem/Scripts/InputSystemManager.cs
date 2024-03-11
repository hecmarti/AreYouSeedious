using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// This is a replacement InputManager if you prefer using Unity's InputSystem over the legacy one.
    /// Note that it's not the default solution in the engine at the moment, because older versions of Unity don't support it, 
    /// and most people still prefer not using it
    /// You can see an example of how to set it up in the MinimalLevel_InputSystem demo scene
    /// </summary>
    public class InputSystemManager : InputManager
    {
        /// a set of input actions to use to read input on
        public CorgiEngineInputActions InputActions;

        protected bool _inputActionsEnabled = true;
        protected bool _initialized = false;
        
        protected override void Start()
        {
            if (!_initialized)
            {
                Initialization();
            }            
        }

        /// <summary>
        /// On init we register to all our actions
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();

            _inputActionsEnabled = true;

            InputActions = new CorgiEngineInputActions();

            InputActions.PlayerControls.PrimaryMovement.performed += context => _primaryMovement = context.ReadValue<Vector2>();
            InputActions.PlayerControls.SecondaryMovement.performed += context => _secondaryMovement = context.ReadValue<Vector2>();

            InputActions.PlayerControls.Jump.performed += context => { BindButton(context, JumpButton); };
            InputActions.PlayerControls.Run.performed += context => { BindButton(context, RunButton); };
            InputActions.PlayerControls.Dash.performed += context => { BindButton(context, DashButton); };
            InputActions.PlayerControls.Shoot.performed += context => { BindButton(context, ShootButton); };
            InputActions.PlayerControls.SecondaryShoot.performed += context => { BindButton(context, SecondaryShootButton); };
            InputActions.PlayerControls.Interact.performed += context => { BindButton(context, InteractButton); };
            InputActions.PlayerControls.Reload.performed += context => { BindButton(context, ReloadButton); };
            InputActions.PlayerControls.Pause.performed += context => { BindButton(context, PauseButton); };
            InputActions.PlayerControls.SwitchWeapon.performed += context => { BindButton(context, SwitchWeaponButton); };
            InputActions.PlayerControls.SwitchCharacter.performed += context => { BindButton(context, SwitchCharacterButton); };
            InputActions.PlayerControls.TimeControl.performed += context => { BindButton(context, TimeControlButton); };
            InputActions.PlayerControls.Roll.performed += context => { BindButton(context, TimeControlButton); };

            InputActions.PlayerControls.Swim.performed += context => { BindButton(context, SwimButton); };
            InputActions.PlayerControls.Glide.performed += context => { BindButton(context, GlideButton); };
            InputActions.PlayerControls.Jetpack.performed += context => { BindButton(context, JetpackButton); };
            InputActions.PlayerControls.Fly.performed += context => { BindButton(context, FlyButton); };
            InputActions.PlayerControls.Grab.performed += context => { BindButton(context, GrabButton); };
            InputActions.PlayerControls.Throw.performed += context => { BindButton(context, ThrowButton); };
            InputActions.PlayerControls.Push.performed += context => { BindButton(context, PushButton); };

            _initialized = true;
        }

        /// <summary>
        /// Changes the state of our button based on the input value
        /// </summary>
        /// <param name="context"></param>
        /// <param name="imButton"></param>
        protected virtual void BindButton(InputAction.CallbackContext context, MMInput.IMButton imButton)
        {
            var control = context.control;

            if (control is ButtonControl button)
            {
                if (button.wasPressedThisFrame)
                {
                    imButton.TriggerButtonDown();
                }
                if (button.wasReleasedThisFrame)
                {
                    imButton.TriggerButtonUp();
                }
            }
        }

        protected override void Update()
        {
            if (IsMobile && _inputActionsEnabled)
            {
                _inputActionsEnabled = false;
                InputActions.Disable();
            }

            if (!IsMobile && (InputDetectionActive != _inputActionsEnabled))
            {
                if (InputDetectionActive)
                {
                    _inputActionsEnabled = true;
                    InputActions.Enable();
                    ForceRefresh();
                }
                else
                {
                    _inputActionsEnabled = false;
                    InputActions.Disable();
                }
            }
        }

        protected virtual void ForceRefresh()
        {
            _primaryMovement = InputActions.PlayerControls.PrimaryMovement.ReadValue<Vector2>();
            _secondaryMovement = InputActions.PlayerControls.SecondaryMovement.ReadValue<Vector2>();
        }

        /// <summary>
        /// On enable we enable our input actions
        /// </summary>
        protected virtual void OnEnable()
        {
            if (!_initialized)
            {
                Initialization();
            }
            InputActions.Enable();
        }

        /// <summary>
        /// On disable we disable our input actions
        /// </summary>
        protected virtual void OnDisable()
        {
            InputActions.Disable();
        }
    }
}