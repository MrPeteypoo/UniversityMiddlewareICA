using UnityEngine;



/// <summary>
/// MovingCamera is a base class used for various different types of dynamic cameras, it provides basic functionality requred to move a
/// camera on the fly.
/// </summary>
[RequireComponent (typeof (Camera))]
public class MovingCamera : MultiUpdate
{// Unity editable values
	[SerializeField, Range (0.001f, 10f)] protected float m_moveDamping = 1f;		// Used in SmoothDamp for movement
	[SerializeField, Range (0.1f, 20f)] protected float m_rotateSpeed = 2f;			// How fast to move to certain points
	[SerializeField, Range (0.001f, 0.5f)] protected float m_distanceMargin = 0.1f;	// The distance tolerance for when to stop interpolating
	[SerializeField, Range (0.001f, 0.5f)] protected float m_rotationMargin = 0.1f;	// The angular tolerance for when to stop interpolating


	// Member variables
	protected Vector3 m_targetPosition;			// Where the camera should move to
	protected Quaternion m_targetRotation;		// The desired rotation of the camera
	protected Vector3 velocity = Vector3.zero;	// Used in SmoothDamp transitions


	// Properties
	public Vector3 targetPosition
	{
		get { return m_targetPosition; }
		set { m_targetPosition = value; }
	}
	
	public Quaternion targetRotation
	{
		get { return m_targetRotation; }
		set { m_targetRotation = value; }
	}

	public new bool enabled
	{
		get { return camera.enabled; }
		set { camera.enabled = value; }
	}

	
	protected virtual void Awake ()
	{
		m_targetPosition = transform.position;
		m_targetRotation = transform.rotation;
	}

	
	// Useful if tracking physics objects
	protected override void FixedUpdate()
	{
		if (camera.enabled && m_updateType == UpdateType.FixedUpdate)
		{
			MoveCloser();
			RotateCloser();
		}
	}


	// Useful for tracking general objects
	protected override void Update()
	{
		if (camera.enabled && m_updateType == UpdateType.Update)
		{
			MoveCloser();
			RotateCloser();
		}
	}


	// Ideal when movement needs to be performed last
	protected override void LateUpdate()
	{
		if (camera.enabled && m_updateType == UpdateType.LateUpdate)
		{
			MoveCloser();
			RotateCloser();
		}
	}


	// Obtain frame-correct target values if desired
	protected void UpdateTargets (Transform target)
	{
		m_targetPosition = target.position;
		m_targetRotation = target.rotation;
	}


	// Move the camera closer to the desired position
	protected virtual void MoveCloser()
	{
		if (transform.position != m_targetPosition)
		{
			transform.position = Vector3.SmoothDamp (transform.position, m_targetPosition, ref velocity, m_moveDamping);
		}
	}


	// Rotate the camera closer to the desired rotation
	protected virtual void RotateCloser()
	{
		if (transform.rotation != m_targetRotation)
		{
			// Obtain and check the difference against the margin
			float difference = Mathf.Abs (Quaternion.Angle (transform.rotation, m_targetRotation));
			if (difference <= m_rotationMargin)
			{
				transform.rotation = m_targetRotation;
			}
			
			// Interpolate to create a smooth transition
			else
			{
				transform.rotation = Quaternion.Slerp (transform.rotation, m_targetRotation, m_rotateSpeed * Time.deltaTime);
			}
		}
	}
}
