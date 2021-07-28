using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveGuideBehavior : MonoBehaviour
{
	public Transform guidingTransform;
	Light theLight;

    // Start is called before the first frame update
    void Start()
    {
		theLight = GetComponent<Light>();
		transform.parent = guidingTransform;
        transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
    }

	void UpdateLights() {
		var gamepad = Gamepad.current;
		if(gamepad == null) return;
		Vector2 leftStick = gamepad.leftStick.ReadValue();
		theLight.color = new Color((leftStick.x + 1f) / 2f, 0, (leftStick.y + 1f) / 2f);
		//Vector2 rightStick = gamepad.rightStick.ReadValue();
	}

	public void UpdateInputs(Vector2 leftStick, Vector2 rightStick) {
		theLight.color = new Color((leftStick.x + 1f) / 2f, (leftStick.y + 1f) / 2f, rightStick.y);
	}
		
}
