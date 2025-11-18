using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;
using ControllerInputs;

public class PlayerMovement : MonoBehaviour
{
    // Animation Targets
    public void OpenFollowThrough() { AttackSystem.OpenFollowThrough(); }
    public void CloseFollowThrough() { AttackSystem.CloseFollowThrough(); }
    public void ResetSwingCount() { AttackSystem.ResetSwingCount(); }
    public void FinishedAttack() { AttackSystem.FinishedAttack(); }
    public void ClimbedLedge() { PlayerManager.Instance.hanging = false; }
    public void EnableSword() { PlayerManager.Instance.playerSword.AllowInfliction(); }
    public void DisableSword() { PlayerManager.Instance.playerSword.DisableInfliction(); }
    public void EnableController()
    {
        PlayerManager.Instance.EnableController();
        PlayerManager.Instance.ResetCamera();
    }

    void Start()
    {
        Application.targetFrameRate = 75;

        PlayerManager.Instance.SetState(
            new NormalMovement()
        );
    }

    void Update()
    {
        if (PlayerManager.Instance.performingStealthKill) return;

        if (PlayerManager.Instance.HasState())
        {
            if (PlayerManager.Instance.GetState().ReadSignal() == null)
            {
                // player can not change position around when blocking
                if (!PlayerManager.Instance.blocking)
                {
                    // perform current state when there is no signal
                    PlayerManager.Instance.GetState().Perform();
                }

                if (PlayerManager.Instance.GetState() is NormalMovement s)
                {
                    // constantly running while in normal movement state
                    ShowStealthFeedback();

                    if (PlayerManager.Instance.lockedIn)
                    {    
                        IEnemy target = FindingSystem.FindClosestEnemy(PlayerManager.Instance.enemySearchRange);
                        if (target != null)
                        {
                            Vector3 camPos = PlayerManager.Instance.GetPlayerCamera().transform.position;
                            Vector3 lookDir = s.LockOn(target, camPos);
                            if (lookDir != Vector3.zero)
                            {
                                PlayerManager.Instance.GetController().transform.rotation = Quaternion.Slerp(
                                    PlayerManager.Instance.GetController().transform.rotation,
                                    Quaternion.LookRotation(lookDir),
                                    50f * Time.deltaTime
                                );
                            }
                        }
                    }
                }
            } else
                {
                    Debug.Log("Transitioning to New State");
                    PlayerState nState = PlayerManager.Instance.GetState().ReadSignal();
                    PlayerManager.Instance.SetState(nState);
                }
        }
    }

    void FixedUpdate()
    {
        if (PlayerManager.Instance.performingStealthKill) return;

        if (PlayerManager.Instance.HasState())
        {
            if (PlayerManager.Instance.GetState().ReadSignal() == null)
            {
                if (!PlayerManager.Instance.blocking)
                {
                    PlayerManager.Instance.GetState().FixedPerform();
                }
            }
        }
    }

    void UpdatePlayerRotation()
    {
        if (PlayerManager.Instance.performingStealthKill) return;
        
        // type checking using C# keyword is
        if (PlayerManager.Instance.GetState() is NormalMovement s)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 inputDir = new Vector3(x, 0f, z);

            if (inputDir != Vector3.zero)
            {
                if (!PlayerManager.Instance.lockedIn)
                {
                    if (!PlayerManager.Instance.focused && !PlayerManager.Instance.isRolling)
                    {
                        Vector3 moveDir = s.CalculateLookDirection(inputDir);
                        s.RotatePlayer(moveDir);
                    }
                }
            }
        }
    }

    void OnAnimatorMove()
    {
        if (GetComponent<CharacterController>() is CharacterController controller)
        {
            if (PlayerManager.Instance.AnimExists())
            {
                // manually apply position
                Vector3 deltaPos = PlayerManager.Instance.GetPlayerAnimator().deltaPosition;
                controller.Move(deltaPos);

                // manually apply rotation
                UpdatePlayerRotation();
            }
        }
    }

    bool slidingOffEnemy = false;
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // if the player lands on an enemy the player
        // will slide off the enemy model
        bool isEnemy = ((1 << hit.gameObject.layer) & PlayerManager.Instance.enemyLayer) != 0;
        if (!slidingOffEnemy && !PlayerManager.Instance.isGrounded && isEnemy)
        {
            // run and forget about it
            _ = SlideOffEnemy(hit);
        }
    }

    // apply constant force against the player so they are pushed off an enemy
    async Task SlideOffEnemy(ControllerColliderHit hit)
    {
        slidingOffEnemy = true;
        while (!PlayerManager.Instance.isGrounded)
        {
            if (Vector3.Dot(hit.normal, Vector3.up) > 0.5f)
            {
                // calculate the direction to slide the player and apply it
                // to the character controller
                Vector3 slideDir = Vector3.ProjectOnPlane(hit.moveDirection, Vector3.up).normalized;

                if (slideDir == Vector3.zero)
                {
                    slideDir = transform.right;
                }

                PlayerManager.Instance.GetController().Move(
                    slideDir * PlayerManager.Instance.slideForce * Time.deltaTime
                );
            }
            await Task.Yield();
        }
        slidingOffEnemy = false;
    }

    IEnemy lastTarget = null;
    void ShowStealthFeedback()
    {
        IEnemy target = FindingSystem.GetStealthKillTarget();
        if (target != null)
        {
            if (target != lastTarget && lastTarget != null)
            {
                lastTarget.HideMarker();
            }

            lastTarget = target;
            target.ShowMarker();
        } else
            {
                if (lastTarget != null) lastTarget.HideMarker();
            }
    }

    void OnDrawGizmos()
    {
        if (PlayerManager.Instance == null) return;

        Gizmos.DrawSphere(PlayerManager.Instance.ledgeCheckPoint.position, 0.1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            PlayerManager.Instance.GetController().transform.position,
            PlayerManager.Instance.enemySearchRange
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            PlayerManager.Instance.GetController().transform.position,
            PlayerManager.Instance.stealthKillRange
        );
    }
}//EndScript