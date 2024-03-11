using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This class handles the rotation of the RetroCopter's model based on its current speed
	/// </summary>
	public class RetroCopter : CharacterAbility
	{
		/// The maximum rotation angle of the copter
		[Tooltip("The maximum rotation angle of the copter")]
		public float MaximumAllowedAngle = -30f;
		/// the speed at which the rotation lerps
		[Tooltip("the speed at which the rotation lerps")]
		public float CharacterRotationSpeed = 10f;

		protected CharacterFly _characterFly;
		protected GameObject _model;
		protected float _currentAngle;
		protected Quaternion _newRotation;

		/// <summary>
		/// On Start(), we set our tunnel flag to false
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_model = _character.CharacterModel;
			_characterFly = this.gameObject.MMGetComponentNoAlloc<CharacterFly>();
		}

		/// <summary>
		/// Every frame, we check if we're crouched and if we still should be
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
            
			// if we don't have a model, we do nothing and exit
			if ((_model == null) || (_characterFly == null))
			{
				return;
			}

			// determines and applies the rotation based on the controller speed
			_currentAngle = MMMaths.Remap(_controller.Speed.x, 0f, _characterFly.FlySpeed, 0f, MaximumAllowedAngle);                         
			_newRotation = Quaternion.Euler(_currentAngle * Vector3.forward);                       
			_model.transform.rotation = Quaternion.Lerp(_model.transform.rotation, _newRotation, CharacterRotationSpeed * Time.deltaTime);            
            
		}

	}
}