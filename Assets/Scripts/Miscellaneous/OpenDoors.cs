using UnityEngine;



/// <summary>
/// Used to trigger animation transitions of door game objects.
/// </summary>
public class OpenDoors : MonoBehaviour 
{
	// Unity modifiable variables
	public GameObject[] doors;											// The container of doors
	[SerializeField] private AudioClip m_doorAudio;						// The audio to play on setting doors
	[SerializeField, Range (0f, 1f)] private float m_doorVolume = 0.4f;	// How loud the audio is

	
	// Member variables
	private Animator m_doorAnimator;	// Used for caching
	private HashIDs m_hashes;			// Speeds up animator operation


	private void Awake()
	{
		m_hashes = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<HashIDs>();

		if (!m_hashes)
		{
			Debug.LogError ("Unable to determine OpenDoors: .m_hashes. Doors may malfunction.");
		}
	}

	public void SetDoors (bool open)
	{
		foreach (GameObject door in doors)
		{
			m_doorAnimator = door.GetComponent<Animator>();

			if (m_doorAnimator)
			{
				// Use efficient approach
				if (m_hashes)
				{
					m_doorAnimator.SetBool (m_hashes.openBool, open);
				}

				// Fallback to using a string
				else
				{
					m_doorAnimator.SetBool ("Open", open);
				}
				
				if (m_doorAudio)
				{
					AudioSource.PlayClipAtPoint (m_doorAudio, door.transform.position, m_doorVolume);
				}
			}
		}
	}
}
