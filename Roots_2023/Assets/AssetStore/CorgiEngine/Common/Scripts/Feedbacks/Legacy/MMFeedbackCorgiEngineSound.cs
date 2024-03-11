using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine.Audio;

namespace MoreMountains.CorgiEngine
{
	[AddComponentMenu("")]
	[FeedbackPath("Audio/Corgi Engine Sound")]
	[FeedbackHelp("This feedback lets you play a sound through the Corgi Engine's SoundManager")]
	public class MMFeedbackCorgiEngineSound : MMFeedback
	{
		[Header("Corgi Engine Sound")]

		/// the audio clip to play
		[Tooltip("the audio clip to play")]
		public AudioClip SoundFX;
		/// whether or not to play this sound in a loop
		[Tooltip("whether or not to play this sound in a loop")]
		public bool Loop = false;
        
		protected AudioSource _audioSource;

		protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
		{
			if (Active)
			{
				if (SoundFX != null)
				{
					_audioSource = MMSoundManagerSoundPlayEvent.Trigger(SoundFX, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position, loop:Loop);
				}
			}
		}
        
		/// <summary>
		/// This method describes what happens when the feedback gets stopped
		/// </summary>
		/// <param name="position"></param>
		/// <param name="attenuation"></param>
		protected override void CustomStopFeedback(Vector3 position, float attenuation = 1.0f)
		{
			if (_audioSource == null)
			{
				return;
			}
            
			if (Loop)
			{
				_audioSource.Stop();
			}
            
			MMSoundManager.Instance.FreeSound(_audioSource);
		}
	}
}