using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// A struct used to store targets for the SurfaceModifier class
	/// </summary>
	[System.Serializable]
	public class SurfaceModifierTarget
	{
		public CorgiController TargetController;
		public Character TargetCharacter;
		public float LastForceAppliedAtX;
		public float LastForceAppliedAtY;
		public bool TargetAffectedBySurfaceModifier;
	}

	/// <summary>
	/// Add this component to a platform and define its new friction or force which will be applied to any CorgiController that walks on it
	/// </summary>
	[AddComponentMenu("Corgi Engine/Environment/Surface Modifier")]
	public class SurfaceModifier : CorgiMonoBehaviour 
	{
		/// the possible modes this surface can apply forces
		public enum ForceApplicationModes { AddForce, SetForce }

		[Header("Friction")]
		[MMInformation("Set a friction between 0.01 and 0.99 to get a slippery surface (close to 0 is very slippery, close to 1 is less slippery).\nOr set it above 1 to get a sticky surface. The higher the value, the stickier the surface.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]

		/// the amount of friction to apply to a CorgiController walking over this surface		
		[Tooltip("the amount of friction to apply to a CorgiController walking over this surface")]
		public float Friction;

		[Header("Force")]
		[MMInformation("Use these to add X or Y (or both) forces to any CorgiController that gets grounded on this surface. " +
		               "Adding a X force will create a treadmill (negative value > treadmill to the left, positive value > treadmill to the right). " +
		               "A positive y value will create a trampoline, a bouncy surface, or a jumper for example." +
		               "Furthermore, you can define cooldowns between two force applications. It's better to have one when applying vertical forces, to increase consistency.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

		/// the chosen mode to use to apply force. Add will add to existing forces, Set will force that specific value, ignoring current forces.
		[Tooltip("the chosen mode to use to apply force. Add will add to existing forces, Set will force that specific value, ignoring current forces.")]
		public ForceApplicationModes ForceApplicationMode = ForceApplicationModes.AddForce;
		/// the amount of force to add to a CorgiController walking over this surface
		[Tooltip("the amount of force to add to a CorgiController walking over this surface")]
		public Vector2 AddedForce = Vector2.zero;
		/// if this is true, the force will only be applied if the colliding character is grounded
		[Tooltip("if this is true, the force will only be applied if the colliding character is grounded")]
		public bool OnlyApplyForceIfGrounded = false;
		/// the cooldown to apply (in seconds) between two force applications, on x and y forces respectively
		[Tooltip("the cooldown to apply (in seconds) between two force applications, on x and y forces respectively")]
		public Vector2 ForceApplicationCooldownDuration = new Vector2(0f, 0.25f);
		/// whether or not to reset forces when an object exits the surface, useful when moving inactive objects
		[Tooltip("whether or not to reset forces when an object exits the surface, useful when moving inactive objects")]
		public bool ResetForcesOnExit = false;
		/// a list of all the targets currently impacted by this surface modifier
		public List<SurfaceModifierTarget> _targets { get; set; }
        
		protected CorgiController _controller;
		protected Character _character;

		/// <summary>
		/// On awake we initialize our list of targets
		/// </summary>
		protected virtual void Awake()
		{
			_targets = new List<SurfaceModifierTarget>();
		}

		/// <summary>
		/// Triggered when a CorgiController collides with the surface
		/// </summary>
		/// <param name="collider">Collider.</param>
		public virtual void OnTriggerStay2D(Collider2D collider)
		{
			_controller = collider.gameObject.MMGetComponentNoAlloc<CorgiController>();
			_character = collider.gameObject.MMGetComponentNoAlloc<Character>();

			if (_controller == null)
			{
				return;
			}
			_controller.CurrentSurfaceModifier = this;

			bool found = false;
			foreach(SurfaceModifierTarget target in _targets)
			{
				if (target.TargetController == _controller)
				{
					found = true;
					target.TargetAffectedBySurfaceModifier = true;
				}
			}
			if (!found)
			{
				SurfaceModifierTarget newSurfaceModifierTarget = new SurfaceModifierTarget();
				newSurfaceModifierTarget.TargetController = _controller;
				newSurfaceModifierTarget.TargetCharacter = _character;
				newSurfaceModifierTarget.TargetAffectedBySurfaceModifier = true;
				_targets.Add(newSurfaceModifierTarget);
			}
		}

		/// <summary>
		/// On trigger exit, we lose all reference to the controller and character
		/// </summary>
		/// <param name="collision"></param>
		protected virtual void OnTriggerExit2D(Collider2D collider)
		{
			_controller = collider.gameObject.MMGetComponentNoAlloc<CorgiController>();
			_character = collider.gameObject.MMGetComponentNoAlloc<Character>();

			if (_controller == null)
			{
				return;
			}

			bool found = false;
			int index = 0;
			int counter = 0;
			foreach (SurfaceModifierTarget target in _targets)
			{
				if (target.TargetController == _controller)
				{
					index = counter;
					found = true;
				}
				counter++;
			}
			if (found)
			{
				if (ResetForcesOnExit)
				{
					_controller.SetForce(Vector2.zero);
				}
				_targets[index].TargetAffectedBySurfaceModifier = false;
			}
            
			_controller.CurrentSurfaceModifier = null;
		}

		/// <summary>
		/// On Update, we make sure we have a controller and a live character, and if we do, we apply a force to it
		/// </summary>
		protected virtual void Update()
		{
			ProcessSurface();
		}

		/// <summary>
		/// A method used to go through all targets and apply force if needed
		/// </summary>
		protected virtual void ProcessSurface()
		{
			if (_targets.Count == 0)
			{
				return;
			}

			bool removeNeeded = false;
			int counter = 0;
			int removeIndex = 0;
			foreach(SurfaceModifierTarget target in _targets)
			{
				if (!target.TargetAffectedBySurfaceModifier)
				{
					continue;
				}
                
				_controller = target.TargetController;
				_character = target.TargetCharacter;

				if ((_character != null) && (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead))
				{
					removeNeeded = true;
					removeIndex = counter;
					_character = null;
					_controller = null;
					continue;
				}

				if (ForceApplicationConditionsMet())
				{
					ApplyHorizontalForce(target);
					ApplyVerticalForce(target);    
				}
			}            

			if (removeNeeded)
			{
				_targets.RemoveAt(removeIndex);
			}
		}

		/// <summary>
		/// This method returns true if conditions to apply force are met
		/// </summary>
		/// <returns></returns>
		protected virtual bool ForceApplicationConditionsMet()
		{
			if (_controller == null)
			{
				return false;
			}

			if (OnlyApplyForceIfGrounded && !_controller.State.IsGrounded)
			{
				return false;
			}

			return true; 
		}

		/// <summary>
		/// This method applies horizontal forces if needed
		/// </summary>
		/// <param name="target"></param>
		protected virtual void ApplyHorizontalForce(SurfaceModifierTarget target)
		{
			if (Time.time - target.LastForceAppliedAtX > ForceApplicationCooldownDuration.x)
			{
				if (ForceApplicationMode == ForceApplicationModes.AddForce)
				{
					_controller.AddHorizontalForce(AddedForce.x * 10f * Time.deltaTime);
				}
				else
				{
					_controller.SetHorizontalForce(AddedForce.x);
				}
				target.LastForceAppliedAtX = Time.time;
			}  
		}

		/// <summary>
		/// This method applies vertical forces if needed
		/// </summary>
		/// <param name="target"></param>
		protected virtual void ApplyVerticalForce(SurfaceModifierTarget target)
		{
			if (Time.time - target.LastForceAppliedAtY > ForceApplicationCooldownDuration.y)
			{
				if (ForceApplicationMode == ForceApplicationModes.AddForce)
				{
					float verticalForce = Mathf.Sqrt(2f * AddedForce.y * -_controller.Parameters.Gravity);
					_controller.AddVerticalForce(verticalForce);
				}
				else
				{
					_controller.SetVerticalForce(AddedForce.y);
				}
				target.LastForceAppliedAtY = Time.time;
			}
		}
	}
}