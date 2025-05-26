using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Character : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float rotationSpeed = 720f;
    public Transform cameraTransform;
    public float jumpHeight = 2.0f; // Height of the jump
    public float gravity = 9.81f;  // Gravity applied to the character
    public float strafeThreshold = 0.2f; // Threshold for triggering strafing
    public float forwardThreshold = 0.1f; // Threshold for forward/backward motion to take priority
    public float strafeToRunTime = 2.0f; // Time before strafing transitions to running
    public float mouseDistanceThreshold = 1.0f; // Minimum mouse distance for accurate direction
    public float punchComboTime = 1.0f; // Time window for punch combo

    private bool isBlocking;

    private Animator animator;

    private Animator upperBodyAnimator;

    private CharacterController characterController;
    private float verticalVelocity; // Tracks vertical movement
    private bool isJumping; // Tracks if the character is currently jumping

    private bool isPunching; // Tracks if a punch is currently active
    private bool useLeftHand; // Determines which hand is used for the next punch
    private float lastPunchTime; // Tracks the time of the last punch

    private float strafeTimer = 0f; // Tracks strafing duration
    private bool isStrafing = false; // Tracks whether the player is currently strafing


    public float rollSpeed = 5f;
    public float rollDuration = 0.5f;

    private bool isRolling = false;
    private float rollTimer = 0f;
    private Vector3 rollDirection;

    // Reference to the punch hitbox GameObject / This should have a trigger collider and the PunchHitbox script attached
    public GameObject punch_L;
    public GameObject punch_R;

    // Reference to the LowerBody GameObject (will be found at runtime)
    //private Transform lowerBody;
    private Transform upperBody;

    //public GameObject legs;
    //public GameObject feet;
    //public GameObject ObjectlowerBody;
    public GameObject ObjectupperBody;   // Separate LowerBody GameObject
    public GameObject legs;
    public GameObject feet;
    public GameObject head;
    public GameObject neck;
    public GameObject chest;
    public GameObject arms;
    public GameObject hands;
    public GameObject customisation;
    bool hasClicked = false;

    public GameObject respawnPoint;  // Assign your respawn object here in the Inspector
    private Vector3 initialPosition;


    private int hitCounter = 0;      // A private integer to keep track of how many times the enemy has been hit
    private bool isDead = false;     // A private boolean to make sure the death logic only runs once
    private bool isHit = false;

    void Start()
    {
        if (respawnPoint != null)
        {
            initialPosition = respawnPoint.transform.position;
        }
        else
        {
            initialPosition = transform.position;
        }

        animator = GetComponent<Animator>();

        characterController = GetComponent<CharacterController>();




        // Try to find the child named "LowerBody" (case-sensitive)
        //Transform foundLowerBody = transform.Find("LowerBody");
        //lowerBody = foundLowerBody;
        Transform foundUpperBody = transform.Find("UpperBody");
        upperBody = foundUpperBody;

        // Optionally auto-find them if not assigned
        //if (legs == null) legs = transform.Find("Mesh/Body/Legs")?.gameObject;
        //if (feet == null) feet = transform.Find("Mesh/Body/Feet")?.gameObject;
        //if (ObjectlowerBody == null) ObjectlowerBody = transform.Find("LowerBody")?.gameObject;
        if (ObjectupperBody == null) ObjectupperBody = transform.Find("UpperBody")?.gameObject;
        if (legs == null) legs = transform.Find("Mesh/Body/Legs")?.gameObject;
        if (feet == null) feet = transform.Find("Mesh/Body/Feet")?.gameObject;
        if (head == null) head = transform.Find("Mesh/Body/Head")?.gameObject;
        if (neck == null) neck = transform.Find("Mesh/Body/Neck")?.gameObject;
        if (chest == null) chest = transform.Find("Mesh/Body/Chest")?.gameObject;
        if (arms == null) arms = transform.Find("Mesh/Body/Arms")?.gameObject;
        if (hands == null) hands = transform.Find("Mesh/Body/Hands")?.gameObject;
        if (customisation == null) customisation = transform.Find("Mesh/Customization")?.gameObject;

    }

    void Update()
    {

        // Adjust camera position
        UpdateCameraPosition();



        upperBodyAnimator = ObjectupperBody.GetComponent<Animator>();



        RotateCharacterToMouse();
        /*
        //  Sync LowerBody to stay visually aligned with main character
        if (lowerBody != null)
        {
            //  Keep it locked to this character’s position
            lowerBody.position = transform.position;

            //  Match this character’s facing direction
            lowerBody.rotation = transform.rotation;
        }
        */

        // Get input
        bool forward = Input.GetKey(KeyCode.W);
        bool backward = Input.GetKey(KeyCode.S);
        bool left = Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.D);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool punch = Input.GetMouseButtonDown(0); // Left mouse button for punching
        bool roll = Input.GetKey(KeyCode.LeftShift);

        if (isDead)
        {
            // Do something when the character is dead
            this.enabled = false;
        }


        if (Input.GetKey(KeyCode.LeftShift) && !isRolling)
        {
            StartRoll();
        }

        if (isRolling)
        {
            Roll();
        }

        // Reset animator parameters
        ResetAnimatorParameters();

        // Handle punches
        HandlePunch(punch);


        if (Input.GetMouseButtonDown(0))
        {
            hasClicked = true;
        }







        // Calculate world movement direction
        Vector3 direction = Vector3.zero;


        if (Input.GetKey(KeyCode.LeftShift)) // Shift
        {
            animator.SetTrigger("Roll"); // Play the Roll animation
        }

        if (Input.GetMouseButton(1)) // While right mouse is held
        {
            ObjectupperBody.GetComponent<Animator>().SetBool("isBlocking", true);
            animator.SetBool("isBlocking", true);

            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("BlockingLoop"))
            {
                //animator.Play("BlockingLoop", 0);
                isBlocking = true;
                animator.SetBool("isBlocking", true);

                //ObjectupperBody.GetComponent<Animator>().SetBool("isBlocking", true);
            }
        }
        else
        {
            //ObjectupperBody.GetComponent<Animator>().SetBool("isBlocking", false);
            animator.SetBool("isBlocking", false);

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("BlockingLoop"))
            {
                //animator.Play("Idle", 0); // Or your desired fallback animation
                isBlocking = false;
                animator.SetBool("isBlocking", false);
                //ObjectupperBody.GetComponent<Animator>().SetBool("isBlocking", false);
            }
        }


        if (!isRolling && !animator.GetBool("isBlocking"))
        {

            if (forward)
            {
                direction += Vector3.forward;
            }

            if (backward)
            {
                direction += Vector3.back;
            }

            if (left)
            {
                direction += Vector3.left;
            }

            if (right)
            {
                direction += Vector3.right;
            }

            if (forward && hasClicked)
            {
                //animator.SetBool("Idle", false);
                animator.SetBool("RunForward", true);
                //animator.Play("RunForward");
                direction += Vector3.forward;
            }

            if (backward && hasClicked)
            {
                //animator.Play("RunBackward");
                direction += Vector3.back;
                animator.SetBool("RunBackward", true);
            }

            if (left && hasClicked)
            {
                //animator.Play("StrafeLeft");
                direction += Vector3.left;
                animator.SetBool("StrafeLeft", true);
            }

            if (right && hasClicked)
            {
                //animator.Play("StrafeRight");
                direction += Vector3.right;
                animator.SetBool("StrafeRight", true);
            }


        }

        // Normalize direction to ensure consistent movement speed
        direction.Normalize();

        // Handle animations based on relative movement (if not jumping or punching)
        if (!isJumping && !isPunching)
        {
            Vector3 localDirection = transform.InverseTransformDirection(direction);

            // Prioritise forward/backward running
            if (Mathf.Abs(localDirection.z) > forwardThreshold)
            {
                if (localDirection.z > 0.1f) // Moving forward
                {
                    animator.SetBool("RunForward", true);
                }
                else if (localDirection.z < -0.1f) // Moving backward
                {
                    animator.SetBool("RunBackward", true);
                }

                // Reset strafing state
                isStrafing = false;
                strafeTimer = 0f;
            }
            // Handle strafing and transition to running
            else if (Mathf.Abs(localDirection.x) > strafeThreshold)
            {
                if (localDirection.x > 0.1f) // Strafing right
                {
                    animator.SetBool("StrafeRight", true);
                }
                else if (localDirection.x < -0.1f) // Strafing left
                {
                    animator.SetBool("StrafeLeft", true);
                }

                // Start or continue strafing
                if (!isStrafing)
                {
                    isStrafing = true;
                    strafeTimer = 0f; // Reset timer
                }
                else
                {
                    strafeTimer += Time.deltaTime; // Increment timer
                }

                // Transition to running if strafing for too long
                if (strafeTimer > strafeToRunTime)
                {
                    if (localDirection.x > 0.1f) // Running right
                    {
                        animator.SetBool("RunForward", true); // Use forward running animation for smooth transition
                    }
                    else if (localDirection.x < -0.1f) // Running left
                    {
                        animator.SetBool("RunForward", true); // Use forward running animation
                    }

                    isStrafing = false; // Reset strafing state
                    strafeTimer = 0f; // Reset timer
                }
            }
            else
            {
                // Reset strafing state if no significant sideways movement
                isStrafing = false;
                strafeTimer = 0f;
            }
        }

        // Handle jump
        if (characterController.isGrounded) // Only jump if on the ground
        {
            if (jump)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * 1.3f * gravity); // Calculate initial jump velocity
                //isJumping = true;
                //animator.SetTrigger("Jump"); // Trigger Jump animation
                animator.Play("Jump");

            }
            else if (!isJumping)
            {
                verticalVelocity = -gravity * Time.deltaTime; // Reset vertical velocity when grounded
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime; // Apply gravity while in the air
        }

        // Combine movement direction with vertical velocity
        Vector3 movement = direction * moveSpeed * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;

        // Move the character
        characterController.Move(movement);

        // Check if character lands
        if (characterController.isGrounded && isJumping)
        {
            isJumping = false; // Reset jumping state
        }

        // Rotate character to face the mouse
        RotateCharacterToMouse();

        // Set idle if no movement and not jumping
        if (direction.magnitude < 0.1f && characterController.isGrounded && !isJumping && !isPunching && !isHit)
        {
            animator.SetBool("Idle", true);
        }




        







    }

    private void HandlePunch(bool punch)
    {
        if (punch)
        {

            if (isPunching)
                return; // Ignore additional punches if one is already active

            float currentTime = Time.time;

            // Determine which hand to use
            if (currentTime - lastPunchTime <= punchComboTime)
            {
                useLeftHand = !useLeftHand; // Alternate hands for combo
            }
            else
            {
                useLeftHand = UnityEngine.Random.Range(0, 2) == 0; // Randomly select the first hand
            }

            // Trigger the punch animation
            if (useLeftHand)
            {
                //animator.SetTrigger("PunchLeft");

                isPunching = true;
                //when punching
                //ObjectlowerBody.SetActive(true);
                ObjectupperBody.SetActive(true);

                head.SetActive(false);
                neck.SetActive(false);
                chest.SetActive(false);
                arms.SetActive(false);
                hands.SetActive(false);
                customisation.SetActive(false);

                //legs.SetActive(false);
                //feet.SetActive(false);
                //animator.Play("PunchLeft");

                StartCoroutine(EnablePunchHitbox_L());
            }
            else
            {
                //animator.SetTrigger("PunchRight");
                isPunching = true;
                //when punching
                //ObjectlowerBody.SetActive(true);
                ObjectupperBody.SetActive(true);

                head.SetActive(false);
                neck.SetActive(false);
                chest.SetActive(false);
                arms.SetActive(false);
                hands.SetActive(false);
                customisation.SetActive(false);

                //legs.SetActive(false);
                //feet.SetActive(false);
                //animator.Play("PunchRight");
                StartCoroutine(EnablePunchHitbox_R());
            }

            lastPunchTime = currentTime; // Update last punch time
            //StartCoroutine(PunchCooldown());
        }
    }

    private IEnumerator PunchCooldown()
    {



        //yield return new WaitForSeconds(0.80f); // Wait for animation to complete (adjust as needed)
        yield return new WaitForSeconds(0.10f); // Wait for animation to complete (adjust as needed)
                                                //animator.ResetTrigger("PunchLeft");
                                                //animator.ResetTrigger("PunchRight");
        //ObjectlowerBody.SetActive(false); // Hide punch legs
        ObjectupperBody.SetActive(false);

        head.SetActive(true);
        neck.SetActive(true);
        chest.SetActive(true);
        arms.SetActive(true);
        hands.SetActive(true);
        customisation.SetActive(true);

        //legs.SetActive(true);            // Restore legs
        //feet.SetActive(true);            // Restore feet

        isPunching = false; // Reset punching state  
        //animator.Play("Idle", 0);
    }

    private void RotateCharacterToMouse()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
        Plane groundPlane = new Plane(Vector3.up, this.transform.position);




        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPosition = ray.GetPoint(distance);
            Vector3 lookDirection = mouseWorldPosition - transform.position;
            lookDirection.y = 0;

            if (lookDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void ResetAnimatorParameters()
    {
        animator.SetBool("Idle", false);
        animator.SetBool("RunForward", false);
        animator.SetBool("RunBackward", false);
        animator.SetBool("StrafeRight", false);
        animator.SetBool("StrafeLeft", false);
    }

    private void UpdateCameraPosition()
    {
        if (cameraTransform != null)
        {
            Vector3 isometricOffset = new Vector3(0, 5, -5);
            cameraTransform.position = transform.position + isometricOffset;
            cameraTransform.LookAt(transform.position);
        }
    }



    void StartRoll()
    {
        animator.SetTrigger("Roll"); // Play the roll animation
        isRolling = true;
        rollTimer = rollDuration;
        rollDirection = transform.forward; // Set roll direction to current forward
    }

    void Roll()
    {
        if (rollTimer > 0)
        {
            characterController.Move(rollDirection * rollSpeed * Time.deltaTime); // Move forward
            rollTimer -= Time.deltaTime;
        }
        else
        {
            isRolling = false; // End the roll
        } 
    }


    /*
    private void PerformPunch()
    {
        // Play the punch animation on the character's Animator
        // Make sure this matches the name of the punch animation (e.g., "PunchRight" or "PunchLeft")

        // Start a coroutine to activate the punch hitbox for a short time
        StartCoroutine(EnablePunchHitbox());
    }
    */

    IEnumerator EnablePunchHitbox_R()
    {

        //animator.SetBool("PunchRight", true);
        ObjectupperBody.GetComponent<Animator>().SetBool("PunchRight", true);

        //animator.Play("PunchRight");
        //ObjectlowerBody.SetActive(true);
        ObjectupperBody.SetActive(true);

        head.SetActive(false);
        neck.SetActive(false);
        chest.SetActive(false);
        arms.SetActive(false);
        hands.SetActive(false);
        customisation.SetActive(false);

        //legs.SetActive(false);
        //feet.SetActive(false);
        yield return new WaitForSeconds(0.29f);
        punch_R.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        punch_R.SetActive(false);
        ObjectupperBody.GetComponent<Animator>().SetBool("PunchRight", false);

        StartCoroutine(PunchCooldown());
    }

    IEnumerator EnablePunchHitbox_L()
    {

        //animator.SetBool("PunchLeft", true);
        ObjectupperBody.GetComponent<Animator>().SetBool("PunchLeft", true);
        //animator.Play("PunchLeft");
        //ObjectlowerBody.SetActive(true);
        ObjectupperBody.SetActive(true);

        head.SetActive(false);
        neck.SetActive(false);
        chest.SetActive(false);
        arms.SetActive(false);
        hands.SetActive(false);
        customisation.SetActive(false);

        //legs.SetActive(false);
        //feet.SetActive(false);
        yield return new WaitForSeconds(0.29f);
        punch_L.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        punch_L.SetActive(false);
        ObjectupperBody.GetComponent<Animator>().SetBool("PunchLeft", false);
        
        StartCoroutine(PunchCooldown());
    }




    private IEnumerator HandleHitReaction()
    {
        hitCounter++;

        if (hitCounter >= 9)         // Check if the hit counter has reached 3 or more
        {
            isHit = true;
            animator.SetBool("isHit", true);
            animator.StopPlayback();
            characterController.enabled = false;
            StartCoroutine(Die()); // If so, start the IsDead coroutine to handle the death process
            isHit = false;
            animator.SetBool("isHit", false);
        }
        else
        {
            isHit = true;
            animator.SetBool("isHit", true);
            Vector3 movement = Vector3.zero;
            animator.Play("GetHit");
            characterController.enabled = false;

            yield return new WaitForSeconds(0.4f);
            characterController.enabled = true;
            isHit = false;
            animator.SetBool("isHit", false);
            ObjectupperBody.SetActive(false);
        }
        head.SetActive(true);
        neck.SetActive(true);
        chest.SetActive(true);
        arms.SetActive(true);
        hands.SetActive(true);
        customisation.SetActive(true);

        characterController.enabled = true;
        animator.SetBool("isHit", false);
        isHit = false;
        //StopAllCoroutines();
        //animator.SetBool("Idle", true);
    }



    public void OnTriggerEnter(Collider other)
    {
        
        // Step 1: Check if the colliding object has the "Glove" tag
        if (other.CompareTag("Punch") && !isBlocking)
        {
            characterController.enabled = false;
            UnityEngine.Debug.Log("playerhit");
            //StopAllCoroutines();
            //animator.StopPlayback();
            Vector3 movement = Vector3.zero;
            //UnityEngine.Debug.Log("hit");
            StartCoroutine(HandleHitReaction());
            //StopAllCoroutines();
            //animator.StopPlayback();
            //Vector3 movement = Vector3.zero;
            //UnityEngine.Debug.Log("hit");
            //StartCoroutine(HandleHitReaction());
        }
    }


    public IEnumerator Die()     // Coroutine that handles what happens when the enemy dies
    {
        //animator.StopPlayback();
        //StopAllCoroutines();
        // Set the isDead flag to true to prevent further hits
        Vector3 movement = Vector3.zero;
        CapsuleCollider col = GetComponent<CapsuleCollider>();   // Disable CapsuleCollider
        col.enabled = false;
        animator.Play("Death");
        yield return new WaitForSeconds(1.2f);

        animator.enabled = false;
        isDead = true;
        
        
        yield return new WaitForSeconds(2f); // Wait for 1 second (can simulate death animation duration, etc.)
        UnityEngine.Debug.Log("dead");
        //ObjectupperBody.SetActive(false);


        SceneManager.LoadScene(SceneManager.GetActiveScene().name);





        //animator.Play("Death");
        //yield return new WaitForSeconds(1f); // Wait for 1 second (can simulate death animation duration, etc.)

        //Destroy(gameObject);         // Remove the enemy GameObject from the scene
    }


}

