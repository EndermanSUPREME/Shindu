using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;
using ControllerInputs;

// derived / concrete class
public class NormalMovement : PlayerState
{
    const float gravity = -9.81f;
    Vector3 velocity;

    int jumpCount = 0;
    float moveBlend = 0;
    float styleBlend = 0;
    float wallPressTime = 0;

    Transform leftFoot, rightFoot, wallCheckPoint;

    public NormalMovement()
    {
        leftFoot = PlayerManager.Instance.leftFoot;
        rightFoot = PlayerManager.Instance.rightFoot;
        wallCheckPoint = PlayerManager.Instance.wallCheckPoint;

        PlayerManager.Instance.GetPlayerAnimator().SetFloat("style", 0);
        PlayerManager.Instance.GetPlayerAnimator().SetFloat("speed", 0);

        controller = PlayerManager.Instance.GetController();

        nextState = null;

        SetColliderRadious(PlayerManager.Instance.defaultColliderRadius);

        PlayerManager.Instance.EnableRoot();
    }
    ~NormalMovement()
    {
        // when dtor is ran reset falling vel
        PlayerManager.Instance.SetFallingVelocity(0);
    }

    // run within Update
    public override void Perform()
    {
        if (controller == null || nextState != null) return;

        Move();

        if (PlayerManager.Instance.AnimExists())
        {
            UpdateAnimator();
        }
    }
    // run within FixedUpdate()
    public override void FixedPerform()
    {
        if (controller == null || nextState != null) return;

        // physics related tasks
        CheckWallCollision();
        CheckLedges();
    }

    public override PlayerState ReadSignal() { return nextState; }
    public override void Signal(PlayerState pState) { nextState = pState; }

    bool IsCrouching()
    {
        return ControllerInput.HoldingRightBumper() && PlayerManager.Instance.isGrounded;
    }

    // returns true if either foot sphere is touching a ground layered object
    bool IsGrounded()
    {
        bool leftFootGrounded = Physics.CheckSphere(leftFoot.transform.position, PlayerManager.Instance.groundCheckRadius, PlayerManager.Instance.groundMask);
        bool rightFootGrounded = Physics.CheckSphere(rightFoot.transform.position, PlayerManager.Instance.groundCheckRadius, PlayerManager.Instance.groundMask);
        
        if (PlayerManager.Instance.droppingDown) {
            return leftFootGrounded || rightFootGrounded;
        }

        // check if we rolled off an edge
        if (PlayerManager.Instance.isRolling)
        {
            if (!leftFootGrounded && !rightFootGrounded)
            {
                PlayerManager.Instance.falling = true;
            }
            return leftFootGrounded || rightFootGrounded;
        }
        
        return leftFootGrounded || rightFootGrounded;
    }

    // specific methods for this derived class
    protected override void Move()
    {
        if (controller != null && controller.enabled)
        {
            // these 3 variables are needed here
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 inputDir = new Vector3(x, 0f, z);

            // mark when player is in a crouched state
            PlayerManager.Instance.crouched = IsCrouching();

            if (inputDir != Vector3.zero && !PlayerManager.Instance.focused && !PlayerManager.Instance.isRolling)
            {
                if (inputDir.magnitude >= 0.1f)
                {
                    moveBlend = inputDir.magnitude;

                    Vector3 moveDir = CalculateLookDirection(inputDir);

                    // apply controller input
                    if (jumpCount > 0)
                    {
                        // not using root-motion when falling/jumping
                        controller.Move(moveDir.normalized * PlayerManager.Instance.moveSpeed * Time.deltaTime);
                    }
                }
            
            } else
                {
                    moveBlend = 0;
                }

            PlayerManager.Instance.isGrounded = IsGrounded();

            if (!PlayerManager.Instance.isGrounded)
            {
                // NOT GROUNDED
                if (ControllerInput.PressedA() && jumpCount == 1)
                {
                    if (PlayerManager.Instance.AnimExists())
                    {
                        PlayerManager.Instance.DisableRoot();
                        PlayerManager.Instance.GetPlayerAnimator().Play("jumpTwo");
                    }

                    jumpCount = 2;
                    velocity.y = Mathf.Sqrt(PlayerManager.Instance.jumpForce * -2f * gravity);
                } else
                    {
                        // falling over-time
                        velocity.y += gravity * PlayerManager.Instance.gravityMultiplier * Time.deltaTime;
                        PlayerManager.Instance.SetFallingVelocity(Mathf.Abs(velocity.y));

                        if (!PlayerManager.Instance.falling)
                        {
                            PlayerManager.Instance.falling = true;
                        }
                    }
            } else
                {
                    PlayerManager.Instance.SetFallingVelocity(0);

                    // IS GROUNDED
                    if (PlayerManager.Instance.falling)
                    {
                        if (PlayerManager.Instance.AnimExists())
                        {
                            if (!PlayerManager.Instance.isRolling)
                            {
                                PlayerManager.Instance.EnableRoot();
                                
                                float landingVel = Mathf.Abs(velocity.y);
                                if (landingVel < 18)
                                {
                                    PlayerManager.Instance.GetPlayerAnimator().Play("soft_landing");
                                } else
                                    {
                                        PlayerManager.Instance.GetPlayerAnimator().Play("hard_landing");
                                    }
                            } else
                                {
                                    PlayerManager.Instance.isRolling = false;
                                }
                        }
                        PlayerManager.Instance.falling = false;
                    }

                    // if the player is dropping down once we hit the ground we
                    // can re-allow ledge finding
                    if (PlayerManager.Instance.droppingDown)
                    {
                        PlayerManager.Instance.droppingDown = false;
                    }

                    // base downwards vel while grounded
                    if (velocity.y < 0)
                    {
                        jumpCount = 0;
                        velocity.y = -2;
                    }

                    if (ControllerInput.PressedA() && jumpCount == 0)
                    {
                        if (!PlayerManager.Instance.crouched)
                        {
                            if (!PlayerManager.Instance.isRolling)
                            {
                                if (PlayerManager.Instance.AnimExists())
                                {
                                    PlayerManager.Instance.DisableRoot();
                                    PlayerManager.Instance.GetPlayerAnimator().Play("jumpOne");
                                }

                                jumpCount = 1;
                                velocity.y = Mathf.Sqrt(PlayerManager.Instance.jumpForce * -2f * gravity);
                            }
                        } else
                            {
                                PlayerManager.Instance.isRolling = true;
                                PlayerManager.Instance.GetPlayerAnimator().Play("crouch_roll");
                            }
                    };
                }

            // apply the falling vel
            controller.Move(velocity * Time.deltaTime);
        }
    }

    // executes when player is moving normally
    void CheckWallCollision()
    {
        if (PlayerManager.Instance.IsMoving() && !PlayerManager.Instance.crouched && PlayerManager.Instance.isGrounded)
        {
            RaycastHit hit;
            PlayerManager.Instance.GetPlayerAnimator().ResetTrigger("ExitWallHug");

            if (Physics.Raycast(wallCheckPoint.transform.position, wallCheckPoint.transform.forward, out hit, 1, PlayerManager.Instance.wallLayer))
            {
                // set time of first wall contact
                if (wallPressTime == 0) wallPressTime = Time.time;

                // after pushing into the wall for long enough switch states
                float timeElapsed = Time.time - wallPressTime;
                if (timeElapsed >= 0.35f)
                {
                    Debug.Log("Next State Dispatched [WallMovement]");
                    Signal(new WallMovement(-hit.normal));
                }
            }
        } else
            {
                wallPressTime = 0;
            }
    }

    // runs while the player is not grounded
    // checks for nearby ledges to grab onto
    void CheckLedges()
    {
        if (PlayerManager.Instance.ledgeCheckPoint == null)
        {
            Debug.LogError("PlayerManager Scriptable Object attribute 'ledgeCheckPoint' not defined!");
            return;
        }
        if (PlayerManager.Instance.isGrounded || PlayerManager.Instance.droppingDown) return;

        Vector3 pos = PlayerManager.Instance.ledgeCheckPoint.position;
        Collider[] nearbyLedges = Physics.OverlapSphere(
            pos, 0.1f, PlayerManager.Instance.ledgeLayer, QueryTriggerInteraction.UseGlobal
        );

        if (nearbyLedges.Length > 0 && nearbyLedges[0].GetComponent<Ledge>() != null)
        {
            Ledge ledge = nearbyLedges[0].GetComponent<Ledge>();

            // we found a ledge signal to use ledge movement
            Debug.Log("Next State Dispatched [LedgeMovement]");
            Signal(new LedgeMovement(ledge));
        }
    }

    //===============================================================================

    void UpdateAnimator()
    {
        styleBlend = Mathf.Lerp(styleBlend, IsCrouching() ? 1 : 0, 5 * Time.deltaTime);
        PlayerManager.Instance.GetPlayerAnimator().SetFloat("style", styleBlend);
        PlayerManager.Instance.GetPlayerAnimator().SetFloat("speed", moveBlend);
    }

    public Vector3 CalculateLookDirection(Vector3 inputDir)
    {
        // get camera directions
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0; // flatten to horizontal plane
        camForward.Normalize();

        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        // move relative to the camera
        Vector3 newFwdDir = camForward * inputDir.z + camRight * inputDir.x;
        newFwdDir.Normalize();

        return newFwdDir;
    }

    public void RotatePlayer(Vector3 moveDir)
    {
        if (moveDir != Vector3.zero)
        {
            Debug.DrawRay(controller.transform.position, moveDir * 2f, Color.red);

            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            controller.transform.rotation = Quaternion.RotateTowards(
                controller.transform.rotation,
                toRotation,
                25f
            );
        }
    }
}