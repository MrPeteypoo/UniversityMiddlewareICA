using UnityEngine;



/// <summary>
/// Used to kill/destroy the colliding object. It tests if the object has a Health component and if so provides maximum damage, however, if
/// otherwise the collider is the player it will trigger repawn in the last safe location. All other objects will be destroyed outright. 
/// Typically used for boundaries.
/// </summary>
public sealed class KillCollider : MonoBehaviour 
{
	private void OnTriggerEnter (Collider other)
	{
		// Handle player collisions
		if (other.CompareTag (Tags.player))
		{
			// Apply penalty when moving to safety
			other.GetComponent<PlayerController>().MoveToSafety (true);
		}

		else
		{
			// Test if other has a Health component which can be used to kill it
			Health health = other.GetComponent<Health>();
			if (health)
			{
				health.Damage (float.MaxValue);
			}
			
			else
			{
				Destroy (other.gameObject);
			}
		}
	}
}
