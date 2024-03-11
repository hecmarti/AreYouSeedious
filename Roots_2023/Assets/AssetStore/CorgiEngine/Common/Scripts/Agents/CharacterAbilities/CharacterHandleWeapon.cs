using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to a character so it can use weapons
	/// Note that this component will trigger animations (if their parameter is present in the Animator), based on 
	/// the current weapon's Animations
	/// Animator parameters : defined from the Weapon's inspector
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Handle Weapon")] 
	public class CharacterHandleWeapon : CharacterAbility 
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component will allow your character to pickup and use weapons. What the weapon will do is defined in the Weapon classes. This just describes the behaviour of the 'hand' holding the weapon, not the weapon itself. Here you can set an initial weapon for your character to start with, allow weapon pickup, and specify a weapon attachment (a transform inside of your character, could be just an empty child gameobject, or a subpart of your model."; }

		[Header("Weapon")]

		/// the initial weapon owned by the character
		[Tooltip("the initial weapon owned by the character")]
		public Weapon InitialWeapon;
		/// if this is set to true, the character can pick up PickableWeapons
		[Tooltip("if this is set to true, the character can pick up PickableWeapons")]
		public bool CanPickupWeapons = true;

		[Header("Binding")]

		/// the position the weapon will be attached to. If left blank, will be this.transform.
		[Tooltip("the position the weapon will be attached to. If left blank, will be this.transform.")]
		public Transform WeaponAttachment;
		/// if this is true, the weapon's scale will be forced to 1,1,1 when equipped
		[Tooltip("if this is true, the weapon's scale will be forced to 1,1,1 when equipped")]
		public bool ForceWeaponScaleResetOnEquip = false;
		/// if this is true, the weapon's rotation will be forced to Identity when equipped
		[Tooltip("if this is true, the weapon's rotation will be forced to Identity when equipped")]
		public bool ForceWeaponRotationResetOnEquip = false;
		/// if this is true this animator will be automatically bound to the weapon 
		[Tooltip("if this is true this animator will be automatically bound to the weapon")]
		public bool AutomaticallyBindAnimator = true;
		/// the ID of the AmmoDisplay this ability should update
		[Tooltip("the ID of the AmmoDisplay this ability should update")]
		public int AmmoDisplayID = 0;

		[Header("Input and automation")]

		/// if this is true you won't have to release your fire button to auto reload
		[Tooltip("if this is true you won't have to release your fire button to auto reload")]
		public bool ContinuousPress = false;
		/// whether or not this character getting hit should interrupt its attack (will only work if the weapon is marked as interruptable)
		[Tooltip("whether or not this character getting hit should interrupt its attack (will only work if the weapon is marked as interruptable)")]
		public bool GettingHitInterruptsAttack = false;
		/// whether or not this character is allowed to shoot while on a ladder)
		[Tooltip("whether or not this character is allowed to shoot while on a ladder")]
		public bool CanShootFromLadders = false;
		/// if this is set to true, the character will be forced to face the current weapon direction
		[Tooltip("if this is set to true, the character will be forced to face the current weapon direction")]
		public bool FaceWeaponDirection = false;
		/// if this is true, horizontal aim will be inverted when shooting while wallclinging, to shoot away from the wall
		[Tooltip("if this is true, horizontal aim will be inverted when shooting while wallclinging, to shoot away from the wall")] 
		public bool InvertHorizontalAimWhenWallclinging = false;

		[Header("Buffering")]

		/// whether or not attack input should be buffered, letting you prepare an attack while another is being performed, making it easier to chain them
		[Tooltip("whether or not attack input should be buffered, letting you prepare an attack while another is being performed, making it easier to chain them")]
		public bool BufferInput;
		/// if this is true, every new input will prolong the buffer
		[MMCondition("BufferInput", true)]
		[Tooltip("if this is true, every new input will prolong the buffer")]
		public bool NewInputExtendsBuffer;
		/// the maximum duration for the buffer, in seconds
		[MMCondition("BufferInput", true)]
		[Tooltip("the maximum duration for the buffer, in seconds")]
		public float MaximumBufferDuration = 0.25f;
        
		[Header("Debug")]
		/// returns the current equipped weapon
		[MMReadOnly]
		[Tooltip("returns the current equipped weapon")]
		public Weapon CurrentWeapon;

		/// the ID / index of this CharacterHandleWeapon. This will be used to determine what handle weapon ability should equip a weapon.
		/// If you create more Handle Weapon abilities, make sure to override and increment this  
		public virtual int HandleWeaponID { get { return 1; } }

		public Animator CharacterAnimator { get; set; }

		protected float _fireTimer = 0f;
		protected float _secondaryHorizontalMovement;
		protected float _secondaryVerticalMovement;
		protected WeaponAim _aimableWeapon;
		protected WeaponIK _weaponIK;
		protected Transform _leftHandTarget = null;
		protected Transform _rightHandTarget = null;

		protected float _bufferEndsAt = 0f;
		protected bool _buffering = false;
		protected bool _charHztlMvmtFlipInitialSetting;
		protected bool _charHztlMvmtFlipInitialSettingSet = false;
		protected Vector2 _invertedHorizontalAimMultiplier = new Vector2(-1f, 1f);

		// Initialization
		protected override void Initialization () 
		{
			base.Initialization();
			if (_characterHorizontalMovement != null)
			{
				_charHztlMvmtFlipInitialSetting = _characterHorizontalMovement.FlipCharacterToFaceDirection;
			}
			Setup ();
		}

		/// <summary>
		/// Grabs various components and inits stuff
		/// </summary>
		public virtual void Setup()
		{
			_character = gameObject.GetComponentInParent<Character>();
			CharacterAnimator = _animator;
            
			// filler if the WeaponAttachment has not been set
			if (WeaponAttachment==null)
			{
				WeaponAttachment=transform;
			}		
			if (_animator != null)
			{
				_weaponIK = _animator.GetComponent<WeaponIK> ();
			}	
			// we set the initial weapon
			if (InitialWeapon != null)
			{
				ChangeWeapon(InitialWeapon, null);
			}
		}

		/// <summary>
		/// Every frame we check if it's needed to update the ammo display
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility ();
			UpdateAmmoDisplay (); 
			HandleBuffer();
			HandleFacingDirection();
			HandleWeaponStop();
		}

		/// <summary>
		/// Checks for state changes to trigger stop feedbacks
		/// </summary>
		protected virtual void HandleWeaponStop()
		{
			if (CurrentWeapon == null)
			{
				return;
			}

			if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponStop)
			{
				PlayAbilityStopFeedbacks();	
			}
		}

		/// <summary>
		/// If FaceWeaponDirection is true, will force the character to face the weapon direction
		/// </summary>
		protected virtual void HandleFacingDirection()
		{
			if ((_characterHorizontalMovement != null) && FaceWeaponDirection && (_aimableWeapon != null))
			{
				_characterHorizontalMovement.FlipCharacterToFaceDirection = false;
				_charHztlMvmtFlipInitialSettingSet = true;
			}

			if (_charHztlMvmtFlipInitialSettingSet && (_aimableWeapon == null))
			{
				_characterHorizontalMovement.FlipCharacterToFaceDirection = _charHztlMvmtFlipInitialSetting;
			}
            
			if (InvertHorizontalAimWhenWallclinging && (_aimableWeapon != null) && (_movement.CurrentState == CharacterStates.MovementStates.WallClinging))
			{
				_aimableWeapon.CurrentAimMultiplier = _invertedHorizontalAimMultiplier;
			}

			// if we're not in FaceWeaponDirection mode, if we don't have a HztalMvmt ability, or a weapon aim, we do nothing and exit
			if (!FaceWeaponDirection || (_characterHorizontalMovement == null) || (_aimableWeapon == null))
			{
				return;
			}

			if ((_aimableWeapon.CurrentAngleRelative < -90f) || (_aimableWeapon.CurrentAngleRelative > 90f))
			{
				_character.Flip();
			}
		}

		/// <summary>
		/// Gets input and triggers methods based on what's been pressed
		/// </summary>
		protected override void HandleInput ()
		{			

			if ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonDown))
			{
				ShootStart();
			}

			if (CurrentWeapon != null)
			{
				if (ContinuousPress && (CurrentWeapon.TriggerMode == Weapon.TriggerModes.Auto) && (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed))
				{
					ShootStart();
				}
				if (ContinuousPress && (CurrentWeapon.TriggerMode == Weapon.TriggerModes.Auto) && (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonPressed))
				{
					ShootStart();
				}
			}			

			if (_inputManager.ReloadButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				Reload();
			}

			if ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonUp) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonUp))
			{
				ShootStop();
			}

			if (CurrentWeapon != null)
			{
				if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses)
				    && ((_inputManager.ShootAxis == MMInput.ButtonStates.Off) && (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.Off)))
				{
					CurrentWeapon.WeaponInputStop();
				}
			}            
		}

		/// <summary>
		/// Triggers an attack if the weapon is idle and an input has been buffered
		/// </summary>
		protected virtual void HandleBuffer()
		{
			if (CurrentWeapon == null)
			{
				return;
			}

			// if we are currently buffering an input and if the weapon is now idle
			if (_buffering && (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle))
			{
				// and if our buffer is still valid, we trigger an attack
				if (Time.time < _bufferEndsAt)
				{
					ShootStart();
				}
				_buffering = false;
			}
		}
						
		/// <summary>
		/// Causes the character to start shooting
		/// </summary>
		public virtual void ShootStart()
		{
			// if the Shoot action is enabled in the permissions, we continue, if not we do nothing.  If the player is dead we do nothing.
			if ( !AbilityAuthorized
			     || (CurrentWeapon == null)
			     || ((_condition.CurrentState != CharacterStates.CharacterConditions.Normal) && (_condition.CurrentState != CharacterStates.CharacterConditions.ControlledMovement)))
			{
				return;
			}

			if (!CanShootFromLadders && (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing))
			{
				return;
			}

			//  if we've decided to buffer input, and if the weapon is in use right now
			if (BufferInput && (CurrentWeapon.WeaponState.CurrentState != Weapon.WeaponStates.WeaponIdle))
			{
				// if we're not already buffering, or if each new input extends the buffer, we turn our buffering state to true
				if (!_buffering || NewInputExtendsBuffer)
				{
					_buffering = true;
					_bufferEndsAt = Time.time + MaximumBufferDuration;
				}
			}

			PlayAbilityStartFeedbacks();
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.HandleWeapon, MMCharacterEvent.Moments.Start);
			CurrentWeapon.WeaponInputStart();
		}
		
		/// <summary>
		/// Causes the character to stop shooting
		/// </summary>
		public virtual void ShootStop()
		{
			// if the Shoot action is enabled in the permissions, we continue, if not we do nothing
			if (!AbilityAuthorized
			    || (CurrentWeapon == null)
			    || (_movement == null))
			{
				return;		
			}		

			if (!CanShootFromLadders && _movement.CurrentState == CharacterStates.MovementStates.LadderClimbing && CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle)
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload)
			    || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart)
			    || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop)
			    || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse))
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse) && (!CurrentWeapon.DelayBeforeUseReleaseInterruption))
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses) && (!CurrentWeapon.TimeBetweenUsesReleaseInterruption))
			{
				return;
			}

			StopStartFeedbacks();
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.HandleWeapon, MMCharacterEvent.Moments.End);
			CurrentWeapon.TurnWeaponOff();
		}
		
		/// <summary>
		/// A method used (usually by AIs) to force the weapon to stop
		/// </summary>
		public virtual void ForceStop()
		{
			StopStartFeedbacks();
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.HandleWeapon, MMCharacterEvent.Moments.End);
			CurrentWeapon?.TurnWeaponOff();
		}

		/// <summary>
		/// Reloads the weapon
		/// </summary>
		public virtual void Reload()
		{
			if (CurrentWeapon != null)
			{
				CurrentWeapon.InitiateReloadWeapon ();
			}
		}
		
		/// <summary>
		/// Changes the character's current weapon to the one passed as a parameter
		/// </summary>
		/// <param name="newWeapon">The new weapon.</param>
		public virtual void ChangeWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{
			// if the character already has a weapon, we make it stop shooting
			if (CurrentWeapon != null)
			{
				CurrentWeapon.ResetComboAnimatorParameter();

				if (!combo)
				{
					ShootStop();
					if (_character._animator != null)
					{
						AnimatorControllerParameter[] parameters = _character._animator.parameters;
						foreach(AnimatorControllerParameter parameter in parameters)
						{
							if (parameter.name == CurrentWeapon.EquippedAnimationParameter)
							{
								MMAnimatorExtensions.UpdateAnimatorBool(_animator, CurrentWeapon.EquippedAnimationParameter, false);
							}
						}
					}

					Destroy(CurrentWeapon.gameObject);
				}
			}
            
			if (newWeapon != null)
			{			
				if (!combo)
				{
					CurrentWeapon = (Weapon)Instantiate(newWeapon, WeaponAttachment.transform.position + newWeapon.WeaponAttachmentOffset, Quaternion.identity);
				}				
				CurrentWeapon.transform.SetParent(WeaponAttachment.transform);
				if (ForceWeaponScaleResetOnEquip)
				{
					CurrentWeapon.transform.localScale = Vector3.one;
				}
				if (ForceWeaponRotationResetOnEquip)
				{
					CurrentWeapon.transform.localRotation = Quaternion.identity;    
				}
                
				CurrentWeapon.SetOwner (_character, this);
				CurrentWeapon.WeaponID = weaponID;
				_aimableWeapon = CurrentWeapon.GetComponent<WeaponAim> ();
				// we handle (optional) inverse kinematics (IK) 
				if (_weaponIK != null)
				{
					_weaponIK.SetHandles(CurrentWeapon.LeftHandHandle, CurrentWeapon.RightHandHandle);
				}
				// we turn off the gun's emitters.
				CurrentWeapon.Initialization();
				CurrentWeapon.InitializeComboWeapons();
				CurrentWeapon.InitializeAnimatorParameters();
				InitializeAnimatorParameters();
				if ((_character != null) && !combo)
				{
					if (!_character.IsFacingRight)
					{
						if (CurrentWeapon != null)
						{
							CurrentWeapon.FlipWeapon();
							CurrentWeapon.FlipWeaponModel();
						}
					}
				}				
			}
			else
			{
				CurrentWeapon = null;
			}
		}	

		/// <summary>
		/// Flips the current weapon if needed
		/// </summary>
		public override void Flip()
		{
			if (CurrentWeapon != null)
			{
				CurrentWeapon.FlipWeapon();
				if (CurrentWeapon.FlipWeaponOnCharacterFlip)
				{
					CurrentWeapon.FlipWeaponModel();
				}
			}
		}

		/// <summary>
		/// Updates the ammo display bar and text.
		/// </summary>
		public virtual void UpdateAmmoDisplay()
		{
			if ( (GUIManager.HasInstance) && (_character.CharacterType == Character.CharacterTypes.Player) )
			{
				if (CurrentWeapon == null)
				{
					GUIManager.Instance.SetAmmoDisplays (false, _character.PlayerID, AmmoDisplayID);
					return;
				}

				if (!CurrentWeapon.MagazineBased && (CurrentWeapon.WeaponAmmo == null))
				{
					GUIManager.Instance.SetAmmoDisplays (false, _character.PlayerID, AmmoDisplayID);
					return;
				}

				if (CurrentWeapon.WeaponAmmo == null)
				{					
					GUIManager.Instance.SetAmmoDisplays (true, _character.PlayerID, AmmoDisplayID);
					GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, 0, 0, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, _character.PlayerID, AmmoDisplayID, false);	
					return;
				}
				else
				{
					GUIManager.Instance.SetAmmoDisplays (true, _character.PlayerID, AmmoDisplayID);
					GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, CurrentWeapon.WeaponAmmo.CurrentAmmoAvailable + CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.WeaponAmmo.MaxAmmo, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, _character.PlayerID, AmmoDisplayID, true);
					return;
				}
			}
		}
		
		/// <summary>
		/// On respawn we setup our weapon again
		/// </summary>
		protected override void OnRespawn()
		{
			base.OnRespawn();
			Setup();
		}
        
		/// <summary>
		/// On hit we interrupt our weapon if needed
		/// </summary>
		protected override void OnHit()
		{
			base.OnHit();
			if (GettingHitInterruptsAttack && (CurrentWeapon != null))
			{
				CurrentWeapon.Interrupt();
			}
		}

		/// <summary>
		/// On death we stop shooting if needed
		/// </summary>
		protected override void OnDeath()
		{
			base.OnDeath();
			ShootStop();
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			if (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
			{
				ShootStop();
			}
		}
	}
}