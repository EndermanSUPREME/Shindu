using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;

public class WallMovement : PlayerState
{
    public WallMovement(CharacterController ctrler, CapsuleCollider capCol, Vector3 wallNormal)
    {
        controller = ctrler;
        capCollider = capCol;

        nextState = null;

        // run and forget about it
        _ = FlushWithWall(wallNormal);
        HugWall();
    }

    public override void Perform()
    {
        if (controller == null || nextState != null) return;

        // cancel wall hug
        if (!PlayerManager.Instance.IsMoving())
        {
            PlayerManager.Instance.huggingWall = false;
            PlayerManager.Instance.GetPlayerAnimator().SetTrigger("ExitWallHug");
            SetColliderRadious(PlayerManager.Instance.defaultColliderRadius);

            Debug.Log("Next State Dispatched [NormalMovement]");
            Signal(new NormalMovement(controller, capCollider));
            return;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(x, 0f, z).normalized;

        if (inputDir != Vector3.zero && PlayerManager.Instance.AnimExists())
        {
            // compare the player fwd to the input dir
            // to calculate the move direction along the wall

            float angleToRight = Vector3.Angle(controller.transform.right, inputDir);
            float angleToLeft = Vector3.Angle(-controller.transform.right, inputDir);
            if (angleToRight < 50)
            {
                PlayerManager.Instance.GetPlayerAnimator().SetFloat("wall_move_dir", -1);
            } else if (angleToLeft < 50)
                {
                    PlayerManager.Instance.GetPlayerAnimator().SetFloat("wall_move_dir", 1);
                } else
                    {
                        PlayerManager.Instance.GetPlayerAnimator().SetFloat("wall_move_dir", 0);
                    }
        } else
            {
                PlayerManager.Instance.GetPlayerAnimator().SetFloat("wall_move_dir", 0);
            }
    }
    public override PlayerState ReadSignal() { return nextState; }
    public override void Signal(PlayerState pState) { nextState = pState; }

    async Task FlushWithWall(Vector3 normal)
    {
        Quaternion targetRotation = Quaternion.LookRotation(normal, Vector3.up);

        // Rotate until close enough
        while (Vector3.Angle(controller.transform.forward, normal) > 0.1f)
        {
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
}