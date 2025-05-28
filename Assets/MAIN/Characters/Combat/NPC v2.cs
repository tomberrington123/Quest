using System.Collections;
using UnityEngine;


public class NPCv2 : MonoBehaviour
{
    // ==== CONFIGURABLE VALUES ====

    // Speed at which the NPC moves when chasing or strafing
    public float moveSpeed = 3f;

    // Maximum distance at which the NPC will begin chasing the player
    public float chaseRange = 10f;

    // Distance at which the NPC stops chasing and begins combat behaviours like strafing
    public float combatRange = 3f;

    // Distance at which the NPC begins attacking the player
    public float attackRange = 1f;


    // ==== REFERENCES ====

    // Player's transform (auto-assigned if Character script found)
    public Transform player;

    // CharacterController used for NPC movement
    public CharacterController characterController;

    // Animator used to play movement and attack animations
    public Animator animator;


    // ==== STATE CONTROL ====

   

    // Gravity value applied if NPC is not grounded
    private float gravity = -9.81f;

    // Current vertical movement used for gravity calculation
    private float verticalVelocity = 0f;


    // Whether NPC is in hit reaction
    private bool isHit = false;
    public GameObject glove_L, glove_R;
    public GameObject punch_L, punch_R;

    private int hitCounter = 0;      // A private integer to keep track of how many times the enemy has been hit
    private bool isDead = false;     // A private boolean to make sure the death logic only runs once

    // ==== NPC STATES ====
    private enum NPCState
    {
        Idle,
        Chasing,
        Combat,
        Attacking,
        Hit,
        Dead
    }
    // Current state of the NPC
    NPCState currentState = NPCState.Idle;  


    // ==== INITIAL SETUP ====

    void Start()
    {
        // Attempt to find the player based on Character script
        Character characterScript = FindObjectOfType<Character>();
        try
        {
            player = Transform.FindFirstObjectByType<Character>().transform;
        }
        catch (System.Exception e)
        {
            Debug.Log("No player found, please assign manually.");
            Debug.LogError(e.Message);
        }


        // Get references to essential components
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }


    // ==== MAIN UPDATE LOOP ====

    void Update()
    {

        // Exit early if player/controller not assigned or NPC is currently attacking
        if (player == null || characterController == null || IsAttacking || isHit) return;


        // Calculate how far away the player is
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Don't perform new behaviours if already doing one
        //H: I added the !isHit here, because it was on all the other checks, so it makes sense to have it here too.
        if (isDead) return; // If dead, skip all logic


        if (currentState != NPCState.Combat && !isHit)
        {
            if (distanceToPlayer <= attackRange)
            {
                // Close enough to punch
                StartCoroutine(PerformAttack());
            }
            else if (distanceToPlayer <= combatRange)
            {
                // In combat zone (e.g., for strafing)
                StartCoroutine(GenerateRandomBehaviour());
            }
            else if (distanceToPlayer <= chaseRange)
            {
                // Not in combat zone yet, keep chasing
                MoveTowardsPlayer();
            }
        }
    }


    // ==== NPC CHASES PLAYER ====

    public void MoveTowardsPlayer()
    {

        if (!isHit)
        {
            // Direction toward the player on the XZ plane
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;

            // Apply gravity if not grounded
            verticalVelocity += characterController.isGrounded ? -verticalVelocity : gravity * Time.deltaTime;

            // Final movement vector including horizontal and vertical movement
            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            movement.y = verticalVelocity * Time.deltaTime;

            // Apply movement
            characterController.Move(movement);

            // Smooth rotation towards player
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, moveSpeed * 100 * Time.deltaTime);
            }

            // Play running animation
            animator.Play("RunForward");
        }       
    }


    // ==== NPC ATTACKS ====

    public IEnumerator PerformAttack()
    {
        // Lock out other actions during attack
        IsAttacking = true;

        // Choose random punch animation
        string attackAnim = Random.Range(0, 2) == 0 ? "PunchLeft" : "PunchRight";
        animator.Play(attackAnim);
        punch_L.SetActive(true);
        punch_R.SetActive(true);

        // Stand still while punching
        // TODO: This is saying "Move to nowhere at zero speed for 1 second"... which is kinda like doing nothing or waiting for 1 second??
        yield return MoveOverTime(Vector3.zero, 1f, 0f);

        // Re-enable actions
        IsAttacking = false;
        punch_L.SetActive(false);
        punch_R.SetActive(false);

        // Follow up with combat behaviour
        //H: i dont' quite get this code, here, what is it trying to do if on update the combat behavior is also enacted...
        StartCoroutine(GenerateRandomBehaviour());

    }


    // ==== COMBAT MOVEMENT BEHAVIOURS (STRAFE, ADVANCE) ====
    private enum CombatBehaviour
    {
        StrafeLeft,
        StrafeRight,
        RunBackward,
        Idle,
        RunForward,
        RollBackward,
        BlockingLoop
    }
    CombatBehaviour currentCombatBehaviour = CombatBehaviour.Idle;
    private CombatBehaviour GetRandomCombatBehaviour()
    {
        CombatBehaviour[] values = (CombatBehaviour[])System.Enum.GetValues(typeof(CombatBehaviour));
        int randomIndex = Random.Range(0, values.Length);
        return values[randomIndex];
    }

    public bool IsAttacking {
        get { return currentState == NPCState.Attacking; }
        private set { currentState = NPCState.Attacking; }
    }

    IEnumerator GenerateRandomBehaviour()
    {
        // Prevent overlapping behaviours
        currentState = NPCState.Combat;

        // Pick a random behaviour to execute
        CombatBehaviour randomCombatBehavior = GetRandomCombatBehaviour();
        Vector3 moveDirection = Vector3.zero;

        // Direction toward player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;

        // Check how close the player is now
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Choose combat behaviour based on roll
        switch (randomCombatBehavior)
        {                
            case CombatBehaviour.StrafeLeft:
                // Strafe left
                animator.Play("StrafeLeft");
                moveDirection = -transform.right;
                break;
            case CombatBehaviour.StrafeRight:
                // Strafe right
                animator.Play("StrafeRight");
                moveDirection = transform.right;
                break;
            case CombatBehaviour.RunBackward:
                animator.Play("RunBackward");
                moveDirection = -transform.forward;
                break;
            case CombatBehaviour.BlockingLoop:
                /*
                animator.Play("Idle");
                moveDirection = Vector3.zero;
                break;
                */
                if (distanceToPlayer > attackRange && !isHit)
                {
                    // Move forward if not yet close enough to attack
                    animator.Play("BlockingLoop");
                    moveDirection = Vector3.zero;
                }
                else
                {
                    ;
                    // Too close, just attack again instead
                    //H: let's heck on this, because it seems it will never be executed, because the distance is always less than attackRange
                    StartCoroutine(PerformAttack());
                    yield break;
                }
                break;
            case CombatBehaviour.RunForward:
                if (distanceToPlayer > attackRange && !isHit)
                {
                    // Move forward if not yet close enough to attack
                    animator.Play("RunForward");
                    moveDirection = transform.forward;
                }
                else
                {
                    // Too close, just attack again instead
                    //H: let's heck on this, because it seems it will never be executed, because the distance is always less than attackRange

                    StartCoroutine(PerformAttack());
                    yield break;
                }
                break;
          
            case CombatBehaviour.RollBackward:

                if (distanceToPlayer > attackRange && !isHit)
                {
                    // Move forward if not yet close enough to attack
                    animator.Play("RollBackward");
                    moveDirection = -transform.forward;
                }
                else
                {
                    // Too close, just attack again instead
                    animator.Play("RollBackward");
                    moveDirection = -transform.forward;
                    yield break;
                }
                break;


        }

        // Perform movement for a short time
        yield return MoveOverTime(moveDirection, 1f, 2f);

        // Allow new behaviours after finishing
        currentState = NPCState.Combat;
    }


    // ==== GENERALIZED SMOOTH MOVEMENT HANDLER ====

    IEnumerator MoveOverTime(Vector3 moveDirection, float duration, float speed)
    {
        float elapsedTime = 0f;


        while (elapsedTime < duration)
        {
            // Keep facing the player throughout the movement
            Vector3 updatedDirection = (player.position - transform.position).normalized;
            updatedDirection.y = 0;

            // Smooth rotation
            Quaternion targetRotation = Quaternion.LookRotation(updatedDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360 * Time.deltaTime);

            // Move in chosen direction at specified speed
            characterController.Move(moveDirection * speed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for next frame
        }
    }



    // ==== HIT REACTION ====

    /*
     * 
     * 
     * Design: Whenever I (I am the NPC) get hit by a punch, ... scream.
     * 
     * How to:
     * Check whether I was hit by any collider....
     * And if the collider is a "Glove"
     * Then:
     * Yell!
     * 
     */



    /// Freezes all behaviour and plays GetHit animation.

    private IEnumerator HandleHitReaction()
    {
        hitCounter++;
        if (hitCounter >= 3)         // Check if the hit counter has reached 3 or more
        {

            animator.StopPlayback();
            StartCoroutine(Die()); // If so, start the IsDead coroutine to handle the death process
        }
        else
        {
            isHit = true;
            IsAttacking = false;
            currentState = NPCState.Idle; // Set the current state to Idle to prevent further actions

            //AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            //Vector3 moveDirection = Vector3.zero;
            // Play the GetHit animation
            //StopAllCoroutines();
            animator.StopPlayback();
            animator.Play("GetHit", 0, 0f);
            // Increment the hit counter by 1
            // Wait for the duration of the hit animation (adjust as needed)
            //Vector3 movement = Vector3.zero;
            yield return new WaitForSeconds(0.667f);
            // Resume normal behaviour
            isHit = false;
            //isAttacking = true;
            //playingCombatBehaviour = true;
            //StartCoroutine(GenerateRandomBehaviour());
        }




    }



    private void OnTriggerEnter(Collider other)
    {
        // Step 1: Check if the colliding object has the "Glove" tag
        if (other.gameObject == glove_L || other.gameObject == glove_R)
        {
            animator.StopPlayback();
            StopAllCoroutines();
            Vector3 movement = Vector3.zero;
            UnityEngine.Debug.Log("hit");
            StartCoroutine(HandleHitReaction());
        }
    }



    public IEnumerator Die()     // Coroutine that handles what happens when the enemy dies
    {
        animator.StopPlayback();
        StopAllCoroutines();
        isDead = true;               // Set the isDead flag to true to prevent further hits
        CapsuleCollider col = GetComponent<CapsuleCollider>();   // Disable CapsuleCollider
        col.enabled = false;

        Vector3 movement = Vector3.zero;
        animator.Play("Death");
        yield return new WaitForSeconds(1f); // Wait for 1 second (can simulate death animation duration, etc.)
        UnityEngine.Debug.Log("dead");
        //animator.Play("Death");
        //yield return new WaitForSeconds(1f); // Wait for 1 second (can simulate death animation duration, etc.)

        //Destroy(gameObject);         // Remove the enemy GameObject from the scene
    }
}