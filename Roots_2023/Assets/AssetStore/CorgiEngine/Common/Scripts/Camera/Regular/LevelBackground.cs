using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this class to a background image so it will act as your level's background
	/// </summary>
	[AddComponentMenu("Corgi Engine/Camera/Level Background")]
	public class LevelBackground : CorgiMonoBehaviour
	{
		/// if this is true, the background will be following the camera 
		public bool Following = true;
		/// the offset to apply relative to the camera
		public Vector3 BackgroundOffset = Vector3.zero;
		
		/// test button to start following the camera
		[MMInspectorButton("StartFollowing")] 
		public bool StartFollowingBtn;
		/// test button to stop following the camera
		[MMInspectorButton("StopFollowing")] 
		public bool StopFollowingBtn;
		
		protected Transform _initialParent;
		protected float _initialOffsetZ;
		protected bool _initialized = false;
		
		/// <summary>
		/// On enable, we get the main camera
		/// </summary>
		protected virtual void OnEnable ()
		{
			_initialParent = this.transform.parent;
			if (Following)
			{
				StartFollowing();
			}
		}

		/// <summary>
		/// Lets the background follow its camera
		/// </summary>
		public virtual void StartFollowing()
		{
			Following = true;
			this.transform.SetParent(Camera.main.transform);
			if (!_initialized)
			{
				_initialOffsetZ = this.transform.localPosition.z;
				_initialized = true;
			}

			BackgroundOffset.z = BackgroundOffset.z + _initialOffsetZ;
			this.transform.localPosition = BackgroundOffset;
		}

		/// <summary>
		/// Prevents the background from following the camera
		/// </summary>
		public virtual void StopFollowing()
		{
			Following = false;
			this.transform.SetParent(_initialParent);
		}

		/// <summary>
		/// Applies a new z offset for the background
		/// </summary>
		/// <param name="newOffset"></param>
		public virtual void SetZOffset(float newOffset)
		{
			_initialOffsetZ = newOffset;
		}
	}
}