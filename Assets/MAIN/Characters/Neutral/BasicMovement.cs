using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    public float walkDuration = 3f; // How long to walk
    public float walkSpeed = 2f; // Speed of movement
    public Vector3 walkDirection = Vector3.forward; // Direction of walking

    private float walkTimer;
    private bool isWalking = false;
    private bool isPausedByProximity = false;

    private float nextWalkTriggerTime = 0f;

    private Vector3 moveVelocity; // Holds current movement velocity
    private float verticalVelocity = 0f; // Tracks Y-axis velocity for gravity

    private CharacterController characterController;
    private Animator animator;

    public Transform mainCharacter;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (mainCharacter == null)
        {
            GameObject mc = GameObject.FindGameObjectWithTag("Player");
            if (mc != null) mainCharacter = mc.transform;
        }

        ResetWalkTriggerTimer();
    }

    void Update()
    {
        HandleProximity();

        // Handle walking triggers
        if (!isWalking && !isPausedByProximity)
        {
            nextWalkTriggerTime -= Time.deltaTime;
            if (nextWalkTriggerTime <= 0f)
            {
                TriggerWalk();
                ResetWalkTriggerTimer();
            }
        }

        // Handle walking movement
        if (isWalking && !isPausedByProximity)
        {
            moveVelocity = walkDirection.normalized * walkSpeed;
            walkTimer += Time.deltaTime;

            Vector3 direction = new Vector3(walkDirection.x, 0, walkDirection.z); // ignore vertical
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // 5f is rotation speed
            }
            if (walkTimer >= walkDuration)
            {
                StopWalking();
            }
        }
        else
        {
            moveVelocity = Vector3.zero;
        }

        // Gravity
        if (characterController.isGrounded)
        {
            verticalVelocity = -1f; // Small downward force to keep grounded
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime; // Apply gravity
        }

        moveVelocity.y = verticalVelocity;

        characterController.Move(moveVelocity * Time.deltaTime); // Move with full velocity
    }

    public void TriggerWalk()
    {
        if (isPausedByProximity) return;

        walkTimer = 0f;
        isWalking = true;
        animator.SetBool("isWalking", true);

        // Generate a random 360° direction
        float angle = Random.Range(0f, 360f);
        walkDirection = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)).normalized;
    }

    private void StopWalking()
    {
        isWalking = false;
        animator.SetBool("isWalking", false);

    }

    private void HandleProximity()
    {
        if (mainCharacter == null) return;

        float distance = Vector3.Distance(transform.position, mainCharacter.position);

        if (distance <= 3f)
        {
            if (!isPausedByProximity)
            {
                isPausedByProximity = true;
                StopWalking();
            }

            // Rotate to face MainCharacter
            Vector3 lookDirection = mainCharacter.position - transform.position;
            lookDirection.y = 0f; // prevent looking up/down
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        else
        {
            if (isPausedByProximity)
            {
                isPausedByProximity = false;
            }
        }
    }

    private void ResetWalkTriggerTimer()
    {
        nextWalkTriggerTime = Random.Range(1f, 5f);
    }
}