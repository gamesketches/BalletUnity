using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceTest : MonoBehaviour
{
	Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
		Debug.Log(Mathf.Sin((Time.realtimeSinceStartup + 1) / 2));
        animator.SetFloat("Blend", Mathf.Sin((Time.realtimeSinceStartup + 1) / 2));
    }
}
