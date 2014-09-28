using UnityEngine;



/// <summary>
/// Certain objects such as the players torches need to modify an external GameObject's shader to give the impression of illumination.
/// ChangeShaderOnLight provides exactly this functionality.
/// </summary>
[RequireComponent (typeof (Light))]
public sealed class ChangeShaderOnLight : MonoBehaviour 
{
	// Unity modifiable variables
	[SerializeField] private GameObject m_externalObject;				// The external object to modify
	[SerializeField] private Shader m_lightOnShader;					// The shader used when the light is on
	[SerializeField] private Shader m_lightOffShader;					// The shader used when the light is off
	[SerializeField, Range (0, 25)] private int m_materialsIndex = 0;	// Which material to change


	// Member variables
	private bool m_change = false;	// Used to check whether the shader needs changing


	// Check references
	private void Awake()
	{
		if (!m_externalObject || !m_lightOnShader || !m_lightOffShader)
		{
			Debug.LogError ("Unable to determine ChangeShaderOnLight: .m_externalObject || m_lightOnshader || m_lightOffShader.");
		}

		int length = m_externalObject.renderer.materials.Length;
		if (length == 0)
		{
			Debug.LogError ("Invalid material length in ChangeShaderOnLight.m_externalObject.");
		}

		else if (m_materialsIndex > length)
		{
			m_materialsIndex = length - 1;
		}
	}


	// Update the shader based on whether the light is on or not
	private void Update()
	{
		if (light.enabled == !m_change)
		{
			m_change = !m_change;

			// Light on
			if (m_change)
			{
				m_externalObject.renderer.materials[m_materialsIndex].shader = m_lightOnShader;
			}

			// Light off
			else
			{
				m_externalObject.renderer.materials[m_materialsIndex].shader = m_lightOffShader;
			}
		}
	}
}
