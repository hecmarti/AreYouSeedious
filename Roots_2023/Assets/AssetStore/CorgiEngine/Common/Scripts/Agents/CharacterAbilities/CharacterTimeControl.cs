using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to a character and it'll be able to slow down time when pressing down the TimeControl button
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Time Control")] 
	public class CharacterTimeControl : CharacterAbility
	{
		[Header("Time Control")]
		[MMInformation("This ability lets a character alter the timescale when pressing down the TimeControl button.", MMInformationAttribute.InformationType.Info, false)]

		/// the time scale to switch to when the time control button gets pressed
		[Tooltip("the time scale to switch to when the time control button gets pressed")]
		public float TimeScale = 0.5f;
		/// the duration for which to keep the timescale changed
		[Tooltip("the duration for which to keep the timescale changed")]
		public float Duration = 1f;
		/// whether or not the timescale should get lerped
		[Tooltip("whether or not the timescale should get lerped")]
		public bool LerpTimeScale = true;
		/// the speed at which to lerp the timescale
		[Tooltip("the speed at which to lerp the timescale")]
		public float LerpSpeed = 5f;
		/// the cooldown for this ability
		[Tooltip("the cooldown for this ability")]
		public MMCooldown Cooldown;

		protected bool _timeControlled = false;

		/// <summary>
		/// Watches for input press
		/// </summary>
		protected override void HandleInput()
		{
			base.HandleInput();
			if (_inputManager.TimeControlButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				TimeControlStart();
			}
			if (_inputManager.TimeControlButton.State.CurrentState == MMInput.ButtonStates.ButtonUp)
			{
				TimeControlStop();
			}
		}

		/// <summary>
		/// On initialization, we init our cooldown
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			Cooldown.Initialization();
		}

		/// <summary>
		/// Starts the time scale modification
		/// </summary>
		public virtual void TimeControlStart()
		{
			if (Cooldown.Ready())
			{
				PlayAbilityStartFeedbacks();
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, Duration, LerpTimeScale, LerpSpeed, true);
				Cooldown.Start();
				_timeControlled = true;
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.TimeControl, MMCharacterEvent.Moments.Start);
			}
		}

		/// <summary>
		/// Stops the time control
		/// </summary>
		public virtual void TimeControlStop()
		{
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
			Cooldown.Stop();
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.TimeControl, MMCharacterEvent.Moments.End);
		}

		/// <summary>
		/// On update, we unfreeze time if needed
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			Cooldown.Update();

			if ((Cooldown.CooldownState != MMCooldown.CooldownStates.Consuming) && _timeControlled)
			{
				_timeControlled = false;
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
			}
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();

			if (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
			{
				TimeControlStop();    
			}
		}
	}
}