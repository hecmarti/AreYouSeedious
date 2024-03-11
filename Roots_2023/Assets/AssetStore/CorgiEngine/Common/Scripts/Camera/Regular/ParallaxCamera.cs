using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to a camera to have it support parallax layers
	/// </summary>
	[AddComponentMenu("Corgi Engine/Camera/Parallax Camera")]
	public class ParallaxCamera : CorgiMonoBehaviour 
	{
		[Header("Parallax")]
		[MMInformation("If you set MoveParallax to true, the camera movement will cause parallax elements to move accordingly.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]

		/// whether or not the camera movement should impact parallax elements in the scene
		[Tooltip("whether or not the camera movement should impact parallax elements in the scene")]
		public bool MoveParallax = true;
	}
}