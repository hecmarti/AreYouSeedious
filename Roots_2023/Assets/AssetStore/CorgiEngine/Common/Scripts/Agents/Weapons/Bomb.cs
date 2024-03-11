using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A class used to trigger a damage area zone of the selected shape (rectangle or circle) after the defined time before explosion.
	/// Typically used for grenades.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Weapons/Bomb")]
	public class Bomb : CorgiMonoBehaviour 
	{
		/// the possible types of shapes for damage areas
		public enum DamageAreaShapes { Rectangle, Circle }

		[Header("Bindings")] 
		/// the renderer this script will pilot - if left empty, will try to grab this component on itself
		[Tooltip("the renderer this script will pilot - if left empty, will try to grab this component on itself")]
		public Renderer TargetRenderer; 
		/// the poolable object component this script will pilot - if left empty, will try to grab this component on itself
		[Tooltip("the poolable object component this script will pilot - if left empty, will try to grab this component on itself")]
		public MMPoolableObject TargetPoolableObject;

		[Header("Explosion")]

		/// the duration(in seconds) before the explosion
		[Tooltip("the duration(in seconds) before the explosion")]
		public float TimeBeforeExplosion = 2f;
		/// the MMFeedbacks to trigger on explosion
		[Tooltip("the MMFeedbacks to trigger on explosion")]
		public MMFeedbacks ExplosionFeedback;

		[Header("Flicker")]

		/// whether or not the sprite attached to this bomb should flicker before exploding
		[Tooltip("whether or not the sprite attached to this bomb should flicker before exploding")]
		public bool FlickerSprite = true;
		/// the time (in seconds) before the flicker
		[Tooltip("the time (in seconds) before the flicker")]
		public float TimeBeforeFlicker = 1f;

		[Header("Damage Area")]

		/// the collider that defines the damage area
		[Tooltip("the collider that defines the damage area")]
		public Collider2D DamageAreaCollider;
		/// the duration (in seconds) during which the damage area should be active
		[Tooltip("the duration (in seconds) during which the damage area should be active")]
		public float DamageAreaActiveDuration = 1f;

		protected float _timeSinceStart;
		protected bool _flickering;
		protected bool _damageAreaActive;
		protected Color _initialColor;
		protected Color _flickerColor = new Color32(255, 20, 20, 255);
		protected bool _rendererIsNotNull;

		/// <summary>
		/// On enable we initialize our bomb
		/// </summary>
		protected virtual void OnEnable()
		{
			Initialization ();
		}

		/// <summary>
		/// Grabs renderer and pool components
		/// </summary>
		protected virtual void Initialization()
		{
			if (DamageAreaCollider == null)
			{
				Debug.LogWarning ("There's no damage area associated to this bomb : " + this.name + ". You should set one via its inspector.");
				return;
			}
			DamageAreaCollider.isTrigger = true;
			DisableDamageArea ();

			if (TargetRenderer == null)
			{
				TargetRenderer = gameObject.MMGetComponentNoAlloc<Renderer> ();	
			}
			
			_rendererIsNotNull = TargetRenderer != null;
			
			if (_rendererIsNotNull)
			{
				if (TargetRenderer.material.HasProperty("_Color"))
				{
					_initialColor = TargetRenderer.material.color;
				}
			}

			if (TargetPoolableObject == null)
			{
				TargetPoolableObject = gameObject.MMGetComponentNoAlloc<MMPoolableObject> ();	
			}
			
			if (TargetPoolableObject != null)
			{
				TargetPoolableObject.LifeTime = 0;
			}

			_timeSinceStart = 0;
			_flickering = false;
			_damageAreaActive = false;
		}

		/// <summary>
		/// On Update we handle our cooldowns and activate the bomb if needed
		/// </summary>
		protected virtual void Update()
		{
			_timeSinceStart += Time.deltaTime;
			// flickering
			if (_timeSinceStart >= TimeBeforeFlicker)
			{
				if (!_flickering && FlickerSprite)
				{
					// We make the bomb's sprite flicker
					if (TargetRenderer != null)
					{
						StartCoroutine(MMImage.Flicker(TargetRenderer,_initialColor,_flickerColor,0.05f,(TimeBeforeExplosion - TimeBeforeFlicker)));	
					}
				}
			}

			// activate damage area
			if (_timeSinceStart >= TimeBeforeExplosion && !_damageAreaActive)
			{
				EnableDamageArea ();
				if (_rendererIsNotNull)
				{
					TargetRenderer.enabled = false;	
				}
				ExplosionFeedback?.PlayFeedbacks();
				_damageAreaActive = true;
			}

			if (_timeSinceStart >= TimeBeforeExplosion + DamageAreaActiveDuration)
			{
				DestroyBomb ();
			}
		}

		/// <summary>
		/// On destroy we disable our object and handle pools
		/// </summary>
		protected virtual void DestroyBomb()
		{
			if (_rendererIsNotNull)
			{
				TargetRenderer.enabled = true;
				TargetRenderer.material.color = _initialColor;	
			}
			
			if (TargetPoolableObject != null)
			{
				TargetPoolableObject.Destroy ();	
			}
			else
			{
				Destroy(this.gameObject);
			}

		}

		/// <summary>
		/// Enables the damage area.
		/// </summary>
		protected virtual void EnableDamageArea()
		{
			DamageAreaCollider.enabled = true;
		}

		/// <summary>
		/// Disables the damage area.
		/// </summary>
		protected virtual void DisableDamageArea()
		{
			DamageAreaCollider.enabled = false;
		}
	}
}