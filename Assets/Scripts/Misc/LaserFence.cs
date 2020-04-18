using UnityEngine;
using System.Collections;

public class LaserFence : MonoBehaviour {

    public AudioClip laserFenceSound;

    public float minIntensity = 2.0f;
    public float maxIntensity = 2.2f;

    Light lightComponent;

    void Start() {
        lightComponent = GetComponent<Light>();

        // Play the gun shot clip at the position of the muzzle flare.
        AudioManager.instance.PlaySound(GetComponent<AudioSource>(), laserFenceSound, 0.1f, 50f, true);

        InvokeRepeating("CheckSoundOcclusion", 1f, 1f);
    }

    void CheckSoundOcclusion() {
        AudioManager.instance.CheckDistanceAndOcclusionToListener(GetComponent<AudioSource>());
    }

    void Update() {
        float val = minIntensity + Mathf.PingPong(Time.time * 3, maxIntensity - minIntensity);
        GetComponent<Renderer>().materials[0].SetColor("_EmissionColor", Color.red * val);

        lightComponent.intensity = val * 0.65f;
    }
}
