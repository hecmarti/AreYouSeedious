using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this class on a character and it'll be able to dash just like the regular dash, and apply damage to everything its DamageOnTouch zone touches
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Damage Dash")]
	public class CharacterDamageDash : CharacterDash
	{
		[Header("Damage Dash")]
		/// the DamageOnTouch object to activate when dashing (usually placed under the Character's model, will require a Collider2D of some form, set to trigger
		[Tooltip("the DamageOnTouch object to activate when dashing (usually placed under the Character's model, will require a Collider2D of some form, set to trigger")]
		public DamageOnTouch TargetDamageOnTouch;
        
		/// <summary>
		/// On initialization, we disable our damage on touch object
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			TargetDamageOnTouch?.gameObject.SetActive(false);
		}

		/// <summary>
		/// When we start to dash, we activate our damage object
		/// </summary>
		public override void InitiateDash()
		{
			base.InitiateDash();
			TargetDamageOnTouch?.gameObject.SetActive(true);
		}

		/// <summary>
		/// When we stop dashing, we disable our damage object
		/// </summary>
		public override void StopDash()
		{
			base.StopDash();
			TargetDamageOnTouch?.gameObject.SetActive(false);
		}
	}
}