using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

    public static AudioManager instance;

    public LayerMask layerMask;
    public AudioClip ambientOutdoors;
    public AudioClip ambientIndoors;

    const float defaultMinDistance = 5f;
    const float defaultMaxDistance = 50f;
    const float maxCutoffFrequency = 6000f;
    const bool looping = false;

    AudioListener listener;
    AudioSource audioSource;

    void Awake() {
        instance = this;
        listener = Camera.main.GetComponent<AudioListener>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start() {
        audioSource.clip = ambientOutdoors;
        audioSource.Play();
    }

    public void CameraInside(bool value) {
        if (value) {
            audioSource.clip = ambientIndoors;
            audioSource.Play();
        }
        else {
            audioSource.clip = ambientOutdoors;
            audioSource.Play();
        }
    }

    /**
     * Plays a sound on the supplied Audiosource with our settings so
     * we don't have to set them on every single audiosorce.
     */
    public void PlaySound(AudioSource source, AudioClip clip, float minDistance = defaultMinDistance, float maxDistance = defaultMaxDistance, bool loop = looping) {
        source.clip = clip;

        ApplyDefaultAudioSourceSettings(source, minDistance, maxDistance, loop);

        CheckDistanceAndOcclusionToListener(source);

        source.Play();
    }

    /**
     * Plays a sound on the supplied Audiosource with our settings so
     * we don't have to set them on every single audiosorce.
     */
    public void PlaySoundOneShot(AudioSource source, AudioClip clip) {
        ApplyDefaultAudioSourceSettings(source);

        CheckDistanceAndOcclusionToListener(source);

        source.PlayOneShot(clip);
    }

    /**
     * Creates a gameobject with an audiosource, plays the clip and the destroys the game object.
     * Useful for projectiles playing and explode sound or something and the projectile is
     * destroyed before the sound can play or has finished playing.
     */
    public void PlaySoundAtPosition(AudioClip clip, Vector3 position) {
        GameObject go = new GameObject();
        go.name = "PlaySoundAtPosition";
        go.transform.position = position;

        go.AddComponent<AudioSource>();
        go.GetComponent<AudioSource>().clip = clip;

        ApplyDefaultAudioSourceSettings(go.GetComponent<AudioSource>());

        CheckDistanceAndOcclusionToListener(go.GetComponent<AudioSource>());

        go.GetComponent<AudioSource>().Play();

        Destroy(go, clip.length);
    }

    public void ApplyDefaultAudioSourceSettings(AudioSource source, float minDistance = defaultMinDistance, float maxDistance = defaultMaxDistance, bool loop = looping) {
        if (source.gameObject.GetComponent<AudioLowPassFilter>() == null) {
            source.gameObject.AddComponent<AudioLowPassFilter>();
        }

        source.pitch = 1;
        source.dopplerLevel = 0;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.loop = loop;
        source.playOnAwake = false;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    /**
     * This method is public because looping sounds like the laser fences
     * need to call it periodically to check the listener position.
     */
    public void CheckDistanceAndOcclusionToListener(AudioSource source) {
        bool occluded = true;

        Vector3 rayOrigin = source.transform.position;
        Vector3 rayDirection = listener.transform.position - source.transform.position;

        float distance = rayDirection.magnitude;
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, distance, layerMask)) {
            if (hit.transform.tag == Tags.mainCamera) {
                occluded = false;
            }
        }

        AudioLowPassFilter lowPass = source.gameObject.GetComponent<AudioLowPassFilter>();
        if (occluded) {
            lowPass.cutoffFrequency = maxCutoffFrequency - ((distance / defaultMaxDistance) * maxCutoffFrequency);
            lowPass.enabled = true;
        }
        else {
            lowPass.enabled = false;
        }

        source.pitch = 1 - ((distance / defaultMaxDistance) / 10);

        // Debug.
        //print("low: " + lowPass.enabled + " occl: " + occluded + " frequency: " + lowPass.cutoffFrequency + " pitch: " + source.pitch + " dist: " + distance);
    }
}
