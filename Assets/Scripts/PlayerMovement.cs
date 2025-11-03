using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;

public class PlayerMovement : MonoBehaviour
{
    public void FinishedRoll() { PlayerManager.Instance.isRolling = false; }

    void Start()
    {
        Application.targetFrameRate = 75;

        if (GetComponent<CharacterController>() != null)
        {
            PlayerManager.Instance.defaultColliderRadius = GetComponent<CharacterController>().radius;
        }

        if (GetComponent<Animator>() != null)
        {
            PlayerManager.Instance.SetPlayerAnimator(GetComponent<Animator>());
        } else
            {
                Debug.LogWarning("PlayerMovement Missing Value for type 'Animator'!");
            }

        PlayerManager.Instance.SetState(
            new NormalMovement(
                GetComponent<CharacterController>(),
                GetComponent<CapsuleCollider>()
            )
        );
    }

    void Update()
    {
        if (PlayerManager.Instance.HasState())
        {
            if (PlayerManager.Instance.GetState().ReadSignal() == null)
            {
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
}//EndScript