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

    // Start is called before the first frame update
    void Start()
    {
       for(int i = 0; i < choreography.Length; i++) {
			GameObject newMoveCard = Instantiate(moveCardPrefab, canvas);
			newMoveCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100f, -63.1f + (i * -100f));
			newMoveCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(choreography[i].moveType.ToString());
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class Move {
	public MoveType moveType;
	public float startBeat;
	public float trueStartBeat;
	public float targetBeat;
	
	public Move(MoveType theMove, float startingBeat, float trueStartingBeat, float theTargetBeat) {
		moveType = theMove;
		startBeat = startingBeat;
		trueStartBeat = trueStartingBeat;
		targetBeat = theTargetBeat;
	}
}
