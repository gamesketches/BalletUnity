using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicsController : MonoBehaviour
{
	public Transform rightThigh;
	public Transform rightCalf;
	public Transform rightFoot;
	Quaternion footFlatRotation;
	Quaternion firstPosThigh;

    // Start is called before the first frame update
    void Start()
    {
        firstPosThigh = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
		var gamepad = Gamepad.current;
		if(gamepad == null) return;
		Vector2 leftStick = gamepad.leftStick.ReadValue();
		Vector2 rightStick = gamepad.rightStick.ReadValue();
		if(leftStick.y <= 0) {
			rightThigh.localRotation  = Quaternion.Euler(-90 + -Mathf.Acos(-leftStick.x) *Mathf.Rad2Deg, firstPosThigh.eulerAngles.y, Mathf.Asin(leftStick.y) *Mathf.Rad2Deg);
		} else {
			rightThigh.localRotation  = Quaternion.Euler(-90 + -Mathf.Acos(-leftStick.x) *Mathf.Rad2Deg,  Mathf.Asin(leftStick.y) *Mathf.Rad2Deg, transform.rotation.eulerAngles.z);
		}
		Vector3 rightFootAngles = rightFoot.localEulerAngles;
		Debug.Log(leftStick.magnitude);
		rightFootAngles.z = -60 + (leftStick.magnitude * 60);
		rightFoot.localEulerAngles = rightFootAngles;
		Vector3 oldAngles = rightCalf.localEulerAngles;
		oldAngles.z = 135 * gamepad.leftTrigger.ReadValue();
		rightCalf.localEulerAngles = oldAngles;
    }
}
