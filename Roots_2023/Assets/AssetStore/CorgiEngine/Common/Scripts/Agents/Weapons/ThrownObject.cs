using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// A class used to create physics based projectiles, meant to be thrown like grenades
	/// </summary>
	[RequireComponent(typeof(Rigidbody2D))]
	[AddComponentMenu("Corgi Engine/Weapons/ThrownObject")]
	public class ThrownObject : Projectile
	{
		/// if true, the projectile will rotate to match its trajectory (useful for arrows for example)
		[Tooltip("if true, the projectile will rotate to match its trajectory (useful for arrows for example)")]
		public bool AutoOrientAlongTrajectory = false;
		
		protected Rigidbody2D _rigidBody2D;
		protected Vector2 _throwingForce;
		protected bool _forceApplied = false;

		protected override void Initialization()
		{
			base.Initialization();
			_rigidBody2D = this.gameObject.GetComponent<Rigidbody2D>();
		}

		/// <summary>
		/// On enable, we reset the object's speed
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			_forceApplied = false;
		}

		/// <summary>
		/// Handles the projectile's movement, every frame
		/// </summary>
		public override void Movement()
		{
			if (!_forceApplied && (Direction != Vector3.zero))
			{
				_throwingForce = Direction * Speed;
				_rigidBody2D.AddForce (_throwingForce);
				_forceApplied = true;
			}

			OrientAlongTrajectory();
		}

		/// <summary>
		/// Rotates the object to match its rigidbody's trajectory
		/// </summary>
		protected virtual void OrientAlongTrajectory()
		{
			if (AutoOrientAlongTrajectory)
			{
				if (_rigidBody2D.velocity.magnitude > 0)
				{
					float angle = Mathf.Atan2(_rigidBody2D.velocity.y, _rigidBody2D.velocity.x) * Mathf.Rad2Deg;
					if (!_spawnerIsFacingRight)
					{
						angle += 180f;
					}
					Quaternion newRotation = Quaternion.AngleAxis(angle, Vector3.forward);
					this.transform.rotation = newRotation;
				}
			}
		}
	}
}