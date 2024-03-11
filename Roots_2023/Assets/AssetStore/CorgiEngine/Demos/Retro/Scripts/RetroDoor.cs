using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A class designed to handle the automatic coloration of door parts in the Retro demo scenes
	/// </summary>
	public class RetroDoor : CorgiMonoBehaviour
	{
		/// the color to apply
		[Tooltip("the color to apply")]
		public Color DoorColor = Color.yellow;
		/// the sprite renderer to modify
		[Tooltip("the sprite renderer to modify")]
		public SpriteRenderer DoorLightModel;
		/// the particle system to modify
		[Tooltip("the particle system to modify")]
		public ParticleSystem DoorParticles;

		/// <summary>
		/// On awake we apply the specified color
		/// </summary>
		protected virtual void Awake()
		{
			DoorLightModel.color = DoorColor;
			ParticleSystem.MainModule mainModule = DoorParticles.main;
			mainModule.startColor = DoorColor;
		}
	}
}