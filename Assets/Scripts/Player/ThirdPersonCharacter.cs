using UnityEngine;

public class ThirdPersonCharacter : MonoBehaviour {

    // The guards and the camera aim at this.
    public Transform head;
    public LayerMask layerMask;
    // an array of footstep sounds that will be randomly selected from.
    public AudioClip[] footstepSounds;
    public float movingTurnSpeed = 360;
    public float stationaryTurnSpeed = 180;
    [HideInInspector]
    public string state = "sneak";

    // A reference to the main camera in the scenes transform
    Transform cam;
    // The current forward direction of the camera  
    Vector3 camForward;
    // the world-relative desired move direction, calculated from the camForward and user input.  
    Vector3 move;                     
    bool crouch;
    float h;
    float v;
    bool walk;
    bool run;
    new Rigidbody rigidbody;
    Animator animator;
    float moveSpeedMultiplier = 1f;
    float animSpeedMultiplier = 1f;
    float turnAmount;
    float forwardAmount;
    float capsuleHeight;
    Vector3 capsuleCenter;
    CapsuleCollider capsule;
    AudioSource audioSource;
    bool playedLeftFootSound = false;
    bool playedRightFootSound = false;
    PlayerHealth health;
    bool disabled = false;
    // Pause menu.
    Pause pauseMenu;

    void Awake() {
        cam = Camera.main.transform;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        health = GetComponent<PlayerHealth>();
        pauseMenu = GameObject.Find("MenuCanvas").GetComponent<Pause>();
    }

    void Start() {
        capsuleHeight = capsule.height;
        capsuleCenter = capsule.center;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update() {
        if (pauseMenu.isPaused) {
            return;
        }

        if (disabled) {
            return;
        }

        if (health.playerDead) {
            return;
        }

        state = "sneak";

        // read inputs
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        // TODO: This movement state code is a mess. Clean it up!
        if (Input.GetKeyDown(KeyCode.C)) {
            crouch = !crouch;
            ScaleCapsuleForCrouching();
        }

        if (crouch) {
            state = "crouch";
        }

        // walk speed multiplier
        if (Input.GetKey(KeyCode.LeftShift) && (h != 0 || v != 0)) {
            run = true;
            state = "run";
        }
        else {
            run = false;
        }

        HandleSpeeds();
    }

    // Fixed update is called in sync with physics
    void FixedUpdate() {
        if (disabled) {
            return;
        }

        if (health.playerDead) {
            return;
        }

        // calculate move direction to pass to character
        if (cam != null) {
            // calculate camera relative direction to move:
            camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
            move = v * camForward + h * cam.right;
        }
        else {
            // we use world-relative directions in the case of no main camera
            move = v * Vector3.forward + h * Vector3.right;
        }

        // pass all parameters to the character control script
        Move(move);
    }

    public void Disable(bool value) {
        disabled = value;
        if (disabled) {
            v = 0;
            h = 0;
            move = Vector3.zero;
            forwardAmount = 0;
            state = "sneak";
            animator.SetFloat("Forward", 0);
            animator.SetBool("Crouch", false);
            animator.SetBool("Run", false);
        }
    }

    public void Move(Vector3 move) {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) {
            move.Normalize();
        }

        move = transform.InverseTransformDirection(move);
        turnAmount = Mathf.Atan2(move.x, move.z);
        forwardAmount = move.z;

        ApplyExtraTurnRotation();

        // send input and other state parameters to the animator
        UpdateAnimator(move);
    }

    void ScaleCapsuleForCrouching() {
        if (crouch) {
            capsule.height = capsule.height * 0.5f;
            capsule.center = capsule.center * 0.5f;
        }
        else {
            Ray crouchRay = new Ray(rigidbody.position + Vector3.up * capsule.radius * 0.5f, Vector3.up);
            float crouchRayLength = capsuleHeight - capsule.radius * 0.5f;
            if (Physics.SphereCast(crouchRay, capsule.radius * 0.5f, crouchRayLength, layerMask)) {
                crouch = true;
                return;
            }
            capsule.height = capsuleHeight;
            capsule.center = capsuleCenter;
        }
    }

    void HandleSpeeds() {
        switch (state) {
            case "crouch":
                moveSpeedMultiplier = 0.75f;
                animSpeedMultiplier = 2f;
                audioSource.volume = 0.15f;
                break;
            case "run":
                moveSpeedMultiplier = 1f;
                animSpeedMultiplier = 1f;
                audioSource.volume = 1f;
                break;
            default:
                moveSpeedMultiplier = 1f;
                if (move.magnitude > 0) {
                    animSpeedMultiplier = 2f;
                }
                else {
                    animSpeedMultiplier = 1f;
                }
                audioSource.volume = 0.2f;
                break;
        }
    }

    void UpdateAnimator(Vector3 move) {
        // update the animator parameters
        animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        animator.SetBool("Crouch", crouch);
        animator.SetBool("Run", run);

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        animator.speed = animSpeedMultiplier;
    }

    void ApplyExtraTurnRotation() {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
    }

    public void OnAnimatorMove() {
        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (Time.deltaTime > 0) {
            Vector3 v = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            v.y = rigidbody.velocity.y;
            rigidbody.velocity = v;
        }
    }

    void OnAnimatorIK(int layerIndex) {
        PlayFootFallSound(true);
        PlayFootFallSound(false);
    }

    // Check if the foot position is 
    bool CheckFootPosition(bool left) {
        AvatarIKGoal ikGoal = left ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
        float footBottomHeight = left ? animator.leftFeetBottomHeight : animator.rightFeetBottomHeight;

        Vector3 footPos = animator.GetIKPosition(ikGoal);

        footPos -= transform.position;

        return footPos.y <= footBottomHeight + 0.05f;
    }

    void PlayFootFallSound(bool left) {
        if (CheckFootPosition(left)) {
            if (left) {
                playedLeftFootSound = false;
            }
            else {
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
        else {
            playedRightFootSound = true;
        }
        
    }
}
