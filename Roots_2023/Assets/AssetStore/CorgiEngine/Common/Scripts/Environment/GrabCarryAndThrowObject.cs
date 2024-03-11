using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this component to an object and it'll become carryable by a Character with the appropriate ability (CharacterGrabCarryAndThrow)
	/// </summary>
	[AddComponentMenu("Corgi Engine/Environment/Grab Carry and Throw Object")]
	public class GrabCarryAndThrowObject : CorgiMonoBehaviour
	{
		/// The possible ways this object can be carried :
		/// - Parent : the object gets parented to the Character, optionnally under a specified transform
		/// - Follow : the object uses an additional MMFollowTarget component (you'll need to add it) to smoothly follow the Character
		public enum CarryMethods { Parent, Follow }

		[Header("Carry")]

		/// the selected carry method
		[Tooltip("the selected carry method")]
		public CarryMethods CarryMethod = CarryMethods.Parent;
		/// the offset to apply when attaching the object to the character
		[Tooltip("the offset to apply when attaching the object to the character")]
		public Vector3 CarryOffset = Vector3.zero;
		/// the mask the object should be moved to while carried
		[Tooltip("the mask the object should be moved to while carried")]
		public string CarryLayerMask = "Projectiles";
		/// an ID that will get passed to the Character's animator when this object is carried. Use it to differentiate objects to get different carry animations
		[Tooltip("an ID that will get passed to the Character's animator when this object is carried. Use it to differentiate objects to get different carry animations")]
		public int CarryingAnimationID = 0;
		/// whether this object is being carried this frame or not
		[Tooltip("whether this object is being carried this frame or not")]
		public bool Carried = false;

		[Header("Throw")]

		/// the direction the object should be thrown in if the Character is facing right (x will be reversed for left facing throws)
		[Tooltip("the direction the object should be thrown in if the Character is facing right (x will be reversed for left facing throws)")]
		public Vector2 ThrowDirection = new Vector2(1f, 0.25f);
		/// the force at which this object should be thrown
		[Tooltip("the force at which this object should be thrown")]
		public float Force = 1f;
		/// the force mode to apply when throwing
		[Tooltip("the force mode to apply when throwing")]
		public ForceMode2D ForceMode = ForceMode2D.Impulse;
		/// the cooldown (in seconds) after which to reset the layer and scale of our object after a throw
		[Tooltip("the cooldown (in seconds) after which to reset the layer and scale of our object after a throw")]
		public float ThrowColliderCooldown = 0.2f;
		/// the recoil to apply to the throwing Character after a throw
		[Tooltip("the recoil to apply to the throwing Character after a throw")]
		public float Recoil = 0f;

		[Header("Events")]
		/// a UnityEvent triggered when the object gets grabbed
		[Tooltip("a UnityEvent triggered when the object gets grabbed")]
		public UnityEvent OnGrabbed;
		/// a UnityEvent triggered when the object gets thrown
		[Tooltip("a UnityEvent triggered when the object gets thrown")]
		public UnityEvent OnThrown;

		protected Rigidbody2D _rigidbody2D;
		protected Vector2 _throwVector;
		protected WaitForSeconds _throwColliderCooldownWaitForSeconds;
		protected int _originalLayer;
		protected Vector3 _originalScale;
		protected MMFollowTarget _followTarget;

		/// <summary>
		/// On Awake we initialize our object
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs and stores components
		/// </summary>
		protected virtual void Initialization()
		{
			_rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
			_throwColliderCooldownWaitForSeconds = new WaitForSeconds(ThrowColliderCooldown);
			_followTarget = this.gameObject.GetComponent<MMFollowTarget>();
		}

		/// <summary>
		/// Triggered when this objects starts being carried
		/// Attaches to the grabber, and sets state
		/// </summary>
		/// <param name="targetParent"></param>
		public virtual void Grab(Transform targetParent)
		{
			_originalScale = this.transform.localScale;
			_originalLayer = this.gameObject.layer;

			switch (CarryMethod)
			{
				case CarryMethods.Parent:
					this.transform.SetParent(targetParent);
					this.transform.localPosition = CarryOffset;
					break;
				case CarryMethods.Follow:
					_followTarget.Target = targetParent;
					_followTarget.Initialization();
					_followTarget.StartFollowing();
					break;
			}

			this.gameObject.layer = LayerMask.NameToLayer(CarryLayerMask);
			if (_rigidbody2D != null)
			{
				_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
			}
			
			OnGrabbed?.Invoke();

			Carried = true;
		}

		/// <summary>
		/// Triggered when this object gets thrown
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="forceMultiplier"></param>
		public virtual void Throw(int direction, float forceMultiplier)
		{
			StartCoroutine(ResetCollisions());

			switch (CarryMethod)
			{
				case CarryMethods.Parent:
					this.transform.SetParent(null);
					break;
				case CarryMethods.Follow:
					_followTarget.Target = null;
					_followTarget.StopFollowing();
					break;
			}

			_throwVector = ThrowDirection.normalized;
			if (direction < 0)
			{
				_throwVector.x *= -1f;
			}
			_throwVector = _throwVector * Force * forceMultiplier;

			if (_rigidbody2D != null)
			{
				_rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
				_rigidbody2D.AddForce(_throwVector, ForceMode);
			}
			
			OnThrown?.Invoke();

			Carried = false;
		}  
        
		/// <summary>
		/// An internal coroutine used to reset collisions after a throw
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ResetCollisions()
		{
			yield return _throwColliderCooldownWaitForSeconds;
			this.gameObject.layer = _originalLayer;
			this.transform.localScale = _originalScale;
		}
	}
}