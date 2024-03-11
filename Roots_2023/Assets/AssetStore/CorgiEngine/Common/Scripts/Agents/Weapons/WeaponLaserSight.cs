using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this class to a weapon and it'll project a laser ray towards the direction the weapon is facing
	/// </summary>
	[RequireComponent(typeof(Weapon))]
	[AddComponentMenu("Corgi Engine/Weapons/Weapon Laser Sight")]
	public class WeaponLaserSight : CorgiMonoBehaviour 
	{
		[Header("Raycasts")]
		[MMInformation("Add this class to a weapon and it'll project a laser ray towards the direction the weapon is facing",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]

		/// the origin of the raycast used to detect obstacles
		[Tooltip("the origin of the raycast used to detect obstacles")]
		public Vector3 RaycastOriginOffset;
		/// the origin of the visible laser
		[Tooltip("the origin of the visible laser")]
		public Vector3 LaserOriginOffset;
		/// the maximum distance to which we should draw the laser
		[Tooltip("the maximum distance to which we should draw the laser")]
		public float LaserMaxDistance = 50;
		/// the collision mask containing all layers that should stop the laser
		[Tooltip("the collision mask containing all layers that should stop the laser")]
		public LayerMask LaserCollisionMask = LayerManager.ObstaclesLayerMask;
		/// the width of the laser
		[Tooltip("the width of the laser")]
		public Vector2 LaserWidth = new Vector2(0.05f, 0.05f);
		
		[Header("Appearance")]
		/// the material used to render the laser
		[Tooltip("the material used to render the laser")]
		public Material LaserMaterial;

		protected Weapon _weapon;
		protected Vector3 _direction;
		protected LineRenderer _line;
		protected RaycastHit2D _hit;
		protected Vector3 _origin;
		protected Vector3 _destination;
		protected Vector3 _laserOffset;
		protected Vector3 _weaponPosition;
		protected Quaternion _weaponRotation;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Initialization()
		{
			_weapon = GetComponent<Weapon>();
			if (_weapon == null)
			{
				Debug.LogWarning("This WeaponLaserSight is not associated to a weapon. Please add it to a gameobject with a Weapon component.");
			}

			_line = gameObject.AddComponent<LineRenderer>();
			_line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			_line.receiveShadows = true;
			_line.startWidth = LaserWidth.x;
			_line.endWidth = LaserWidth.y;
			_line.material = LaserMaterial;
		}

		/// <summary>
		/// Every frame we draw our laser
		/// </summary>
		protected virtual void Update()
		{
			ShootLaser();
		}

		/// <summary>
		/// Draws the actual laser
		/// </summary>
		protected virtual void ShootLaser()
		{
			_weaponPosition = _weapon.transform.position;
			_weaponRotation = _weapon.transform.rotation;
			
			// we determine our direction based on whether or not our weapon is flipped.
			_direction = _weapon.Flipped ? Vector3.left : Vector3.right ;

			_laserOffset = LaserOriginOffset;
			if (_direction == Vector3.left)
			{
				_laserOffset.x = -LaserOriginOffset.x;
			}

			// our laser will be shot from the weapon's laser origin
			_origin = MMMaths.RotatePointAroundPivot (_weaponPosition + _laserOffset, _weaponPosition, _weaponRotation);

			// we cast a ray in front of the weapon to detect an obstacle
			_hit = MMDebug.RayCast(_origin, _weaponRotation * _direction, LaserMaxDistance, LaserCollisionMask, Color.yellow, true);

			// if we've hit something, our destination is the raycast hit
			if (_hit)
			{
				_destination = _hit.point;
			}
			// otherwise we just draw our laser in front of our weapon 
			else
			{
				_destination = _origin;
				_destination.x = _destination.x + LaserMaxDistance * _direction.x;
				_destination = MMMaths.RotatePointAroundPivot (_destination, _weaponPosition, _weaponRotation);
			}

			// we set our laser's line's start and end coordinates
			_line.SetPosition(0, _origin);
			_line.SetPosition(1, _destination);
		}

		/// <summary>
		/// Turns the laser on or off depending on the status passed in parameters
		/// </summary>
		/// <param name="status">If set to <c>true</c> status.</param>
		public virtual void LaserActive(bool status)
		{
			_line.enabled = status;
		}

	}
}