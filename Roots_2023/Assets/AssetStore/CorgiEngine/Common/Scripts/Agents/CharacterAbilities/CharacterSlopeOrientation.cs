using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this component to a Character and it'll rotate according to the current slope angle.
	/// Animator parameters : none
	/// </summary>
	[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Slope Orientation")] 
	public class CharacterSlopeOrientation : CharacterAbility 
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component will orient the character's model so it is perpendicular to the slope it's walking on. Note that this only works if your model is not on the top level of your character, but instead nested under it."; }
		/// The object to rotate when walking on slopes. A good hierarchy is like so :
		/// - top level : Corgi Controller, collider, character, abilities, etc
		/// - - slope object to rotate
		/// - - - model 
		[Tooltip("The object to rotate when walking on slopes. A good hierarchy is like so :\n" +
		         "- top level : Corgi Controller, collider, character, abilities, etc\n" +
		         "- - slope object to rotate\n" +
		         "- - - model ")]
		public GameObject ObjectToRotate;

		[Header("Rotation")]
		[MMInformation("Here you can define the speed at which the character should rotate to be perpendicular to the slope. 0 means instant rotation, low value is slow, high value is fast, 10 is the default. You can also specify minimum and maximum angles at which your character's rotation will be clamped.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]

		/// the rotation at which to rotate the object
		[Tooltip("the rotation at which to rotate the object")]
		public float CharacterRotationSpeed = 10f;
		/// the minimum angle the rotation will be clamped at
		[Tooltip("the minimum angle the rotation will be clamped at")]
		public float MinimumAllowedAngle = -90f;
		/// the maximum angle the rotation will be clamped at
		[Tooltip("the maximum angle the rotation will be clamped at")]
		public float MaximumAllowedAngle = 90f;
		/// should the rotation be reset when the character jumps
		[Tooltip("should the rotation be reset when the character jumps")]
		public bool ResetAngleInTheAir = true;
		/// should the weapon rotate as well
		[Tooltip("should the weapon rotate as well")]
		public bool RotateWeapon = true;
		/// the slope detection raycast length
		[Tooltip("the slope detection raycast length")]
		public float RaycastLength = 1f;

		protected GameObject _model;
		protected Quaternion _newRotation;
		protected float _currentAngle;
		protected CharacterHandleWeapon _handleWeapon;
		protected WeaponAim _weaponAim;

		protected float _rayLength;
		protected RaycastHit2D _raycastLeft;
		protected RaycastHit2D _raycastMid;
		protected RaycastHit2D _raycastRight;
		protected Vector3 _slopeAngleCross;

		/// <summary>
		/// On Start(), we set our tunnel flag to false
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();

			if (ObjectToRotate != null)
			{
				_model = ObjectToRotate;
			}
			else
			{
				_model = _character.CharacterModel;
			}			

			_handleWeapon = _character?.FindAbility<CharacterHandleWeapon> ();
			if (_handleWeapon != null)
			{
				if (_handleWeapon.CurrentWeapon != null)
				{
					_weaponAim = _handleWeapon.CurrentWeapon.GetComponent<WeaponAim> ();
				}
			}
		}
        
		/// <summary>
		/// Every frame, we check if we're crouched and if we still should be
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();

			// if we don't have a model, we do nothing and exit
			if (_model == null)
			{
				return;
			}

			_currentAngle = DetermineAngle();
            
			if (_characterGravity != null)
			{
				_currentAngle += _characterGravity.GravityAngle;
			}

			// we determine the new rotation
			_newRotation = Quaternion.Euler (_currentAngle * Vector3.forward);

			// if we want instant rotation, we apply it directly
			if (CharacterRotationSpeed == 0)
			{
				_model.transform.rotation = _newRotation;	
			}
			// otherwise we lerp the rotation
			else
			{				
				_model.transform.rotation = Quaternion.Lerp (_model.transform.rotation, _newRotation, CharacterRotationSpeed * Time.deltaTime);
			}

			if ((_weaponAim == null) && (_handleWeapon != null))
			{
				if (_handleWeapon.CurrentWeapon != null)
				{
					_weaponAim = _handleWeapon.CurrentWeapon.GetComponent<WeaponAim>();
				}
			}

			// if we're supposed to also rotate the weapon
			if (RotateWeapon && (_weaponAim != null))
			{
				if (_characterGravity != null)
				{
					_currentAngle -= _characterGravity.GravityAngle;
				}
				_weaponAim.ResetAdditionalAngle();
				_weaponAim.AddAdditionalAngle (_currentAngle);
			}
		}

		/// <summary>
		/// Determines the angle to consider when orientating
		/// </summary>
		/// <returns></returns>
		protected virtual float DetermineAngle()
		{
			float currentAngle;

			_raycastLeft = MMDebug.RayCast(_controller.BoundsLeft, -transform.up, RaycastLength, _controller.PlatformMask, MMColors.LightBlue, _controller.Parameters.DrawRaycastsGizmos);
			_raycastMid = MMDebug.RayCast(_controller.BoundsCenter, -transform.up, RaycastLength, _controller.PlatformMask, MMColors.LightBlue, _controller.Parameters.DrawRaycastsGizmos);
			_raycastRight = MMDebug.RayCast(_controller.BoundsRight, -transform.up, RaycastLength, _controller.PlatformMask, MMColors.LightBlue, _controller.Parameters.DrawRaycastsGizmos);

			float leftAngle = _raycastLeft ? ComputeSlopeAngle(_raycastLeft.normal) : 0f;
			float midAngle = _raycastMid ? ComputeSlopeAngle(_raycastMid.normal) : 0f;
			float rightAngle = _raycastRight ? ComputeSlopeAngle(_raycastRight.normal) : 0f;
			float meanAngle = 0f;

			meanAngle = (leftAngle + midAngle + rightAngle) / 3f;

			currentAngle = meanAngle;

			// if we're in the air and if we should be resetting the angle, we reset it
			if ((!_controller.State.IsGrounded) && ResetAngleInTheAir)
			{
				currentAngle = 0;
			}
            
			// we clamp our angle
			currentAngle = Mathf.Clamp(currentAngle, MinimumAllowedAngle, MaximumAllowedAngle);

			return currentAngle;
		}

		/// <summary>
		/// Computes the angle of the slope
		/// </summary>
		/// <param name="normal"></param>
		/// <returns></returns>
		protected virtual float ComputeSlopeAngle(Vector2 normal)
		{
			float slopeAngle;
			slopeAngle = Vector2.Angle(normal, transform.up);
			_slopeAngleCross = Vector3.Cross(transform.up, normal);
			if (_slopeAngleCross.z < 0)
			{
				slopeAngle = -slopeAngle;
			}
			return slopeAngle;
		}
	}
}