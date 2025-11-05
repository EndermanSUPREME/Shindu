using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private string stateName;

    public void FinishedRoll() { PlayerManager.Instance.isRolling = false; }
    public void ClimbedLedge() { PlayerManager.Instance.hanging = false; }

    void Start()
    {
        Application.targetFrameRate = 75;

        PlayerManager.Instance.SetState(
            new NormalMovement()
        );
    }

    void Update()
    {
        if (PlayerManager.Instance.HasState())
        {
            if (PlayerManager.Instance.GetState().ReadSignal() == null)
            {
                switch (PlayerManager.Instance.GetState())
                {
                    case NormalMovement:
                        stateName = "NormalMovement";
                    break;
                    case WallMovement:
                        stateName = "WallMovement";
                    break;
                    case LedgeMovement:
                        stateName = "LedgeMovement";
                    break;
                    default:
                        stateName = "Unknown";
                    break;
                }

                // perform current state when there is no signal
                PlayerManager.Instance.GetState().Perform();
            } else
                {
                    Debug.Log("Transitioning to New State");
                    PlayerState nState = PlayerManager.Instance.GetState().ReadSignal();
                    PlayerManager.Instance.SetState(nState);
                }
        }
    }

    void FixedUpdate()
    {
        if (PlayerManager.Instance.HasState())
        {
            if (PlayerManager.Instance.GetState().ReadSignal() == null)
            {
                PlayerManager.Instance.GetState().FixedPerform();
            }
        }
    }

    void UpdatePlayerRotation()
    {
        // type checking using C# keyword is
        if (PlayerManager.Instance.GetState() is NormalMovement s)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 inputDir = new Vector3(x, 0f, z);
            
            if (inputDir != Vector3.zero && !PlayerManager.Instance.focused && !PlayerManager.Instance.isRolling)
            {
                Vector3 moveDir = s.CalculateLookDirection(inputDir);
                s.RotatePlayer(moveDir);
            }
        }
    }

    void OnAnimatorMove()
    {
        if (GetComponent<CharacterController>() is CharacterController controller)
        {
            if (PlayerManager.Instance.AnimExists())
            {
                // manually apply position
                Vector3 deltaPos = PlayerManager.Instance.GetPlayerAnimator().deltaPosition;
                controller.Move(deltaPos);

                // manually apply rotation
                UpdatePlayerRotation();
            }
        }
    }

    void OnDrawGizmos()
    {
        // Gizmos.DrawSphere(PlayerManager.Instance.ledgeCheckPoint.position, 0.1f);
    }
}//EndScript