using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A weapon class aimed specifically at allowing the creation of various projectile weapons, from shotgun to machine gun, via plasma gun or rocket launcher
	/// </summary>
	[MMHiddenProperties("WeaponOnMissFeedback","ApplyRecoilOnHitDamageable","ApplyRecoilOnHitNonDamageable","ApplyRecoilOnHitNothing","ApplyRecoilOnKill")]
	[AddComponentMenu("Corgi Engine/Weapons/Projectile Weapon")]
	public class ProjectileWeapon : Weapon
	{
		[MMInspectorGroup("Projectile Spawn", true, 65)]

		/// the transform to use as the center reference point of the spawn
		[Tooltip("the transform to use as the center reference point of the spawn")]
		public Transform ProjectileSpawnTransform;
		/// the offset position at which the projectile will spawn
		[Tooltip("the offset position at which the projectile will spawn")]
		public Vector3 ProjectileSpawnOffset = Vector3.zero;
		/// the number of projectiles to spawn per shot
		[Tooltip("the number of projectiles to spawn per shot")]
		public int ProjectilesPerShot = 1;
		/// the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile
		[Tooltip("the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile")]
		public Vector3 Spread = Vector3.zero;
		/// whether or not the weapon should rotate to align with the spread angle
		[Tooltip("whether or not the weapon should rotate to align with the spread angle")]
		public bool RotateWeaponOnSpread = false;
		/// whether or not the spread should be random (if not it'll be equally distributed)
		[Tooltip("whether or not the spread should be random (if not it'll be equally distributed)")]
		public bool RandomSpread = true;
		/// the object pooler used to spawn projectiles, if left empty, this component will try to find one on its game object
		[Tooltip("the object pooler used to spawn projectiles, if left empty, this component will try to find one on its game object")]
		public MMObjectPooler ObjectPooler;
		/// the local position at which this projectile weapon should spawn projectiles
		[MMReadOnly]
		[Tooltip("the local position at which this projectile weapon should spawn projectiles")]
		public Vector3 SpawnPosition = Vector3.zero;
		protected Vector3 _flippedProjectileSpawnOffset;
		protected Vector3 _randomSpreadDirection;
		protected Vector3 _spawnPositionCenter;
		protected bool _poolInitialized = false;

		/// <summary>
		/// Initialize this weapon
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			_aimableWeapon = GetComponent<WeaponAim> ();

			if (!_poolInitialized)
			{
				if (ObjectPooler == null)
				{
					ObjectPooler = GetComponent<MMObjectPooler>();	
				}
				if (ObjectPooler == null)
				{
					Debug.LogWarning(this.name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
					return;
				}

				_flippedProjectileSpawnOffset = ProjectileSpawnOffset;
				_flippedProjectileSpawnOffset.y = -_flippedProjectileSpawnOffset.y;
				_poolInitialized = true;
			}            
		}

		/// <summary>
		/// Called everytime the weapon is used
		/// </summary>
		protected override void WeaponUse()
		{
			base.WeaponUse ();

			DetermineSpawnPosition ();
            			
			for (int i = 0; i < ProjectilesPerShot; i++)
			{
				SpawnProjectile(SpawnPosition, i, ProjectilesPerShot, true);
			}			
		}

		/// <summary>
		/// Spawns a new object and positions/resizes it
		/// </summary>
		public virtual GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
		{
			/// we get the next object in the pool and make sure it's not null
			GameObject nextGameObject = ObjectPooler.GetPooledGameObject();

			// mandatory checks
			if (nextGameObject==null)	{ return null; }
			if (nextGameObject.GetComponent<MMPoolableObject>()==null)
			{
				throw new Exception(gameObject.name+" is trying to spawn objects that don't have a PoolableObject component.");		
			}	
			// we position the object
			nextGameObject.transform.position = spawnPosition;
			// we set its direction

			Projectile projectile = nextGameObject.GetComponent<Projectile>();
			if (projectile != null)
			{				
				projectile.SetWeapon(this);
				if (Owner != null)
				{
					projectile.SetOwner(Owner.gameObject);
				}				
			}
			// we activate the object
			nextGameObject.gameObject.SetActive(true);


			if (projectile != null)
			{
				if (RandomSpread)
				{
					_randomSpreadDirection.x = UnityEngine.Random.Range(-Spread.x, Spread.x);
					_randomSpreadDirection.y = UnityEngine.Random.Range(-Spread.y, Spread.y);
					_randomSpreadDirection.z = UnityEngine.Random.Range(-Spread.z, Spread.z);
				}
				else
				{
					if (totalProjectiles > 1)
					{
						_randomSpreadDirection.x = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.x, Spread.x);
						_randomSpreadDirection.y = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.y, Spread.y);
						_randomSpreadDirection.z = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.z, Spread.z);
					}
					else
					{
						_randomSpreadDirection = Vector3.zero;
					}
				}               

				Quaternion spread = Quaternion.Euler(_randomSpreadDirection);
				bool facingRight = (Owner == null) || Owner.IsFacingRight;
				projectile.SetDirection(spread * transform.right * (Flipped ? -1 : 1), transform.rotation, facingRight);
				if (RotateWeaponOnSpread)
				{
					this.transform.rotation = this.transform.rotation * spread;
				}
			}

			if (triggerObjectActivation)
			{
				if (nextGameObject.GetComponent<MMPoolableObject>()!=null)
				{
					nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
				}
			}

			return (nextGameObject);
		}

		/// <summary>
		/// Determines the spawn position based on the spawn offset and whether or not the weapon is flipped
		/// </summary>
		public virtual void DetermineSpawnPosition()
		{
			_spawnPositionCenter = (ProjectileSpawnTransform == null) ? this.transform.position : ProjectileSpawnTransform.transform.position;

			if (Flipped && FlipWeaponOnCharacterFlip)
			{
				SpawnPosition = _spawnPositionCenter - this.transform.rotation * _flippedProjectileSpawnOffset;
			}
			else
			{
				SpawnPosition = _spawnPositionCenter + this.transform.rotation * ProjectileSpawnOffset;
			}
		}

		/// <summary>
		/// When the weapon is selected, draws a circle at the spawn's position
		/// </summary>
		protected virtual void OnDrawGizmosSelected()
		{
			DetermineSpawnPosition ();

			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(SpawnPosition, 0.2f);	
		}
	}
}