using TMPro;
using UnityEngine;

namespace SSPot
{
    public class TextureChanger : MonoBehaviour
    {
        // private static readonly int mainTex = Shader.PropertyToID("_MainTex");

        // 8 textures:
        // 0- white, 1- black, 2- red, 3- orange, 4- yellow
        // 5- +lime, 6- green, 7- +aqua, 8- cyan, 9- +sky
        // 10- +blue, 11- +purple, 12- magenta, 13- +pink
        [SerializeField] private Texture[] textures;

        private GameObject canvas;
        private Renderer playerRenderer;
        private TMP_Dropdown colorDropdown;


        private void Start()
        {
            canvas = transform.GetChild(0).gameObject;
            playerRenderer = GetComponentInChildren<Renderer>();
            colorDropdown = canvas.GetComponentInChildren<TMP_Dropdown>();

            foreach (var texture in textures) colorDropdown.options.Add(new TMP_Dropdown.OptionData(texture.name));

            colorDropdown.onValueChanged.AddListener(OnTextureSelected);
            OnTextureSelected(3);
        }


        private void OnTextureSelected(int index)
        {
            playerRenderer.material.mainTexture = textures[index];
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
