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

    void Start()
    {
        Application.targetFrameRate = 75;

        PlayerManager.Instance.SetState(
            new NormalMovement()
        );
    }

    void Update()
    {
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

                if (PlayerManager.Instance.GetState() is NormalMovement s && PlayerManager.Instance.lockedIn)
                {
                    IEnemy target = FindClosestEnemy();
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

    IEnemy FindClosestEnemy()
    {
        float closestDist = -1;
        IEnemy closestEnemy = null;

        Vector3 pos = PlayerManager.Instance.GetController().transform.position;

        // look for IEnemy objects with triggers
        Collider[] nearbyEnemies = Physics.OverlapSphere(
            pos,
            PlayerManager.Instance.enemySearchRange,
            PlayerManager.Instance.enemyLayer,
            QueryTriggerInteraction.UseGlobal
        );

        Debug.Log($"Count of Nearby Enemies: {nearbyEnemies.Length}");

        for (int i = 0; i < nearbyEnemies.Length; ++i)
        {
            IEnemy enemy = nearbyEnemies[i].transform.GetComponent<IEnemy>();
            if (enemy != null)
            {
                float d = Vector3.Distance(pos, nearbyEnemies[i].transform.position);
                if (closestDist == -1 || closestDist < d)
                {
                    closestDist = d;
                    closestEnemy = enemy;
                    Debug.Log($"Closest Enemy: {nearbyEnemies[i].transform.name}");
                }
            }
        }

        return closestEnemy;
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

    void OnDrawGizmos()
    {
        // Gizmos.DrawSphere(PlayerManager.Instance.ledgeCheckPoint.position, 0.1f);
        Gizmos.DrawWireSphere(
            PlayerManager.Instance.GetController().transform.position,
            PlayerManager.Instance.enemySearchRange
        );
    }
}//EndScript