using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;

public class WallMovement : PlayerState
{
    public WallMovement(Vector3 wallNormal)
    {
        controller = PlayerManager.Instance.GetController();
        nextState = null;

        // run and forget about it
        _ = FlushWithWall(wallNormal);
        HugWall();
    }
    async Task FlushWithWall(Vector3 normal)
    {
        Quaternion targetRotation = Quaternion.LookRotation(normal, Vector3.up);

        // Rotate until close enough
        while (Vector3.Angle(controller.transform.forward, normal) > 0.1f)
        {
            if (controller == null || nextState != null) return;
            
            controller.transform.rotation = Quaternion.Slerp(
                controller.transform.rotation,
                targetRotation,
                PlayerManager.Instance.rotationSpeed * Time.deltaTime);
            await Task.Yield();
        }
    }
    // Transition from normal state into wall-hugging state
    void HugWall()
    {
        if (!PlayerManager.Instance.huggingWall)
        {
            PlayerManager.Instance.huggingWall = true;
            
            SetColliderRadious(PlayerManager.Instance.defaultColliderRadius / 4f);

            if (PlayerManager.Instance.AnimExists())
            {
                PlayerManager.Instance.GetPlayerAnimator().Play("wall_hug");
            }
        }
    }

    public override void Perform()
    {
        if (controller == null || nextState != null) return;
        if (!controller.enabled) return;

        Move();
    }
    public override PlayerState ReadSignal() { return nextState; }
    public override void Signal(PlayerState pState) { nextState = pState; }
    protected override void Move()
    {
        // cancel wall hug
        if (!PlayerManager.Instance.IsMoving())
        {
            PlayerManager.Instance.huggingWall = false;
            PlayerManager.Instance.GetPlayerAnimator().SetTrigger("ExitWallHug");
            SetColliderRadious(PlayerManager.Instance.defaultColliderRadius);

            Debug.Log("Next State Dispatched [NormalMovement]");
            Signal(new NormalMovement());
            return;
        }

        float x = Input.GetAxis("Horizontal");
        if (x < -0.5f)
        {
            PlayerManager.Instance.GetPlayerAnimator().SetFloat("wall_move_dir", 1);
        } else if (x > 0.5)
            {
                PlayerManager.Instance.GetPlayerAnimator().SetFloat("wall_move_dir", -1);
            } else
                {
                    PlayerManager.Instance.GetPlayerAnimator().SetFloat("wall_move_dir", 0);
                }
    }
}