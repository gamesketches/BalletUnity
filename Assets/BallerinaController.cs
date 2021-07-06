using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		float rightStickX = Input.GetAxis("RightStickX");
		animator.SetFloat("RightStickX", rightStickX);
		float rightStickY = Input.GetAxis("RightStickY");
		animator.SetFloat("RightStickY", rightStickY);
		float leftStickX = Input.GetAxis("LeftStickX");
		animator.SetFloat("LeftStickX", leftStickX);
		float leftStickY = Input.GetAxis("LeftStickY");
		animator.SetFloat("LeftStickY", leftStickY);
	}

	void GetButtonInputs() {
		if(Input.GetButtonDown("SwitchPosition")) {
			if(animator.GetInteger("Position") == 1) {
				animator.SetInteger("Position", 5);
			} else {
				animator.SetInteger("Position", 1);
			}
		}
	}
}
