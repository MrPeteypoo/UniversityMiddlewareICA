using UnityEngine;


/// <summary>
/// The Torch class is used for torch-like dynamic lighting systems in the game.
/// </summary>
[RequireComponent (typeof (Light))]
public sealed class Torch : MonoBehaviour 
{
	// Unity modifiable variables
	[SerializeField, Range (0f, 8f)] private float m_maxIntensity = 1f;	// How intense the lighting can be
	[SerializeField, Range (0f, 8f)] private float m_minIntensity = 0f;	// The minimum intensity of the lighting
	[SerializeField, Range (0.1f, 10f)] private float m_smoothing = 1f;	// How long to smooth the intensity change

	[SerializeField] private bool m_pingPong = false;	// Cause the intensity to ping-pong between intensities


	// Member variables
	private float m_currentSmooth = 0f;		// Used for SmoothDamp velocity
	private float m_targetIntensity = 0f;	// The current target intensity
	private bool m_lightOn = false;			// Indicates whether the light is on

	private Behaviour m_halo;				// An unfortunate way to access the Halo component

	// Properties
	public bool isOn { get { return m_lightOn; } }
	public bool pingPong { get { return m_pingPong; } set { m_pingPong = value; } }



	// Set default values
	private void Awake() 
	{
		m_halo = (Behaviour) GetComponent ("Halo");

		if (m_maxIntensity < m_minIntensity)
		{
			float temp = m_maxIntensity;
			m_maxIntensity = m_minIntensity;
			m_minIntensity = temp;
		}

		if (light.intensity == 0f)
		{
			m_lightOn = false;
		}

		else
		{
			m_lightOn = true;
			if (light.intensity > m_minIntensity)
			{
				m_targetIntensity = m_maxIntensity;
			}

			else
			{
				m_targetIntensity = m_minIntensity;
			}
		}

		light.enabled = m_lightOn;

		// Not all objects will have Halo components
		if (m_halo) { m_halo.enabled = m_lightOn; }
	}


	// Ensure the light is heading towards the correct intensity
	private void Update() 
	{
		if (light.intensity != m_targetIntensity)
		{
			// Avoid precision problems by checking for approximation
			float difference = Mathf.Abs (light.intensity - m_targetIntensity);

			if (difference <= 0.05f)
			{
				if (m_pingPong && m_targetIntensity != 0f)
				{
					AlternateIntensity (true);
				}

				else
				{
					if (m_targetIntensity == m_maxIntensity)
					{
						light.intensity = m_maxIntensity;
					}

					else if (m_targetIntensity == m_minIntensity)
					{
						light.intensity = m_minIntensity;
					}

					else { light.intensity = 0f; }
				}
			}

			// SmoothDamp the light intensity
			else
			{
				light.intensity = Mathf.SmoothDamp (light.intensity, m_targetIntensity, ref m_currentSmooth, m_smoothing);
			}
		}

		else
		{
			if (m_pingPong)
			{
				AlternateIntensity (true);
			}
		}

		// Disable light if it isn't turned on
		m_lightOn = light.intensity == 0f ? false : true;
		light.enabled = m_lightOn;

		if (m_halo) { m_halo.enabled = m_lightOn; }
	}


	public void AlternateIntensity (bool smoothTransition)
	{
		if (m_targetIntensity == m_minIntensity || m_targetIntensity == 0f)
		{
			if (!smoothTransition) { light.intensity = m_maxIntensity; }
			m_targetIntensity = m_maxIntensity;
			light.enabled = true;
		}

		else
		{
			if (!smoothTransition) { light.intensity = m_minIntensity; }
			m_targetIntensity = m_minIntensity;
		}
	}


	public void TurnOn (bool fade)
	{
		// If !fade set light to max intensity
		if (!fade) { light.intensity = m_maxIntensity; }
		m_targetIntensity = m_maxIntensity;
		light.enabled = true;
	}


	public void TurnOff (bool fade)
	{
		// If !fade set light to min intensity
		if (!fade) 
		{ 
			light.intensity = 0f;
			light.enabled = false;
		}
		m_targetIntensity = 0f;
	}
}
