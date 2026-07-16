using Photon.Pun;

namespace SSPot.Utilities
{
    public abstract class NetworkedSingleton<T> : MonoBehaviourPun where T: NetworkedSingleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (!instance)
                    instance = FindAnyObjectByType<T>();
                
                return instance;
            }
        }
        
        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = (T)this;
        }
    }
}