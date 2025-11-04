using UnityEngine;
using System.Threading.Tasks;

namespace ShinduPlayer
{
    // universal Singleton template object
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static bool quittingApp = false;

        public static T Instance
        {
            get // getter C# property
            {
                if (quittingApp) return null;
                if (instance != null) return instance;

                // Try to find one in the scene first
                instance = FindFirstObjectByType<T>();

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    instance = singletonObject.AddComponent<T>();
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                // create the new instance
                instance = this as T;
            } else if (instance != this)
                {
                    // remove other instances
                    Destroy(gameObject);
                }
        }

        // when the application quits perform the following
        void OnApplicationQuit()
        {
            quittingApp = true;
        }
    }

    // base / abstract class
    public abstract class PlayerState
    {
        // shared variables between derived classes
        protected CharacterController controller;
        protected PlayerState nextState = null;

        // shared methods between derived classes
        protected void SetColliderRadious(float r)
        {
            controller.radius = r;
        }

        // derived classes must implement this function
        public abstract void Perform();
        // Check if the next state has been dispatched, returns null if not dispatched
        public abstract PlayerState ReadSignal();
        // dispatch the next state
        public abstract void Signal(PlayerState pState);
    }

    public struct MovementTransforms
    {
        public Transform leftFoot, rightFoot, wallCheckPoint;

        public MovementTransforms(
            Transform leftFoot,
            Transform rightFoot,
            Transform wallCheckPoint
        )
        {
            this.leftFoot = leftFoot;
            this.rightFoot = rightFoot;
            this.wallCheckPoint = wallCheckPoint;
        }
    }
}