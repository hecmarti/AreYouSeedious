using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A simple component you can use to control a weapon and have it start and stop on demand, without having a character to handle it
	/// You can see it in action in the KoalaHealth demo scene, it's powering that demo's cannons
	/// </summary>
	public class WeaponHandler : CorgiMonoBehaviour
	{
		[Header("Weapon")]
		/// the weapon you want this component to pilot
		[Tooltip("the weapon you want this component to pilot")]
		public Weapon TargetWeapon;
		
		[Header("On Start")]
		/// if this is true, the WeaponHandler will begin shooting automatically on start
		[Tooltip("if this is true, the WeaponHandler will begin shooting automatically on start")]
		public bool StartShootingOnStart = false;
		/// the delay, in seconds, to wait before shooting on start 
		[Tooltip("the delay, in seconds, to wait before shooting on start")]
		public float InitialDelayOnStart = 1f;

		[Header("Debug")] 
		[MMInspectorButton("StartShooting")]
		public bool StartShootingButton;
		[MMInspectorButton("StopShooting")]
		public bool StopShootingButton;

		/// <summary>
		/// On Start, starts shooting if needed
		/// </summary>
		protected virtual void Start()
		{
			if (StartShootingOnStart)
			{
				StartCoroutine(StartShootingOnStartCo());
			}
		}

		protected virtual IEnumerator StartShootingOnStartCo()
		{
			yield return MMCoroutine.WaitFor(InitialDelayOnStart);
			StartShooting();
		}

		/// <summary>
		/// Makes the associated weapon start shooting
		/// </summary>
		public virtual void StartShooting()
		{
			if (TargetWeapon == null)
			{
				return;
			}
			TargetWeapon.WeaponInputStart();
		}

		/// <summary>
		/// Makes the associated weapon stop shooting
		/// </summary>
		public virtual void StopShooting()
		{
			if (TargetWeapon == null)
			{
				return;
			}
			TargetWeapon.WeaponInputStop();
		}
	}
}