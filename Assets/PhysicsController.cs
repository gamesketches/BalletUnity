using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PhysicsController : MonoBehaviour
{
	public Transform bodyTransform;
	public Leg rightLeg;
	public Leg leftLeg;
	public Text joystickOutput;
	Leg workingLeg;
	Leg supportingLeg;
	Quaternion footFlatRotation;
	Quaternion firstPosThigh;

    // Start is called before the first frame update
    void Start()
    {
		workingLeg = rightLeg;
		supportingLeg = leftLeg;
        firstPosThigh = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
		var gamepad = Gamepad.current;
		if(gamepad == null) return;
		Vector2 leftStick = gamepad.leftStick.ReadValue();
		Vector2 rightStick = gamepad.rightStick.ReadValue();
		MoveData curMove = MoveInterpreter.instance.AssessMoveType(leftStick.x, leftStick.y);
		Debug.Log("X: " + leftStick.x.ToString() + ", Y: " + leftStick.y.ToString());
		if(curMove != null) {
			Debug.Log("found the move");
			joystickOutput.text = curMove.displayString;
		}
		else {
			joystickOutput.text = "";
		}
		UpdateWorkingThigh(leftStick, curMove);
		UpdateWorkingCalf(gamepad.leftTrigger.ReadValue());
		UpdateSupportingLeg(rightStick);
    }

	void UpdateWorkingThigh(Vector2 joystickVals, MoveData curMove) {
		float thighRotation = curMove == null ? firstPosThigh.eulerAngles.y : curMove.thighRotation;
		if(joystickVals.y <= 0) {
			workingLeg.thigh.localRotation  = 
					Quaternion.Euler(-90 + -Mathf.Acos(-joystickVals.x) *Mathf.Rad2Deg, 
										thighRotation,
										//firstPosThigh.eulerAngles.y, 
											Mathf.Asin(joystickVals.y) *Mathf.Rad2Deg);
		} else {
			workingLeg.thigh.localRotation  = 
					Quaternion.Euler(-90 + -Mathf.Acos(joystickVals.x) *Mathf.Rad2Deg,  
										thighRotation,
						//transform.rotation.eulerAngles.y, 
							-90 - Mathf.Asin(joystickVals.y) *Mathf.Rad2Deg 
							);
		}
		workingLeg.UpdateFootZ(-60 + (joystickVals.magnitude * 60));
	}
	
	void UpdateWorkingCalf(float buttonValue) {
		workingLeg.UpdateCalfZ(70 * buttonValue);
	}

	void UpdateSupportingLeg(Vector2 joystickVals) {
		Vector3 curPos = bodyTransform.position;
		if(joystickVals.y < 0) {
			supportingLeg.UpdateThighZ(45 * -joystickVals.y + 180);
			supportingLeg.UpdateCalfZ(115 * joystickVals.y);
			supportingLeg.UpdateFootZ(65 + (-joystickVals.y * 25));
			curPos.y = 0.43f * joystickVals.y;
		} else if(joystickVals.y > 0) {
			supportingLeg.UpdateFootZ(65 - (65 * joystickVals.y));
			curPos.y = 0.14f * joystickVals.y;
		} else {
			supportingLeg.UpdateThighZ(45 * -joystickVals.y + 180);
			supportingLeg.UpdateCalfZ(115 * joystickVals.y);
			supportingLeg.UpdateFootZ(65 + (-joystickVals.y * 25));
			curPos.y = 0;
		}
		bodyTransform.position = curPos;
			
	}
}

[System.Serializable]
public class Leg {
	public Transform thigh; 
	public Transform calf; 
	public Transform foot;

	public Leg(Transform legThigh, Transform legCalf, Transform legFoot) {
		thigh = legThigh;
		calf = legCalf;
		foot = legFoot;
	}

	public void UpdateThighX(float newX) {
		Vector3 thighAngles = thigh.localEulerAngles;
		thighAngles.x = newX;
		thigh.localEulerAngles = thighAngles;
	}
	
	public void UpdateThighY(float newY) {
		Vector3 thighAngles = thigh.localEulerAngles;
		thighAngles.y = newY;
		thigh.localEulerAngles = thighAngles;
	}
	
	public void UpdateThighZ(float newZ) {
		Vector3 thighAngles = thigh.localEulerAngles;
		thighAngles.z = newZ;
		thigh.localEulerAngles = thighAngles;
	}

	public void UpdateCalfX(float newX) {
		Vector3 calfAngles = calf.localEulerAngles;
		calfAngles.x = newX;
		calf.localEulerAngles = calfAngles;
	}
	
	public void UpdateCalfY(float newY) {
		Vector3 calfAngles = calf.localEulerAngles;
		calfAngles.y = newY;
		calf.localEulerAngles = calfAngles;
	}
	
	public void UpdateCalfZ(float newZ) {
		Vector3 calfAngles = calf.localEulerAngles;
		calfAngles.z = newZ;
		calf.localEulerAngles = calfAngles;
	}
	
	public void UpdateFootX(float newX) {
		Vector3 footAngles = foot.localEulerAngles;
		footAngles.x = newX;
		foot.localEulerAngles = footAngles;
	}
	
	public void UpdateFootY(float newY) {
		Vector3 footAngles = foot.localEulerAngles;
		footAngles.y = newY;
		foot.localEulerAngles = footAngles;
	}
	
	public void UpdateFootZ(float newZ) {
		Vector3 footAngles = foot.localEulerAngles;
		footAngles.z = newZ;
		foot.localEulerAngles = footAngles;
	}
}
