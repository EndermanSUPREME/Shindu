using System;
using System.Collections;
using System.Collections.Generic;
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
    int hitCount = 0;
    bool knockedBack = false;
    bool alerted = false, killedQuietly = false;

    bool gamePaused = false;
    [SerializeField] bool iFramesActive = false;

    [SerializeField] Transform frontStabPosition, backStabPosition, stealthMarker;
    [SerializeField] Transform[] cinematicPoints;

    Animator anim;
    NavMeshAgent agent;

    bool isAlive = true;
    bool showMarker = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        anim.SetBool("isDead", !isAlive);

        if (stealthMarker != null)
        {
            // dynamic scaling of the particle system transform
            stealthMarker.localScale = Vector3.Lerp(
                stealthMarker.localScale,
                (showMarker && isAlive) ? Vector3.one : Vector3.zero,
                5 * Time.deltaTime
            );
        }
    }

    public bool isDead() => !isAlive;
    public void TakeDamage(int amount, bool front)
    {
        if (!isAlive) return; // cant damage dead things
        if (iFramesActive) return; // cant damage when iframe state active

        if (!isAlerted())
        {
            // player initiates stealth kill
            health = 0;
            killedQuietly = true;
            Die();
            return;
        }

        health -= amount;

        Flinch(front);

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

    // enemy animator target
    public void ResetFlinchCount()
    {
        if (health <= 0) return;

        hitCount = 0;
        knockedBack = false;

        Debug.Log("Full Reset from Flinch");
    }

    void Flinch(bool front)
    {
        System.Random rand = new System.Random();
        int choice = rand.Next(2); // returns 0 or 1

        if (++hitCount > 2)
        {
            if (front)
                anim.Play("hurt_fallForward");
            else
                anim.Play("hurt_fallBack");

            knockedBack = true;
            ResetFlinchCount();
        } else
            {
                if (choice == 0)
                {
                    if (!front)
                        anim.Play("hurtOne", 0, 0);
                    else
                        anim.Play("hurtOne_behind", 0, 0);
                } else
                    {
                        if (!front)
                            anim.Play("hurtTwo", 0, 0);
                        else
                            anim.Play("hurtTwo_behind", 0, 0);
                    }   
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


    public Vector3 GetFrontPosition() => frontStabPosition.position;
    public Vector3 GetBackPosition() => backStabPosition.position;

    public void ShowMarker() { showMarker = true; }
    public void HideMarker() { showMarker = false; }

    public bool isAlerted() => alerted;
    public void StealthKill(string animName, bool front)
    {
        // ensure the insta-kill effect is pushed into the enemy state
        TakeDamage(0, front);

        Animator playerAnim = PlayerManager.Instance.GetPlayerAnimator();
        Transform player = PlayerManager.Instance.GetController().transform;
        
        // reposition the player for the animation

        // have to disable the CharacterController to change the position
        PlayerManager.Instance.GetController().enabled = false;
        PlayerManager.Instance.DisableController();
        
        ShowCinemaView();

        if (front)
        {
            player.position = GetFrontPosition();
        } else
            {
                player.position = GetBackPosition();
            }
        // re-enable the controller so root-motion runs properly
        PlayerManager.Instance.GetController().enabled = true;

        // rotate the player to the correct orientation
        Vector3 targetPos = new Vector3(transform.position.x, player.position.y, transform.position.z);
        Vector3 lookDir = targetPos - player.position;
        // snap rotation
        player.rotation = Quaternion.LookRotation(lookDir, Vector3.up);

        // play player animation
        playerAnim.Play(animName);

        // play enemy animation
        anim.Play(animName);
    }

    // move the player camera to a cinematic view point
    public void ShowCinemaView()
    {
        List<Transform> validPoints = new List<Transform>();
        foreach (Transform t in cinematicPoints)
        {
            // check for clipping against a position
            if (Physics.CheckSphere(t.position, 0.5f, PlayerManager.Instance.obstacleLayer)) continue;

            // work with non-clipping points
            validPoints.Add(t);
        }

        // pick random point in valid collection
        Transform randCinemaPoint = validPoints[UnityEngine.Random.Range(0, validPoints.Count-1)];

        // set camera orientation to said point
        Camera.main.transform.position = randCinemaPoint.position;
        Camera.main.transform.rotation = randCinemaPoint.rotation;
    }
    
    void Die()
    {
        Debug.Log("Test Dummy Died!");
        isAlive = false;

        if (!killedQuietly)
        {    
            if (!knockedBack)
            {
                // play standard standing death animation
                anim.Play("deathThree");
            }
        }

        // disable enemy limbs (turn off colliders so the player can pass through the enemy)
        EnemyLimb[] limbs = GetComponentsInChildren<EnemyLimb>(true);
        foreach (EnemyLimb limb in limbs)
        {
            limb.DisableLimb();
        }
    }

    void OnDrawGizmos()
    {
        if (cinematicPoints.Length == 0) return;
        foreach (Transform t in cinematicPoints)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(t.position, 0.1f);
            Debug.DrawRay(t.position, t.forward * 0.5f);
        }
    }
}//EndScript