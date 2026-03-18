using System.Collections.Generic;

namespace D1
{
    public class DialogueResult
    {
        public DialogueData Node { get; }
        public CharacterData Character { get; }
        public IReadOnlyList<SelectData> Options { get; }
        public bool CanNext { get; }
        public bool IsEnd { get; }

        public DialogueResult(DialogueData node, CharacterData character, List<SelectData> options)
        {
            Node = node;
            Character = character;
            Options = options ?? new List<SelectData>();
            CanNext = node != null && node.NextID != 0;
            IsEnd = node == null || (node.NextID == 0 && Options.Count == 0);
        }
    }
}
