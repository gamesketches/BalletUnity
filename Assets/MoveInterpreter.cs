using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveLevel {Low, Mid, High};
public class MoveInterpreter : MonoBehaviour
{
	public bool doMoveInterpolation;
	public List<MoveData> lowMoveSet;
	public List<MoveData> midLevelMoveSet;
	public List<MoveData> upperLevelMoveSet;
	List<MoveData> curFollowUpMoves;
	public static MoveInterpreter instance;
	MoveData curMove, lastMove, tweenMove;
	public float tolerance = 0.01f;
    // Start is called before the first frame update
    void Start()
    {
		instance = this;
		tweenMove = new MoveData(MoveType.Tween, 0, 0, 0, 0, Vector3.zero, 0, 0);
		curFollowUpMoves = new List<MoveData>();
		//InitializeFollowUpMoves(lowMoveSet);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


	public MoveData AssessMoveType(float xVal, float yVal, bool leftBumper, bool rightBumper, 
																Vector2 velocity) {
		MoveData theMove;
		if(leftBumper) {
			if(rightBumper) theMove = AssessHighMoveType(xVal, yVal);
			else theMove = AssessMidMoveType(xVal, yVal);
		} else {
			theMove = AssessGroundedMoveType(xVal, yVal);
		}
		if(theMove == null) {
			Debug.Log("the move is null");
			if(curMove != null) {
				Debug.Log("Doing tweening");
				lastMove = curMove;
				curMove = null;
			} 
			if(lastMove != null) {
				if(doMoveInterpolation) {
					Vector2 curPos = new Vector2(xVal, yVal);
					MoveData targetMove = lastMove.FindMostLikelyFollowUp(curPos, velocity, curFollowUpMoves.ToArray());
					Debug.Log(targetMove.displayString);
					float lerpProportion = FindRotationProportion(lastMove, targetMove, xVal, yVal);
					tweenMove.localRotation = Vector3.Lerp(lastMove.localRotation, targetMove.localRotation, 
																									lerpProportion);
					theMove = tweenMove;
				}
			}
		} else {
			curMove = theMove;
			if(leftBumper) {
				if(rightBumper) GetFollowUpMoves(upperLevelMoveSet);
				else GetFollowUpMoves(midLevelMoveSet);
			} else {
				GetFollowUpMoves(lowMoveSet);
			}
		}
		return theMove;
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
				Debug.Log("looking for followup move: " + moveName);
				for(int i = 0; i < moves.Count; i++) {
					if(moves[i].displayString == moveName) {
						move.AddFollowUpMoveData(moves[i]);
						Debug.Log("Added follow up move " + moveName);
						break;
					}
				}
			}
		}
	}

	void GetFollowUpMoves(List<MoveData> potentialFollowUps) {
		curFollowUpMoves.Clear();
		foreach(string moveName in curMove.followUpMoves) {
			for(int i = 0; i < potentialFollowUps.Count; i++) {
				if(potentialFollowUps[i].displayString == moveName) {
					curFollowUpMoves.Add(potentialFollowUps[i]);
				}
			}
		}
	}

	float FindRotationProportion(MoveData startMove, MoveData endMove, float xVal, float yVal) {
		Vector2 curVector = new Vector2(xVal, yVal);
		Vector2 startTarget = startMove.GetTargetPoint();
		Vector2 endTarget = endMove.GetTargetPoint();
		float distanceFromStart = Vector2.Distance(startTarget, curVector);
		float distanceFromEnd = Vector2.Distance(curVector, endTarget);
		if(distanceFromStart == 0) return 0;
		else if(distanceFromEnd == 0) return 1;
		float proportion = distanceFromStart / (distanceFromStart + distanceFromEnd);
		Debug.Log(proportion);
		return proportion;
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
		followUpMoveData = new MoveData[0];
	}

	public bool WithinParameters(float xVal, float yVal) {
		bool x = CheckBetween(xMin, xMax, xVal);
		bool y = CheckBetween(yMin, yMax, yVal);
		//bool x = xMin <= xVal && xVal <= xMax;
		//bool y = yMin <= yVal && yVal <= yMax;
		return x && y;
	}

	bool CheckBetween(float val1, float val2, float target) {
		return (val1 < target && target < val2) || (val1 > target && target > val2);
	}

	public Quaternion MoveRotationVal() {
		if(this.calcedRotation == null) this.calcedRotation = Quaternion.Euler(this.localRotation);
		return Quaternion.Euler(this.localRotation);
	}

	public void AddFollowUpMoveData(MoveData followUpMove) {
		List<MoveData> temp;
		if(this.followUpMoveData != null) temp = new List<MoveData>(this.followUpMoveData);
		else temp = new List<MoveData>();
		temp.Add(followUpMove);
		followUpMoveData = temp.ToArray();
	}

	public Vector2 GetTargetPoint() {
		float xVal = Mathf.Lerp(xMin, xMax, 0.5f);
		float yVal = Mathf.Lerp(yMin, yMax, 0.5f);
		return new Vector2(xVal, yVal);
	}

	public MoveData FindMostLikelyFollowUp(Vector2 curPos, Vector2 velocity, MoveData[] followUps) {
		Vector2 originPoint = this.GetTargetPoint();
		float bestDistance = 1000;
		MoveData followUp = null; 
		for(int i = 0; i < followUps.Length; i++) {
		//for(int i = 0; i < followUpMoveData.Length; i++) {
			Vector2 targetPoint = followUps[i].GetTargetPoint();
			float newDistance = Vector2.Distance(curPos + velocity, targetPoint);
			if(newDistance < bestDistance) {
				bestDistance = newDistance;
				followUp = followUps[i];
			}
		}
		return followUp;
	}
}
