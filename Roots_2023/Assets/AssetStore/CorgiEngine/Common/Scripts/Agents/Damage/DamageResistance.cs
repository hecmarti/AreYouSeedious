using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Used by the DamageResistanceProcessor, this class defines the resistance versus a certain type of damage. 
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Health/Damage Resistance")]
	public class DamageResistance : CorgiMonoBehaviour
	{
		public enum DamageModifierModes { Multiplier, Flat }

		[Header("General")]
		/// The priority of this damage resistance. This will be used to determine in what order damage resistances should be evaluated. Lowest priority means evaluated first.
		[Tooltip("The priority of this damage resistance. This will be used to determine in what order damage resistances should be evaluated. Lowest priority means evaluated first.")]
		public float Priority = 0;
		/// The label of this damage resistance. Used for organization, and to activate/disactivate a resistance by its label.
		[Tooltip("The label of this damage resistance. Used for organization, and to activate/disactivate a resistance by its label.")]
		public string Label = "";
		
		[Header("Damage Resistance Settings")]
		/// Whether this resistance impacts base damage or typed damage
		[Tooltip("Whether this resistance impacts base damage or typed damage")]
		public DamageTypeModes DamageTypeMode = DamageTypeModes.BaseDamage;
		/// In TypedDamage mode, the type of damage this resistance will interact with
		[Tooltip("In TypedDamage mode, the type of damage this resistance will interact with")]
		[MMEnumCondition("DamageTypeMode", (int)DamageTypeModes.TypedDamage)]
		public DamageType TypeResistance;
		/// the way to reduce (or increase) received damage. Multiplier will multiply incoming damage by a multiplier, flat will subtract a constant value from incoming damage. 
		[Tooltip("the way to reduce (or increase) received damage. Multiplier will multiply incoming damage by a multiplier, flat will subtract a constant value from incoming damage.")]
		public DamageModifierModes DamageModifierMode = DamageModifierModes.Multiplier;

		[Header("Damage Modifiers")]
		/// In multiplier mode, the multiplier to apply to incoming damage. 0.5 will reduce it in half, while a value of 2 will create a weakness to the specified damage type, and damages will double.
		[Tooltip("In multiplier mode, the multiplier to apply to incoming damage. 0.5 will reduce it in half, while a value of 2 will create a weakness to the specified damage type, and damages will double.")]
		[MMEnumCondition("DamageModifierMode", (int)DamageModifierModes.Multiplier)]
		public float DamageMultiplier = 0.25f;
		/// In flat mode, the amount of damage to subtract every time that type of damage is received
		[Tooltip("In flat mode, the amount of damage to subtract every time that type of damage is received")]
		[MMEnumCondition("DamageModifierMode", (int)DamageModifierModes.Flat)]
		public float FlatDamageReduction = 10f;
		/// whether or not incoming damage of the specified type should be clamped between a min and a max
		[Tooltip("whether or not incoming damage of the specified type should be clamped between a min and a max")] 
		public bool ClampDamage = false;
		/// the values between which to clamp incoming damage
		[Tooltip("the values between which to clamp incoming damage")]
		[MMVector("Min","Max")]
		public Vector2 DamageModifierClamps = new Vector2(0f,10f);

		[Header("Condition Change")]
		/// whether or not condition change for that type of damage is allowed or not
		[Tooltip("whether or not condition change for that type of damage is allowed or not")]
		public bool PreventCharacterConditionChange = false;
		/// whether or not movement modifiers are allowed for that type of damage or not
		[Tooltip("whether or not movement modifiers are allowed for that type of damage or not")]
		public bool PreventMovementModifier = false;

		[Header("Feedbacks")]
		/// This feedback will only be triggered if damage of the matching type is received
		[Tooltip("This feedback will only be triggered if damage of the matching type is received")]
		public MMFeedbacks OnDamageReceived;
		/// whether or not this feedback can be interrupted (stopped) when that type of damage is interrupted
		[Tooltip("whether or not this feedback can be interrupted (stopped) when that type of damage is interrupted")]
		public bool InterruptibleFeedback = false;
		/// if this is true, the feedback will always be preventively stopped before playing
		[Tooltip("if this is true, the feedback will always be preventively stopped before playing")]
		public bool AlwaysInterruptFeedbackBeforePlay = false;
		/// whether this feedback should play if damage received is zero
		[Tooltip("whether this feedback should play if damage received is zero")]
		public bool TriggerFeedbackIfDamageIsZero = false;

		/// <summary>
		/// On awake we initialize our feedback
		/// </summary>
		protected virtual void Awake()
		{
			OnDamageReceived?.Initialization(this.gameObject);
		}
		
		/// <summary>
		/// When getting damage, goes through damage reduction and outputs the resulting damage
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="type"></param>
		/// <param name="damageApplied"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public virtual float ProcessDamage(float damage, DamageType type, bool damageApplied)
		{
			if (!this.gameObject.activeInHierarchy)
			{
				return damage;
			}
			
			if ((type == null) && (DamageTypeMode != DamageTypeModes.BaseDamage))
			{
				return damage;
			}

			if ((type != null) && (DamageTypeMode == DamageTypeModes.BaseDamage))
			{
				return damage;
			}

			if ((type != null) && (type != TypeResistance))
			{
				return damage;
			}
			
			// applies damage modifier or reduction
			switch (DamageModifierMode)
			{
				case DamageModifierModes.Multiplier:
					damage = damage * DamageMultiplier;
					break;
				case DamageModifierModes.Flat:
					damage = damage - FlatDamageReduction;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			// clamps damage
			damage = ClampDamage ? Mathf.Clamp(damage, DamageModifierClamps.x, DamageModifierClamps.y) : damage;

			if (damageApplied)
			{
				if (!TriggerFeedbackIfDamageIsZero && (damage == 0))
				{
					// do nothing
				}
				else
				{
					if (AlwaysInterruptFeedbackBeforePlay)
					{
						OnDamageReceived?.StopFeedbacks();
					}
					OnDamageReceived?.PlayFeedbacks(this.transform.position);	
				}
			}

			return damage;
		}
	}
}