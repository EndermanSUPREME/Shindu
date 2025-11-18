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
                IEnemy target = FindingSystem.GetStealthKillTarget();
                if (target != null)
                {
                    // classes that inherit IEnemy also will inherit Monobehaviour allowing for this type casting
                    Transform enemy = (target as MonoBehaviour).transform;

                    Transform player = PlayerManager.Instance.GetController().transform;

                    // based on enemy forward determine if player is in-front or behind enemy
                    Vector3 dir = player.position - enemy.position;
                    float angle = Vector3.Angle(enemy.forward, dir);

                    StealthKill(ref target, !(angle < 95f));

                    return;
                }
                // this bool focuses on grounded attacks
                PlayerManager.Instance.attacking = true;
                StandardAttack();
            }
    }

    // assumes target is always a non-null reference
    static void StealthKill(ref IEnemy target, bool front)
    {
        System.Random rand = new System.Random();
        int choice = rand.Next(2);

        if (front)
        {
            // front
            if (choice == 0) {
                target.StealthKill("frontOne", true);
            } else if (choice == 1) {
                target.StealthKill("frontTwo", true);
            }
        } else
            {
                // back
                if (choice == 0) {
                    target.StealthKill("backOne", false);
                } else if (choice == 1) {
                    target.StealthKill("backTwo", false);
                }
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