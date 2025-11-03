using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;

public class PlayerMovement : MonoBehaviour
{
    // these variables all get copied to the PlayerManager singleton
    [SerializeField] LayerMask wallLayer;
    [SerializeField] Animator anim;
    [SerializeField] Transform leftFoot, rightFoot, wallCheckPoint;
    [SerializeField] float moveSpeed = 4, jumpForce = 3, rotationSpeed = 120,
                            gravityMultiplier = 2, groundCheckRadius = 0.25f;
    [SerializeField] LayerMask groundMask;

    public void FinishedRoll() { PlayerManager.Instance.isRolling = false; }

    void Start()
    {
        Application.targetFrameRate = 75;

        if (GetComponent<CharacterController>() != null)
        {
            PlayerManager.Instance.defaultColliderRadius = GetComponent<CharacterController>().radius;
        }

        if (anim != null)
        {
            PlayerManager.Instance.SetPlayerAnimator(anim);
        } else
            {
                Debug.LogWarning("PlayerMovement Missing Value for 'anim'!");
            }

        PlayerManager.Instance.moveSpeed = moveSpeed;
        PlayerManager.Instance.jumpForce = jumpForce;
        PlayerManager.Instance.rotationSpeed = rotationSpeed;
        PlayerManager.Instance.gravityMultiplier = gravityMultiplier;
        PlayerManager.Instance.groundCheckRadius = groundCheckRadius;

        PlayerManager.Instance.groundMask = groundMask;
        PlayerManager.Instance.wallLayer = wallLayer;

        PlayerManager.Instance.movementTransforms = new MovementTransforms(
                                leftFoot,
                                rightFoot,
                                wallCheckPoint
                            );

        PlayerManager.Instance.SetState(
            new NormalMovement(
                GetComponent<CharacterController>(),
                GetComponent<CapsuleCollider>()
            )
        );
    }

    void Update()
    {
        if (PlayerManager.Instance.HasState())
        {
            if (PlayerManager.Instance.GetState().ReadSignal() == null)
            {
                // perform current state when there is no signal
                PlayerManager.Instance.GetState().Perform();
            } else
                {
                    Debug.Log("Transitioning to New State");
                    PlayerState nState = PlayerManager.Instance.GetState().ReadSignal();
                    PlayerManager.Instance.SetState(nState);
                }
        }
    }
}//EndScript