using Photon.Pun;
using SSPot.Scenes;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace SSPot.Menu
{
    public class MainMenuManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject settingsButton;
        public string language = "pt-BR";

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        
            if(PhotonNetwork.InLobby)
                PhotonNetwork.LeaveLobby();
            if(PhotonNetwork.InRoom)
                PhotonNetwork.LeaveRoom();

            // If platform is mobile, disable settings button
            bool isMobile = Application.isMobilePlatform;
            if (isMobile) settingsButton.SetActive(false);

            language = LocalizationSettings.SelectedLocale.Identifier.Code;
        }

        #region Offline
        public void PlayOffline()
        {
            PhotonNetwork.OfflineMode = true;

            if (PhotonNetwork.IsConnectedAndReady)
            {
                OnConnectedToMaster();
            }
        }
    
        public override void OnJoinedRoom()
        {
            SceneLoader.Instance.LoadTutorial();
        }
        #endregion

        #region Online
        public void PlayOnline()
        {
            PhotonNetwork.OfflineMode = false;
            PhotonNetwork.ConnectUsingSettings();
        }
    
        public override void OnJoinedLobby()
        {
            SceneLoader.Instance.LoadLobby();
        }
        #endregion

        #region Master Connection
        public override void OnConnectedToMaster()
        {
            if(PhotonNetwork.OfflineMode)
            {
                Debug.Log("Starting game offline");
                PhotonNetwork.CreateRoom(null);
            }
            else
            {
                Debug.Log("Starting game online");
                PhotonNetwork.JoinLobby();
            }
        }
        #endregion

        #region Quit
        public void Quit()
        {
            Application.Quit();
        }
        #endregion

        #region Settings
        public void Location() {
            Debug.Log("Changing language...");

            if(language == "pt-BR")
                language = "en";
            else
                language = "pt-BR";

            LocaleIdentifier localCode = new LocaleIdentifier(language);

            for(int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++) {
                Locale aLocale = LocalizationSettings.AvailableLocales.Locales[i];
                LocaleIdentifier anIdentifier = aLocale.Identifier;

                if(anIdentifier == localCode)
                    LocalizationSettings.SelectedLocale = aLocale;
            }
	    }
        #endregion
    }
}