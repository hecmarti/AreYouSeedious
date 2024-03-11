using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A class used to describe the behaviour of a projectile, usually spawned by a ProjectileWeapon
	/// </summary>
	[AddComponentMenu("Corgi Engine/Weapons/Projectile")]
	public class Projectile : MMPoolableObject 
	{
		[Header("Movement")] 

		/// if true, the projectile will rotate at initialization towards its rotation
		[Tooltip("if true, the projectile will rotate at initialization towards its rotation")]
		public bool FaceDirection = true;
		/// the speed of the object (relative to the level's speed)
		[Tooltip("the speed of the object (relative to the level's speed)")]
		public float Speed = 200;
		/// the acceleration of the object over time. Starts accelerating on enable.
		[Tooltip("the acceleration of the object over time. Starts accelerating on enable.")]
		public float Acceleration = 0;
		/// the current direction of the object
		[Tooltip("the current direction of the object")]
		public Vector3 Direction = Vector3.left;
		/// if set to true, the spawner can change the direction of the object. If not the one set in its inspector will be used.
		[Tooltip("if set to true, the spawner can change the direction of the object. If not the one set in its inspector will be used.")]
		public bool DirectionCanBeChangedBySpawner = true;
		/// the flip factor to apply if and when the projectile is mirrored
		[Tooltip("the flip factor to apply if and when the projectile is mirrored")]
		public Vector3 FlipValue = new Vector3(-1,1,1);
		/// determines whether or not the projectile is facing right
		[Tooltip("determines whether or not the projectile is facing right")]
		public bool ProjectileIsFacingRight = true;

		[Header("Spawn")]
		[MMInformation("Here you can define an initial delay (in seconds) during which this object won't take or cause damage. This delay starts when the object gets enabled. You can also define whether the projectiles should damage their owner (think rockets and the likes) or not",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]

		/// the initial delay during which the projectile can't be destroyed
		[Tooltip("the initial delay during which the projectile can't be destroyed")]
		public float InitialInvulnerabilityDuration=0f;
		/// should the projectile damage its owner ?
		[Tooltip("should the projectile damage its owner ?")]
		public bool DamageOwner = false;
		/// if this is true, the projectile will perform an extra check on initialization to make sure it's not within an obstacle. If it is, it'll disable itself. 
		[Tooltip("if this is true, the projectile will perform an extra check on initialization to make sure it's not within an obstacle. If it is, it'll disable itself.")]
		public bool SpawnSecurityCheck = false;
		/// the layermask to use when performing the security check
		[Tooltip("the layermask to use when performing the security check")]
		public LayerMask SpawnSecurityCheckLayerMask;

		/// Returns the associated damage on touch zone
		public DamageOnTouch TargetDamageOnTouch { get { return _damageOnTouch; } }

		protected Weapon _weapon;
		protected GameObject _owner;
		protected Vector3 _movement;
		protected float _initialSpeed;
		protected SpriteRenderer _spriteRenderer;
		protected DamageOnTouch _damageOnTouch;
		protected WaitForSeconds _initialInvulnerabilityDurationWFS;

		protected const float _raycastSkinSecurity=0.01f;
		protected BoxCollider2D _collider;
		protected Vector2 _raycastOrigin;
		protected Vector2 _raycastDestination;
		protected bool _facingRightInitially;
		protected bool _initialFlipX;
		protected Vector3 _initialLocalScale;
		protected RaycastHit2D _hit2D;
		protected Health _health;
		protected bool _spawnerIsFacingRight;

		/// <summary>
		/// On awake, we store the initial speed of the object 
		/// </summary>
		protected virtual void Awake ()
		{
			_facingRightInitially = ProjectileIsFacingRight;
			_initialSpeed = Speed;
			_collider = GetComponent<BoxCollider2D> ();
			_spriteRenderer = GetComponent<SpriteRenderer> ();
			_damageOnTouch = GetComponent<DamageOnTouch>();
			_health = GetComponent<Health>();
			_initialInvulnerabilityDurationWFS = new WaitForSeconds (InitialInvulnerabilityDuration);
			if (_spriteRenderer != null) {	_initialFlipX = _spriteRenderer.flipX ;		}
			_initialLocalScale = transform.localScale;		
		}

		/// <summary>
		/// Handles the projectile's initial invincibility
		/// </summary>
		/// <returns>The invulnerability.</returns>
		protected virtual IEnumerator InitialInvulnerability()
		{
			if (_damageOnTouch == null) { yield break; }
			if (_weapon == null) { yield break; }
			_damageOnTouch.ClearIgnoreList();
			_damageOnTouch.IgnoreGameObject(_weapon.Owner.gameObject);
			yield return _initialInvulnerabilityDurationWFS;
			if (DamageOwner)
			{
				_damageOnTouch.StopIgnoringObject(_weapon.Owner.gameObject);
			}
		}

		/// <summary>
		/// Initializes the projectile
		/// </summary>
		protected virtual void Initialization()
		{
			Speed = _initialSpeed;
			ProjectileIsFacingRight = _facingRightInitially;
			if (_spriteRenderer != null) {	_spriteRenderer.flipX = _initialFlipX;	}
			transform.localScale = _initialLocalScale;
			CheckForCollider();
		}

		/// <summary>
		/// Performs a local check to see if the projectile is within a collider or not
		/// </summary>
		protected virtual void CheckForCollider()
		{
			if (!SpawnSecurityCheck)
			{
				return;
			}

			if (_collider == null)
			{
				return;
			}

			_hit2D = Physics2D.BoxCast(this.transform.position, _collider.bounds.size, this.transform.eulerAngles.z, Vector3.forward, 1f, SpawnSecurityCheckLayerMask);
			if (_hit2D)
			{
				gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// On FixedUpdate(), we move the object based on the level's speed and the object's speed, and apply acceleration
		/// </summary>
		protected virtual void FixedUpdate ()
		{
			Movement();
		}

		/// <summary>
		/// Handles the projectile's movement, every frame
		/// </summary>
		public virtual void Movement()
		{
			_movement = Direction * (Speed / 10) * Time.deltaTime;
			transform.Translate(_movement,Space.World);
			// We apply the acceleration to increase the speed
			Speed += Acceleration * Time.deltaTime;
		}

		/// <summary>
		/// Sets the projectile's direction.
		/// </summary>
		/// <param name="newDirection">New direction.</param>
		/// <param name="newRotation">New rotation.</param>
		/// <param name="spawnerIsFacingRight">If set to <c>true</c> spawner is facing right.</param>
		public virtual void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight=true)
		{
			_spawnerIsFacingRight = spawnerIsFacingRight;
			if (DirectionCanBeChangedBySpawner)
			{
				Direction = newDirection;
			}
			if (ProjectileIsFacingRight != spawnerIsFacingRight)
			{
				Flip ();
			}
			if (FaceDirection)
			{
				transform.rotation = newRotation;
			}
		}

		/// <summary>
		/// Flip the projectile
		/// </summary>
		protected virtual void Flip()
		{
			if (_spriteRenderer != null)
			{
				_spriteRenderer.flipX = !_spriteRenderer.flipX;
			}	
			else
			{
				this.transform.localScale = Vector3.Scale(this.transform.localScale,FlipValue) ;
			}
		}

		/// <summary>
		/// Sets the projectile's parent weapon.
		/// </summary>
		/// <param name="newWeapon">New weapon.</param>
		public virtual void SetWeapon(Weapon newWeapon)
		{
			_weapon = newWeapon;
		}

		/// <summary>
		/// Sets the projectile's owner.
		/// </summary>
		/// <param name="newOwner">New owner.</param>
		public virtual void SetOwner(GameObject newOwner)
		{
			_owner = newOwner;
			DamageOnTouch damageOnTouch = this.gameObject.MMGetComponentNoAlloc<DamageOnTouch>();            
			if (damageOnTouch != null)
			{
				damageOnTouch.Owner = newOwner;
				if (!DamageOwner)
				{
					damageOnTouch.ClearIgnoreList();
					damageOnTouch.IgnoreGameObject(newOwner);
				}                
			}
		}

		/// <summary>
		/// Returns the current Owner of the projectile
		/// </summary>
		/// <returns></returns>
		public virtual GameObject GetOwner()
		{
			return _owner;
		}
		
		/// <summary>
		/// Sets the damage caused by the projectile's DamageOnTouch to the specified value
		/// </summary>
		/// <param name="newDamage"></param>
		public virtual void SetDamage(int newDamage)
		{
			if (_damageOnTouch != null)
			{
				_damageOnTouch.MinDamageCaused = newDamage;
			}
		}
		
		/// <summary>
		/// On hit, we trigger a hit on our owner weapon
		/// </summary>
		protected virtual void OnHit()
		{
			if (_weapon != null)
			{
				_weapon.WeaponHit();
			}
		}
		
		/// <summary>
		/// On hit damageable, we trigger a hit damageable on our owner weapon
		/// </summary>
		protected virtual void OnHitDamageable()
		{
			if (_weapon != null)
			{
				_weapon.WeaponHitDamageable();
			}
		}
		
		/// <summary>
		/// On hit non damageable, we trigger a hit non damageable on our owner weapon
		/// </summary>
		protected virtual void OnHitNonDamageable()
		{
			if (_weapon != null)
			{
				_weapon.WeaponHitNonDamageable();
			}
		}
		
		/// <summary>
		/// On kill, we trigger a kill on our owner weapon
		/// </summary>
		protected virtual void OnKill()
		{
			if (_weapon != null)
			{
				_weapon.WeaponKill();
			}
		}

		/// <summary>
		/// On enable, we reset the object's speed
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			Initialization();
			if (InitialInvulnerabilityDuration>0)
			{
				StartCoroutine(InitialInvulnerability());
			}

			if (_damageOnTouch != null)
			{
				_damageOnTouch.OnKill += OnKill;
				_damageOnTouch.OnHit += OnHit;
				_damageOnTouch.OnHitDamageable += OnHitDamageable;
				_damageOnTouch.OnHitNonDamageable += OnHitNonDamageable;
			}
		}

		/// <summary>
		/// On disable, we unsubscribe from our delegates
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();
			if (_damageOnTouch != null)
			{
				_damageOnTouch.OnKill -= OnKill;
				_damageOnTouch.OnHit -= OnHit;
				_damageOnTouch.OnHitDamageable -= OnHitDamageable;
				_damageOnTouch.OnHitNonDamageable -= OnHitNonDamageable;
			}
		}
	}	
}