using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	[RequireComponent(typeof(Collider2D))]

	/// <summary>
	/// Parameters for the Corgi Controller class.
	/// This is where you define your slope limit, gravity, and speed dampening factors
	/// </summary>

	[Serializable]
	public class CorgiControllerParameters 
	{
		[Header("Gravity")]

		/// The force to apply vertically at all times
		[Tooltip("The force to apply vertically at all times")]
		public float Gravity = -30f;
		/// a multiplier applied to the character's gravity when going down
		[Tooltip("a multiplier applied to the character's gravity when going down")]
		public float FallMultiplier = 1f;
		/// a multiplier applied to the character's gravity when going up
		[Tooltip("a multiplier applied to the character's gravity when going up")]
		public float AscentMultiplier = 1f;

		[Header("Speed")]

		/// Maximum velocity for your character, to prevent it from moving too fast on a slope for example
		[Tooltip("Maximum velocity for your character, to prevent it from moving too fast on a slope for example")]
		public Vector2 MaxVelocity = new Vector2(100f, 100f);
		/// Speed factor on the ground
		[Tooltip("Speed factor on the ground")]
		public float SpeedAccelerationOnGround = 20f;
		/// if this is true, a separate deceleration value will be used when decelerating on ground. If false, SpeedAccelerationOnGround will be used
		[Tooltip("if this is true, a separate deceleration value will be used when decelerating on ground. If false, SpeedAccelerationOnGround will be used")]
		public bool UseSeparateDecelerationOnGround = false;
		/// a speed modifier to apply when not applying input to stop the character from moving when on the ground
		[Tooltip("a speed modifier to apply when not applying input to stop the character from moving when on the ground")]
		[MMCondition("UseSeparateDecelerationOnGround", true)]
		public float SpeedDecelerationOnGround = 20f;
		/// Speed factor in the air
		[Tooltip("Speed factor in the air")]
		public float SpeedAccelerationInAir = 5f;
		/// if this is true, a separate deceleration value will be used when decelerating in the air. If false, SpeedAccelerationInAir will be used
		[Tooltip("if this is true, a separate deceleration value will be used when decelerating in the air. If false, SpeedAccelerationInAir will be used")]
		public bool UseSeparateDecelerationInAir = false;
		/// a speed modifier to apply when not applying input to stop the character from moving when in the air
		[Tooltip("a speed modifier to apply when not applying input to stop the character from moving when in the air")]
		[MMCondition("UseSeparateDecelerationInAir", true)]
		public float SpeedDecelerationInAir = 5f;
		/// general speed factor
		[Tooltip("general speed factor")]
		public float SpeedFactor = 1f;

		[Header("Slopes")]

		/// Maximum angle (in degrees) the character can walk on
		[Tooltip("Maximum angle (in degrees) the character can walk on")]
		[Range(0,90)]
		public float MaximumSlopeAngle = 30f;
		/// the speed multiplier to apply when walking on a slope
		[Tooltip("the speed multiplier to apply when walking on a slope")]
		public AnimationCurve SlopeAngleSpeedFactor = new AnimationCurve(new Keyframe(-90f,1f),new Keyframe(0f,1f),new Keyframe(90f,1f));

		[Header("Physics2D Interaction [Experimental]")]

		/// if set to true, the character will transfer its force to all the rigidbodies it collides with horizontally
		[Tooltip("if set to true, the character will transfer its force to all the rigidbodies it collides with horizontally")]
		public bool Physics2DInteraction = true;
		/// the force applied to the objects the character encounters
		[Tooltip("the force applied to the objects the character encounters")]
		public float Physics2DPushForce = 2.0f;

		[Header("Gizmos")]
		/// if set to true, will draw the various raycasts used by the CorgiController to detect collisions in scene view if gizmos are active
		[Tooltip("if set to true, will draw the various raycasts used by the CorgiController to detect collisions in scene view if gizmos are active")]
		public bool DrawRaycastsGizmos = true;
		/// if this is true, warnings will be displayed if settings are not done properly
		[Tooltip("if this is true, warnings will be displayed if settings are not done properly")]
		public bool DisplayWarnings = true;
	}
}