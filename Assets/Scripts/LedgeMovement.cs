using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;
using ControllerInputs;

public class LedgeMovement : PlayerState
{
    Ledge grabbedLedge;
    bool climbingUp = false;

    public LedgeMovement(Ledge ledge)
    {
        controller = PlayerManager.Instance.GetController();
        grabbedLedge = ledge;
        nextState = null;

        PlayerManager.Instance.huggingWall = PlayerManager.Instance.falling = false;
        PlayerManager.Instance.hanging = true;

        _ = FlushWithLedge();
        _ = RepositionBody();
        GrabLedge();
    }

    async Task FlushWithLedge()
    {
        Vector3 grabPos = grabbedLedge.CalculateGrabPosition();
        Quaternion targetRotation = Quaternion.LookRotation(grabbedLedge.transform.forward, Vector3.up);

        // Rotate until close enough
        while (Vector3.Angle(controller.transform.forward, grabPos) > 0.1f)
        {
            if (controller == null || nextState != null) return;
            
            controller.transform.rotation = Quaternion.Slerp(
                controller.transform.rotation,
                targetRotation,
                PlayerManager.Instance.rotationSpeed * Time.deltaTime);
            await Task.Yield();
        }
    }

    Vector3 CalculateHangPosition()
    {
        // find the base position
        Vector3 pos = grabbedLedge.CalculateGrabPosition();
        // lower the player body by an amount
        pos -= new Vector3(0, PlayerManager.Instance.playerHeight, 0);
        // push the player body outside of a potential mesh by an amount
        pos -= grabbedLedge.transform.forward.normalized * grabbedLedge.offset;

        // return optimal position
        return pos;
    }

    async Task RepositionBody()
    {
        // calculate position we need to snap the
        // transform to when grabbing a ledge
        Vector3 pos = CalculateHangPosition();

        while (Vector3.Distance(controller.transform.position, pos) > 0.5f)
        {
            controller.transform.position = Vector3.Lerp(
                controller.transform.position,
                pos,
                10 * Time.deltaTime
            );

            await Task.Yield();
        }
    }

    void GrabLedge()
    {
        PlayerManager.Instance.EnableRoot();
        SetColliderRadious(PlayerManager.Instance.defaultColliderRadius / 4f);

        if (PlayerManager.Instance.AnimExists())
        {
            PlayerManager.Instance.GetPlayerAnimator().Play("ledge_grab");
        }
    }

    public override void Perform()
    {
        if (controller == null || nextState != null) return;
        Move();
    }
    public override PlayerState ReadSignal() { return nextState; }
    public override void Signal(PlayerState pState) { nextState = pState; }

    protected override void Move()
    {
        if (climbingUp)
        {
            if (!PlayerManager.Instance.hanging)
            {
                Debug.Log("Next State Dispatched [NormalMovement]");
                Signal(new NormalMovement());
            }

            return;
        }

        // based on stick orientation update animator to move along the ledge
        float x = Input.GetAxis("Horizontal");
        if (x < -0.5f)
        {
            PlayerManager.Instance.GetPlayerAnimator().SetFloat("ledge_move_dir", 1);
        } else if (x > 0.5)
            {
                PlayerManager.Instance.GetPlayerAnimator().SetFloat("ledge_move_dir", -1);
            } else
                {
                    PlayerManager.Instance.GetPlayerAnimator().SetFloat("ledge_move_dir", 0);
                }

        if (ControllerInput.PressedA())
        {
            climbingUp = true;
            PlayerManager.Instance.GetPlayerAnimator().Play("ledge_climb");
            return;
        }

        if (ControllerInput.PressedB())
        {
            PlayerManager.Instance.GetPlayerAnimator().Play("ledge_drop");
            PlayerManager.Instance.droppingDown = true;
            
            Debug.Log("Next State Dispatched [NormalMovement]");
            Signal(new NormalMovement());

            return;
        }
    }
}