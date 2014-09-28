using UnityEngine;



/// <summary>
/// CameraTrigger is a class used to trigger camera transitions in-game. Given a camera object, the object will cause the given camera
/// to become the main camera used in the game.
/// </summary>
[RequireComponent (typeof (Collider))]
public sealed class CameraTrigger : MonoBehaviour 
{
	// Unity modifiable variables
	[SerializeField] private Camera m_camera;	// The camera to change to on trigger


	// References
	private CameraManager m_cameraManager;	// A refernece to the CameraManager


	// Obtain reference on load
	private void Awake()
	{
		m_cameraManager = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<CameraManager>();

		if (!m_camera || !m_cameraManager)
		{
			Debug.LogError ("Unable to detemine CameraTrigger: .m_camera || m_cameraManager.");
		}
	}


	// Change the current camera upon entry
	private void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag (Tags.player))
		{
			if (m_cameraManager.mainCamera != m_camera)
			{
				m_cameraManager.mainCamera = m_camera;
			}
		}
	}
}
