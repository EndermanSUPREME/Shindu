using UnityEngine;

public interface IEnemy
{
    void TakeDamage(int amount, bool front);
    Vector3 GetPosition();
}
public interface IDamageable
{
    void TakeDamage(int amount);
}