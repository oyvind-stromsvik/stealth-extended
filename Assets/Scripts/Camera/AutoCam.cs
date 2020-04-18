using System;
using System.Collections;
using UnityEngine;

public class AutoCam : MonoBehaviour {

    // The transform to follow.
    public Transform target;
    // How fast the rig will move to keep up with target's position
    public float moveSpeed = 3;
    // How fast the rig will turn to keep up with target's rotation
    public float turnSpeed = 1;
    // The minimum velocity above which the camera turns towards the object's velocity. Below this we use the object's forward direction.
    public float targetVelocityLowerLimit = 4f;
    // The smoothing for the camera's rotation
    public float smoothTurnTime = 0.2f;
    // A layer mask used for the camera intersection raycast.
    public LayerMask layerMask;
    // The maximum distance of the camera.
    public float maxDistance = 2f;
    // The manual rotate speed for the camera.
    public float rotateSpeed = 3.0f;
    // The minimum y-value for manual rotation of the camera.
    public float yMin = -80f;
    // The maximum y-value for manual rotation of the camera.
    public float yMax = 80f;

    // The rigidbody attached to the target. Used for the automatic rotation.
    Rigidbody targetRigidbody;
    // How much to turn the camera
    float currentTurnAmount;
    // The change in the turn speed velocity
    float turnSpeedVelocityChange;
    // The transform of the camera
    Transform cam;
    // The point at which the camera pivots around
    Transform pivot;
    // The current distance between the camera and its target.
    float distance;
    // The rotation for the pivot
    float pivotRotation;
    // The distance the camera wants to achieve.
    float wantedCameraDistance;
    // Mouse input.
    Vector2 input;
    // Pause menu.
    Pause pauseMenu;

    void Awake() {
        cam = GetComponentInChildren<Camera>().transform;
        pivot = cam.parent;
        targetRigidbody = target.GetComponentInParent<Rigidbody>();
        pauseMenu = GameObject.Find("MenuCanvas").GetComponent<Pause>();
    }

    void Update() {
        if (pauseMenu.isPaused) {
            return;
        }

        // Camera position moves towards target position.
        AutoFollow();

        // The camera's rotation is aligned towards the object's velocity direction.
        AutoRotate();

        // Manually rotate the camera. If we're moving we're also turning the player.
        ManualRotation();

        // Makes sure the camera doesn't intersect geometry.
        UpdateCameraLocationPosition();
    }

    /**
     * Makes the camera smoothly follow the target. 
     */
    void AutoFollow() {
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * moveSpeed);
    }

    /**
     * Makes the camera smoothly rotate to align with the direction the target is moving.
     */
    void AutoRotate() {
        // We don't want to rotate if we're not moving. Then the manual rotation should have full control.
        if (targetRigidbody.velocity.magnitude < targetVelocityLowerLimit) {
            return;
        }

        // If we move the mouse about the automatic rotation.
        if (input.x != 0 || input.y != 0) {
            return;
        }

        // Rotate the camera towards the direction the target is moving.
        Vector3 targetForward = targetRigidbody.velocity.normalized;
        currentTurnAmount = Mathf.SmoothDamp(currentTurnAmount, 1, ref turnSpeedVelocityChange, smoothTurnTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetForward, Vector3.up), turnSpeed * currentTurnAmount * Time.deltaTime);
    }

    /**
     * Enables us to manually rotate the camera around the player.
     * Also turns the player if he's moving because his movement is relative
     * to the camera view.
     */
    void ManualRotation() {
        // Catch mouse input.
        input.x = Input.GetAxis("Mouse X") * rotateSpeed;
        input.y = Input.GetAxis("Mouse Y") * rotateSpeed;

        // Abort if there is no mouse input.
        if (input.x == 0 && input.y == 0) {
            return;
        }

        // Rotate the rig around the y-axis.
        Quaternion rigRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + input.x, 0);
        transform.rotation = rigRotation;

        // Rotate the pivot around the x-axis.
        pivotRotation -= input.y;
        if (pivotRotation > yMax) {
            pivotRotation = yMax;
        }
        if (pivotRotation < yMin) {
            pivotRotation = yMin;
        }
        pivot.localRotation = Quaternion.Euler(pivotRotation, 0, 0);
    }

    /**
     * Checks for geometry between the camera and the player and
     * moves the camera in front of it.
     */
    void UpdateCameraLocationPosition() {
        Vector3 origin = pivot.position;
        Vector3 direction = cam.position - pivot.position;
        RaycastHit hit;
        float distance = (cam.position - pivot.position).magnitude;

        if (Physics.Raycast(origin, direction, out hit, distance, layerMask)) {
            wantedCameraDistance = -1 * (hit.point - pivot.position).magnitude;
            cam.localPosition = new Vector3(0, 0, wantedCameraDistance);
        }
        else {
            wantedCameraDistance = -1 * maxDistance;
        }

        cam.localPosition = new Vector3(0, 0, Mathf.Lerp(cam.localPosition.z, wantedCameraDistance, Time.deltaTime));
    }
}
