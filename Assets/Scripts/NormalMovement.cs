using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;

// derived / concrete class
public class NormalMovement : PlayerState
{
    public NormalMovement(CharacterController ctrler)
    {
        leftFoot = PlayerManager.Instance.leftFoot;
        rightFoot = PlayerManager.Instance.rightFoot;
        wallCheckPoint = PlayerManager.Instance.wallCheckPoint;

        controller = ctrler;
        nextState = null;

        defaultColliderRadius = controller.radius;
    }

    public override void Perform()
    {
        if (controller == null || nextState != null) return;

        CheckWallCollision();
        CheckLedges();
        Move();

        if (PlayerManager.Instance.AnimExists())
        {
            UpdateAnimator();
        }
    }
    public override PlayerState ReadSignal() { return nextState; }
    public override void Signal(PlayerState pState) { nextState = pState; }

    const float gravity = -9.81f;
    Vector3 velocity;
    int jumpCount = 0;
    float moveBlend = 0;
    float styleBlend = 0;
    float wallPressTime = 0;
    float defaultColliderRadius = 0;

    Transform leftFoot, rightFoot, wallCheckPoint;

    bool IsCrouching()
    {
        return Input.GetButton("RightBumper") && PlayerManager.Instance.isGrounded;
    }

    // returns true if either foot sphere is touching a ground layered object
    bool IsGrounded()
    {
        bool leftFootGrounded = Physics.CheckSphere(leftFoot.transform.position, PlayerManager.Instance.groundCheckRadius, PlayerManager.Instance.groundMask);
        bool rightFootGrounded = Physics.CheckSphere(rightFoot.transform.position, PlayerManager.Instance.groundCheckRadius, PlayerManager.Instance.groundMask);
        return PlayerManager.Instance.isRolling || leftFootGrounded || rightFootGrounded;
    }

    void UpdateAnimator()
    {
        styleBlend = Mathf.Lerp(styleBlend, IsCrouching() ? 1 : 0, 5 * Time.deltaTime);
        PlayerManager.Instance.GetPlayerAnimator().SetFloat("style", styleBlend);
        PlayerManager.Instance.GetPlayerAnimator().SetFloat("speed", moveBlend);
    }

    // specific methods for this derived class
    void Move()
    {
        if (controller.enabled)
        {
            // these 3 variables are needed here
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 inputDir = new Vector3(x, 0f, z).normalized;

            // mark when player is in a crouched state
            PlayerManager.Instance.crouched = IsCrouching();

            if (inputDir != Vector3.zero && !PlayerManager.Instance.focused && !PlayerManager.Instance.isRolling)
            {
                // get camera directions
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0; // flatten to horizontal plane
                camForward.Normalize();

                Vector3 camRight = Camera.main.transform.right;
                camRight.y = 0;
                camRight.Normalize();

                // move relative to the camera
                Vector3 moveDir = camForward * z + camRight * x;

                // track magnitude to smoothly move the blend-tree parameter
                moveBlend = moveDir.magnitude;

                moveDir.Normalize();

                // smooth rotation via slerp
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                controller.transform.rotation = Quaternion.Slerp(
                    controller.transform.rotation,
                    targetRotation,
                    PlayerManager.Instance.rotationSpeed * Time.deltaTime
                );
            
                // apply controller input
                if (jumpCount > 0)
                {
                    // not using root-motion when falling/jumping
                    controller.Move(moveDir * PlayerManager.Instance.moveSpeed * Time.deltaTime);
                }
            } else
                {
                    moveBlend = 0;
                }

            PlayerManager.Instance.isGrounded = IsGrounded();

            if (!PlayerManager.Instance.isGrounded)
            {
                // NOT GROUNDED
                if (Input.GetButtonDown("Jump") && jumpCount == 1)
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

                        if (!PlayerManager.Instance.falling)
                        {
                            PlayerManager.Instance.falling = true;
                        }
                    }
            } else
                {
                    // IS GROUNDED
                    if (PlayerManager.Instance.falling)
                    {
                        if (PlayerManager.Instance.AnimExists())
                        {
                            float landingVel = Mathf.Abs(velocity.y);
                            if (landingVel < 18)
                            {
                                PlayerManager.Instance.GetPlayerAnimator().Play("soft_landing");
                            } else
                                {
                                    PlayerManager.Instance.GetPlayerAnimator().Play("hard_landing");
                                }
                            PlayerManager.Instance.EnableRoot();
                        }
                        PlayerManager.Instance.falling = false;
                    }

                    // base downwards vel while grounded
                    if (velocity.y < 0)
                    {
                        jumpCount = 0;
                        velocity.y = -2;
                    }

                    if (Input.GetButtonDown("Jump") && jumpCount == 0)
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
                    Signal(new WallMovement(controller, -hit.normal));
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
        if (PlayerManager.Instance.isGrounded) return;

        Vector3 pos = PlayerManager.Instance.ledgeCheckPoint.position;
        Collider[] nearbyLedges = Physics.OverlapSphere(
            pos, 0.1f, PlayerManager.Instance.ledgeLayer, QueryTriggerInteraction.UseGlobal
        );

        if (nearbyLedges.Length > 0 && nearbyLedges[0].GetComponent<Ledge>() != null)
        {
            Ledge ledge = nearbyLedges[0].GetComponent<Ledge>();

            // we found a ledge signal to use ledge movement
            Debug.Log("Next State Dispatched [WallMovement]");
            Signal(new LedgeMovement(controller, ledge));
        }
    }
}