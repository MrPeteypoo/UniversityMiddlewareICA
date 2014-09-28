using UnityEngine;
using System.Collections;



/// <summary>
/// Hint is used to provide an easy and reusable way of displaying a hint on the screen to the player.
/// </summary>
[System.Serializable]
public sealed class Hint : MonoBehaviour
{
	// Unity modifiable values
	[SerializeField, Range (0f, 1000f)] private float m_hintTimer = 2f;	// How long to wait to show the hint text
	[SerializeField] private string m_hintText = "";					// The text to display



	// Functions	
	private IEnumerator WaitForHint (InGameGUI gui)
	{
		yield return new WaitForSeconds (m_hintTimer);

		if (gui)
		{
			gui.displayHint = true;
			gui.hintText = m_hintText;
		}
	}


	public void ShowHint (InGameGUI gui)
	{
		if (gui)
		{
			StartCoroutine ("WaitForHint", gui);
		}
	}	

	
	public void HideHint (InGameGUI gui)
	{
		StopCoroutine ("WaitForHint");

		if (gui)
		{
			gui.displayHint = false;
		}
	}
}
