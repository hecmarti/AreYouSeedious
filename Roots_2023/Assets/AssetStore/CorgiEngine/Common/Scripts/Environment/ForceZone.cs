using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this zone to a trigger collider 2D and it'll let you apply the specified force to any Corgi Controller that enters it
	/// </summary>
	public class ForceZone : CorgiMonoBehaviour
	{
		/// whether to add a force to controllers inside the zone, or to set the force
		public enum ForceModes { AddForce, SetForce }
		/// whether the force should be applied on enter, on stay or on exit. If on stay, the force will be multiplied by delta time
		public enum ApplicationModes { TriggerEnter, TriggerStay, TriggerExit }
        
		[Header("Initialization")]
		/// if this is true, the zone will force its collider to true on awake
		[Tooltip("if this is true, the zone will force its collider to true on awake")]
		public bool AutoTriggerSetup = true;

		[Header("Force")] 
        
		/// whether to add a force to controllers inside the zone, or to set the force
		[Tooltip("whether to add a force to controllers inside the zone, or to set the force")]
		public ApplicationModes ApplicationMode = ApplicationModes.TriggerEnter;
		/// whether the force should be applied on enter, on stay or on exit. If on stay, the force will be multiplied by delta time
		[Tooltip("whether the force should be applied on enter, on stay or on exit. If on stay, the force will be multiplied by delta time")]
		public ForceModes ForceMode = ForceModes.AddForce;

		/// the amount of force to add to a CorgiController walking over this surface
		[Tooltip("the amount of force to add to a CorgiController walking over this surface")]
		public Vector2 AddedForce = Vector2.zero;
		/// the cooldown to apply (in seconds) between two force applications, on x and y forces respectively
		[Tooltip("the cooldown to apply (in seconds) between two force applications, on x and y forces respectively")]
		public Vector2 ForceApplicationCooldownDuration = new Vector2(0f, 0.25f);
		/// whether or not the zone should reset forces on the controller (on entry or exit only)
		[Tooltip("whether or not the zone should reset forces on the controller (on entry or exit only)")]
		public bool ResetForces = true;

		protected Collider2D _collider2D;
		protected CorgiController _controller;
		protected Vector2 _lastForceAppliedAt = Vector2.zero;

		/// <summary>
		/// On Awake we initialize our zone
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// On init, we force our collider's trigger settings to true if needed
		/// </summary>
		protected virtual void Initialization()
		{
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			if (AutoTriggerSetup)
			{
				_collider2D.isTrigger = true;
			}
		}

		/// <summary>
		/// When something triggers with our zone, we apply force
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			if (ApplicationMode == ApplicationModes.TriggerEnter)
			{
				HandleForce(collider);    
			}
		}
        
		protected virtual void OnTriggerExit2D(Collider2D collider)
		{
			if (ApplicationMode == ApplicationModes.TriggerExit)
			{
				HandleForce(collider);    
			}
		}
        
		protected virtual void OnTriggerStay2D(Collider2D collider)
		{
			if (ApplicationMode == ApplicationModes.TriggerStay)
			{
				HandleForce(collider);    
			}
		}

		/// <summary>
		/// Makes sure we have a controller, resets forces if needed, applies horizontal and vertical force if needed
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void HandleForce(Collider2D collider)
		{
			_controller = collider.gameObject.MMGetComponentNoAlloc<CorgiController>();
			if (_controller == null)
			{
				return;
			}

			// reset forces if needed
			if (ResetForces && (ApplicationMode != ApplicationModes.TriggerStay))
			{
				_controller.SetForce(Vector2.zero);
			}
            
			// horizontal force
			if (Time.time - _lastForceAppliedAt.x > ForceApplicationCooldownDuration.x)
			{
				float forceX = AddedForce.x;

				if (ApplicationMode == ApplicationModes.TriggerStay)
				{
					forceX *= Time.deltaTime;
				}

				if (ForceMode == ForceModes.AddForce)
				{
					_controller.AddHorizontalForce(forceX);    
				}
				else
				{
					_controller.SetHorizontalForce(forceX);
				}
                
				_lastForceAppliedAt.x = Time.time;
			}

			// vertical force
			if (Time.time - _lastForceAppliedAt.y > ForceApplicationCooldownDuration.y)
			{
				float forceY = AddedForce.y;

				if (ApplicationMode == ApplicationModes.TriggerStay)
				{
					forceY *= Time.deltaTime;
				}

				if (ForceMode == ForceModes.AddForce)
				{
					_controller.AddVerticalForce(forceY);    
				}
				else
				{
					_controller.SetVerticalForce(forceY);
				}
				_lastForceAppliedAt.y = Time.time;
			}
		}
	}
}