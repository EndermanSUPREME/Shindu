using UnityEngine;

public class EnemyLimb : MonoBehaviour, IDamageable
{
    IEnemy Host;
    CharacterController playerController;

    void Start()
    {
        // Find the first parent object that implements IEnemy
        Host = GetComponentInParent<IEnemy>();
        playerController = FindFirstObjectByType<CharacterController>();

        if (playerController == null)
        {
            Debug.LogWarning("Player Controller cannot be Located!");
        }
    }

    AttackSystem.AttackDirection CalcAttackDir()
    {
        if (Host == null || playerController == null)
        {
            return AttackSystem.AttackDirection.NONE;
        } else
            {
                // since classes inheriting IEnemy also inherit MonoBehaviour
                // we can safely cast it so we can pull the transform component
                Transform hostTransform = (Host as MonoBehaviour).transform;
                Transform playerTransform = playerController.transform;

                Vector3 dir = playerTransform.position - hostTransform.position;
                float angle = Vector3.Angle(hostTransform.forward, dir);

                if (angle < 95f)
                {
                    return AttackSystem.AttackDirection.FRONT;
                } else
                    {
                        return AttackSystem.AttackDirection.BACK;
                    }
            }
    }

    public void TakeDamage(int amount)
    {
        if (Host != null) {
            Host.TakeDamage(amount, (CalcAttackDir() == AttackSystem.AttackDirection.FRONT) ? true : false);
        }
    }
}//EndScript