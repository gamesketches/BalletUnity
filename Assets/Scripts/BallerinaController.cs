using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallerinaController : MonoBehaviour
{
	Animator animator;
	public bool playerControlled = false;
	public MoveGuideBehavior movementGuide;
	bool holdPosition = false;
	public float rotateSpeed;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
		if(playerControlled) {
			GetInputs();
		}
    }

	void GetInputs() {
		GetButtonInputs();
		GetLegInputs();
	}

	void GetLegInputs() {
		var gamepad = Gamepad.current;
		if(gamepad == null) return;
		Vector2 leftStick = gamepad.leftStick.ReadValue();
		Vector2 rightStick = gamepad.rightStick.ReadValue();
		SetLegInputs(leftStick, rightStick);
	}

	public void SetLegInputs(Vector2 leftStick, Vector2 rightStick) {
		if(holdPosition) {
			if(Mathf.Abs(rightStick.x) > 0.1f) {
				transform.Rotate(0, rightStick.x * rotateSpeed * Time.deltaTime * -1, 0);
			}
			return;
		}
		animator.SetFloat("LeftStickX", leftStick.x);
		animator.SetFloat("LeftStickY", leftStick.y);
		animator.SetFloat("RightStickX", rightStick.x);
		animator.SetFloat("RightStickY", rightStick.y);
		float tenduDistance = Mathf.Abs(leftStick.x) + Mathf.Abs(leftStick.y);
		if(leftStick.y > 0.3f) tenduDistance = 0;
		animator.SetFloat("TenduDistance", tenduDistance);
		if(movementGuide)
			movementGuide.UpdateInputs(leftStick, rightStick);
	}

	void GetButtonInputs() {
		var gamepad = Gamepad.current;
		if(gamepad == null) return;
		if(gamepad.rightShoulder.wasPressedThisFrame) {
			if(animator.GetInteger("Position") == 1) {
				animator.SetInteger("Position", 5);
			} else {
				animator.SetInteger("Position", 1);
			}
		}
		if(gamepad.leftShoulder.wasPressedThisFrame) {
			bool positionOpen = animator.GetBool("Open");
			animator.SetBool("Open", !positionOpen);
		}
		if(gamepad.rightTrigger.isPressed) {
			holdPosition = true;
		} else {
			holdPosition = false;
		}
	}

	public Vector2 GetTenduValues() {
		return new Vector2(animator.GetFloat("LeftStickX"), animator.GetFloat("LeftStickY"));
	}
	
	public float GetPlieReleveValue() {
		return animator.GetFloat("RightStickY");
	}

	public int GetPosition() {
		return animator.GetInteger("Position");
	}

	public bool GetClosed() {
		return !animator.GetBool("Open");
	}

	public void SetPosition(int newPosition) {
		if(animator.GetInteger("Position") != newPosition) {
			animator.SetInteger("Position", newPosition);
		}
	}
}
