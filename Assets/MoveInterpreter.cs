using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInterpreter : MonoBehaviour
{
	List<MoveData> moveSet;
	public static MoveInterpreter instance;
	public float tolerance = 0.01f;
    // Start is called before the first frame update
    void Start()
    {
		instance = this;
        InitializeMoveSet();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public MoveData AssessMoveType(float xVal, float yVal) {
		foreach(MoveData move in moveSet) {
			if(move.WithinParameters(xVal, yVal)) {
				return move;
			}
		}
		return null;
	}

	void InitializeMoveSet() {
		moveSet = new List<MoveData>();
		moveSet.Add(new MoveData(MoveType.TenduFront, 0.25f, 0.35f, 0, 0, tolerance));
		moveSet.Add(new MoveData(MoveType.TenduSide, 0, 0, -0.45f, -0.35f, tolerance));
		moveSet.Add(new MoveData(MoveType.TenduBack, -0.5f, -0.4f, 0, 0, 0.04f));
	}
}

public class MoveData {
	public string displayString;
	MoveType moveType;
	float xMin, xMax, yMin, yMax;
	
	public MoveData(MoveType theMove, float minX, float maxX, float minY, float maxY, float tolerance = 0.01f) {
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
		moveType = theMove;
		displayString = theMove.ToString();
	}

	public bool WithinParameters(float xVal, float yVal) {
		bool x = xMin <= xVal && xVal <= xMax;
		bool y = yMin <= yVal && yVal <= yMax;
		Debug.Log(x);
		Debug.Log(y);
		return x && y;
	}
}
