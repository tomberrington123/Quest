using System;
using System.Collections; // Needed for coroutines
using System.Diagnostics;
using UnityEngine; // Core Unity functionality
using UnityEngine.SceneManagement; // Needed to reload the scene on death

public class Character_v2 : MonoBehaviour
{
    // --- Variables ---
    // Movement speed while walking
    public float moveSpeed = 5f;
    // Speed multiplier when sprinting
    public float sprintMultiplier = 1.5f;
    // Height the character can jump
    public float jumpHeight = 2f;
    // Gravity strength pulling the character down
    public float gravity = 9.81f;
    // How quickly the character rotates to face the mouse
    public float rotationSpeed = 720f;

    // Time window to allow punch combos
    public float punchComboTime = 1f;
    // Speed during a roll
    public float rollSpeed = 5f;
    // Duration of a roll
    public float rollDuration = 0.5f;

    // Camera following the player
    public Transform cameraTransform;
    // Hitbox object for left punch
    public GameObject punch_L;
    // Hitbox object for right punch
    public GameObject punch_R;
    // Respawn position when dying
    public GameObject respawnPoint;
    // Upper body object for animation separation
    public GameObject upperBodyObj;
    // Body parts that are hidden during punching
    public GameObject[] bodyParts;

    // Cached Animator component
    private Animator animator;
    // Animator for the upper body
    private Animator upperAnimator;
    // Character Controller component for movement
    private CharacterController controller;

    // Starting or respawn position
    private Vector3 initialPosition;
    // Direction to roll in
    private Vector3 rollDirection;
    // Vertical motion (gravity, jumping)
    private float verticalVelocity;
    // Remaining time while rolling
    private float rollTimer;
    // Time the last punch happened
    private float lastPunchTime;

    // Character state flags
    private bool isPunching;
    private bool useLeftHand;
    private bool isRolling;
    private bool isBlocking;
    private bool isJumping;
    private bool isDead;
    
    // --- Methods ---

    // Called when the game starts
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        initialPosition = respawnPoint ? respawnPoint.transform.position : transform.position;

        // Now safe to get Animator from it
        upperAnimator = upperBodyObj.GetComponent<Animator>();
    }

    // Called once per frame to update character state
    void Update()
    {
        if (isDead) return; // Don't do anything if dead

        HandleCamera();
        RotateToMouse();
        HandleInput();
        ApplyGravity();
        //MoveCharacter();
        HandleIdleState();
    }

    // Handles all player input (movement, attack, block, roll, jump)
    void HandleInput()
    {
        Vector3 inputDir = Vector3.zero;

        bool forward = Input.GetKey(KeyCode.W);
        bool backward = Input.GetKey(KeyCode.S);
        bool left = Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.D);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool punch = Input.GetMouseButtonDown(0);
        bool block = Input.GetMouseButton(1);
        bool roll = Input.GetKeyDown(KeyCode.LeftShift);

        if (roll && !isRolling)
            StartRoll();

        if (punch)
            HandlePunch();

        if (block)
            StartBlock();
        else
            StopBlock();

        if (isRolling)
        {
            Roll();
            return;
        }

        if (forward) inputDir += Vector3.forward;
        if (backward) inputDir += Vector3.back;
        if (left) inputDir += Vector3.left;
        if (right) inputDir += Vector3.right;

        if (controller.isGrounded && jump)
            Jump();

        Move(inputDir.normalized);
    }

    // Moves the character based on input
    void Move(Vector3 dir)
    {
        Vector3 move = dir * moveSpeed * Time.deltaTime;
        move.y = verticalVelocity * Time.deltaTime;
        controller.Move(move);

        animator.SetBool("RunForward", dir.magnitude > 0.1f);
    }

    // Applies gravity to the character
    void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            verticalVelocity = -gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }

    // Makes the character jump
    void Jump()
    {
        verticalVelocity = Mathf.Sqrt(jumpHeight * 2 * gravity);
        animator.Play("Jump");
    }

    // Starts a roll
    void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;
        rollDirection = transform.forward;
        animator.SetTrigger("Roll");
    }

    // Executes rolling movement
    void Roll()
    {
        if (rollTimer > 0)
        {
            controller.Move(rollDirection * rollSpeed * Time.deltaTime);
            rollTimer -= Time.deltaTime;
        }
        else
        {
            isRolling = false;
        }
    }

    // Handles starting a punch attack
    void HandlePunch()
    {
        if (isPunching) return;

        isPunching = true;
        useLeftHand = (Time.time - lastPunchTime <= punchComboTime) ? !useLeftHand : (UnityEngine.Random.value > 0.5f);
        lastPunchTime = Time.time;

        StartCoroutine(DoPunch());
    }

    // Performs the actual punch attack
    IEnumerator DoPunch()
    {
        HideBodyParts();
        upperAnimator.SetBool(useLeftHand ? "PunchLeft" : "PunchRight", true);

        yield return new WaitForSeconds(0.3f);
        (useLeftHand ? punch_L : punch_R).SetActive(true);

        yield return new WaitForSeconds(0.5f);
        (useLeftHand ? punch_L : punch_R).SetActive(false);

        upperAnimator.SetBool(useLeftHand ? "PunchLeft" : "PunchRight", false);
        ShowBodyParts();

        isPunching = false;
    }

    // Starts blocking state
    void StartBlock()
    {
        isBlocking = true;
        animator.SetBool("isBlocking", true);
        upperAnimator.SetBool("isBlocking", true);
    }

    // Stops blocking state
    void StopBlock()
    {
        isBlocking = false;
        animator.SetBool("isBlocking", false);
        upperAnimator.SetBool("isBlocking", false);
    }

    // Updates the idle animation when not moving
    void HandleIdleState()
    {
        bool isIdle = controller.velocity.magnitude < 0.1f && controller.isGrounded && !isPunching;
        animator.SetBool("Idle", isIdle);
    }

    // Rotates character to face the mouse pointer
    void RotateToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, transform.position);

        if (ground.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            Vector3 dir = point - transform.position;
            dir.y = 0;

            if (dir.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    // Updates the camera position relative to the player
    void HandleCamera()
    {
        if (cameraTransform)
        {
            cameraTransform.position = transform.position + new Vector3(0, 5, -5);
            cameraTransform.LookAt(transform.position);
        }
    }

    // Hides upper body parts during a punch
    void HideBodyParts()
    {
        foreach (var part in bodyParts)
            part.SetActive(false);
    }

    // Restores upper body parts after a punch
    void ShowBodyParts()
    {
        foreach (var part in bodyParts)
            part.SetActive(true);
    }

    // Reacts when hit by enemy punch
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Punch") && !isBlocking)
            StartCoroutine(HandleHit());
    }

    // Handles getting hit by an enemy attack
    IEnumerator HandleHit()
    {
        animator.Play("GetHit");
        controller.enabled = false;

        yield return new WaitForSeconds(0.4f);

        controller.enabled = true;
    }

    // Handles death and respawn
    public IEnumerator Die()
    {
        isDead = true;
        animator.Play("Death");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}