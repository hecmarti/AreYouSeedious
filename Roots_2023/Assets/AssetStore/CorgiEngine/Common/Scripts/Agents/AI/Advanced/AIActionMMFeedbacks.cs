using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This action is used to play a MMFeedbacks
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AIActionMMFeedbacks")]
	public class AIActionMMFeedbacks : AIAction
	{
		/// The MMFeedbacks to play when this action gets performed by the AIBrain
		[Tooltip("The MMFeedbacks to play when this action gets performed by the AIBrain")]
		public MMFeedbacks TargetFeedbacks;
		/// If this is false, the feedback will be played every PerformAction (by default every frame while in this state), otherwise it'll only play once, when entering the state
		[Tooltip("If this is false, the feedback will be played every PerformAction (by default every frame while in this state), otherwise it'll only play once, when entering the state")]
		public bool OnlyPlayWhenEnteringState = true;

		protected bool _played = false;

		/// <summary>
		/// On PerformAction we play our MMFeedbacks
		/// </summary>
		public override void PerformAction()
		{
			PlayFeedbacks();
		}

		/// <summary>
		/// Plays the target MMFeedbacks
		/// </summary>
		protected virtual void PlayFeedbacks()
		{
			if (OnlyPlayWhenEnteringState && _played)
			{
				return;
			}

			if (TargetFeedbacks != null)
			{
				TargetFeedbacks.PlayFeedbacks();
				_played = true;
			}
		}

		/// <summary>
		/// On enter state we initialize our _played bool
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			_played = false;
		}
	}
}