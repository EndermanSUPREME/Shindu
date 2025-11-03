using UnityEngine;
using System.Threading.Tasks;

namespace ShinduPlayer
{
    [CreateAssetMenu(fileName = "PlayerManager", menuName = "Scriptable_Objects/PlayerManager")]
    public class PlayerManager : ScriptableObject
    {
        PlayerState state;
        Animator playerAnim;

        [HideInInspector] public MovementTransforms movementTransforms;

        [Header("Player Stats")]
        public float moveSpeed = 4, jumpForce = 3, rotationSpeed = 120,
                    gravityMultiplier = 2, groundCheckRadius = 0.25f;

        [Header("Player Transforms")]
        public Transform leftFoot, rightFoot, wallCheckPoint;

        [HideInInspector] public float defaultColliderRadius;

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

        private static PlayerManager _instance;
        public static PlayerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<PlayerManager>("PlayerManager");
                    if (_instance == null)
                        Debug.LogError("PlayerManager asset not found in Resources folder!");
                }
                return _instance;
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