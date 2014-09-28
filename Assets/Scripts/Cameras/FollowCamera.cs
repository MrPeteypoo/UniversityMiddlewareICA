using UnityEngine;



/// <summary>
/// FollowCamera is used to provide a dynamic, moving camera whose specific purpose is to track an object and follow it. FollowCamera objects
/// will follow their target regardless of whether the camera is enabled or not.
/// </summary>
public class FollowCamera : MovingCamera
{
	// Unity modifiable variables
	[SerializeField] protected Transform m_target;			// The target to follow
	[SerializeField] protected Vector3 m_positionOffset;	// The offset for the camera relative to m_target
	[SerializeField] protected Vector3 m_customRotation;		// A custom rotation to add to the targets rotation
	

	// Ensure m_target isn't null
	protected override void Awake()
	{
		if (!m_target)
		{
			m_target = transform;
			Debug.LogError ("Unable to determine FollowCamera: .m_target.");
		}

		UpdateTargets (m_target);
	}
	
	
	// Useful if tracking physics objects
	protected override void FixedUpdate()
	{
		if (m_updateType == UpdateType.FixedUpdate)
		{
			CalculatePosition();
			CalculateRotation();

			MoveCloser();
			RotateCloser();
		}
	}
	
	
	// Useful for tracking general objects
	protected override void Update()
	{
		if (m_updateType == UpdateType.Update)
		{
			CalculatePosition();
			CalculateRotation();

			MoveCloser();
			RotateCloser();
		}
	}
	
	
	// Ideal when movement needs to be performed last
	protected override void LateUpdate()
	{
		if (m_updateType == UpdateType.LateUpdate)
		{
			CalculatePosition();
			CalculateRotation();

			MoveCloser();
			RotateCloser();
		}
	}


	// Calculate the correct position for the camera to move to
	protected virtual void CalculatePosition()
	{
		float x = Helper.MaxRaycastDistance (m_target.position, m_target.right, m_positionOffset.x);
		float y = Helper.MaxRaycastDistance (m_target.position, m_target.up, m_positionOffset.y);
		float z = Helper.MaxRaycastDistance (m_target.position, m_target.forward, m_positionOffset.z);

		m_targetPosition = 	m_target.position +
						   	m_target.right * x + 
						   	m_target.up * y +
						   	m_target.forward * z;
	}


	// Calculate the correct rotation for the camera to look at
	protected virtual void CalculateRotation()
	{
		// Create the new rotation
		m_targetRotation = Quaternion.Euler (m_target.rotation.eulerAngles.x + m_customRotation.x,
			                                 m_target.rotation.eulerAngles.y + m_customRotation.y,
			                                 m_target.rotation.eulerAngles.z + m_customRotation.z);

	}
}
