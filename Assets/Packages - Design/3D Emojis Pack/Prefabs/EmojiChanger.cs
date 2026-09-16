using TMPro;
using UnityEngine;


namespace SSPot
{
    public class EmojiGridChanger : MonoBehaviour
    {
        [SerializeField] private Texture[] emojis;

        private GameObject canvas;
        private Renderer playerRenderer;
        private TMP_Dropdown colorDropdown;


        private void Start()
        {
            canvas = transform.GetChild(0).gameObject;
            playerRenderer = GetComponentInChildren<Renderer>();
            colorDropdown = canvas.GetComponentInChildren<TMP_Dropdown>();

            foreach (var emoji in emojis) colorDropdown.options.Add(new TMP_Dropdown.OptionData(emoji.name));

            colorDropdown.onValueChanged.AddListener(OnTextureSelected);
            OnTextureSelected(3);
        }


        private void OnTextureSelected(int index)
        {
            playerRenderer.material.mainTexture = emojis[index];
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C)) canvas.SetActive(!canvas.activeSelf);
        }


        private void OnDestroy()
        {
            colorDropdown.onValueChanged.RemoveListener(OnTextureSelected);
        }
    }
}
