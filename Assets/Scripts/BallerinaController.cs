using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallerinaController : MonoBehaviour
{
	Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
		GetInputs();
    }

	void GetInputs() {
		GetLegInputs();
		GetButtonInputs();
	}

	void GetLegInputs() {
		var gamepad = Gamepad.current;
		if(gamepad == null) return;
		if(gamepad.rightShoulder.wasPressedThisFrame) Debug.Log("Saw the right shoulder press!!");
		Debug.Log(gamepad.rightStick.ReadValue());
		Vector2 leftStick = gamepad.leftStick.ReadValue();
		Vector2 rightStick = gamepad.rightStick.ReadValue();
		animator.SetFloat("LeftStickX", leftStick.x);
		animator.SetFloat("LeftStickY", leftStick.y);
		animator.SetFloat("RightStickX", rightStick.x);
		animator.SetFloat("RightStickY", rightStick.y);
		/*float rightStickX = Input.GetAxis("RightStickX");
		animator.SetFloat("RightStickX", rightStickX);
		float rightStickY = Input.GetAxis("RightStickY");
		animator.SetFloat("RightStickY", rightStickY);
		float leftStickX = Input.GetAxis("LeftStickX");
		animator.SetFloat("LeftStickX", leftStickX);
		float leftStickY = Input.GetAxis("LeftStickY");
		animator.SetFloat("LeftStickY", leftStickY);
		*/
	}

	void GetButtonInputs() {
		var gamepad = Gamepad.current;
		if(gamepad == null) return;
		if(gamepad.rightShoulder.wasPressedThisFrame) {
		//if(Input.GetButtonDown("SwitchPosition")) {
			if(animator.GetInteger("Position") == 1) {
				animator.SetInteger("Position", 5);
			} else {
				animator.SetInteger("Position", 1);
			}
		}
	}
}
