using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this component to a one way platform (a platform with an edge collider, on the OneWayPlatforms layer) and it will automatically turn its collider on or off based on the player position, letting you easily move it around, and will also let the player collide with it laterally,
	/// like a mid height one way platform would, but regardless of its placement or rotation. It's slightly more costly than a regular, static one, but will offer more options too
	/// </summary>
	[RequireComponent(typeof(EdgeCollider2D))]
	public class AnyHeightOneWayPlatform : CorgiMonoBehaviour
	{
		[Header("Settings")]
		/// the ID of the target Player to consider for this platform - usually 0
		[Tooltip("the ID of the target Player to consider for this platform - usually 0")]
		public int PlayerID = 0;
		/// the offset to consider for the player's position - usually 0,0
		[Tooltip("the offset to consider for the player's position - usually 0,0")]
		public Vector2 Offset;
		/// if this is true, this component will try to automatically adapt its offset based on the player's dimensions 
		[Tooltip("if this is true, this component will try to automatically adapt its offset based on the player's dimensions")]
		public bool AutoAdaptOffsetBasedOnPlayerHeight = true;
		/// if this is true, the platform's coordinates will be recomputed every frame, only turn this on if your platform moves. Ideally you'll want to keep this off and manually call CacheColliderWorldCoordinates() every time your platform moves
		[Tooltip("if this is true, the platform's coordinates will be recomputed every frame, only turn this on if your platform moves. Ideally you'll want to keep this off and manually call CacheColliderWorldCoordinates() every time your platform moves")]
		public bool CacheWorldCoordinatesEveryFrame = false;
		/// if this is false, the platform will turn itself off when the player is below it, and on otherwise. If Inverted is set to true, then the platform will turn itself off when the player is above, and on when it's below
		[Tooltip("if this is false, the platform will turn itself off when the player is below it, and on otherwise. If Inverted is set to true, then the platform will turn itself off when the player is above, and on when it's below")]
		public bool Inverted = false;
		
		protected EdgeCollider2D _edgeCollider2D;
		protected Vector2 _edgeA;
		protected Vector2 _edgeB;
		protected Transform _playerTransform;
		
		/// <summary>
		/// On Awake we initialize our platform
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// Stores the collider and computes its points' world coordinates
		/// </summary>
		public virtual void Initialization()
		{
			_edgeCollider2D = this.gameObject.GetComponent<EdgeCollider2D>();
			CacheColliderWorldCoordinates();
		}
		
		/// <summary>
		/// On LateUpdate we turn our collider on or off
		/// </summary>
		protected virtual void LateUpdate()
		{
			HandleColliderBasedOnPlayerPosition();
		}

		/// <summary>
		/// Turns the collider on if the player is above it, off otherwise 
		/// </summary>
		protected virtual void HandleColliderBasedOnPlayerPosition()
		{
			if (_playerTransform == null)
			{
				GrabTargetPlayer();
			}
			else
			{
				if (CacheWorldCoordinatesEveryFrame)
				{
					CacheColliderWorldCoordinates();
				}
				_edgeCollider2D.enabled = Inverted ? !PlayerIsAbove() : PlayerIsAbove();
			}
		}

		/// <summary>
		/// Stores the target player character
		/// </summary>
		public virtual void GrabTargetPlayer()
		{
			_playerTransform = LevelManager.Instance.Players[PlayerID].transform;
			if (AutoAdaptOffsetBasedOnPlayerHeight)
			{
				Offset.y -= _playerTransform.gameObject.GetComponent<CorgiController>().Height() / 2 +
				            _playerTransform.gameObject.GetComponent<CorgiController>().ColliderOffset.y ;
			}
		}

		/// <summary>
		/// Stores the positions of the platform's edges
		/// </summary>
		public virtual void CacheColliderWorldCoordinates()
		{
			_edgeA = _edgeCollider2D.transform.TransformPoint(_edgeCollider2D.points[0]);
			_edgeB = _edgeCollider2D.transform.TransformPoint(_edgeCollider2D.points[1]);
		}

		/// <summary>
		/// Returns true if the player is above the platform, false otherwise
		/// </summary>
		/// <returns></returns>
		protected virtual bool PlayerIsAbove()
		{
			return Mathf.Sign((_edgeB.x - _edgeA.x) * (PlayerPositionY - _edgeA.y) - (_edgeB.y - _edgeA.y) * (PlayerPositionX - _edgeA.x)) > 0;
		}

		/// <summary>
		/// The x position of the player
		/// </summary>
		protected virtual float PlayerPositionX => _playerTransform.position.x + Offset.x;
		
		/// <summary>
		/// The y position of the player
		/// </summary>
		protected virtual float PlayerPositionY => _playerTransform.position.y + Offset.y;
	} 
}
