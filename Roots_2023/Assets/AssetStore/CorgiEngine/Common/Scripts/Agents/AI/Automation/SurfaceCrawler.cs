using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	// Add this component to an object, and it will try to follow a surface (as defined by colliders on the specified SurfaceLayerMask
	public class SurfaceCrawler : CorgiMonoBehaviour
	{
		/// the potential directions this object can move towards 
		public enum Directions { Clockwise, CounterClockwise }
        
		[Header("Crawl")] 
		/// whether or not this crawler can move. If false, it'll stay where it is.
		[Tooltip("whether or not this crawler can move. If false, it'll stay where it is.")] 
		public bool CanMove = true; 
		/// the current direction this crawler is moving towards
		[Tooltip("the current direction this crawler is moving towards")]
		public Directions Direction = Directions.Clockwise; 
		/// the speed at which the crawler should move along the surface
		[Tooltip("the speed at which the crawler should move along the surface")]
		public float Speed = 10f; 
		/// the distance from the surface at which the object should remain
		[Tooltip("the distance from the surface at which the object should remain")]
		public float DistanceToSurface = 0.5f; 
		/// the layers the object will consider as a crawlable surface
		[Tooltip("the layers the object will consider as a crawlable surface")]
		public LayerMask SurfaceLayerMask = LayerManager.ObstaclesLayerMask;
        
		[Header("Rotation")] 
		/// the visual representation of our crawler, usually nested under the top level containing the logic (the Crawler component)
		[Tooltip("the visual representation of our crawler, usually nested under the top level containing the logic (the Crawler component)")]
		public Transform Model;
		/// whether or not to rotate the crawler's model to match its movement direction
		[Tooltip("whether or not to rotate the crawler's model to match its movement direction")]
		public bool RotateModel = true; 
		/// if RotateModel is true, the speed at which the model should rotate to match its movement direction
		[Tooltip("if RotateModel is true, the speed at which the model should rotate to match its movement direction")]
		[MMCondition("RotateModel", true)] 
		public float RotationSpeed = 10f;

		[Header("Flip")] 
		/// whether or not the model should flip its x scale as it changes direction
		[Tooltip("whether or not the model should flip its x scale as it changes direction")]
		public bool FlipModelOnDirectionChange = true;
        
		[Header("Forward Raycast")] 
		/// the offset (from the crawler's position) from which we'll cast our forward ray
		[Tooltip("the offset (from the crawler's position) from which we'll cast our forward ray")]
		public Vector2 ForwardRaycastOffset = Vector2.zero; 
		/// the length of the forward raycast. This raycast detects upcoming walls, so you'll want to make sure it's of appropriate length. Don't hesitate to enable debug gizmos and check if it behaves as you expect
		[Tooltip("the length of the forward raycast. This raycast detects upcoming walls, so you'll want to make sure it's of appropriate length. Don't hesitate to enable debug gizmos and check if it behaves as you expect")]
		public float ForwardRaycastLength = 2f;
        
		[Header("Downwards Raycast")] 
		/// the offset (from the crawler's position) from which we'll cast our downward ray
		[Tooltip("the offset (from the crawler's position) from which we'll cast our downward ray")]
		public Vector2 DownwardRaycastOffset = Vector2.zero; 
		/// the length of the downward raycast. This raycast detects the surface below the crawler, so you'll want to make sure it's of appropriate length. Don't hesitate to enable debug gizmos and check if it behaves as you expect
		[Tooltip("the length of the downward raycast. This raycast detects the surface below the crawler, so you'll want to make sure it's of appropriate length. Don't hesitate to enable debug gizmos and check if it behaves as you expect")]
		public float DownwardRaycastLength = 2f;
        
		[Header("Limits")] 
		/// when the crawler doesn't find a ground under itself, it'll start rotating on itself (in the movement's direction) until it finds a surface. This defines the increment at which it does that. The smaller the value, the more it will have to rotate to find ground. This should be tweaked based on the amount of different corner angles this crawler has to handle. If you only have right angles, then a value like 45 should be fine, for example.
		[Tooltip("when the crawler doesn't find a ground under itself, it'll start rotating on itself (in the movement's direction) until it finds a surface. This defines the increment at which it does that. The smaller the value, the more it will have to rotate to find ground. This should be tweaked based on the amount of different corner angles this crawler has to handle. If you only have right angles, then a value like 45 should be fine, for example.")]
		public float RaycastRotationAngleInterval = 10f; 
		/// the maximum amount of rotations the object will perform on itself after reaching an edge to try and find new ground. You'll want to tweak it based on your level's geometry, angle interval and max distance to a potential wall behind on the path
		[Tooltip("the maximum amount of rotations the object will perform on itself after reaching an edge to try and find new ground. You'll want to tweak it based on your level's geometry, angle interval and max distance to a potential wall behind on the path")]
		public int MaxAmountOfRotations = 5; 
		/// the maximum distance interval between two movements. This should be adjusted based on the size of the crawler and the terrain it needs to navigate. That's the distance at which the crawler will reevaluate its path. 
		[Tooltip("the maximum distance interval between two movements. This should be adjusted based on the size of the crawler and the terrain it needs to navigate. That's the distance at which the crawler will reevaluate its path.")]
		public float MaxDistanceInterval = 0.5f;

		[Header("Debug")]  
		/// whether or not to draw gizmos in scene view to show the various raycasts 
		[Tooltip("whether or not to draw gizmos in scene view to show the various raycasts")]
		public bool DrawGizmos = true;
		[MMInspectorButton("ChangeDirection")]
		public bool ChangeDirectionButton;

		protected Vector2 _previousNormal;
		protected Vector2 _raycastOrigin;
		protected Vector2 _movementDirection;
		protected Vector3 _translation;
		protected Vector2 _forwardRaycastDirection;
		protected float _movedDistance;
		protected float _targetDistance = 0f;
		protected Transform _rotationReference;
		protected Vector2 _initialModelScale;
		protected Vector2 _modelScale;
		protected Quaternion _initialRotation;
        
		/// <summary>
		/// On awake we initialize our crawler
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// On init we create a rotation reference we'll keep around as a reference of our current normal
		/// </summary>
		protected virtual void Initialization()
		{
			_rotationReference = new GameObject("CrawlerRotationReference").transform;
			_rotationReference.rotation = this.transform.rotation;
			_rotationReference.SetParent(this.transform);
			_rotationReference.localPosition = Vector3.zero;
			
			if (Model != null)
			{
				_initialModelScale = Model.localScale;
			} 
		}

		/// <summary>
		/// On fixed update we crawl and rotate our model
		/// </summary>
		protected virtual void FixedUpdate()
		{
			PerformCrawl();
			HandleModelRotation();
			HandleModelOrientation();
		}

		/// <summary>
		/// Call this method to have the crawler change direction
		/// </summary>
		public virtual void ChangeDirection()
		{
			Direction = (Direction == Directions.Clockwise) ? Directions.CounterClockwise : Directions.Clockwise;
		}

		/// <summary>
		/// Use this method to set a new direction for the crawler
		/// </summary>
		/// <param name="newDirection"></param>
		public virtual void SetDirection(Directions newDirection)
		{
			Direction = newDirection;
		}

		/// <summary>
		/// Moves along the surface at the specified speed
		/// </summary>
		protected virtual void PerformCrawl()
		{
			if (!CanMove)
			{
				return;
			}

			bool shouldKeepMoving = true;
			_targetDistance = 0f;
			_movedDistance = 0f;
            
			while (shouldKeepMoving)
			{
				DetermineDirection();

				_translation = _movementDirection * Speed * Time.deltaTime;
				if (_targetDistance == 0f) { _targetDistance = _translation.magnitude; }
				float newDistance = Mathf.Min(MaxDistanceInterval, _targetDistance - _movedDistance);
                
				_translation = _movementDirection * newDistance;
                
				if (newDistance >= MaxDistanceInterval)
				{
					_movedDistance += newDistance;
				}
				else
				{
					shouldKeepMoving = false;
				}
            
				this.transform.Translate(_translation, Space.World);    
			}
		}

		/// <summary>
		/// Changes the scale of the model to match its current direction
		/// </summary>
		protected virtual void HandleModelOrientation()
		{
			if (!FlipModelOnDirectionChange)
			{
				return;
			}
            
			if (Direction == Directions.Clockwise)
			{
				_modelScale = _initialModelScale;
				_modelScale.x = _initialModelScale.x;
				Model.localScale = _modelScale;
			}
			else
			{
				_modelScale = _initialModelScale;
				_modelScale.x = - _initialModelScale.x;
				Model.localScale = _modelScale;
			}
		}

		/// <summary>
		/// Rotates the model to tend towards the reference rotation at the specified speed
		/// </summary>
		protected virtual void HandleModelRotation()
		{
			if (!RotateModel)
			{
				return;
			}

			Model.transform.rotation = Quaternion.Slerp(Model.transform.rotation, _rotationReference.rotation,
				Time.deltaTime * RotationSpeed);
		}

		/// <summary>
		/// Casts a number of rays to determine what direction the object should take next
		/// </summary>
		protected virtual void DetermineDirection()
		{
			_raycastOrigin = this.transform.position;
            
			_forwardRaycastDirection = (Direction == Directions.Clockwise) ? _rotationReference.right : -_rotationReference.right;

			_raycastOrigin += ComputeOffset(this.transform, ForwardRaycastOffset);
            
			RaycastHit2D forwardHit = MMDebug.RayCast(_raycastOrigin, _forwardRaycastDirection, ForwardRaycastLength, SurfaceLayerMask, Color.black, DrawGizmos);
            
			// we cast a ray in front of us to detect walls
			if (forwardHit)
			{
				RegisterHit(forwardHit, ForwardRaycastOffset);
			}
			else
			{
				_raycastOrigin = this.transform.position;
				_raycastOrigin += ComputeOffset(this.transform, DownwardRaycastOffset);
                
				// we cast a ray downwards to detect ground
				RaycastHit2D downwardHit = MMDebug.RayCast(_raycastOrigin, -_rotationReference.up, DownwardRaycastLength, SurfaceLayerMask, Color.blue, DrawGizmos);
				if (downwardHit)
				{
					RegisterHit(downwardHit, DownwardRaycastOffset);
				}
				else
				{
					// we rotate until we hit something
					bool foundNewGround = false;
					_initialRotation = _rotationReference.rotation;
					for (int i = 0; i < MaxAmountOfRotations; i++)
					{
						downwardHit = MMDebug.RayCast(_raycastOrigin, -_rotationReference.up, DownwardRaycastLength, SurfaceLayerMask, Color.red, DrawGizmos);
						if (downwardHit && (downwardHit.normal != _previousNormal))
						{
							RegisterHit(downwardHit, DownwardRaycastOffset);
							foundNewGround = true;
							break;
						}
						float searchRotationAngle = (Direction == Directions.Clockwise) ? -RaycastRotationAngleInterval : RaycastRotationAngleInterval;
						_rotationReference.Rotate(0f, 0f, searchRotationAngle);
					}

					if (!foundNewGround)
					{
						_rotationReference.rotation = _initialRotation;
					}
				}
			}
		}

		/// <summary>
		/// Computes the offset based on the current direction and rotation of our crawler
		/// </summary>
		/// <param name="t"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		protected virtual Vector2 ComputeOffset(Transform t, Vector2 offset)
		{
			if (Direction == Directions.CounterClockwise)
			{
				offset.x = -offset.x;
			}
			return _rotationReference.rotation * offset;
		}

		/// <summary>
		/// When we register a hit, we move at the specified distance and offset, and rotate our reference accordingly.
		/// </summary>
		/// <param name="hit"></param>
		/// <param name="offset"></param>
		protected virtual void RegisterHit(RaycastHit2D hit, Vector2 offset)
		{
			_movementDirection = hit.normal.MMRotate(-90f);
            
			_rotationReference.rotation = MMMaths.LookAt2D(_movementDirection);
			this.transform.position = hit.point + hit.normal * DistanceToSurface - ComputeOffset(this.transform, offset);
			_previousNormal = hit.normal;
            
			float directionModifier = (Direction == Directions.Clockwise) ? 1f : -1f;
			_movementDirection = _movementDirection.normalized * directionModifier;
		}
	}
}