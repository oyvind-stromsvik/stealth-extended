using UnityEngine;
using System.Collections;

public class AnimatorSetup {

    // Damping time for the Speed parameter.
    public float speedDampTime = 0.1f;			
	// Damping time for the AngularSpeed parameter
    public float angularSpeedDampTime = 0.7f;		
    // Response time for turning an angle into angularSpeed.
    public float angleResponseTime = 0.6f;			

    // Reference to the animator component.
    Animator anim;					
	// Reference to the HashIDs script.	
    HashIDs hash;						

    // Constructor
    public AnimatorSetup(Animator animator, HashIDs hashIDs) {
        anim = animator;
        hash = hashIDs;
    }

    public void Setup(float speed, float angle) {
        float angularSpeed = angle / angleResponseTime;

        // Set the mecanim parameters and apply the appropriate damping to them.
        anim.SetFloat(hash.speedFloat, speed, speedDampTime, Time.deltaTime);
        anim.SetFloat(hash.angularSpeedFloat, angularSpeed, angularSpeedDampTime, Time.deltaTime);
    }
}
