using UnityEngine;



/// <summary>
/// Helper is a class dedicated entirely to providing useful helper functions which may be required in various different classes.
/// </summary>
public sealed class Helper 
{
	// Calculate and return a Vector3 containing the correct offset values to add
	public static float MaxRaycastDistance (Vector3 origin, Vector3 direction, float distance)
	{
		bool returnNegative = false;

		// Pre-condition: Desired distance is greater than 0
		if (distance < 0)
		{
			direction = -direction;
			distance = -distance;
			returnNegative = true;
		}

		// Try raycasting the maximum distance
		RaycastHit hit;
		if (distance != 0 && Physics.Raycast (origin, direction, out hit, distance))
		{
			return returnNegative ? -hit.distance : hit.distance;
		}
		
		// Maximum distance successful
		else
		{
			return returnNegative ? -distance : distance;
		}
	}
}
