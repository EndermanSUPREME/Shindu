using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;

public class PlayerManager : Singleton<PlayerManager>
{
    PlayerState state;

    [Header("Player Stats")]
    public float moveSpeed = 4, jumpForce = 3, rotationSpeed = 120,
                gravityMultiplier = 2, groundCheckRadius = 0.25f, playerHeight = 2.75f;

    [Header("Player States")]
    public bool focused;
    public bool lockedIn;
    public bool isGrounded;
    public bool falling;
    public bool huggingWall;
    public bool crouched;
    public bool isRolling;
    public bool hanging;

    [Header("Raycast Layers")]
    public LayerMask groundMask;
    public LayerMask wallLayer;
    public LayerMask ledgeLayer;

    // runtime unity variables
    Animator playerAnim;
    public MovementTransforms movementTransforms;
    public Transform leftFoot, rightFoot, wallCheckPoint, ledgeCheckPoint;
    [HideInInspector] public float defaultColliderRadius;

    // check if the player is using the movement-stick
    public bool IsMoving()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(x, 0f, z).normalized;

        return inputDir != Vector3.zero;
    }

    public void SetPlayerAnimator(Animator anim) { playerAnim = anim; }
    public Animator GetPlayerAnimator() { return playerAnim; }
    public bool AnimExists() { return playerAnim != null; }

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