using UnityEngine;



/// <summary>
/// TweenCamera is a scripted Camera component which is used as a bridge between different cameras. It's a MovingCamera with the ability to
/// track a Camera object and keep moving towards it.
/// </summary>
public sealed class TweenCamera : MovingCamera 
{
	// Unity modifiable variables
	[SerializeField] private SpeedChange m_speedUpDistance = new SpeedChange();	// Used to move faster to the target when inside the margin
	[SerializeField] private SpeedChange m_speedUpRotate = new SpeedChange();	// Used to rotate faster to the target when inside the margin

	// Member variables
	private Camera m_target;	// The target camera to follow


	// Properties
	public Camera target
	{
		get { return m_target; }
		set 
		{ 
			if (value) 
			{
				m_target = value;
				UpdateTargets (m_target.transform);
			}
		}
	}

	public bool reachedTarget
	{
		get 
		{ 
			return transform.position == m_targetPosition && transform.rotation == m_targetRotation;
		}
	}


	// Assign default values
	protected override void Awake()
	{
		// Default to the main camera
		m_target = Camera.main;

		// If unavailable just track self
		if (!m_target)
		{
			m_target = this.camera;
		}

		UpdateTargets (m_target.transform);
	}


	// Useful if tracking physics objects
	protected override void FixedUpdate() 
	{
		if (camera.enabled && m_updateType == UpdateType.FixedUpdate)
		{
			UpdateTargets (m_target.transform);
			MoveCloser();
			RotateCloser();
		}
	}


	// Useful for tracking general objects
	protected override void Update() 
	{
		if (camera.enabled && m_updateType == UpdateType.Update)
		{
			UpdateTargets (m_target.transform);
			MoveCloser();
			RotateCloser();
		}		
	}


	protected override void LateUpdate() 
	{
		if (camera.enabled && m_updateType == UpdateType.LateUpdate)
		{
			UpdateTargets (m_target.transform);
			MoveCloser();
			RotateCloser();
		}		
	}


	// Move the camera closer to the desired position
	protected override void MoveCloser()
	{
		if (transform.position != m_targetPosition)
		{
			// Obtain and check the difference against the margin
			float difference = Mathf.Abs (Vector3.Distance (transform.position, m_targetPosition));
			if (difference <= m_distanceMargin)
			{
				transform.position = m_targetPosition;
			}
			
			// Increase interpolation speed
			else if (m_speedUpDistance.changeSpeed && difference <= m_speedUpDistance.margin)
			{
				transform.position = Vector3.SmoothDamp (transform.position, m_targetPosition, ref velocity, 
				                                         m_moveDamping / m_speedUpDistance.factor);
			}
			
			// Interpolate to create a smooth transition
			else
			{
				transform.position = Vector3.SmoothDamp (transform.position, m_targetPosition, ref velocity, m_moveDamping);
			}
		}
	}


	// Rotate the camera closer to the desired rotation
	protected override void RotateCloser()
	{
		if (transform.rotation != m_targetRotation)
		{
			// Obtain and check the difference against the margin
			float difference = Mathf.Abs (Quaternion.Angle (transform.rotation, m_targetRotation));
			if (difference <= m_rotationMargin)
			{
				transform.rotation = m_targetRotation;
			}

			// Increase interpolation speed
			else if (m_speedUpRotate.changeSpeed && difference <= m_speedUpRotate.margin)
			{
				transform.rotation = Quaternion.Slerp (transform.rotation, m_targetRotation, 
				                                       m_rotateSpeed * m_speedUpRotate.factor * Time.deltaTime);
			}
			
			// Interpolate to create a smooth transition
			else
			{
				transform.rotation = Quaternion.Slerp (transform.rotation, m_targetRotation, m_rotateSpeed * Time.deltaTime);
			}
		}
	}


	// Start moving towards the target camera from the given origin
	public void ChaseCamera (Transform origin, Camera newTarget)
	{
		transform.position = origin.position;
		transform.rotation = origin.rotation;

		m_target = newTarget;
		UpdateTargets (m_target.transform);
	}
}
