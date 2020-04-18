using UnityEngine;
using System.Collections;

public class ReverbZone : MonoBehaviour {

    AudioReverbFilter filter;

    void Start() {
        filter = GameObject.FindGameObjectWithTag(Tags.player).GetComponent<AudioReverbFilter>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject == Camera.main.gameObject) {
            filter.enabled = true;
            AudioManager.instance.CameraInside(true);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject == Camera.main.gameObject) {
            filter.enabled = false;
            AudioManager.instance.CameraInside(false);
        }
    }
}
