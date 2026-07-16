using UnityEngine;

namespace SSPot
{
    public class ToggleCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject canvas;
        private bool isToggled;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                isToggled = !isToggled;
                Cursor.lockState = isToggled ? CursorLockMode.None : CursorLockMode.Locked;
                canvas.SetActive(isToggled);
            }
        }
    }
}
