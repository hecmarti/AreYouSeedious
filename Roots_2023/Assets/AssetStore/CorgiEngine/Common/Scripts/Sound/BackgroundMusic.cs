using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this class to a GameObject to have it play a background music when instanciated.
	/// Careful : only one background music will be played at a time.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Audio/Background Music")]
	public class BackgroundMusic : MMPersistentHumbleSingleton<BackgroundMusic>
	{
		/// the background music audio clip to play
		[Tooltip("the background music audio clip to play")]
		public AudioClip SoundClip ;
		/// whether or not the music should loop
		[Tooltip("whether or not the music should loop")]
		public bool Loop = true;

		protected AudioSource _source;

		/// <summary>
		/// Gets the AudioSource associated to that GameObject, and asks the GameManager to play it.
		/// </summary>
		protected virtual void Start () 
		{
			MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
			options.Loop = Loop;
			options.Location = Vector3.zero;
			options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
            
			MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);
		}
	}
}