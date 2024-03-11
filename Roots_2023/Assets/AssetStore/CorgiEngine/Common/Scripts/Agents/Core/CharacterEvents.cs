using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// A list of possible events used by the character
	/// </summary>
	public enum MMCharacterEventTypes
	{
		ButtonActivation,
		Jump,
		AbilityNodeSwap,
		Bounce,
		Crouch,
		Crush,
		Dash,
		Dangling,
		Dive,
		FallDamage,
		Fly,
		FollowPath,
		Glide,
		Grab,
		Grip,
		HandleWeapon,
		Jetpack,
		Ladder,
		LedgeHang,
		LookUp,
		Roll,
		Run,
		Stun,
		Swap,
		TimeControl,
		WallCling,
		WallJump
	}

	/// <summary>
	/// MMCharacterEvents are used in addition to the events triggered by the character's state machine, to signal stuff happening that is not necessarily linked to a change of state
	/// </summary>
	public struct MMCharacterEvent
	{
		public enum Moments { OneTime, Start, End }
		
		public Character TargetCharacter;
		public MMCharacterEventTypes EventType;
		public Moments Moment;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.CorgiEngine.MMCharacterEvent"/> struct.
		/// </summary>
		/// <param name="character">Character.</param>
		/// <param name="eventType">Event type.</param>
		public MMCharacterEvent(Character character, MMCharacterEventTypes eventType, Moments moment = Moments.OneTime)
		{
			TargetCharacter = character;
			EventType = eventType;
			Moment = moment;
		}

		static MMCharacterEvent e;
		public static void Trigger(Character character, MMCharacterEventTypes eventType, Moments moment = Moments.OneTime)
		{
			e.TargetCharacter = character;
			e.EventType = eventType;
			e.Moment = moment;
			MMEventManager.TriggerEvent(e);
		}
	} 

	/// <summary>
	/// An event fired when something takes damage
	/// </summary>
	public struct MMDamageTakenEvent
	{
		public Health AffectedHealth;
		public GameObject Instigator;
		public float CurrentHealth;
		public float DamageCaused;
		public float PreviousHealth;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.CorgiEngine.MMDamageTakenEvent"/> struct.
		/// </summary>
		/// <param name="affectedHealth">Affected Health.</param>
		/// <param name="instigator">Instigator.</param>
		/// <param name="currentHealth">Current health.</param>
		/// <param name="damageCaused">Damage caused.</param>
		/// <param name="previousHealth">Previous health.</param>
		public MMDamageTakenEvent(Health affectedHealth, GameObject instigator, float currentHealth, float damageCaused, float previousHealth)
		{
			AffectedHealth = affectedHealth;
			Instigator = instigator;
			CurrentHealth = currentHealth;
			DamageCaused = damageCaused;
			PreviousHealth = previousHealth;
		}

		static MMDamageTakenEvent e;
		public static void Trigger(Health affectedHealth, GameObject instigator, float currentHealth, float damageCaused, float previousHealth)
		{
			e.AffectedHealth = affectedHealth;
			e.Instigator = instigator;
			e.CurrentHealth = currentHealth;
			e.DamageCaused = damageCaused;
			e.PreviousHealth = previousHealth;
			MMEventManager.TriggerEvent(e);
		}
	}
}