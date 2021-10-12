using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveLevel {Low, Mid, High};
public class MoveInterpreter : MonoBehaviour
{
	public List<MoveData> lowMoveSet;
	public List<MoveData> midLevelMoveSet;
	public List<MoveData> upperLevelMoveSet;
	public static MoveInterpreter instance;
	MoveData curMove, lastMove;
	public float tolerance = 0.01f;
    // Start is called before the first frame update
    void Start()
    {
		instance = this;
        //InitializeMoveSet();
		InitializeFollowUpMoves(lowMoveSet);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


	public MoveData AssessMoveType(float xVal, float yVal, bool leftBumper, bool rightBumper) {
		if(leftBumper) {
			if(rightBumper) return AssessHighMoveType(xVal, yVal);
			else return AssessMidMoveType(xVal, yVal);
		}
		return AssessGroundedMoveType(xVal, yVal);
	}

	public MoveData AssessGroundedMoveType(float xVal, float yVal) {
		return SearchMoves(xVal, yVal, lowMoveSet);
		/*foreach(MoveData move in lowMoveSet) {
			if(move.WithinParameters(xVal, yVal)) {
				return move;
			}
		}
		return null;*/
	}

	public MoveData AssessMidMoveType(float xVal, float yVal) {
		return SearchMoves(xVal, yVal, midLevelMoveSet);
	}

	public MoveData AssessHighMoveType(float xVal, float yVal) {
		return SearchMoves(xVal, yVal, upperLevelMoveSet);
	}

	MoveData SearchMoves(float xVal, float yVal, List<MoveData> moves) {
		foreach(MoveData move in moves) {
			if(move.WithinParameters(xVal, yVal)) {
				return move;
			}
		}
		return null;
	}

	void InitializeFollowUpMoves(List<MoveData> moves) {
		foreach(MoveData move in moves) {
			foreach(string moveName in move.followUpMoves) {
				for(int i = 0; i < moves.Count; i++) {
					if(moves[i].displayString == moveName) {
						move.AddFollowUpMoveData(moves[i]);
						break;
					}
				}
			}
		}
	}
}

[System.Serializable]
public class MoveData {
	public string displayString;
	public MoveType moveType;
	public float xMin, xMax, yMin, yMax;
	public float thighRotation, calfRotation;
	public Vector3 localRotation;
	Quaternion calcedRotation;
	public float inputTolerance = 0.01f;
	public MoveLevel moveLevel;
	public string[] followUpMoves;
	MoveData[] followUpMoveData;
	
	public MoveData(MoveType theMove, float minX, float maxX, float minY, float maxY, Vector3 moveRotation, float thigh, float calf, MoveLevel level = MoveLevel.Low, float tolerance = 0.01f) {
		moveType = theMove;
		moveLevel = level;
		localRotation = moveRotation;
		calcedRotation = Quaternion.Euler(localRotation);
		if(minX == maxX && maxX == 0) {
			xMin = -tolerance;
			xMax = tolerance;
		} else {
			xMin = minX;
			xMax = maxX;
		}
		if(minY == maxY && maxY == 0) {
			yMin = -tolerance;
			yMax = tolerance;
		} else {
			yMin = minY;
			yMax = maxY;
		}
		thighRotation = thigh;
		calfRotation = calf;
		moveType = theMove;
		displayString = theMove.ToString();
		inputTolerance = tolerance;
	}

	public bool WithinParameters(float xVal, float yVal) {
		bool x = xMin <= xVal && xVal <= xMax;
		bool y = yMin <= yVal && yVal <= yMax;
		return x && y;
	}

	public Quaternion MoveRotationVal() {
		if(this.calcedRotation == null) this.calcedRotation = Quaternion.Euler(this.localRotation);
		Debug.Log(Quaternion.Euler(this.localRotation));
		return Quaternion.Euler(this.localRotation);
	}

	public void AddFollowUpMoveData(MoveData followUpMove) {
		List<MoveData> temp = new List<MoveData>(followUpMoveData);
		temp.Add(followUpMove);
		followUpMoveData = temp.ToArray();
	}

	public Vector2 GetTargetPoint() {
		float xVal = Mathf.Lerp(xMin, xMax, 0.5f);
		float yVal = Mathf.Lerp(yMin, yMax, 0.5f);
		return new Vector2(xVal, yVal);
	}

	public MoveData FindMostLikelyFollowUp(Vector2 velocity) {
		Vector2 originPoint = this.GetTargetPoint();
		float bestDistance = 1000;
		MoveData followUp = null; 
		for(int i = 0; i < followUpMoveData.Length; i++) {
			Vector2 targetPoint = followUpMoveData[i].GetTargetPoint();
			float newDistance = Vector2.Distance(originPoint + velocity, targetPoint);
			if(newDistance < bestDistance) {
				bestDistance = newDistance;
				followUp = followUpMoveData[i];
			}
		}
		return followUp;
	}
}
