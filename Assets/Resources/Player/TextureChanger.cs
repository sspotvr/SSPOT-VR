using System;
using TMPro;
using UnityEngine;

namespace SSPot
{
    /// <summary>
    /// Helmet emoji. Fills the emoji dropdown and shows the chosen texture; which emoji is shown (and its
    /// sync over the network) is decided by <see cref="PlayerAppearance"/>.
    /// </summary>
    public class TextureChanger : MonoBehaviour
    {
        // 0- white, 1- black, 2- red, 3- orange, 4- yellow
        // 5- +lime, 6- green, 7- +aqua, 8- cyan, 9- +sky
        // 10- +blue, 11- +purple, 12- magenta, 13- +pink
        [SerializeField] private Texture[] textures;
        [SerializeField] private GameObject emojiCanvas;
        private Renderer playerRenderer;
        private TMP_Dropdown colorDropdown;

        /// <summary>Emoji picked by the local player in the dropdown.</summary>
        public event Action<int> Selected;

        private void Awake()
        {
            playerRenderer = GetComponentInChildren<Renderer>();
            colorDropdown = emojiCanvas ? emojiCanvas.GetComponentInChildren<TMP_Dropdown>(true) : null;
            if (!colorDropdown) return;

            colorDropdown.ClearOptions();
            foreach (var texture in textures) colorDropdown.options.Add(new TMP_Dropdown.OptionData(texture.name));
            colorDropdown.RefreshShownValue();
            colorDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        private void OnDestroy()
        {
            if (colorDropdown) colorDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }

        private void OnDropdownChanged(int index) => Selected?.Invoke(index);

        public void SetDropdownValue(int index)
        {
            if (colorDropdown) colorDropdown.SetValueWithoutNotify(Mathf.Clamp(index, 0, textures.Length - 1));
        }

        public void ApplyTexture(int index)
        {
            if (!playerRenderer || textures.Length == 0) return;
            playerRenderer.material.mainTexture = textures[Mathf.Clamp(index, 0, textures.Length - 1)];
        }
    }
}
