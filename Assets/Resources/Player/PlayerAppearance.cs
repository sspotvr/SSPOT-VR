using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace SSPot
{
    /// <summary>
    /// Suit color and helmet emoji of an avatar. The local player picks them in the customization menus
    /// (key C); the choice is saved in PlayerPrefs, so it carries over to the next levels and sessions, and
    /// published as Photon player properties, so the other player sees it (late joiners included).
    /// </summary>
    public class PlayerAppearance : MonoBehaviourPunCallbacks
    {
        private const string ColorProperty = "suitColor";
        private const string EmojiProperty = "emoji";
        private const string ColorPref = "SSPOT_SuitColor";
        private const string EmojiPref = "SSPOT_Emoji";
        private const float PublishInterval = .2f;

        [Header("Suit")]
        [SerializeField] private Renderer suitRenderer;
        [SerializeField] private int suitMaterialIndex;
        [SerializeField] private FlexibleColorPicker colorPicker;
        [SerializeField] private Color defaultSuitColor = Color.white;

        [Header("Emoji")]
        [SerializeField] private TextureChanger emoji;
        [SerializeField] private int defaultEmoji = 10;

        private Material _suitMaterial;
        private Color _color;
        private int _emojiIndex;
        private bool _dirty;
        private float _nextPublish;

        private void Start()
        {
            if (suitRenderer)
            {
                // Instanced material: changing it must not touch the shared asset (nor the other avatar).
                var materials = suitRenderer.materials;
                if (suitMaterialIndex >= 0 && suitMaterialIndex < materials.Length)
                    _suitMaterial = materials[suitMaterialIndex];
            }

            if (photonView.IsMine)
            {
                _color = LoadColor();
                _emojiIndex = PlayerPrefs.GetInt(EmojiPref, defaultEmoji);
                Apply();

                if (colorPicker)
                {
                    colorPicker.color = _color;
                    colorPicker.onColorChange.AddListener(OnLocalColorChanged);
                }
                if (emoji)
                {
                    emoji.SetDropdownValue(_emojiIndex);
                    emoji.Selected += OnLocalEmojiSelected;
                }

                Publish();
            }
            else
            {
                _color = defaultSuitColor;
                _emojiIndex = defaultEmoji;
                ReadProperties(photonView.Owner?.CustomProperties);
                Apply();
            }
        }

        private void OnDestroy()
        {
            if (colorPicker) colorPicker.onColorChange.RemoveListener(OnLocalColorChanged);
            if (emoji) emoji.Selected -= OnLocalEmojiSelected;
        }

        private void Update()
        {
            if (_dirty && Time.unscaledTime >= _nextPublish) Publish();
        }

        private void OnLocalColorChanged(Color color)
        {
            _color = color;
            Apply();
            PlayerPrefs.SetString(ColorPref, "#" + ColorUtility.ToHtmlStringRGBA(color));
            _dirty = true; // dragging the picker fires every frame: publish at most every PublishInterval
        }

        private void OnLocalEmojiSelected(int index)
        {
            _emojiIndex = index;
            Apply();
            PlayerPrefs.SetInt(EmojiPref, index);
            Publish();
        }

        private void Publish()
        {
            _dirty = false;
            _nextPublish = Time.unscaledTime + PublishInterval;
            if (!PhotonNetwork.InRoom) return;

            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
            {
                [ColorProperty] = "#" + ColorUtility.ToHtmlStringRGBA(_color),
                [EmojiProperty] = _emojiIndex,
            });
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (photonView.IsMine || targetPlayer != photonView.Owner) return;
            ReadProperties(changedProps);
            Apply();
        }

        private void ReadProperties(Hashtable properties)
        {
            if (properties == null) return;
            if (properties[ColorProperty] is string html && ColorUtility.TryParseHtmlString(html, out var color))
                _color = color;
            if (properties[EmojiProperty] is int index)
                _emojiIndex = index;
        }

        private Color LoadColor()
        {
            string html = PlayerPrefs.GetString(ColorPref, string.Empty);
            return ColorUtility.TryParseHtmlString(html, out var color) ? color : defaultSuitColor;
        }

        private void Apply()
        {
            if (_suitMaterial) _suitMaterial.color = _color;
            if (emoji) emoji.ApplyTexture(_emojiIndex);
        }
    }
}
