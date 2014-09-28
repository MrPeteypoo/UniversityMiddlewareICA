using UnityEngine;
using System.Collections;



/// <summary>
/// InputManager is a container class used to store the current and previous state of all input, this allows for more natural feeling input
/// and prevents issues such as toggle keys needing to be let go in a single frame. Each variable is an array,
/// element 0 = current and element 1 = previous
/// </summary>
public sealed class InputManager : MonoBehaviour 
{
	// Movement input
	private float[] m_moveX = {0f, 0f}, m_moveY = {0f, 0f},
					m_rotateX = {0f, 0f}, m_rotateY = {0f, 0f},
					m_changeWeapon = {0f, 0f};


	// Character input
	private bool[] m_walk = {false, false}, m_crouch = {false, false}, m_jump = {false, false}, 
				   m_torch = {false, false}, m_aim = {false, false}, m_shoot = {false, false},
				   m_fireMode = {false, false}, m_reload = {false, false};
					

	// Gameplay input
	private bool[] m_use = {false, false}, m_pause = {false, false};


	// Properties
	public float moveX { get { return m_moveX[0]; } }
	public float prevMoveX { get { return m_moveX[1]; } }
	
	public float moveY { get { return m_moveY[0]; } }
	public float prevMoveY { get { return m_moveY[1]; } }
	
	public float rotateX { get { return m_rotateX[0]; } }
	public float prevRotateX { get { return m_rotateX[1]; } }
	
	public float rotateY { get { return m_rotateY[0]; } }
	public float prevRotateY { get { return m_rotateY[1]; } }
	
	public float changeWeapon { get { return m_changeWeapon[0]; } }
	public float prevChangeWeapon { get { return m_changeWeapon[1]; } }
	
	public bool walk { get { return m_walk[0]; } }
	public bool prevWalk { get { return m_walk[1]; } }
	
	public bool crouch { get { return m_crouch[0]; } }
	public bool prevCrouch { get { return m_crouch[1]; } }
	
	public bool jump { get { return m_jump[0]; } }
	public bool prevJump { get { return m_jump[1]; } }
	
	public bool torch { get { return m_torch[0]; } }
	public bool prevTorch { get { return m_torch[1]; } }
	
	public bool aim { get { return m_aim[0]; } }
	public bool prevAim { get { return m_aim[1]; } }
	
	public bool shoot { get { return m_shoot[0]; } }
	public bool prevShoot { get { return m_shoot[1]; } }
	
	public bool fireMode { get { return m_fireMode[0]; } }
	public bool prevFireMode { get { return m_fireMode[1]; } }

	public bool reload { get { return m_reload[0]; } }
	public bool prevReload { get { return m_reload[1]; } }
	
	public bool use { get { return m_use[0]; } }
	public bool prevUse { get { return m_use[1]; } }
	
	public bool pause { get { return m_pause[0]; } }
	public bool prevPause { get { return m_pause[1]; } }



	private void Update()
	{
		AssignPreviousInput();
		GetCurrentInput();
	}


	private void AssignPreviousInput()
	{
		m_moveX[1] = m_moveX[0];
		m_moveY[1] = m_moveY[0];
		m_rotateX[1] = m_rotateX[0];
		m_rotateY[1] = m_rotateY[0];
		m_changeWeapon[1] = m_changeWeapon[0];

		m_walk[1] = m_walk[0];
		m_crouch[1] = m_crouch[0];
		m_jump[1] = m_jump[0];
		m_torch[1] = m_torch[0];
		m_aim[1] = m_aim[0];
		m_shoot[1] = m_shoot[0];
		m_fireMode[1] = m_fireMode[0];
		m_reload[1] = m_reload[0];
		
		m_use[1] = m_use[0];
		m_pause[1] = m_pause[0];
	}


	private void GetCurrentInput()
	{
		// Axis
		m_moveX[0] = Mathf.Clamp (Input.GetAxis ("MoveX"), -1f, 1f);
		m_moveY[0] = Mathf.Clamp (Input.GetAxis ("MoveY"), -1f, 1f);
		m_rotateX[0] = Mathf.Clamp (Input.GetAxis ("RotateX"), -1f, 1f);
		m_rotateY[0] = Mathf.Clamp (Input.GetAxis ("RotateY"), -1f, 1f);
		m_changeWeapon[0] = Mathf.Clamp (Input.GetAxis ("ChangeWeapon"), -1f, 1f);

		// Buttons
		m_walk[0] = Input.GetButton ("Walk");
		m_crouch[0] = Input.GetButton ("Crouch");
		m_jump[0] = Input.GetButton ("Jump");
		m_torch[0] = Input.GetButton ("Torch");
		m_aim[0] = Input.GetButton ("Aim");
		m_shoot[0] = Input.GetButton ("Shoot");
		m_fireMode[0] = Input.GetButton ("FireMode");
		m_reload[0] = Input.GetButton ("Reload");

		m_use[0] = Input.GetButton ("Use");
		m_pause[0] = Input.GetButton ("Pause");
	}
}
