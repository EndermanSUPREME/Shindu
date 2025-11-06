using UnityEngine;

public class EnemyLimb : MonoBehaviour, IDamageable
{
    IEnemy Host;

    void Start()
    {
        // Find the first parent object that implements IEnemy
        Host = GetComponentInParent<IEnemy>();
    }

    public void TakeDamage(int amount)
    {
        if (Host != null) Host.TakeDamage(amount);
    }
}//EndScript