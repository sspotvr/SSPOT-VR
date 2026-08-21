using UnityEngine;

namespace SSPot
{
    public class ColorCanvasToggler : MonoBehaviour
    {
        [SerializeField] private GameObject canvas;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C)) canvas.SetActive(!canvas.activeSelf);
        }
    }
}
