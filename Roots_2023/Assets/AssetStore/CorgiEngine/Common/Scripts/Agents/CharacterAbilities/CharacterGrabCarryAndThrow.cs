using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This class lets you grab, carry and throw objects with a GrabCarryAndThrowObject component.
	/// 
	/// Animation parameters :
	/// - Grabbing, boolean, triggered when an object is grabbed
	/// - Carrying : boolean, true if an object is being carried, false otherwise
	/// - CarryingID : int, set to whatever value is set on the carried object 
	/// - Throwing, boolean, triggered when an object gets thrown
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Grab, Carry and Throw")]
	public class CharacterGrabCarryAndThrow : CharacterAbility
	{
		public override string HelpBoxText() { return "This class lets you grab, carry and throw objects with a GrabCarryAndThrowObject component." +
		                                              " In the Grab section you can define how you want the raycast that detects grabbable objects to work, " +
		                                              "in the Carry section you can set an optional child transform to attach carried objects to, and in the Throw section you can define how strong you want " +
		                                              "this Character's throw to be, and how much recoil it should get."; }

		[Header("Grab")]

		/// the direction the raycast used to detect grabbable objects will be cast in (if the Character is facing right). Use Vector3.down for Mario2-like grabs from the top, or Vector3.right 
		/// for side grabs for example.
		[Tooltip("the direction the raycast used to detect grabbable objects will be cast in (if the Character is facing right). Use Vector3.down for Mario2-like grabs from the top, or Vector3.right for side grabs for example")]
		public Vector3 RaycastDirection = Vector3.down;
		/// the distance the grab raycast should cover (you'll want it bigger than half your Character's dimensions
		[Tooltip("the distance the grab raycast should cover (you'll want it bigger than half your Character's dimensions")]
		public float RaycastDistance = 1f;
		/// the layer this grab raycast should look for objects on. This should match the layer you put your GrabCarryAndThrowObjects on
		[Tooltip("the layer this grab raycast should look for objects on. This should match the layer you put your GrabCarryAndThrowObjects on")]
		public LayerMask DetectionLayerMask = LayerManager.PlatformsLayerMask | LayerManager.EnemiesLayerMask;
		/// whether or not this Character is grabbing something right now
		[MMReadOnly]
		[Tooltip("whether or not this Character is grabbing something right now")]
		public bool Grabbing = false;

		[Header("Carry")]

		/// a Transform used to attach carried objects to
		[Tooltip("a Transform used to attach carried objects to")]
		public Transform CarryParent;
		/// whether or not this Character is carrying an object this frame
		[MMReadOnly]
		[Tooltip("whether or not this Character is carrying an object this frame")]
		public bool Carrying = false;
		/// the ID of the object being carried
		[MMReadOnly]
		[Tooltip("the ID of the object being carried")]
		public int CarryingID = -1;
		/// a reference to the object being carried
		[MMReadOnly]
		[Tooltip("a reference to the object being carried")]
		public GrabCarryAndThrowObject CarriedObject = null;

		[Header("Throw")]

		/// the force to apply when throwing
		[Tooltip("the force to apply when throwing")]
		public float ThrowForce = 1f;
		/// a modifier to apply to the recoil set on the object
		[Tooltip("a modifier to apply to the recoil set on the object")]
		public float RecoilModifier = 1f;
		/// whether or not this Character is throwing something this frame
		[MMReadOnly]
		[Tooltip("whether or not this Character is throwing something this frame")]
		public bool Throwing = false;

		protected Vector2 _raycastOrigin;
		protected Vector2 _recoilVector;

		// animation parameters
		protected const string _grabbingAnimationParameterName = "Grabbing";
		protected int _grabbingAnimationParameter;
		protected const string _carryingAnimationParameterName = "Carrying";
		protected int _carryingAnimationParameter;
		protected const string _carryingIDAnimationParameterName = "CarryingID";
		protected int _carryingIDAnimationParameter;
		protected const string _throwingAnimationParameterName = "Throwing";
		protected int _throwingAnimationParameter;
		protected Vector3 _actualRaycastDirection;

		/// <summary>
		/// On init we set our CarryParent to the character transform if null
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			if (CarryParent == null)
			{
				CarryParent = this.transform;
			}
		}
        
		/// <summary>
		/// Looks for throw and grab inputs
		/// </summary>
		protected override void HandleInput()
		{
			if (_inputManager.ThrowButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				if (Carrying)
				{
					Throw();
				}
			}
			if (_inputManager.GrabButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				if (!Carrying)
				{
					GrabAttempt();
				}                
			}
		}

		/// <summary>
		/// Tries to grab by casting a raycast
		/// </summary>
		protected virtual void GrabAttempt()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
            
			_raycastOrigin = this.transform.position;
			_actualRaycastDirection = RaycastDirection;
			if (!_character.IsFacingRight)
			{
				_actualRaycastDirection = _actualRaycastDirection.MMSetX(-RaycastDirection.x);
			}
			RaycastHit2D hit = MMDebug.RayCast(_raycastOrigin, _actualRaycastDirection, RaycastDistance, DetectionLayerMask, Color.blue, _controller.Parameters.DrawRaycastsGizmos);
			if (hit)
			{
				// we make sure we have an object that can be carried
				CarriedObject = hit.collider.gameObject.MMGetComponentNoAlloc<GrabCarryAndThrowObject>();                
			}
			if (CarriedObject != null)
			{
				Grab();
			}
		}

		/// <summary>
		/// Sets the ability in carrying mode
		/// </summary>
		protected virtual void Grab()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
            
			Carrying = true;
			CarryingID = CarriedObject.CarryingAnimationID;
			CarriedObject.Grab(CarryParent);
			Grabbing = true;
			PlayAbilityStartFeedbacks();
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Grab, MMCharacterEvent.Moments.Start);
		}

		/// <summary>
		/// Throws the carried object
		/// </summary>
		protected virtual void Throw()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
            
			if (CarriedObject == null)
			{
				return;
			}

			int direction = _character.IsFacingRight ? 1 : -1;
			CarriedObject.Throw(direction, ThrowForce);

			// apply recoil
			if (RecoilModifier != 0f)
			{
				_recoilVector = (direction == 1) ? Vector2.left : Vector2.right;
				_recoilVector *= RecoilModifier * CarriedObject.Recoil;
				_controller.AddForce(_recoilVector);
			}

			StopFeedbacks();
			CarriedObject = null;
			CarryingID = -1;
			Carrying = false;
			Throwing = true;
		}

		/// <summary>
		/// Stops all feedbacks
		/// </summary>
		protected virtual void StopFeedbacks()
		{
			if (_startFeedbackIsPlaying)
			{
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Grab, MMCharacterEvent.Moments.End);
			}
		}
        
		/// <summary>
		/// On late update we reset our states
		/// </summary>
		protected virtual void LateUpdate()
		{
			Grabbing = false;
			Throwing = false;
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_grabbingAnimationParameterName, AnimatorControllerParameterType.Bool, out _grabbingAnimationParameter);
			RegisterAnimatorParameter(_carryingAnimationParameterName, AnimatorControllerParameterType.Bool, out _carryingAnimationParameter);
			RegisterAnimatorParameter(_carryingIDAnimationParameterName, AnimatorControllerParameterType.Int, out _carryingIDAnimationParameter);
			RegisterAnimatorParameter(_throwingAnimationParameterName, AnimatorControllerParameterType.Bool, out _throwingAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we update our animator parameters with our current state
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _grabbingAnimationParameter, Grabbing, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _throwingAnimationParameter, Throwing, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _carryingAnimationParameter, Carrying, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _carryingIDAnimationParameter, CarryingID, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			Throw();
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _grabbingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _throwingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _carryingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}
	}
}