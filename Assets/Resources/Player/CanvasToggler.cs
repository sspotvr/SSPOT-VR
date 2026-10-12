using UnityEngine;

namespace SSPot
{
    public class CanvasToggler : MonoBehaviour
    {
        [SerializeField] private GameObject[] canvasList;

        private void Start()
        {
            foreach (GameObject go in canvasList) go.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C)) foreach (GameObject go in canvasList) go.SetActive(!go.activeSelf);
        }
    }
}
