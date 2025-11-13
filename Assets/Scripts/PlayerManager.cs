using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;

public class PlayerManager : Singleton<PlayerManager>
{
    PlayerState state;

    [Header("Player Stats")]
    public float moveSpeed = 4, jumpForce = 3, rotationSpeed = 120,
                gravityMultiplier = 2, groundCheckRadius = 0.25f, playerHeight = 2.75f,
                enemySearchRange = 5, slideForce = 4f;
    public int attackDamage = 10;

    [Header("Player States")]
    public bool focused;
    public bool lockedIn;
    public bool isGrounded;
    public bool falling;
    public bool huggingWall;
    public bool crouched;
    public bool attacking;
    public bool blocking;
    public bool isRolling;
    public bool hanging;
    public bool droppingDown;

    [Header("Raycast Layers")]
    public LayerMask groundMask;
    public LayerMask wallLayer;
    public LayerMask ledgeLayer;
    public LayerMask enemyLayer;

    // runtime unity variables
    [SerializeField] Animator playerAnim;
    [SerializeField] CharacterController controller;
    [SerializeField] PlayerCamera playerCamera;

    public Transform leftFoot, rightFoot, wallCheckPoint, ledgeCheckPoint, attackPoint;
    public Sword playerSword;
    [HideInInspector] public float defaultColliderRadius;
    float fallVelocity;

    void Start()
    {
        defaultColliderRadius = controller.radius;
        if (playerAnim)
        {
            AttackSystem.SetAnimator(ref playerAnim);
        } else
            {
                Debug.LogWarning("Player Animator was not set!");
            }
    }

    // check if the player is using the movement-stick
    public bool IsMoving()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(x, 0f, z).normalized;

        return inputDir != Vector3.zero;
    }

    public Animator GetPlayerAnimator() { return playerAnim; }
    public bool AnimExists() { return playerAnim != null; }

    public CharacterController GetController() { return controller; }
    public bool ControllerEnabled() { return controller != null && controller.enabled; }

    public PlayerCamera GetPlayerCamera() => playerCamera;

    public float GetFallingVelocity() => fallVelocity;
    public void SetFallingVelocity(float v) { fallVelocity = v; }

    public void DisableRoot()
    {
        if (AnimExists()) { playerAnim.applyRootMotion = false; }
    }
    public void EnableRoot()
    {
        if (AnimExists()) { playerAnim.applyRootMotion = true; }
    }

    public void SetState(PlayerState newState){ state = newState; }
    public bool HasState(){ return state != null; }
    public PlayerState GetState(){ return state; }
}