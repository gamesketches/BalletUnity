using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AudioManager : MonoBehaviour
{
	AudioSource audioSource;
	public float songBPM;
	public float secPerBeat;
	public float songPosition;
	public float songPositionInBeats;
	public float dspSongTime;
	public ChoreographerController choreographer;
	
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
		secPerBeat = 60f / songBPM;
		choreographer.GetSongDetails(secPerBeat);
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame) {
			dspSongTime = (float) AudioSettings.dspTime;
			audioSource.Play();
		}
		if(audioSource.isPlaying) {
			songPosition = (float)(AudioSettings.dspTime - dspSongTime);
			songPositionInBeats = songPosition / secPerBeat;
			choreographer.UpdateCardPosition(songPosition, songPositionInBeats);
			if(Keyboard.current.aKey.wasPressedThisFrame) {
				if(songPositionInBeats % 1 > 0.8f || songPositionInBeats % 1 < 0.2f) {
					Debug.Log("Nice!");
				} else Debug.Log(songPositionInBeats %1);
			}
		}
    }
}
