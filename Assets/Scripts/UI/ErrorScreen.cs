using SSpot.Level;

namespace SSpot.UI
{
    public class ErrorScreen : TextScreen
    {
        private void OnEnable()
        {
            LevelManager.Instance.OnError.AddListener(HandleError);
            LevelManager.Instance.OnSuccess.AddListener(HandleSuccess);
        }

        private void OnDisable()
        {
            if (!LevelManager.Instance) return;
            
            LevelManager.Instance.OnError.RemoveListener(HandleError);
            LevelManager.Instance.OnSuccess.RemoveListener(HandleSuccess);
        }

        
        private void HandleError()
        {
            string text = LevelManager.Instance.CurrentResult.Message;
            ShowText(text);
        }
        
        private void HandleSuccess()
        {
            // Success usually has no message (the level-complete panel and narration announce it) - showing
            // it would pop up an empty panel, so just clear any error still on screen instead.
            string text = LevelManager.Instance.CurrentResult.Message;
            if (string.IsNullOrEmpty(text))
                Close();
            else
                ShowText(text);
        }
    }
}