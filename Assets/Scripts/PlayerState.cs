using UnityEngine;
using System.Threading.Tasks;

namespace ShinduPlayer
{
    public class PlayerManager
    {
        PlayerState state;
        Animator playerAnim;

        public bool focused = false;
        public bool lockedIn = false;
        public bool isGrounded = false;
        public bool falling = false;
        public bool huggingWall = false;
        public bool crouched = false;
        public bool isRolling = false;
        public bool hanging = false;

        public float moveSpeed, jumpForce, rotationSpeed,
                gravityMultiplier, groundCheckRadius, defaultColliderRadius;

        public LayerMask groundMask, wallLayer;

        public MovementTransforms movementTransforms;

        private static PlayerManager _instance;
        // if the _instance exists return it, otherwise create a new instance
        public static PlayerManager Instance => _instance ??= new PlayerManager();

        private PlayerManager(){}

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
            if (PlayerManager.Instance.AnimExists()) { playerAnim.applyRootMotion = false; }
        }
        public void EnableRoot()
        {
            if (PlayerManager.Instance.AnimExists()) { playerAnim.applyRootMotion = true; }
        }

        public void SetState(PlayerState newState){ state = newState; }
        public bool HasState(){ return state != null; }
        public PlayerState GetState(){ return state; }
    }

    // base / abstract class
    public abstract class PlayerState
    {
        // shared variables between derived classes
        protected CharacterController controller;
        protected CapsuleCollider capCollider;
        protected PlayerState nextState = null;

        // shared methods between derived classes
        protected void SetColliderRadious(float r)
        {
            capCollider.radius = r;
            controller.radius = r;
        }

        // derived classes must implement this function
        public abstract void Perform();
        // Check if the next state has been dispatched, returns null if not dispatched
        public abstract PlayerState ReadSignal();
        // dispatch the next state
        public abstract void Signal(PlayerState pState);
    }

    public struct MovementTransforms
    {
        public Transform leftFoot, rightFoot, wallCheckPoint;

        public MovementTransforms(
            Transform leftFoot,
            Transform rightFoot,
            Transform wallCheckPoint
        )
        {
            this.leftFoot = leftFoot;
            this.rightFoot = rightFoot;
            this.wallCheckPoint = wallCheckPoint;
        }
    }
}