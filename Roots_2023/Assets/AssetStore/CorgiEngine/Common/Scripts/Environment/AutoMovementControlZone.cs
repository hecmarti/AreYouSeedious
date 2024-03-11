using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This zone lets you impact a Character equipped with an AutoMovement ability
	/// You'll be able to use it to change its direction, stop it in its tracks, or make it start/stop running
	/// </summary>
	[AddComponentMenu("Corgi Engine/Environment/AutoMovementControlZone")]
	[RequireComponent(typeof(Collider2D))]
	public class AutoMovementControlZone : CorgiMonoBehaviour
	{
		/// the possible modes you can control direction with
		public enum DirectionModes { None, Toggle, ForceLeft, ForceRight, ForceStop }
		/// the possible modes you can change the run state
		public enum RunModes { None, Toggle, ForceRun, ForceWalk }

		[Header("Auto Movement Control")]

		/// the selected direction mode
		/// none : will be ignored
		/// toggle : makes the character change direction (from right to left or from left to right)
		/// force left : makes the character go left
		/// force right : makes the character go right
		/// force stop : stops the character
		[Tooltip("the selected direction mode\n"+
		         "- none : will be ignored\n"+
		         "- toggle : makes the character change direction (from right to left or from left to right)\n" +
		         "- force left : makes the character go left\n" +
		         "- force right : makes the character go right\n" +
		         "- force stop : stops the character")]
		public DirectionModes DirectionMode = DirectionModes.Toggle;
		/// the selected run mode
		/// none : does nothing
		/// toggle : runs if walking, walks if running
		/// force run : makes the character run
		/// force walk : makes the character walk
		[Tooltip("the selected run mode\n"+
		         "- none : does nothing\n" +
		         "- toggle : runs if walking, walks if running\n" +
		         "- force run : makes the character run\n"+
		         "- force walk : makes the character walk")]
		public RunModes RunMode = RunModes.None;

		protected Collider2D _collider2D;
		protected CharacterAutoMovement _characterAutoMovement;

		/// <summary>
		/// On awake grabs the Collider2D and sets it correctly to is trigger
		/// </summary>
		protected virtual void Awake()
		{
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			_collider2D.isTrigger = true;
		}

		/// <summary>
		/// On trigger enter, we handle our collision
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			HandleCollision(collider);
		}

		/// <summary>
		/// Tests if we're colliding with a CharacterAutoMovement and interacts with it if needed
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void HandleCollision(Collider2D collider)
		{
			_characterAutoMovement = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterAutoMovement>();

			if (_characterAutoMovement == null)
			{
				return;
			}

			switch (DirectionMode)
			{
				case DirectionModes.Toggle:
					_characterAutoMovement.ChangeDirection();
					break;
				case DirectionModes.ForceLeft:
					_characterAutoMovement.ForceDirection(-1f);
					break;
				case DirectionModes.ForceRight:
					_characterAutoMovement.ForceDirection(1f);
					break;
				case DirectionModes.ForceStop:
					_characterAutoMovement.ForceDirection(0f);
					break;
			}

			switch (RunMode)
			{
				case RunModes.Toggle:
					_characterAutoMovement.ToggleRun();
					break;
				case RunModes.ForceRun:
					_characterAutoMovement.ForceRun(true);
					break;
				case RunModes.ForceWalk:
					_characterAutoMovement.ForceRun(false);
					break;
			}
		}
	}    
}