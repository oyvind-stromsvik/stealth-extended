using UnityEngine;
using System.Collections;

public class LastPlayerSighting : MonoBehaviour {

    // The audio clip to play when the player triggers the alarm.
    public AudioClip alarmSound;

    // The last global sighting of the player.
    [HideInInspector]
    public Vector3 lastPlayerPosition = new Vector3(1000f, 1000f, 1000f);
    // The default position if the player is not in sight.
    [HideInInspector]
    public Vector3 resetPosition = new Vector3(1000f, 1000f, 1000f);

    // Reference to the AudioSources of the megaphones.					                 
    AudioSource[] alarms;

    bool alarmIsPlaying = false;                              			

    void Awake() {
        // Find an array of the siren gameobjects.
        GameObject[] alarmGameObjects = GameObject.FindGameObjectsWithTag(Tags.siren);

        // Set the sirens array to have the same number of elements as there are gameobjects.
        alarms = new AudioSource[alarmGameObjects.Length];

        // For all the sirens allocate the audio source of the gameobjects.
        for (int i = 0; i < alarms.Length; i++) {
            alarms[i] = alarmGameObjects[i].GetComponent<AudioSource>();
        }
    }

    void Start() {
        // For all the sirens allocate the audio source of the gameobjects.
        for (int i = 0; i < alarms.Length; i++) {
            AudioManager.instance.ApplyDefaultAudioSourceSettings(alarms[i]);
        }
        InvokeRepeating("CheckSoundOcclusion", 1f, 1f);
    }

    void CheckSoundOcclusion() {
        // For all the sirens allocate the audio source of the gameobjects.
        for (int i = 0; i < alarms.Length; i++) {
            AudioManager.instance.CheckDistanceAndOcclusionToListener(alarms[i]);

        }
    }

    void Update() {
        if (lastPlayerPosition != resetPosition && !alarmIsPlaying) {
            TurnOnAlarm();
        }

        if (lastPlayerPosition == resetPosition && alarmIsPlaying) {
            TurnOffAlarm();
        }
    }

    void TurnOnAlarm() {
        alarmIsPlaying = true;

        for (int i = 0; i < alarms.Length; i++) {
            AudioManager.instance.PlaySound(alarms[i], alarmSound, 1f, 10f, true);
        }
    }

    void TurnOffAlarm() {
        alarmIsPlaying = false;

        for (int i = 0; i < alarms.Length; i++) {
            alarms[i].Stop();
        }
    }
}