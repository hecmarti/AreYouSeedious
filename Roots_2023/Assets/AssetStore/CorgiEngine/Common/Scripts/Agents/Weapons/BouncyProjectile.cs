using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Projectile class that will bounce off walls instead of exploding on impact
	/// </summary>
	[AddComponentMenu("Corgi Engine/Weapons/BouncyProjectile")]
	public class BouncyProjectile : Projectile
	{
		[Header("Bounciness Tech")]
		/// the length of the raycast used to detect bounces, should be proportionate to the size and speed of your projectile
		[Tooltip("the length of the raycast used to detect bounces, should be proportionate to the size and speed of your projectile")]
		public float BounceRaycastLength = 1f;
		/// the layers you want this projectile to bounce on
		[Tooltip("the layers you want this projectile to bounce on")]
		public LayerMask BounceLayers;
		/// a feedback to trigger at every bounce
		[Tooltip("a feedback to trigger at every bounce")]
		public MMFeedbacks BounceFeedback;

		[Header("Bounciness")]
		/// the min and max amount of bounces (a value will be picked at random between both bounds)
		[Tooltip("the min and max amount of bounces (a value will be picked at random between both bounds)")]
		[MMVector("Min", "Max")]
		public Vector2Int AmountOfBounces = new Vector2Int(10, 10);
		/// the min and max speed multiplier to apply at every bounce (a value will be picked at random between both bounds)
		[Tooltip("the min and max speed multiplier to apply at every bounce (a value will be picked at random between both bounds)")]
		[MMVector("Min", "Max")]
		public Vector2 SpeedModifier = Vector2.one;

		protected Vector3 _positionLastFrame;
		protected Vector3 _raycastDirection;
		protected Vector3 _reflectedDirection;
		protected int _randomAmountOfBounces;
		protected int _bouncesLeft;
		protected float _randomSpeedModifier;

		/// <summary>
		/// On init we randomize our values, refresh our 2D collider because Unity is weird sometimes
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_randomAmountOfBounces = Random.Range(AmountOfBounces.x, AmountOfBounces.y);
			_randomSpeedModifier = Random.Range(SpeedModifier.x, SpeedModifier.y);
			_bouncesLeft = _randomAmountOfBounces;
			_collider.enabled = false;
			_collider.enabled = true;            
		}

		/// <summary>
		/// On trigger enter 2D, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>S
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// Colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void Colliding(GameObject collider)
		{
			if (!BounceLayers.MMContains(collider.layer))
			{
				return;
			}

			_raycastOrigin = _positionLastFrame;
			_raycastDirection = (this.transform.position - _positionLastFrame);

			RaycastHit2D hit = MMDebug.RayCast(_raycastOrigin, _raycastDirection, BounceRaycastLength, BounceLayers, MMColors.DarkOrange, true);
			EvaluateHit(hit);
		}

		/// <summary>
		/// If we get a prevent collision 2D message, we check if we should bounce
		/// </summary>
		/// <param name="hit"></param>
		public virtual void PreventedCollision2D(RaycastHit2D hit)
		{
			if (_health.CurrentHealth <= 0)
			{
				return;
			}
			EvaluateHit(hit);
		}

		/// <summary>
		/// Decides whether or not we should bounce
		/// </summary>
		protected virtual void EvaluateHit(RaycastHit2D hit)
		{
			if (hit)
			{
				if (_bouncesLeft > 0)
				{
					Bounce(hit);
				}
				else
				{
					_health.Kill();
					_damageOnTouch.HitNonDamageableFeedback?.PlayFeedbacks();
				}
			}
		}

		/// <summary>
		/// Applies a bounce in 2D
		/// </summary>
		/// <param name="hit"></param>
		protected virtual void Bounce(RaycastHit2D hit)
		{
			BounceFeedback?.PlayFeedbacks();
			_reflectedDirection = Vector3.Reflect(_raycastDirection, hit.normal);
			// you could then get the bounce angle like so
			// float angle = Vector3.Angle(_raycastDirection, _reflectedDirection);
			Direction = _reflectedDirection.normalized;

			this.transform.right = _spawnerIsFacingRight ? _reflectedDirection.normalized : -_reflectedDirection.normalized;
			Speed *= _randomSpeedModifier;
			_bouncesLeft--;
		}

		/// <summary>
		/// On late update we store our position
		/// </summary>
		protected virtual void LateUpdate()
		{
			_positionLastFrame = this.transform.position;
		}
	}
}