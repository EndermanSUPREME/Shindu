using UnityEngine;

public interface IEnemy
{
    void TakeDamage(int amount, bool front);
    void StealthKill(string animName, bool front);
    void ShowMarker();
    void HideMarker();
    void ShowCinemaView();

    Vector3 GetPosition();
    Vector3 GetFrontPosition();
    Vector3 GetBackPosition();

    bool isDead();
    bool isAlerted();
}
public interface IDamageable
{
    void TakeDamage(int amount);
}