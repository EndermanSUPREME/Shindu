using UnityEngine;

public static class AttackSystem
{
    static Animator anim;
    static int swingCount = 0;
    static bool followThrough = false;
    static bool continueAttacking = false;

    public enum AttackDirection { FRONT, BACK, NONE };

    public static void SetAnimator(ref Animator anim_) { anim = anim_; }

    // Animator Targets
    public static void OpenFollowThrough()
    {
        continueAttacking = false;
        followThrough = true;
    }
    public static void CloseFollowThrough()
    {
        continueAttacking = false;
        followThrough = false;
    }

    // meant for short grounded one swings such as: crouch_swing
    public static void FinishedAttack() { PlayerManager.Instance.attacking = false; }
    public static void ResetSwingCount()
    {
        // do not reset items
        if (swingCount < 3 && continueAttacking) return;

        swingCount = 0;
        if (anim) anim.SetInteger("swingCount", swingCount);

        followThrough = false;
        PlayerManager.Instance.attacking = false;
    }

    public static void PerformAttack()
    {
        if (anim == null) return;

        bool inTheAir = !PlayerManager.Instance.isGrounded;

        if (inTheAir)
        {
            AirAttack();
        } else
            {
                // this bool focuses on grounded attacks
                PlayerManager.Instance.attacking = true;
                StandardAttack();
            }
    }

    static void AirAttack()
    {
        anim.Play("jumpAttack");
    }

    static void StandardAttack()
    {
        if (PlayerManager.Instance.crouched)
        {
            anim.Play("crouch_attack");
            return;
        }

        if (swingCount == 0)
        {
            // opening swing
            swingCount = 1;
            anim.Play("swingOne");
        } else
            {
                if (followThrough)
                {
                    followThrough = false;
                    continueAttacking = true;
                    ++swingCount;
                }
            }

        anim.SetInteger("swingCount", swingCount);
    }
}