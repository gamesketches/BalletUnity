﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInterpreter : MonoBehaviour
{
	public List<MoveData> moveSet;
	public List<MoveData> midLevelMoveSet;
	public List<MoveData> upperLevelMoveSet;
	public static MoveInterpreter instance;
	public float tolerance = 0.01f;
    // Start is called before the first frame update
    void Start()
    {
		instance = this;
        //InitializeMoveSet();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public MoveData AssessGroundedMoveType(float xVal, float yVal) {
		return SearchMoves(xVal, yVal, moveSet);
		/*foreach(MoveData move in moveSet) {
			if(move.WithinParameters(xVal, yVal)) {
				return move;
			}
		}
		return null;*/
	}

	public MoveData AssessMidLevelMoveType(float xVal, float yVal) {
		return SearchMoves(xVal, yVal, midLevelMoveSet);
	}

	public MoveData AssessUpperLevelMoveType(float xVal, float yVal) {
		return SearchMoves(xVal, yVal, upperLevelMoveSet);
	}

	MoveData SearchMoves(float xVal, float yVal, List<MoveData> moves) {
		foreach(MoveData move in moveSet) {
			if(move.WithinParameters(xVal, yVal)) {
				return move;
			}
		}
		return null;
	}

	void InitializeMoveSet() {
		moveSet = new List<MoveData>();
		moveSet.Add(new MoveData(MoveType.TenduFront, 0.25f, 0.35f, 0, 0, -27f, 0, tolerance));
		moveSet.Add(new MoveData(MoveType.TenduSide, 0, 0, -0.45f, -0.35f, 0, 0, tolerance));
		moveSet.Add(new MoveData(MoveType.TenduBack, -0.5f, -0.4f, 0, 0, -45, 0, 0.04f));
	}
}

[System.Serializable]
public class MoveData {
	public string displayString;
	public MoveType moveType;
	public float xMin, xMax, yMin, yMax;
	public float thighRotation, calfRotation;
	public float inputTolerance = 0.01f;
	
	public MoveData(MoveType theMove, float minX, float maxX, float minY, float maxY, float thigh, float calf, float tolerance = 0.01f) {
		moveType = theMove;
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
}
