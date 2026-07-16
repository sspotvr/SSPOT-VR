using UnityEngine;

namespace SSPot.Utilities
{
    public abstract class Singleton<T> : MonoBehaviour where T: Singleton<T>
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