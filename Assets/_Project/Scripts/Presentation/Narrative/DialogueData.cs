using System;
using UnityEngine;

namespace ActiveTimeBattle.Presentation.Narrative
{
    [CreateAssetMenu(
        fileName = "DialogueData",
        menuName = "Active Time Battle/Dialogue Data")]
    public sealed class DialogueData : ScriptableObject
    {
        [SerializeField] private DialogueLineData[] lines =
            Array.Empty<DialogueLineData>();

        public DialogueLineData[] Lines => lines;
    }

    [Serializable]
    public sealed class DialogueLineData
    {
        [SerializeField] private string speaker;
        [SerializeField, TextArea(2, 5)] private string body;
        [SerializeField] private DialogueChoiceData[] choices =
            Array.Empty<DialogueChoiceData>();

        public string Speaker => speaker;

        public string Body => body;

        public DialogueChoiceData[] Choices => choices;
    }

    [Serializable]
    public sealed class DialogueChoiceData
    {
        [SerializeField] private string text;
        [SerializeField] private string setFlag;

        public string Text => text;

        public string SetFlag => setFlag;
    }
}
