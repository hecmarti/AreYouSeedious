using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	public class MovingPlatformFree : MMPathMovement
	{
		/// the possible methods this object can update at
		public enum UpdateMethods { Update, LateUpdate }

		/// the selected update method for this object
		/// depending on how you move your object (animation, script, etc) you may want to pick one over the other,
		/// to ensure that speed gets computed at the right time
		/// If you're using an animator to move this MovingPlatformFree, you'll probably want to set its UpdateMode on its Animator component to "Animate Physics"
		[Tooltip("the selected update method for this object. depending on how you move your object (animation, script, etc) you may want to pick one over the other, to ensure that speed gets computed at the right time. If you're using an animator to move this MovingPlatformFree, you'll probably want to set its UpdateMode on its Animator component to 'Animate Physics'")]
		public UpdateMethods UpdateMethod = UpdateMethods.Update;
        
		/// a debug display of this platform's current speed
		[MMReadOnly]
		[Tooltip("a debug display of this platform's current speed")]
		public Vector3 DebugCurrentSpeed; 

		protected Vector3 _newSpeed;

		/// <summary>
		/// On Update we compute our speed if needed
		/// </summary>
		protected override void Update()
		{
			if (UpdateMethod == UpdateMethods.Update)
			{
				ComputeSpeed();
			}
		}

		/// <summary>
		/// On Late Update we compute our speed if needed, and store our position
		/// </summary>
		protected override void LateUpdate()
		{
			if (UpdateMethod == UpdateMethods.LateUpdate)
			{
				ComputeSpeed();
			}
			_positionLastFrame = this.transform.position;
		}

		/// <summary>
		/// Computes the speed of this platform based on its current position and its position last frame
		/// </summary>
		protected virtual void ComputeSpeed()
		{
			_newSpeed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
			CurrentSpeed = _newSpeed;
			DebugCurrentSpeed = CurrentSpeed;
		}
	}
}