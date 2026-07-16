using UnityEngine;

namespace SSPot
{
    public class MobileHandle : MonoBehaviour
    {
        public bool forMobile = false;

        void Awake()
        {
            if (forMobile != Application.isMobilePlatform || (!forMobile && !Application.isMobilePlatform))
                Destroy(gameObject);
        }
    }
}
