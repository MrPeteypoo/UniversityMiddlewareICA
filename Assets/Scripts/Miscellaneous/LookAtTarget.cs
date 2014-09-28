using UnityEngine;



/// <summary>
/// LookAtTarget provides the ability to spherically rotate in the direction of the target. This can be turned off entirely or limited to
/// horizontal rotation, vertical rotation of both. Useful for dynamic cameras, enemies, etc.
/// </summary>
public sealed class LookAtTarget : MultiUpdate
{
	// Unity modifable values
	[SerializeField] private Transform m_target;					// The players current transform
	[SerializeField, Range (0.1f, 10f)] private float m_speed = 1f;	// How fast the camera will rotate
	[SerializeField] private Vector3 m_customOffset;				// Used in stopping the camera looking at the characters feet.
	[SerializeField] private bool m_horizontalRotation = true;		// Should the camera rotate horizontally or not
	[SerializeField] private bool m_verticalRotation = true;		// Should the camera rotate vertically or not


	// Check reference to transform
	private void Awake() 
	{
		if (!m_target)
		{
			Debug.LogError ("Unable to determine LookAtTarget: .m_target.");
		}
	}
	
	
	// Useful if tracking physics objects
	protected override void FixedUpdate() 
	{
		if (m_updateType == UpdateType.FixedUpdate)
		{
			SlerpToTarget();
		}
	}
	
	
	// Useful for tracking general objects
	protected override void Update() 
	{
		if (m_updateType == UpdateType.Update)
		{
			SlerpToTarget();
		}
	}
	
	
	// Ideal when rotation needs to be performed last
	protected override void LateUpdate() 
	{
		if (m_updateType == UpdateType.LateUpdate)
		{
			SlerpToTarget();
		}
	}


	private void SlerpToTarget()
	{
		// Calculate desired direction
		Vector3 direction = m_target.position - transform.position;

		// Add the chosen offset to effect the height of the rotation
		direction += m_customOffset;

		// Correct magnitude
		if (direction.magnitude > 1)
		{
			direction.Normalize();
		}

		// Disable horizontal rotation
		if (!m_horizontalRotation)
		{
			direction.x = transform.forward.x;
			direction.z = transform.forward.z;
		}

		// Disable vertical rotation
		if (!m_verticalRotation)
		{
			direction.y = transform.forward.y;
		}

		if (direction != Vector3.zero)
		{
			// Slerp the direction to create fluid movement
			Quaternion lookAt = Quaternion.LookRotation (direction);
			transform.rotation = Quaternion.Slerp (transform.rotation, lookAt, m_speed * Time.deltaTime);
		}
	}
}
