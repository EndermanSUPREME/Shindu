using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class TestDummy : MonoBehaviour, IEnemy
{
    [SerializeField] int health = 100;
    [SerializeField] float iFrameDuration = 0.5f;
    float iFrameTime = 0;

    bool gamePaused = false;
    [SerializeField] bool iFramesActive = false;

    Animator anim;
    NavMeshAgent agent;

    bool isAlive = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {

    }

    public bool isDead() => !isAlive;
    public void TakeDamage(int amount)
    {
        if (!isAlive) return; // cant damage dead things
        if (iFramesActive) return; // cant damage when iframe state active

        health -= amount;
        if (health <= 0)
        {
            health = 0;
            Die();
        } else
            {
                iFramesActive = true;

                // run and forget about it
                _ = IFrames();
            }
    }
    public Vector3 GetPosition() => transform.position;

    // iFrame State
    async Task IFrames()
    {
        iFrameTime = 0;

        while (isAlive && iFrameTime < iFrameDuration)
        {
            // player cannot bypass iframes by pausing for
            // a period of time
            if (!gamePaused)
            {
                iFrameTime += Time.deltaTime;
            }
            await Task.Yield();
        }

        iFramesActive = false;
    }
    
    void Die()
    {
        Debug.Log("Test Dummy Died!");
        isAlive = false;
        gameObject.SetActive(false);
    }
}//EndScript