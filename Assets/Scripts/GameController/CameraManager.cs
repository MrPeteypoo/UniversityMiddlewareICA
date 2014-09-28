using UnityEngine;
using System.Collections;



/// <summary>
/// The CameraManager class is used to keep track of the primary cameras in the scene and which should be used for the levels. 
/// Camera transitions can be triggered externally but the CameraManager specifically manages the interpolated transitions between these.
/// </summary>
[RequireComponent (typeof (GUITexture))]
public sealed class CameraManager : MonoBehaviour 
{
	// Unity modifable variables
	[SerializeField] private Camera m_mainCamera;									// The main camera currently in use
	[SerializeField, Range (0f, 10f)] private float m_cinematicFadeTime = 0.02f;	// How long to fade to black
	[SerializeField, Range (0f, 1f)] private float m_fadeMargin = 0.005f;			// When to snap the the target


	// Member variables
	private TweenCamera m_tweenCamera;			// The camera used to interpolate between the desired camera
	private Camera m_safeCamera;				// The safety camera used when the player teleports to safety
	private Camera m_cinematicCamera;			// Used for all cinematic work in the game


	// Properties
	public Camera mainCamera
	{
		get { return m_mainCamera; }
		set 
		{
			if (value)
			{
				// Disable the old camera
				m_mainCamera.enabled = false;
				m_mainCamera.GetComponent<LookAtTarget>().enabled = false;
				
				// Enable the new camera
				value.enabled = false;
				value.GetComponent<LookAtTarget>().enabled = true;

				// Set the new camera
				if (m_tweenCamera.target == m_mainCamera) 
				{
					m_tweenCamera.target = value;
					m_tweenCamera.enabled = true;
				}
				m_mainCamera = value;
			}

			else
			{
				Debug.LogError ("Attempt to set invalid CameraManager.m_cameraMain.");
			}
		}
	}


	public Camera currentCamera
	{
		get { return m_tweenCamera.target; }
	}


	public bool fading
	{
		get { return guiTexture.color.a != 0f && guiTexture.color.a != 1f; }
	}


	public Color fade
	{
		get { return guiTexture.color; }
	}


	// Use this for initialization
	private void Awake() 
	{
		guiTexture.pixelInset = new Rect (0f, 0f, Screen.width, Screen.height);
		guiTexture.color = Color.clear;
		m_tweenCamera = GameObject.FindGameObjectWithTag (Tags.tweenCamera).GetComponent<TweenCamera>();

		if (!m_mainCamera)
		{
			m_mainCamera = Camera.main;
			Debug.LogError ("Invalid default main camera.");
		}

		// Create a TweenCamera on Awake if necessary
		if (!m_tweenCamera)
		{
			GameObject temp = new GameObject ("camera_tween");
			temp.AddComponent ("TweenCamera");
			temp.tag = Tags.tweenCamera;
			temp.camera.CopyFrom (m_mainCamera);

			m_tweenCamera = temp.GetComponent<TweenCamera>();
		}

		// Disable all cameras in the scene
		foreach (Camera cam in Camera.allCameras)
		{
			if (!cam.CompareTag (Tags.textureCamera))
			{
				cam.enabled = false;
				LookAtTarget script = cam.GetComponent<LookAtTarget>();
				if (script) { script.enabled = false; }
			}
		}
		
		m_mainCamera.enabled = true;
		m_mainCamera.GetComponent<LookAtTarget>().enabled = true;
		m_tweenCamera.ChaseCamera (m_mainCamera.transform, m_mainCamera);
		m_safeCamera = m_mainCamera;
		m_cinematicCamera = m_mainCamera;
	}


	// Use LateUpdate to check the temporary camera is at the correct position
	private void LateUpdate() 
	{
		// If interpolating between cameras check if either of the static cameras should be enabled
		if (m_tweenCamera.enabled && m_tweenCamera.reachedTarget)
		{
			m_tweenCamera.enabled = false;

			if (m_tweenCamera.target == m_mainCamera) { m_mainCamera.enabled = true; }
		}
	}


	public void UpdateSafetyCamera()
	{
		m_safeCamera = m_mainCamera;
	}


	public void RevertToSafetyCamera()
	{
		if (m_safeCamera)
		{
			mainCamera = m_safeCamera;
		}
	}


	public void StartCinematic (Camera cinematic)
	{
		m_mainCamera.enabled = false;
		m_tweenCamera.enabled = false;

		m_cinematicCamera.enabled = false;
		m_cinematicCamera = cinematic;
		m_cinematicCamera.enabled = true;
	}


	public void EndCinematic()
	{
		m_mainCamera.enabled = true;
		m_tweenCamera.enabled = false;
		m_cinematicCamera.enabled = false;
	}


	public void FadeIn()
	{
		StartCoroutine (Fade (.5f));
	}


	public void FadeOut()
	{
		StartCoroutine (Fade (0f));
	}


	private IEnumerator Fade (float value)
	{
		while (Mathf.Abs (guiTexture.color.a - value) > m_fadeMargin)
		{
			yield return guiTexture.color = value == 0f ? 
							Color.Lerp (guiTexture.color, Color.clear, m_cinematicFadeTime * Time.deltaTime) :
							Color.Lerp (guiTexture.color, Color.black, m_cinematicFadeTime * Time.deltaTime);

		}

		guiTexture.color = value == 0f ? Color.clear : Color.black;
	}
}
