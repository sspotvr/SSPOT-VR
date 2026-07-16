using SSPot.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace SSpot.UI
{
    public class TextScreen : MonoBehaviour
    {
        [SerializeField] private Text uiText;
        [SerializeField] private GameObject panel;
        [SerializeField] private float showTime = 5f;
        [SerializeField] private bool startActive;
        
        private Coroutine deactivateCoroutine;

        private void Awake() => panel.SetActive(startActive);

        protected void ShowText(string text)
        {
            if (deactivateCoroutine != null)
            {
                StopCoroutine(deactivateCoroutine);
                deactivateCoroutine = null;
            }
            
            uiText.text = text;
            panel.SetActive(true);
            
            deactivateCoroutine = StartCoroutine(CoroutineUtilities.WaitThen(showTime, Close));
        }

        protected void Close() => panel.SetActive(false);

        private void OnDisable()
        {
            if (deactivateCoroutine != null)
            {
                StopCoroutine(deactivateCoroutine);
                deactivateCoroutine = null;
            }
        }
    }
}