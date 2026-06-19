using TMPro;
using UnityEngine;

namespace ActiveTimeBattle.Presentation.Hub
{
    public sealed class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private TMP_Text targetNameText;
        [SerializeField] private TMP_Text promptText;

        public void Hide()
        {
            targetNameText.text = string.Empty;
            promptText.text = string.Empty;
        }

        public void Show(string displayName, string prompt)
        {
            targetNameText.text = displayName;
            promptText.text = $"[左鍵 / E] {prompt}";
        }
    }
}
