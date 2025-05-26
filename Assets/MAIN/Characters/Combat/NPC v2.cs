using System.Collections;
using System.Diagnostics;
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

    // Whether the NPC is currently executing a combat behaviour (e.g., strafe)
    private bool playingCombatBehaviour = false;

    // Gravity value applied if NPC is not grounded
    private float gravity = -9.81f;

    // Current vertical movement used for gravity calculation
    private float verticalVelocity = 0f;

    // Whether the NPC is currently attacking
    public bool isAttacking = false;

    // Whether NPC is in hit reaction
    private bool isHit = false;
    public GameObject glove_L, glove_R;
    public GameObject punch_L, punch_R;

    private int hitCounter = 0;      // A private integer to keep track of how many times the enemy has been hit
    private bool isDead = false;     // A private boolean to make sure the death logic only runs once




    // ==== INITIAL SETUP ====

    void Start()
    {
        // Attempt to find the player based on Character script
        Character characterScript = FindObjectOfType<Character>();
        if (characterScript != null) player = characterScript.transform;

        // Get references to essential components
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();






    }


    // ==== MAIN UPDATE LOOP ====

    void Update()
    {











            // Exit early if player/controller not assigned or NPC is currently attacking
            if (player == null || characterController == null || isAttacking || isHit) return;
        // Calculate how far away the player is
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Don't perform new behaviours if already doing one
        if (!playingCombatBehaviour)
        {
            if (distanceToPlayer <= attackRange && !isHit)
            {
                // Close enough to punch
                StartCoroutine(PerformAttack());
            }
            else if (distanceToPlayer <= combatRange && !isHit)
            {
                // In combat zone (e.g., for strafing)
                StartCoroutine(GenerateRandomBehaviour());
            }
            else if (distanceToPlayer <= chaseRange && !isHit)
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
        } else
        {
            Vector3 moveDirection = Vector3.zero;
        }
    }


    // ==== NPC ATTACKS ====

    public IEnumerator PerformAttack()
    {
        if (!isHit)
        {
            // Lock out other actions during attack
            isAttacking = true;

            // Choose random punch animation
            string attackAnim = Random.Range(0, 2) == 0 ? "PunchLeft" : "PunchRight";
            animator.Play(attackAnim);
            punch_L.SetActive(true);
            punch_R.SetActive(true);

            // Stand still while punching
            yield return MoveOverTime(Vector3.zero, 1f, 0f);

            // Re-enable actions
            isAttacking = false;
            punch_L.SetActive(false);
            punch_R.SetActive(false);

            // Follow up with combat behaviour
            StartCoroutine(GenerateRandomBehaviour());
        }
        else
        {
            Vector3 moveDirection = Vector3.zero;
        }
    }


    // ==== COMBAT MOVEMENT BEHAVIOURS (STRAFE, ADVANCE) ====

    IEnumerator GenerateRandomBehaviour()
    {
        // Prevent overlapping behaviours
        playingCombatBehaviour = true;

        // Pick a random behaviour to execute
        int randomNumber = Random.Range(1, 8);
        Vector3 moveDirection = Vector3.zero;

        // Direction toward player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;

        // Check how close the player is now
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Choose combat behaviour based on roll
        switch (randomNumber)
        {
            case 1:
                // Strafe left

                    animator.Play("StrafeLeft");
                moveDirection = -transform.right;
                    break;


            case 2:
                        // Strafe right

                            animator.Play("StrafeRight");
                            moveDirection = transform.right;
                            break;
                        

                            

            case 3:

                animator.Play("RunBackward");
                moveDirection = -transform.forward;
                break;



            case 4:
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
                    StartCoroutine(PerformAttack());
                    yield break;
                }
                break;
            case 5:
                if (distanceToPlayer > attackRange && !isHit)
                {
                    // Move forward if not yet close enough to attack
                    animator.Play("RunForward");
                    moveDirection = transform.forward;
                }
                else
                {
                    ;
                    // Too close, just attack again instead
                    StartCoroutine(PerformAttack());
                    yield break;
                }
                break;
            case 6:
                if (distanceToPlayer > attackRange && !isHit)
                {
                    // Move forward if not yet close enough to attack
                    animator.Play("RunForward");
                    moveDirection = transform.forward;
                }
                else
                {
;
                    // Too close, just attack again instead
                    StartCoroutine(PerformAttack());
                    yield break;
                }
                break;

            case 7:

                if (distanceToPlayer > attackRange && !isHit)
                {
                    // Move forward if not yet close enough to attack
                    animator.Play("RollBackward");
                    moveDirection = -transform.forward;
                }
                else
                {
                    ;
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
        playingCombatBehaviour = false;
    }


    // ==== GENERALIZED SMOOTH MOVEMENT HANDLER ====

    IEnumerator MoveOverTime(Vector3 moveDirection, float duration, float speed)
    {
        float elapsedTime = 0f;
        if (!isHit)
        {
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
        else
        {
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
            isAttacking = false;
            playingCombatBehaviour = false;

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