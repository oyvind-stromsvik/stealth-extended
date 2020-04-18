using UnityEngine;
using System.Collections;

public class CCTV : MonoBehaviour {

    public AudioClip servoSound;
    public float maxRotation = 75F;
    public float rotationSpeed = 15;
    public float trackPlayerSpeed = 4;

	// Reference to the player.
    GameObject player;				
	// Reference to the global last sighting of the player.
    LastPlayerSighting lastPlayerSighting;
    // The joint the camera rotates around.
    Transform joint;
    Quaternion initialRotation;
    bool lockedOnPlayer = false;
    float angle;

	void Start() {
		// Setting up the references.
		player = GameObject.FindGameObjectWithTag(Tags.player);
		lastPlayerSighting = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<LastPlayerSighting>();
        joint = transform.parent.parent;
        initialRotation = joint.rotation;

        // Play the gun shot clip at the position of the muzzle flare.
        AudioManager.instance.PlaySound(joint.GetComponent<AudioSource>(), servoSound, 0.1f, 50f, true);

        InvokeRepeating("CheckSoundOcclusion", 1f, 1f);
    }

    void CheckSoundOcclusion() {
        AudioManager.instance.CheckDistanceAndOcclusionToListener(joint.GetComponent<AudioSource>());
    }

    void Update() {
        if (lockedOnPlayer) {
            return;
        }

        joint.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
        if (Quaternion.Angle(initialRotation, joint.rotation) >= maxRotation) {
            rotationSpeed *= -1;
        }
    }
	
	void OnTriggerStay(Collider other) {
		// If the colliding gameobject is the player.
		if (other.gameObject == player) {
            // Don't do anything unless we can see the player. He could be behind something.
            Vector3 direction = player.transform.position - transform.position;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit)) {
                if (hit.transform.gameObject != player) {
                    return;
                }
            }

            lastPlayerSighting.lastPlayerPosition = player.transform.position;
 
            lockedOnPlayer = true;

            Quaternion newRotation = Quaternion.LookRotation(player.transform.position - joint.position, Vector3.up);
            newRotation.x = 0.0f;
            newRotation.z = 0.0f;
            if (Quaternion.Angle(initialRotation, newRotation) <= maxRotation) {
                joint.rotation = Quaternion.Slerp(joint.rotation, newRotation, Time.deltaTime * trackPlayerSpeed);
            }
		}
	}

    void OnTriggerExit(Collider other) {
		// If the colliding gameobject is the player.
        if (other.gameObject == player) {
            lockedOnPlayer = false;
        }
    }
}
