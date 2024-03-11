using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// The CorgiMonoBehaviour class is a base class for all Corgi Engine classes.
	/// It doesn't do anything, but ensures you have a single point of change should you want your classes to inherit from something else than a plain MonoBehaviour
	/// A frequent use case for this would be adding a network layer
	/// </summary>
	public class CorgiMonoBehaviour : MonoBehaviour { }	
}