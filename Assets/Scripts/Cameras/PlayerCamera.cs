using UnityEngine;



/// <summary>
/// PlayerCamera is used exclusively by the player to provide third person shooter functionality. Features include input based camera
/// rotation, rotational limits and others.
/// </summary>
public sealed class PlayerCamera : FollowCamera 
{
	// Unity modifable values
	[SerializeField, Range (0.11f, 100f)] private float m_aimSpeed = 10f;		// The speed of aiming rotation
	[SerializeField, Range (-90f, 90f)] private float m_rotationMinimum = -75f;	// The minimum rotation value for the X axis
	[SerializeField, Range (-90f, 90f)] private float m_rotationMaximum = 75f;	// The maximum rotation value for the X axis


	// Member variables
	InputManager m_input;	// Stores the camera input
	float m_desiredX = 0;	// Used to prevent obtaining Euler angles which can cause clamping issues


	// Ensure m_target isn't null and rotaton values are correct
	protected override void Awake()
	{
		Transform player = GameObject.FindGameObjectWithTag (Tags.player).transform;
		if (m_target != player)
		{
			m_target = player;
			Debug.LogError ("Unable to determine PlayerCamera: .m_target.");
		}
		
		UpdateTargets (m_target);

		if (m_rotationMinimum > m_rotationMaximum)
		{
			float temp = m_rotationMinimum;
			m_rotationMinimum = m_rotationMaximum;
			m_rotationMaximum = temp;
			Debug.LogError ("Attempt to minimum angle higher than maximum angle in PlayerCamera.");
		}

		if (!(m_input = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<InputManager>()))
		{
			Debug.LogError ("Unable to determine PlayerCamera: .m_input.");
		}
	}
	
	
	// Calculate the correct position for the camera to move to
	protected override void CalculatePosition()
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
	protected override void CalculateRotation()
	{
		// Vertical rotaiton
		m_desiredX += m_input.rotateY * m_aimSpeed / 3f;
		m_desiredX = Mathf.Clamp (m_desiredX, m_rotationMinimum, m_rotationMaximum);

		// Horizontal rotation
		float desiredY = m_target.rotation.eulerAngles.y + m_input.rotateX * m_aimSpeed;

		m_targetRotation = Quaternion.Euler (m_desiredX, desiredY, m_target.rotation.eulerAngles.z + m_customRotation.z);
	}
}
