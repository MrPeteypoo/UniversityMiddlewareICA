using UnityEngine;


/// <summary>
/// GUI Sector is used to indicate the positioning of elements in terms of percentages. Once initialised it can be used to obtain pixel
/// values.
/// </summary>
[System.Serializable]
public sealed class GUISector
{
	// Unity modifiable values
	[SerializeField, Range (0f, 1f)] private float m_x = 0f;		// The x position
	[SerializeField, Range (0f, 1f)] private float m_y = 0f;		// The y position
	[SerializeField, Range (0f, 1f)] private float m_width = 0f;	// The element width
	[SerializeField, Range (0f, 1f)] private float m_height = 0f;	// The element height


	// Member variables
	private Rect m_rectangle = new Rect();


	// Properties
	public Rect rectangle
	{
		get { return m_rectangle; }
	}



	// Functions
	public void Initialise()
	{
		m_rectangle = new Rect (m_x * Screen.width, m_y * Screen.height,
		                        m_width * Screen.width, m_height * Screen.height);
	}
}
