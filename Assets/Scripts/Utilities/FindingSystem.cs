using UnityEngine;

public static class FindingSystem
{
    static float playerFOV = 60f;

    public static IEnemy FindClosestEnemy(float range, bool filter = false)
    {
        float closestDist = -1;
        IEnemy closestEnemy = null;

        Vector3 pos = PlayerManager.Instance.GetController().transform.position;

        // look for IEnemy objects with triggers
        Collider[] nearbyEnemies = Physics.OverlapSphere(
            pos,
            range,
            PlayerManager.Instance.enemyLayer,
            QueryTriggerInteraction.UseGlobal
        );

        for (int i = 0; i < nearbyEnemies.Length; ++i)
        {
            IEnemy enemy = nearbyEnemies[i].transform.GetComponent<IEnemy>();
            if (enemy != null)
            {
                // ignore dead enemies
                if (enemy.isDead()) continue;

                // take player psuedo view field into consideration
                // (ignore if the enemy is behind the player)
                if (filter)
                {
                    Vector3 playerFwd = PlayerManager.Instance.GetController().transform.forward;
                    Vector3 dir = enemy.GetPosition() - PlayerManager.Instance.GetController().transform.position;

                    float a = Vector3.Angle(playerFwd, dir);
                    if (a > playerFOV) continue; // (ignore if the enemy is behind the player)
                }

                float d = Vector3.Distance(pos, nearbyEnemies[i].transform.position);
                if (closestDist == -1 || closestDist < d)
                {
                    closestDist = d;
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }

    // returns reference to IEnemy if the closest IEnemy in front of the player can be stealth killed
    public static IEnemy GetStealthKillTarget()
    {
        IEnemy enemy = FindClosestEnemy(PlayerManager.Instance.stealthKillRange, true);
        if (enemy == null) return null;
        return !enemy.isAlerted() ? enemy : null;
    }
}