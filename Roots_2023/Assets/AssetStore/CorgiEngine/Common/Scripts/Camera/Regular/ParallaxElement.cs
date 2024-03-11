using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	[ExecuteAlways]
	/// <summary>
	/// Add this to a GameObject to have it move in parallax 
	/// </summary>

	// This script is based on David Dion-Paquet's great article on http://www.gamasutra.com/blogs/DavidDionPaquet/20140601/218766/Creating_a_parallax_system_in_Unity3D_is_harder_than_it_seems.php

	[AddComponentMenu("Corgi Engine/Camera/Parallax Element")]
	public class ParallaxElement : CorgiMonoBehaviour 
	{
		/// the possible ticks at which this parallax element can update at
		public enum UpdateModes { Update, LateUpdate, FixedUpdate }

		[Header("Behaviour")]
		[MMInformation("This component will make this GameObject move in parallax (when the camera moves) if the camera's CameraController component has been set to move parallax elements. Here you can determine the relative horizontal and vertical speed, and in which direction the element should move.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

		/// the selected update mode that will determine at which tick this parallax element will run
		[Tooltip("the selected update mode that will determine at which tick this parallax element will run")]
		public UpdateModes UpdateMode = UpdateModes.LateUpdate;
		/// horizontal speed of the layer
		[Tooltip("horizontal speed of the layer")]
		public float HorizontalSpeed;
		/// vertical speed of the layer
		[Tooltip("vertical speed of the layer")]
		public float VerticalSpeed;
		/// defines if the layer moves in the same direction as the camera or not
		[Tooltip("defines if the layer moves in the same direction as the camera or not")]
		public bool MoveInOppositeDirection = true;

		// private stuff
		protected Vector3 _previousCameraPosition;
		protected bool _previousMoveParallax;
		protected ParallaxCamera _parallaxCamera;
		protected Camera _camera;
		protected Transform _cameraTransform;
		protected Vector3 _difference;
		protected Vector3 _speed;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void OnEnable() 
		{
			if (Camera.main == null)
				return;
				
			_camera = Camera.main;
			if (_camera != null)
			{
				_parallaxCamera = _camera.GetComponent<ParallaxCamera>();
				_cameraTransform = _camera.transform;		
				_previousCameraPosition = _cameraTransform.position;	
			}
		}


		/// <summary>
		/// On fixed update, we process our movement if needed
		/// </summary>
		protected virtual void FixedUpdate()
		{
			if (UpdateMode == UpdateModes.FixedUpdate)
			{
				ProcessParallax();
			}
		}


		/// <summary>
		/// On update, we process our movement if needed
		/// </summary>
		protected virtual void Update()
		{
			if (UpdateMode == UpdateModes.Update)
			{
				ProcessParallax();
			}
		}

		/// <summary>
		/// On late update, we process our movement if needed
		/// </summary>
		protected virtual void LateUpdate () 
		{
			if (UpdateMode == UpdateModes.LateUpdate)
			{
				ProcessParallax();
			}
		}

		/// <summary>
		/// Every frame, we move the parallax layer according to the camera's position
		/// </summary>
		protected virtual void ProcessParallax()
		{
			if (_parallaxCamera == null)
			{
				return;
			}

			if (!Application.isPlaying && !_parallaxCamera.MoveParallax)
			{
				return;
			}

			if (_parallaxCamera.MoveParallax && !_previousMoveParallax)
			{
				_previousCameraPosition = _cameraTransform.position;
			}

			_previousMoveParallax = _parallaxCamera.MoveParallax;

			_speed.x = HorizontalSpeed;
			_speed.y = VerticalSpeed;
			_speed.z = 0f;

			_difference = _cameraTransform.position - _previousCameraPosition;
			float direction = (MoveInOppositeDirection) ? -1f : 1f;
			transform.position += Vector3.Scale(_difference, _speed) * direction;

			_previousCameraPosition = _cameraTransform.position;
		}
	}
}