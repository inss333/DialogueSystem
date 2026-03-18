using System.Collections.Generic;

namespace D1
{
    public class DialogueOptionResult
    {
        public int ID { get; }
        public int Goto { get; }
        public string Text { get; }

        public DialogueOptionResult(int id, int gotoId, string text)
        {
            ID = id;
            Goto = gotoId;
            Text = text ?? string.Empty;
        }
    }

    public class DialogueResult
    {
        public DialogueData Node { get; }
        public CharacterData Character { get; }
        public string SpeakerName { get; }
        public string Text { get; }
        public string VoicePath { get; }
        public IReadOnlyList<DialogueOptionResult> Options { get; }
        public bool CanNext { get; }
        public bool IsEnd { get; }

        public DialogueResult(DialogueData node, CharacterData character, string speakerName, string text,
            string voicePath, List<DialogueOptionResult> options)
        {
            Node = node;
            Character = character;
            SpeakerName = speakerName ?? string.Empty;
            Text = text ?? string.Empty;
            VoicePath = voicePath ?? string.Empty;
            Options = options ?? new List<DialogueOptionResult>();
            CanNext = node != null && node.NextID != 0;
            IsEnd = node == null || (node.NextID == 0 && Options.Count == 0);
        }
    }
}
