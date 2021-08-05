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
	AudioSource voiceOver;
	public bool randomExercises;

    // Start is called before the first frame update
    void Start()
    {
		voiceOver = GetComponent<AudioSource>();
		if(randomExercises) {
			int numExercises = 4;
			exercises = new MoveType[numExercises];
			MoveType[] possibleExercises = new MoveType[] {MoveType.TenduFront, MoveType.TenduSide, MoveType.TenduBack};
			for(int i = 0; i < numExercises; i++) {
				exercises[i] = possibleExercises[Mathf.FloorToInt(Random.value * possibleExercises.Length)];
			}
		}
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
		float nonWorkingLeg = dancer.GetPlieReleveValue();
		if(!returnedToNeutral) {
			if(ReturnedToNeutral()) returnedToNeutral = true;
			else return;
		}
		switch(exercises[exerciseProgress]) {
			case MoveType.TenduFront:
				if(tenduVals.x > 0.8f) {
					MoveSuccess();
				}
				break;
			case MoveType.TenduSide:
				if(tenduVals.y < -0.8f && tenduVals.x < 0.2f && tenduVals.x > -0.2f) {
					MoveSuccess();
				}
				break;
			case MoveType.TenduBack:
				if(tenduVals.x < -0.8f) {
					MoveSuccess();
				}
				break;
			case MoveType.Plie:
				if(nonWorkingLeg < -0.8f) {
					MoveSuccess();
				}
				break;
			case MoveType.Releve:
				if(nonWorkingLeg > 0.8f) {
					MoveSuccess();
				}
				break;
		}
	}
			 

	void MoveSuccess() {
		SayMoveName(exercises[exerciseProgress].ToString());
		exerciseCards[exerciseProgress].GetComponent<Image>().color = Color.green;
		StartCoroutine(CollapseCard(exerciseProgress));
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
		StartCoroutine(SayMoveNames());
		exerciseCards = tempExerciseCards.ToArray();
	}

	void SayMoveName(string moveName) {
		voiceOver.clip = Resources.Load<AudioClip>(moveName);
		Debug.Log(voiceOver.clip);
		if(voiceOver.clip != null)
			voiceOver.Play();
	}

	IEnumerator SayMoveNames() {
		foreach(MoveType move in exercises) {
			SayMoveName(move.ToString());
			yield return new WaitForSeconds(voiceOver.clip.length);
		}
	}
	
	IEnumerator CollapseCard(int index) {
		yield return new WaitForSeconds(1.0f); 
		exerciseCards[index].GetComponent<RectTransform>().sizeDelta = new Vector2(-1, 0);
	}

	bool ReturnedToNeutral() {
		Vector2 tenduVals = dancer.GetTenduValues();
		float nonWorkingLeg = dancer.GetPlieReleveValue();
		return Mathf.Abs(tenduVals.x) < 0.1f && Mathf.Abs(tenduVals.y) < 0.1f && Mathf.Abs(nonWorkingLeg) < 0.2f;
	}
}
