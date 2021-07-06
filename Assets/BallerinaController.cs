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
		float rightStickY = Input.GetAxis("RightStickY");
		animator.SetFloat("RightStickY", rightStickY);
		float leftStickY = Input.GetAxis("LeftStickY");
		animator.SetFloat("LeftStickY", leftStickY);
	}
}
