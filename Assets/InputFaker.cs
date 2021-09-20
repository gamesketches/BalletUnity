using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputFaker : MonoBehaviour
{
	public Vector2 leftStick = new Vector2(0.0f, 0.0f);
	public Vector2 rightStick = new Vector2(0.0f, 0.0f);
	public float leftCalfTrigger;
	public float rightCalfTrigger;
	public bool leftBumper;
	public bool rightBumper;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MaintainThresholds(leftStick);
        MaintainThresholds(rightStick);
		leftCalfTrigger = Mathf.Clamp(leftCalfTrigger, 0, 1);
		rightCalfTrigger = Mathf.Clamp(rightCalfTrigger, 0, 1);
    }

	void MaintainThresholds(Vector2 stickVals) {
		stickVals.x = Mathf.Clamp(stickVals.x, -1f, 1f);
		stickVals.y = Mathf.Clamp(stickVals.y, -1f, 1f);
	}
}
