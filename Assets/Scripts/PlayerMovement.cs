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

        PlayerManager.Instance.SetState(
            new NormalMovement(
                GetComponent<CharacterController>(),
                GetComponent<CapsuleCollider>(),
                new MovementTransforms(
                    leftFoot,
                    rightFoot,
                    wallCheckPoint
                )
            )
        );
    }

    void Update()
    {
        /*
        if (PlayerManager.Instance.huggingWall)
        {
            // reset base vars
            jumpCount = 0;
            PlayerManager.Instance.isRolling = false;

            MoveAlongWall();
        } else
            {
                CheckWallCollision();
                Move();
            }
        */

        if (PlayerManager.Instance.HasState())
        {
            if (PlayerManager.Instance.GetState().ReadSignal() == null)
            {
                // perform current state when there is no signal
                PlayerManager.Instance.GetState().Perform();
            } else
                {
                    Debug.Log("Transitioning to New State");
                }
        }
    }

    async Task FlushWithWall(Vector3 normal)
    {
        Quaternion targetRotation = Quaternion.LookRotation(normal, Vector3.up);

        // Rotate until close enough
        while (Vector3.Angle(transform.forward, normal) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            await Task.Yield();
        }
    }

    // Transition from normal state into wall-hugging state
    void HugWall()
    {
        if (!PlayerManager.Instance.huggingWall)
        {
            PlayerManager.Instance.huggingWall = true;
            
            // SetColliderRadious(defaultColliderRadius / 4f);

            if (PlayerManager.Instance.AnimExists())
            {
                anim.Play("wall_hug");
            }
        }
    }

    // Move player along the wall during wall-hug state
    void MoveAlongWall()
    {
        // cancel wall hug
        if (!PlayerManager.Instance.IsMoving())
        {
            PlayerManager.Instance.huggingWall = false;
            anim.SetTrigger("ExitWallHug");
            // SetColliderRadious(defaultColliderRadius);
            return;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(x, 0f, z).normalized;

        if (inputDir != Vector3.zero && PlayerManager.Instance.AnimExists())
        {
            // compare the player fwd to the input dir
            // to calculate the move direction along the wall

            float angleToRight = Vector3.Angle(transform.right, inputDir);
            float angleToLeft = Vector3.Angle(-transform.right, inputDir);
            if (angleToRight < 50)
            {
                anim.SetFloat("wall_move_dir", -1);
            } else if (angleToLeft < 50)
                {
                    anim.SetFloat("wall_move_dir", 1);
                } else
                    {
                        
                        anim.SetFloat("wall_move_dir", 0);
                    }
        } else
            {
                anim.SetFloat("wall_move_dir", 0);
            }
    }

    void LedgeMovement()
    {
    }
}//EndScript