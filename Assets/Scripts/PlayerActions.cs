using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;
using ControllerInputs;

public class PlayerActions : MonoBehaviour
{
    void Update()
    {
        if (PlayerManager.Instance.HasState())
        {
            // exit blocking action
            if (PlayerManager.Instance.blocking && !ControllerInput.HoldingB())
            {
                PlayerManager.Instance.GetPlayerAnimator().SetTrigger("blocking");
                PlayerManager.Instance.blocking = false;
            }

            // Perform some Action
            if (ControllerInput.PressedX())
            {
                Attack();
            } else if (ControllerInput.PressedY())
                {
                    UseItem();
                } else if (ControllerInput.PressedB())
                    {
                        Block();
                    }
        }
    }

    void Block()
    {
        // this action is state specific
        if (PlayerManager.Instance.GetState() is not NormalMovement) return;

        if (PlayerManager.Instance.attacking) return;
        if (PlayerManager.Instance.isRolling) return;
        if (PlayerManager.Instance.blocking) return;
        if (!PlayerManager.Instance.isGrounded) return; // in the air
        if (PlayerManager.Instance.crouched) return;

        // reset the blocking mechanisms
        PlayerManager.Instance.GetPlayerAnimator().ResetTrigger("blocking");
        PlayerManager.Instance.blocking = true;

        PlayerManager.Instance.GetPlayerAnimator().Play("block");
    }

    void Attack()
    {
        // this action is state specific
        if (PlayerManager.Instance.GetState() is not NormalMovement) return;

        if (PlayerManager.Instance.blocking) return;
        if (PlayerManager.Instance.isRolling) return;

        Debug.Log("Attack!");
        AttackSystem.PerformAttack();
    }

    void UseItem()
    {
        // this action is state specific
        if (PlayerManager.Instance.GetState() is not NormalMovement) return;

        if (PlayerManager.Instance.attacking) return;
        if (PlayerManager.Instance.blocking) return;
        if (PlayerManager.Instance.isRolling) return;
        if (!PlayerManager.Instance.isGrounded) return; // in the air

        Debug.Log("Using Item!");
    }
}//EndScript