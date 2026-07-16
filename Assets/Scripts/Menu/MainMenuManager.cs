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
        [SerializeField] private GameObject vrButton;
        [SerializeField] private LocalizeStringEvent vrButtonText;

        string FkeyYes = "YesFullSetting";
        string FkeyNo = "NoFullSetting";

        string VRKeyYes = "YesVRSetting";
        string VRKeyNo = "NoVRSetting";


        public string language = "pt-BR";

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        
            if(PhotonNetwork.InLobby)
                PhotonNetwork.LeaveLobby();
            if(PhotonNetwork.InRoom)
                PhotonNetwork.LeaveRoom();

            bool isMobile = Application.isMobilePlatform;
            if (fullscreenButton != null) fullscreenButton.SetActive(!isMobile);
            if (vrButton != null) vrButton.SetActive(isMobile);

            language = LocalizationSettings.SelectedLocale.Identifier.Code;
        }

        private void Start()
        {
            ChangeFullscreenButtonText(Screen.fullScreen);
        }

        public void ToggleVRMode()
        {
            // Lê o estado atual (0 = Desligado, 1 = Ligado). O padrão é 0.
            int currentVRState = PlayerPrefs.GetInt("MobileVR_Enabled", 0);
            
            // Inverte o estado
            int newState = currentVRState == 0 ? 1 : 0;
            
            // Salva na memória do aparelho
            PlayerPrefs.SetInt("MobileVR_Enabled", newState);
            PlayerPrefs.Save();
            
            Debug.Log($"Preferência de VR salva como: {(newState == 1 ? "Ligado" : "Desligado")}");
            ChangeVrButtonText(newState == 1);
        }

        void ChangeVrButtonText(bool newState)
        {
            vrButtonText.StringReference.SetReference(vrButtonText.StringReference.TableReference, newState ? VRKeyYes : VRKeyNo);
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
            fullscreenButtonText.StringReference.SetReference(fullscreenButtonText.StringReference.TableReference, newState ? FkeyYes : FkeyNo);
        }
        #endregion
    }
}