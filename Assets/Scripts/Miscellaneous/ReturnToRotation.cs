using UnityEngine;


/// <summary>
/// An externally controlled script which returns the object to the specified rotation. Useful for objects that look at others but need
/// a way of stopping when the object is too far away.
/// </summary>
public sealed class ReturnToRotation : MultiUpdate
{
	// Unity modifiable variables
	[SerializeField] private Vector3 m_rotation = new Vector3 (0, 0, 0);		// The rotation to return to
	[SerializeField, Range (0.1f, 10f)] private float m_turnSpeed = 2f;			// How quickly to turn
	[SerializeField, Range (0f, 10f)] private float m_angleMargin = 0.005f;		// How far away to snap to the rotation


	// Member variables
	private Quaternion m_targetRotation;



	// Functions
	private void Awake()
	{
		// Determine actual rotation on Awake to save performance
		m_targetRotation = Quaternion.Euler (m_rotation);
	}


	// Functions
	protected override void FixedUpdate() 
	{
		if (m_updateType == UpdateType.FixedUpdate)
		{
			Rotate();
			CheckRotation();
		}
	}


	protected override void Update() 
	{
		if (m_updateType == UpdateType.Update)
		{
			Rotate();
			CheckRotation();
		}
	}


	protected override void LateUpdate() 
	{
		if (m_updateType == UpdateType.LateUpdate)
		{
			Rotate();
			CheckRotation();
		}
	}


	private void Rotate()
	{
		transform.rotation = Quaternion.Slerp (transform.rotation, m_targetRotation, m_turnSpeed * Time.deltaTime);
	}


	private void CheckRotation()
	{
		float difference = Mathf.Abs (Quaternion.Angle (transform.rotation, m_targetRotation));

		if (difference <= m_angleMargin)
		{
			transform.rotation = m_targetRotation;

			// Automatically disable self
			enabled = false;
		}
	}


	public void Return()
	{
		enabled = true;
	}
}
