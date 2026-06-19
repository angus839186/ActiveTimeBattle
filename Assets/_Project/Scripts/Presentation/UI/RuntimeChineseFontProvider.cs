using TMPro;
using UnityEngine;

namespace ActiveTimeBattle.Presentation.UI
{
    public sealed class RuntimeChineseFontProvider : MonoBehaviour
    {
        private static TMP_FontAsset _runtimeFont;

        private void Awake()
        {
            TMP_FontAsset fontAsset = GetOrCreateFont();
            if (fontAsset == null)
            {
                return;
            }

            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                text.font = fontAsset;
            }
        }

        private static TMP_FontAsset GetOrCreateFont()
        {
            if (_runtimeFont != null)
            {
                return _runtimeFont;
            }

            _runtimeFont = TMP_FontAsset.CreateFontAsset(
                "Microsoft JhengHei",
                "Regular",
                48);
            if (_runtimeFont == null)
            {
                Debug.LogWarning("找不到可用的繁中文字型。");
                return null;
            }

            _runtimeFont.name = "Runtime Chinese Font";
            _runtimeFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            _runtimeFont.hideFlags = HideFlags.DontUnloadUnusedAsset;
            return _runtimeFont;
        }
    }
}
