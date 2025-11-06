using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Sword : MonoBehaviour
{
    bool canInflict = false;

    void Start()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        Rigidbody rigidbody = GetComponent<Rigidbody>();

        boxCollider.isTrigger = true;
        
        rigidbody.useGravity = false;
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void AllowInfliction()
    {
        canInflict = true;
    }
    public void DisableInfliction()
    {
        canInflict = false;
    }

    // Sword Object passes through a GameObject that inherits IDamagable
    // and triggers that Objects TakeDamage method
    void OnTriggerEnter(Collider collider)
    {
        if (!canInflict) return;

        IDamageable damagable = collider.GetComponent<IDamageable>();
        if (damagable != null)
        {
            // only allow one hit per-interval
            damagable.TakeDamage(PlayerManager.Instance.attackDamage);
            DisableInfliction();
        }
    }
}//EndScript