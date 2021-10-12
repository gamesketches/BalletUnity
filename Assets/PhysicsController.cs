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
	public float lockInTime;
	public float lockOutTime;
	Leg workingLeg;
	Leg supportingLeg;
	Quaternion footFlatRotation;
	Quaternion firstPosThigh;
	Vector3 thighLook;
	public bool useFaker = false;
	bool changingLevel;
	InputFaker inputFaker;
	Vector2 leftStick, leftStickVelocity, rightStick, rightStickVelocity;
	float calfFlexValue;
	MoveData curMove;
	bool leftBumper, rightBumper, leftStickDown, rightStickDown;

    // Start is called before the first frame update
    void Start()
    {
		changingLevel = false;
		workingLeg = rightLeg;
		supportingLeg = leftLeg;
        firstPosThigh = transform.rotation;
		thighLook = rightLeg.thigh.forward;
		inputFaker = GetComponent<InputFaker>();
		leftStickVelocity = Vector3.zero;
		rightStickVelocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
		UpdateInputs();
		MoveData newCurMove = MoveInterpreter.instance.AssessMoveType(leftStick.x, leftStick.y, leftBumper, rightBumper);
		Debug.Log("X: " + leftStick.x.ToString() + ", Y: " + leftStick.y.ToString());
		float calfRotation = 0;
		if(newCurMove != curMove && !changingLevel) {
			StartCoroutine(TransitionToMove(curMove, newCurMove));
			curMove = newCurMove;
		}
		else if(!changingLevel) {
			if(newCurMove != null) {
			Debug.Log("found the move: " + curMove.displayString);
			joystickOutput.text = curMove.displayString;
			workingLeg.thigh.localRotation = curMove.MoveRotationVal();
			calfRotation = newCurMove.calfRotation;
			}
			else {
				joystickOutput.text = "";
				if(!changingLevel) {
					workingLeg.thigh.localRotation = GetThighRotation(leftStick, curMove);
				}
			}
		}
		workingLeg.UpdateFootZ(-60 + (leftStick.magnitude * 80));
		UpdateWorkingCalf(calfRotation, calfFlexValue);
		UpdateSupportingLeg(rightStick);
    }

	void UpdateInputs() {
		var gamepad = Gamepad.current;
		if(gamepad == null || useFaker) {
			leftStickVelocity = GetUpdatedVelocity(leftStick, inputFaker.leftStick);
			leftStick = inputFaker.leftStick;
			rightStickVelocity = GetUpdatedVelocity(rightStick, inputFaker.rightStick);
			rightStick = inputFaker.rightStick;
			calfFlexValue = inputFaker.leftCalfTrigger;
			leftBumper = inputFaker.leftBumper;
			rightBumper = inputFaker.rightBumper;
		} else {
			calfFlexValue = gamepad.leftTrigger.ReadValue();
			leftStickVelocity = GetUpdatedVelocity(leftStick, gamepad.leftStick.ReadValue());
			leftStick = gamepad.leftStick.ReadValue();
			rightStickVelocity = GetUpdatedVelocity(rightStick, gamepad.rightStick.ReadValue());
			rightStick = gamepad.rightStick.ReadValue();
			leftStickDown = gamepad.leftStickButton.isPressed;
			rightStickDown = gamepad.rightStickButton.isPressed;
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

	Quaternion GetThighRotation(Vector2 controlStick, MoveData move) {
		if(leftBumper) {
			if(rightBumper) {
				return CalculateThighUpperLevel(leftStick, move);
			} else {
				return CalculateThighMidLevel(leftStick, move);
			}
		} else {
			return CalculateGroundedThigh(leftStick, move);
		}	
	}

	IEnumerator TransitionToMove(MoveData curMove, MoveData newMove) {
		if(!changingLevel) {
			changingLevel = true;
			float transitionTime = curMove == null ? lockInTime : lockOutTime;
			Quaternion startRotation = curMove == null ? GetThighRotation(leftStick, curMove) : curMove.MoveRotationVal();
			Quaternion targetRotation = newMove == null ? GetThighRotation(leftStick, newMove) : newMove.MoveRotationVal();
			for(float t = 0; t < transitionTime; t += Time.deltaTime) {
				workingLeg.thigh.localRotation = Quaternion.Lerp(startRotation, targetRotation, t / transitionTime);
				yield return null;
			}
			workingLeg.thigh.localRotation = targetRotation;
			changingLevel = false;
		}
	}

	Quaternion CalculateGroundedThigh(Vector2 joystickVals, MoveData curMove) {
		float groundedOffset = 0.7f;
		if(joystickVals.x < -0.1f) {
			groundedOffset = 0.8f;
		}
		float thighRotation = curMove == null ? firstPosThigh.eulerAngles.y : curMove.thighRotation;
		float xVal = Mathf.Clamp(-90 + -Mathf.Acos(-joystickVals.x * groundedOffset) *Mathf.Rad2Deg, -208f, -140f);
		float yVal = thighRotation;//firstPosThigh.eulerAngles.y,
		float zVal = Mathf.Clamp(Mathf.Asin(joystickVals.y * groundedOffset) *Mathf.Rad2Deg, -24f, 0);
		return Quaternion.Euler(xVal, thighRotation, zVal);
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
			MoveData startingMove = MoveInterpreter.instance.AssessMidMoveType(leftStick.x, leftStick.y);
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
		float midLevelOffset = 0.7f;
		if(joystickVals.x < -0.1f) {
			midLevelOffset = 0.8f;
		}
		float thighRotation = curMove == null ? firstPosThigh.eulerAngles.y : curMove.thighRotation;
		float xVal = (-90 + -Mathf.Acos(-joystickVals.x *midLevelOffset) *Mathf.Rad2Deg);
		float yVal = thighRotation;
		float zVal = Mathf.Asin(joystickVals.y * midLevelOffset) *Mathf.Rad2Deg;
		if(zVal > 0) zVal = 0;
		return Quaternion.Euler(xVal, yVal, zVal);
	}

	IEnumerator ShiftToUpperLevel() {
		if(!changingLevel) {
			changingLevel = true;
			MoveData startingMove = MoveInterpreter.instance.AssessMidMoveType(leftStick.x, leftStick.y);
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
			MoveData startingMove = MoveInterpreter.instance.AssessHighMoveType(leftStick.x, leftStick.y);
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
		float upperOffset = 0.95f;
		float thighRotation = curMove == null ? firstPosThigh.eulerAngles.y : curMove.thighRotation;
		float xVal = thighRotation - 180;
		float yVal = -90 + Mathf.Acos(joystickVals.x) *Mathf.Rad2Deg;//transform.rotation.eulerAngles.y,
		float zVal = -90 + Mathf.Asin(-joystickVals.y * upperOffset) *Mathf.Rad2Deg;
		return Quaternion.Euler(xVal, yVal, zVal);
	}
	
	void UpdateWorkingCalf(float calfRotation, float buttonValue) {
		workingLeg.UpdateCalfY(calfRotation);
		workingLeg.UpdateCalfZ(70 * buttonValue);
	}

	void UpdateSupportingLeg(Vector2 joystickVals) {
		Vector3 curPos = bodyTransform.position;
		if(joystickVals.y < 0) {
			if(rightStickDown) {
				supportingLeg.UpdateThighZ(225);
				supportingLeg.UpdateCalfZ(-105);
				supportingLeg.UpdateFootZ(90);
				curPos.y = -0.38f;
			} else {
				supportingLeg.UpdateThighZ(22 * -joystickVals.y + 180);
				supportingLeg.UpdateCalfZ(75 * joystickVals.y);
				supportingLeg.UpdateFootZ(65 + (-joystickVals.y * 25));
				curPos.y = 0.215f * joystickVals.y;
			}
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

	Vector2 GetUpdatedVelocity(Vector2 lastPos, Vector2 newPos) {
		return newPos - lastPos;
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
