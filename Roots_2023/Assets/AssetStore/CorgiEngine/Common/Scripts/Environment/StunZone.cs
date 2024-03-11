using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A stun zone will stun any character with a CharacterStun ability entering it
	/// </summary>
	public class StunZone : CorgiMonoBehaviour
	{
		/// the possible stun modes : Forever : stuns until StunExit is called on the CharacterStun component, ForDuration : stuns for a duration, and then the character will exit stun on its own
		public enum StunModes { Forever, ForDuration }

		[Header("Stun Zone")]
		// the layers that will be stunned by this object
		[Tooltip("the layers that will be stunned by this object")]
		public LayerMask TargetLayerMask;
		/// the chosen stun mode (Forever : stuns until StunExit is called on the CharacterStun component, ForDuration : stuns for a duration, and then the character will exit stun on its own)
		[Tooltip("the chosen stun mode (Forever : stuns until StunExit is called on the CharacterStun component, ForDuration : stuns for a duration, and then the character will exit stun on its own)")] 
		public StunModes StunMode = StunModes.ForDuration;
		/// if in ForDuration mode, the duration of the stun in seconds
		[Tooltip("if in ForDuration mode, the duration of the stun in seconds")]
		[MMEnumCondition("StunMode", (int)StunModes.ForDuration)]
		public float StunDuration = 2f;
		/// whether or not to disable the zone after the stun has happened
		[Tooltip("whether or not to disable the zone after the stun has happened")]
		public bool DisableZoneOnStun = true;
        
		[Header("Auto Disable")]
		/// if this is true, the zone will be disabled after the duration has passed
		[Tooltip("if this is true, the zone will be disabled after the duration has passed")]
		public bool AutoDisable = true;
		/// the duration (in seconds) before the zone auto disables itself
		[Tooltip("the duration (in seconds) before the zone auto disables itself")]
		[MMCondition("AutoDisable", true)]
		public float AutoDisableDuration = 0.3f;

		protected Character _character;
		protected CharacterStun _characterStun;

		/// <summary>
		/// On enable, we start our disable countdown if necessary
		/// </summary>
		protected void OnEnable()
		{
			if (AutoDisable)
			{
				StartCoroutine(AutoDisableCo());
			}
		}

		/// <summary>
		/// A coroutine used to disable the zone after a certain duration
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator AutoDisableCo()
		{
			yield return MMCoroutine.WaitFor(AutoDisableDuration);
			this.gameObject.SetActive(false);
		}

		/// <summary>
		/// When colliding with a gameobject, we make sure it's a target, and if yes, we stun it
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void Colliding(GameObject collider)
		{
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return;
			}

			_character = collider.GetComponent<Character>();
			if (_character != null) { _characterStun = _character.FindAbility<CharacterStun>(); }

			if (_characterStun == null)
			{
				return;
			}
            
			if (StunMode == StunModes.ForDuration)
			{
				_characterStun.StunFor(StunDuration);
			}
			else
			{
				_characterStun.Stun();
			}
            
			if (DisableZoneOnStun)
			{
				this.gameObject.SetActive(false);
			}
		}
        
		/// <summary>
		/// When a collision with the player is triggered, we give damage to the player and knock it back
		/// </summary>
		/// <param name="collider">what's colliding with the object.</param>
		public virtual void OnTriggerStay2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger enter 2D, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>S
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}
	}    
}