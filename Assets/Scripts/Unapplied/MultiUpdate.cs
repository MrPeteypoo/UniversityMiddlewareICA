using UnityEngine;



/// <summary>
/// MultiUpdate is an abstract class to be used to enable configurable update functionality. The desired update functionality can be modified
/// in the Unity editor and derived classes must implement each method.
/// </summary>
public abstract class MultiUpdate : MonoBehaviour
{
	// Used to determine which update method to use
	protected enum UpdateType
	{
		FixedUpdate, Update, LateUpdate
	}


	// Unity modifable variables
	[SerializeField] protected UpdateType m_updateType = UpdateType.Update;	// The update method to use


	// Require implementations by child classes
	protected abstract void FixedUpdate();
	protected abstract void Update();
	protected abstract void LateUpdate();
}
