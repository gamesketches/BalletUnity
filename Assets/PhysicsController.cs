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
	public float levelChangeTime;
	Leg workingLeg;
	Leg supportingLeg;
	Quaternion footFlatRotation;
	Quaternion firstPosThigh;
	Vector3 thighLook;
	public bool useFaker = false;
	bool changingLevel;
	InputFaker inputFaker;
	Vector3 leftStick, rightStick;
	float calfFlexValue;
	bool leftBumper, rightBumper;

    // Start is called before the first frame update
    void Start()
    {
		changingLevel = false;
		workingLeg = rightLeg;
		supportingLeg = leftLeg;
        firstPosThigh = transform.rotation;
		thighLook = rightLeg.thigh.forward;
		inputFaker = GetComponent<InputFaker>();
    }

    // Update is called once per frame
    void Update()
    {
		UpdateInputs();
		MoveData curMove = MoveInterpreter.instance.AssessGroundedMoveType(leftStick.x, leftStick.y);
		Debug.Log("X: " + leftStick.x.ToString() + ", Y: " + leftStick.y.ToString());
		float calfRotation = 0;
		if(curMove != null) {
			Debug.Log("found the move: " + curMove.displayString);
			joystickOutput.text = curMove.displayString;
			calfRotation = curMove.calfRotation;
		}
		else {
			joystickOutput.text = "";
		}
		if(!changingLevel) {
			Quaternion workingThighRotation;
			if(leftBumper) {
				if(rightBumper) {
					workingThighRotation = CalculateThighUpperLevel(leftStick, curMove);
				} else {
					workingThighRotation = CalculateThighMidLevel(leftStick, curMove);
				}
			} else {
				workingThighRotation = CalculateGroundedThigh(leftStick, curMove);
			}
			workingLeg.thigh.localRotation = workingThighRotation;
		}
		workingLeg.UpdateFootZ(-60 + (leftStick.magnitude * 80));
		UpdateWorkingCalf(calfRotation, calfFlexValue);
		UpdateSupportingLeg(rightStick);
    }

	void UpdateInputs() {
		var gamepad = Gamepad.current;
		if(gamepad == null || useFaker) {
			leftStick = inputFaker.leftStick;
			rightStick = inputFaker.rightStick;
			calfFlexValue = inputFaker.leftCalfTrigger;
			leftBumper = inputFaker.leftBumper;
			rightBumper = inputFaker.rightBumper;
		} else {
			calfFlexValue = gamepad.leftTrigger.ReadValue();
			leftStick = gamepad.leftStick.ReadValue();
			rightStick = gamepad.rightStick.ReadValue();
			leftBumper = gamepad.leftShoulder.isPressed;
			rightBumper = gamepad.rightShoulder.isPressed;
			if(gamepad.leftShoulder.wasPressedThisFrame) { 
				if(rightBumper) {
					StartCoroutine(ShiftToUpperLevel());
				} else {
					StartCoroutine(ShiftToMidLevel());
				}
			} else if(gamepad.leftShoulder.wasReleasedThisFrame) {
				StartCoroutine(ShiftToGroundedLevel());
			}
			if(gamepad.rightShoulder.wasPressedThisFrame && leftBumper) {
				StartCoroutine(ShiftToUpperLevel());
			} else if(gamepad.rightShoulder.wasReleasedThisFrame) {
				StartCoroutine(UpperToMidLevel());
			}
		}
	}

	Quaternion CalculateGroundedThigh(Vector2 joystickVals, MoveData curMove) {
		float groundedOffset = 0.5f;
		float thighRotation = curMove == null ? firstPosThigh.eulerAngles.y : curMove.thighRotation;
		return Quaternion.Euler(Mathf.Clamp(-90 + -Mathf.Acos(-joystickVals.x * groundedOffset) *Mathf.Rad2Deg, -200f, -140f),
									thighRotation,
									//firstPosThigh.eulerAngles.y, 
										Mathf.Clamp(Mathf.Asin(joystickVals.y * groundedOffset) *Mathf.Rad2Deg, -24f, 0));
	}

	void UpdateWorkingThighGrounded(Vector2 joystickVals, MoveData curMove) {
		float groundedOffset = 0.5f;
		//joystickVals *= groundedOffset;
		/*if(joystickVals.magnitude < 0.95f) {
			if(Mathf.Abs(joystickVals.x) > Mathf.Abs(joystickVals.y)) {
				transform.rotation = firstPosThigh.euler + new Vector3(joystickVals.x, 0, 0);
			} else {
				transform.rotation = firstPosThigh.euler + new Vector3(0, 0, joystickVals.y);
			}
		} else {
			*/
		//workingLeg.thigh.transform.up = (new Vector3(-joystickVals.y, -1, joystickVals.x) - transform.position).normalized;
		float thighRotation = curMove == null ? firstPosThigh.eulerAngles.y : curMove.thighRotation;
		workingLeg.thigh.localRotation  = 
				Quaternion.Euler(Mathf.Clamp(-90 + -Mathf.Acos(-joystickVals.x * groundedOffset) *Mathf.Rad2Deg, -200f, -140f),
									thighRotation,
									//firstPosThigh.eulerAngles.y, 
										Mathf.Clamp(Mathf.Asin(joystickVals.y * groundedOffset) *Mathf.Rad2Deg, -24f, 0));
		workingLeg.UpdateFootZ(-60 + (joystickVals.magnitude * 80));
	}

	IEnumerator ShiftToMidLevel() {
		if(!changingLevel) {
			changingLevel = true;
			MoveData startingMove = MoveInterpreter.instance.AssessGroundedMoveType(leftStick.x, leftStick.y);
			Quaternion groundedPos, OTGPos;
			for(float t = 0; t < levelChangeTime; t += Time.deltaTime) {
				groundedPos = CalculateGroundedThigh(leftStick, startingMove);
				OTGPos = rightBumper ? CalculateThighUpperLevel(leftStick, startingMove) : CalculateThighMidLevel(leftStick, startingMove);
				workingLeg.thigh.localRotation = Quaternion.Lerp(groundedPos, OTGPos, t / levelChangeTime);
				yield return null;
			}
			changingLevel = false;
		}
	}

	IEnumerator ShiftToGroundedLevel() {
		if(!changingLevel) {
			changingLevel = true;
			MoveData startingMove = MoveInterpreter.instance.AssessMidLevelMoveType(leftStick.x, leftStick.y);
			for(float t = 0; t < levelChangeTime; t += Time.deltaTime) {
				Quaternion groundedPos = CalculateGroundedThigh(leftStick, startingMove);
				Quaternion OTGPos = CalculateThighMidLevel(leftStick, startingMove);
				workingLeg.thigh.localRotation = Quaternion.Lerp(OTGPos, groundedPos, t / levelChangeTime);
				yield return null;
			}
			changingLevel = false;
		}
	}

	Quaternion CalculateThighMidLevel(Vector2 joystickVals, MoveData curMove) {
		float thighRotation = curMove == null ? firstPosThigh.eulerAngles.y : curMove.thighRotation;//firstPosThigh.eulerAngles.y
		float xVal = -90 + -Mathf.Acos(-joystickVals.x) *Mathf.Rad2Deg;
		float zVal = Mathf.Asin(joystickVals.y) *Mathf.Rad2Deg;
		if(zVal > 0) zVal = 0;
		return Quaternion.Euler(xVal, thighRotation, zVal);
	}

	IEnumerator ShiftToUpperLevel() {
		if(!changingLevel) {
			changingLevel = true;
			MoveData startingMove = MoveInterpreter.instance.AssessMidLevelMoveType(leftStick.x, leftStick.y);
			Quaternion OTGRot, upperRotation;
			for(float t = 0; t < levelChangeTime; t += Time.deltaTime) {
				OTGRot = CalculateThighMidLevel(leftStick, startingMove);
				upperRotation = CalculateThighUpperLevel(leftStick, startingMove);
				workingLeg.thigh.localRotation = Quaternion.Lerp(OTGRot, upperRotation, t / levelChangeTime);
				yield return null;
			}
			changingLevel = false;
		}
	}

	IEnumerator UpperToMidLevel() {
		if(!changingLevel) {
			changingLevel = true;
			MoveData startingMove = MoveInterpreter.instance.AssessUpperLevelMoveType(leftStick.x, leftStick.y);
			Quaternion OTGRot, upperRotation;
			for(float t = 0; t < levelChangeTime; t += Time.deltaTime) {
				OTGRot = CalculateThighMidLevel(leftStick, startingMove);
				upperRotation = CalculateThighUpperLevel(leftStick, startingMove);
				workingLeg.thigh.localRotation = Quaternion.Lerp(upperRotation, OTGRot, t / levelChangeTime);
				yield return null;
			}
			changingLevel = false;
		}
	}

	Quaternion CalculateThighUpperLevel(Vector2 joystickVals, MoveData curMove) {
		float thighRotation = curMove == null ? firstPosThigh.eulerAngles.y : curMove.thighRotation;
		return Quaternion.Euler(-90 + -Mathf.Acos(joystickVals.x) *Mathf.Rad2Deg,  
										thighRotation,
						//transform.rotation.eulerAngles.y, 
							-90 - Mathf.Asin(joystickVals.y) *Mathf.Rad2Deg 
							);
	}
	
	void UpdateWorkingCalf(float calfRotation, float buttonValue) {
		workingLeg.UpdateCalfY(calfRotation);
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
