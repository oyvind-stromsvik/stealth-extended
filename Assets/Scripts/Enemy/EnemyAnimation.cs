using UnityEngine;
using System.Collections;

public class EnemyAnimation : MonoBehaviour {

    // an array of footstep sounds that will be randomly selected from.
    public AudioClip[] footstepSounds;
    // The number of degrees for which the rotation isn't controlled by Mecanim.
    public float deadZone = 5f;

    // Reference to the player's transform.
    Transform player;
    // Reference to the EnemySight script.				
    EnemySight enemySight;
    // Reference to the nav mesh agent.
    UnityEngine.AI.NavMeshAgent nav;
    // Reference to the Animator.			
    Animator anim;
    // Reference to the HashIDs script.			
    HashIDs hash;
    // An instance of the AnimatorSetup helper class.		
    AnimatorSetup animSetup;
    AudioSource audioSource;
    bool playedLeftFootSound = false;
    bool playedRightFootSound = false;

    void Awake() {
        // Setting up the references.
        player = GameObject.FindGameObjectWithTag(Tags.player).transform;
        enemySight = GetComponent<EnemySight>();
        audioSource = GetComponent<AudioSource>();
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        anim = GetComponent<Animator>();
        hash = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<HashIDs>();

        // Making sure the rotation is controlled by Mecanim.
        nav.updateRotation = false;

        // Creating an instance of the AnimatorSetup class and calling it's constructor.
        animSetup = new AnimatorSetup(anim, hash);

        // Set the weights for the shooting and gun layers to 1.
        anim.SetLayerWeight(1, 1f);
        anim.SetLayerWeight(2, 1f);

        // We need to convert the angle for the deadzone from degrees to radians.
        deadZone *= Mathf.Deg2Rad;
    }

    void Update() {
        // Calculate the parameters that need to be passed to the animator component.
        NavAnimSetup();
    }

    void OnAnimatorMove() {
        if (Time.deltaTime == 0) {
            return;
        }

        // Set the NavMeshAgent's velocity to the change in position since the last frame, by the time it took for the last frame.
        nav.velocity = anim.deltaPosition / Time.deltaTime;

        // The gameobject's rotation is driven by the animation's rotation.
        transform.rotation = anim.rootRotation;
    }

    void NavAnimSetup() {
        // Create the parameters to pass to the helper function.
        float speed;
        float angle;

        // If the player is in sight...
        if (enemySight.playerInShootingRange) {
            // ... the enemy should stop...
            speed = 0f;

            // ... and the angle to turn through is towards the player.
            angle = FindAngle(transform.forward, player.position - transform.position, transform.up);
        }
        else {
            // Debug.
            //print(nav.hasPath + " | " + nav.desiredVelocity + " | " + nav.destination);

            // Otherwise the speed is a projection of desired velocity on to the forward vector...
            speed = Vector3.Project(nav.desiredVelocity, transform.forward).magnitude;

            // ... and the angle is the angle between forward and the desired velocity.
            angle = FindAngle(transform.forward, nav.desiredVelocity, transform.up);
            
            // If the angle is within the deadZone...
            if (Mathf.Abs(angle) < deadZone) {
                // ... set the direction to be along the desired direction and set the angle to be zero.
                transform.LookAt(transform.position + nav.desiredVelocity);
                angle = 0f;
            }
        }

        // Call the Setup function of the helper class with the given parameters.
        animSetup.Setup(speed, angle);
    }

    float FindAngle(Vector3 fromVector, Vector3 toVector, Vector3 upVector) {
        // If the vector the angle is being calculated to is 0...
        if (toVector == Vector3.zero) {
            // ... the angle between them is 0.
            return 0f;
        }

        // Create a float to store the angle between the facing of the enemy and the direction it's travelling.
        float angle = Vector3.Angle(fromVector, toVector);

        // Find the cross product of the two vectors (this will point up if the velocity is to the right of forward).
        Vector3 normal = Vector3.Cross(fromVector, toVector);

        // The dot product of the normal with the upVector will be positive if they point in the same direction.
        angle *= Mathf.Sign(Vector3.Dot(normal, upVector));

        // We need to convert the angle we've found from degrees to radians.
        angle *= Mathf.Deg2Rad;

        return angle;
    }

    void OnAnimatorIK(int layerIndex) {
        PlayFootFallSound(true);
        PlayFootFallSound(false);
    }

    // Check if the foot position is 
    bool CheckFootPosition(bool left) {
        AvatarIKGoal ikGoal = left ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
        float footBottomHeight = left ? anim.leftFeetBottomHeight : anim.rightFeetBottomHeight;

        Vector3 footPos = anim.GetIKPosition(ikGoal);
        Quaternion footRot = anim.GetIKRotation(ikGoal);

        footPos += footRot * new Vector3(0, -footBottomHeight, 0);

        footPos -= transform.position;

        return footPos.y <= footBottomHeight;
    }

    void PlayFootFallSound(bool left) {
        if (!CheckFootPosition(left)) {
            if (left) {
                playedLeftFootSound = false;
            }

            if (!left) {
                playedRightFootSound = false;
            }
            return;
        }

        if (left && playedLeftFootSound) {
            return;
        }

        if (!left && playedRightFootSound) {
            return;
        }

        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, footstepSounds.Length);
        audioSource.clip = footstepSounds[n];
        AudioManager.instance.PlaySoundOneShot(audioSource, audioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        footstepSounds[n] = footstepSounds[0];
        footstepSounds[0] = audioSource.clip;

        if (left) {
            playedLeftFootSound = true;
        }

        if (!left) {
            playedRightFootSound = true;
        }
    }
}
