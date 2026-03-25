using Photon.Pun;
using SSPot.Scenes;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace SSPot.Menu
{
    public class MainMenuManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject fullscreenButton;
        [SerializeField] private LocalizeStringEvent fullscreenButtonText;
        string keyYes = "YesFullSetting";
        string keyNo = "NoFullSetting";


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
            if (isMobile){ 
                fullscreenButton.SetActive(false);
            }

            language = LocalizationSettings.SelectedLocale.Identifier.Code;
        }

        private void Start()
        {
            ChangeFullscreenButtonText(Screen.fullScreen);
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

        public void ToggleFullscreen()
        {
            bool newState = !Screen.fullScreen;
            Screen.fullScreen = newState;
            ChangeFullscreenButtonText(newState);
        }

        void ChangeFullscreenButtonText(bool newState)
        {
            fullscreenButtonText.StringReference.SetReference(fullscreenButtonText.StringReference.TableReference, newState ? keyYes : keyNo);
        }
        #endregion
    }
}