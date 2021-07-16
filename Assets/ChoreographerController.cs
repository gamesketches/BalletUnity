using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum MoveType {TenduFront, TenduSide, TenduBack, Releve, Plie, SwitchPosition, CloseBack};

public class ChoreographerController : MonoBehaviour
{
	public Move[] choreography;
	public GameObject moveCardPrefab;
	public Transform canvas;
	public BallerinaController dancer;
	float secPerBeat;
	public Image songTimeLine;
	public Vector2 songTimeLinePos;
	public float beatVisualSize;
	public float beatsShownInAdvance;
	Move curMove;

    // Start is called before the first frame update
    void Awake()
    {
		for(int i = 0; i < choreography.Length; i++) {
			GameObject newMoveCard = Instantiate(moveCardPrefab, canvas);
			newMoveCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(choreography[i].moveType.ToString());
			choreography[i].rectTransform = newMoveCard.GetComponent<RectTransform>();
			choreography[i].rectTransform.anchoredPosition = Vector2.LerpUnclamped(songTimeLinePos, songTimeLinePos + new Vector2(0, beatVisualSize), choreography[i].startBeat);//new Vector2(-100f, -63.1f + (i * -100f));
		}
		songTimeLine.GetComponent<RectTransform>().anchoredPosition = songTimeLinePos;
		songTimeLine.transform.SetAsLastSibling();
    }

    // Update is called once per frame
    void Update()
    {
    }

	public void GetSongDetails(float secondsPerBeat) {
		secPerBeat = secondsPerBeat;
		UpdateCardPosition(0, 0);
	}

	public void UpdateCardPosition(float songTime, float currentBeat) {
		for(int i = 0; i < choreography.Length; i++) {
			Move theMove = choreography[i];
			theMove.rectTransform.anchoredPosition = 
				Vector2.LerpUnclamped(songTimeLinePos, 
					songTimeLinePos - new Vector2(0, beatVisualSize), theMove.startBeat - currentBeat);
			//theMove.rectTransform.anchoredPosition = new Vector2(-100f, -63.1f + (theMove.startBeat * secPerBeat * -100f) + songTime);
			if(theMove.startBeat < currentBeat && theMove.startBeat + theMove.duration > currentBeat) {
				UpdateMoveScore(theMove);
			}
		}
	}

	void UpdateMoveScore(Move theMove) {
		Debug.Log("Do " + theMove.moveType.ToString());
		switch(theMove.moveType) {
			case MoveType.TenduFront:
			case MoveType.TenduSide:
			case MoveType.TenduBack:
				Vector2 tenduVals = dancer.GetTenduValues();
				if(tenduVals.y < 0.2f) {
					if((tenduVals.x > 0.2f && theMove.moveType == MoveType.TenduFront) ||
						(tenduVals.x > -0.2f && tenduVals.x < 0.2f && theMove.moveType == MoveType.TenduSide) ||
						 tenduVals.x < -0.2f && theMove.moveType == MoveType.TenduBack) {
							theMove.AddPoints();
					}
				}
			break;
			case MoveType.Releve:
				if(dancer.GetPlieReleveValue() > 0.2f) theMove.AddPoints();
			break;
			case MoveType.Plie:
				if(dancer.GetPlieReleveValue() < -0.2f) theMove.AddPoints();
			break;
			case MoveType.SwitchPosition:
				Debug.Log("Not sure how to code position switching yet....");
				theMove.AddPoints();
			break;
			case MoveType.CloseBack:
				if(dancer.GetClosed()) {
					theMove.AddPoints();
				}
			break;
		}
	}
}

[Serializable]
public class Move {
	public MoveType moveType;
	public float startBeat;
	public float trueStartBeat;
	public float targetBeat;
	public float duration;
	public RectTransform rectTransform;
	float pointsScored;
	
	public Move(MoveType theMove, float startingBeat, float trueStartingBeat, float theTargetBeat, float moveDuration) {
		moveType = theMove;
		startBeat = startingBeat;
		trueStartBeat = trueStartingBeat;
		targetBeat = theTargetBeat;
		duration = moveDuration;
		pointsScored = 0;
		rectTransform = null;
	}

	public void AddPoints() {
		Debug.Log(pointsScored);
		pointsScored += Time.deltaTime;
	}
}
