using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingModeController : MonoBehaviour
{
	public GameObject moveCardPrefab;
	public MoveType[] exercises;
	public Transform canvas;
	GameObject[] exerciseCards;
	int exerciseProgress;
	bool returnedToNeutral;
	public BallerinaController dancer;

    // Start is called before the first frame update
    void Start()
    {
        PlaceMoveCards();
		exerciseProgress = 0;
		returnedToNeutral = true;
    }

    // Update is called once per frame
    void Update()
    {
        CheckCurMove();
    }

	void CheckCurMove() {
		if(exerciseProgress >= exercises.Length) return;
		Vector2 tenduVals = dancer.GetTenduValues();
		if(!returnedToNeutral) {
			if(Mathf.Abs(tenduVals.x) < 0.1f && Mathf.Abs(tenduVals.y) < 0.1f) returnedToNeutral = true;
			else return;
		}
		switch(exercises[exerciseProgress]) {
			case MoveType.TenduFront:
				if(tenduVals.x > 0.8f) {
					MoveSuccess();
				}
				break;
			case MoveType.TenduSide:
				if(tenduVals.y > 0.8f && tenduVals.x < 0.2f && tenduVals.x > -0.2f) {
					MoveSuccess();
				}
				break;
			case MoveType.TenduBack:
				if(tenduVals.x < -0.8f) {
					MoveSuccess();
				}
			break;
		}
	}
			 

	void MoveSuccess() {
		exerciseCards[exerciseProgress].GetComponent<Image>().color = Color.green;
		exerciseProgress++;
		if(exerciseProgress < exerciseCards.Length && 
				exercises[exerciseProgress] == exercises[exerciseProgress - 1]) {
			returnedToNeutral = false;
		}
	}

	void PlaceMoveCards() {
		Vector2 separation = moveCardPrefab.GetComponent<RectTransform>().sizeDelta;
		List<GameObject> tempExerciseCards = new List<GameObject>();
		for(int i = 0; i < exercises.Length; i++) {
			GameObject newMoveCard = Instantiate(moveCardPrefab, canvas);
			newMoveCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(exercises[i].ToString());
			tempExerciseCards.Add(newMoveCard);
		}
		exerciseCards = tempExerciseCards.ToArray();
	}
}
