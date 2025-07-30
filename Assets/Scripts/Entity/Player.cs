using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Entity
{
    public static Player instance;

    [MyHeader("Player")]

    [Tooltip("Player's spawn point")]
    public Transform SpawnPoint;

    [Tooltip("Player's move speed")]
    [SerializeField] private float _moveSpeed = 5f;

    [Tooltip("Player's sprint speed")]
    [SerializeField] private float _sprintSpeed = 8f;

    [Tooltip("Sprint Key")]
    [SerializeField] private KeyCode _sprintKey = KeyCode.LeftShift;

    [Tooltip("Speed player rotates when not using mouse")]
    public float RotateSpeed = 0f;

    public Camera GameCamera;

    [Tooltip("Uncheck layers that raycast should ignore")]
    public LayerMask WhatCanMouseSee;

    [Header("References")]
    [Tooltip("Scipt to handle player's holding object")]
    [SerializeField] private PlayerHolder _holder;

    [MyHeader("Sounds")]
    [Tooltip("Time interval in between walking footstep sound")]
    [SerializeField] private float _footStepInterval = 0.1f;
    [Tooltip("Time interval in between running footstep sound")]
    [SerializeField] private float _runInterval = 0.1f;
    
    public PlayerHolder Holder => _holder;

    private bool isControllable = true;
    private float footstepTimer;

    private Animator animator;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    protected override void Start()
    {
        base.Start();

        animator = GetComponentInChildren<Animator>();

        if (!SpawnPoint)
        {
            GameObject spawnPointObject = new GameObject("Player Spawnpoint");
            spawnPointObject.transform.position = transform.position;
            spawnPointObject.transform.rotation = transform.rotation;
        }
    }

    protected override void Update()
    {
        if (!isDead)
        {
            base.Update();

            if (isControllable && !WaypointManager.instance.IsTeleporting)
            {
                MovePlayer();
                footstepTimer -= Time.deltaTime;
            }
            else
            {
                Rigid.velocity = Vector3.up * Rigid.velocity.y;

                animator.SetBool("IsSprinting", false);
                animator.SetBool("IsWalking", false);
            }

            UpdateUIElements();
        }
    }

    public override void OnTakeDamage(float damageAmount)
    {
        base.OnTakeDamage(damageAmount);

        if (!isDead)
        {
            ResetPlayerPos();
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();

        // Reset player position to spawnpoint.
        SceneTransitionController sceneTransition = FindObjectOfType<SceneTransitionController>();

        sceneTransition.DeathIn();
        Invoke("ResetPlayerPos", 0.5f);
        Invoke("ResetHealth", 1f);

        sceneTransition.Invoke("DeathOut", 1f);
    }

    // Function to set if player can control or not.
    public void SetControllable(bool value) => isControllable = value;

    // Function to reset player's position.
    public void ResetPlayerPos() => transform.position = SpawnPoint.position;

    private void UpdateUIElements()
    {
        HUDManager hudM = HUDManager.instance;
        Image healthFill = hudM.healthFill;
        healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, Health / MaxHealth, 5f * Time.deltaTime);
    }

    public float GetSpeed()
    {
        if (Input.GetKey(_sprintKey))
        {
            return _moveSpeed;
        }
        else
            return _sprintSpeed;
    }

    private void MovePlayer()
    {
        // If player is planting, make player unable to move.
        if (animator.GetBool("IsPlanting"))
        {
            Rigid.velocity = Vector3.up * Rigid.velocity.y;
            return;
        }

        // Get input Directions.
        float xMove = Input.GetAxisRaw("Horizontal");
        float zMove = Input.GetAxisRaw("Vertical");

        // Initialized movement direction.
        Vector3 dir = Vector3.zero;

        //target vector based on player input.
        dir += new Vector3(xMove, Rigid.velocity.y, zMove).normalized;
        
        // Set speed based on player's sprinting.
        float speed;

        if (Input.GetKey(_sprintKey))
        {
            animator.SetBool("IsSprinting", false);
            animator.SetBool("IsWalking", true);
            speed = _moveSpeed;
        }
        else
        {
            animator.SetBool("IsSprinting", true);
            animator.SetBool("IsWalking", false);
            speed = _sprintSpeed;
        }

        // Get the camera angle.
        dir = Quaternion.Euler(0, GameCamera.gameObject.transform.eulerAngles.y, 0) * dir;

        // Moves the player
        Rigid.velocity = new Vector3(dir.x * speed, Rigid.velocity.y, dir.z * speed);

        // If the player is moving, translate player's position.
        if (dir.x != 0f || dir.z != 0f)
        {
            // Create a forward rotation toward the target vector.
            Quaternion rotation = Quaternion.LookRotation(dir);
            rotation.x = 0f;
            rotation.z = 0f;

            // Rotates the player toward the target vector by the rotate speed.
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, RotateSpeed);

            // Plays walk/run SFX.
            if (animator.GetBool("IsWalking"))
            {
                PlayFootstep();
            }
            else
            {
                PlayRun();
            }
            
        }

        // If the player isn't moving, cancel player's movement animation.
        if (dir.x == 0f && dir.z == 0f)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsSprinting", false);
        }
    }

    // Function to get player's animator.
    public Animator GetAnimator() => animator;

    private void PlayFootstep()
    {
        if (footstepTimer < 0)
        {
            AudioManager.instance.RandomPlaySound("Walk");
            footstepTimer = _footStepInterval;
        }

    }

    private void PlayRun()
    {
        if (footstepTimer < 0)
        {
            AudioManager.instance.RandomPlaySound("Run");
            footstepTimer = _runInterval;
        }

    }
}
