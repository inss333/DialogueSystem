namespace D1
{
    public class Dialogue
    {
        private readonly DialogueRepository _repository;
        private readonly DialogueResolver _resolver;
        private int _currentDialogueID;
        private bool _hasCurrentDialogue;

        public Dialogue(DialogueRepository repository)
        {
            _repository = repository;
            _resolver = new DialogueResolver(repository, new LocalizationRepository());
        }

        public DialogueResult StartDialogue(int dialogueID)
        {
            if (!_repository.TryGetDialogueData(dialogueID, out var data))
            {
                return null;
            }

            _currentDialogueID = dialogueID;
            _hasCurrentDialogue = true;
            return BuildResult(data);
        }

        public DialogueResult ChoiceDialogue(int dialogueID)
        {
            if (!_repository.TryGetDialogueData(dialogueID, out var data))
            {
                return null;
            }

            _currentDialogueID = dialogueID;
            _hasCurrentDialogue = true;
            return BuildResult(data);
        }

        public DialogueResult NextDialogue()
        {
            if (!_hasCurrentDialogue)
            {
                return null;
            }

            if (!_repository.TryGetDialogueData(_currentDialogueID, out var currentData))
            {
                return null;
            }

            var nextID = currentData.NextID;
            if (nextID == 0)
            {
                return null;
            }

            if (!_repository.TryGetDialogueData(nextID, out var nextData))
            {
                return null;
            }

            _currentDialogueID = nextID;
            return BuildResult(nextData);
        }

        public void EndDialogue()
        {
            _hasCurrentDialogue = false;
            _currentDialogueID = 0;
        }

        public DialogueResult RefreshCurrentDialogue()
        {
            if (!_hasCurrentDialogue)
            {
                return null;
            }

            if (!_repository.TryGetDialogueData(_currentDialogueID, out var dialogueData))
            {
                return null;
            }

            return BuildResult(dialogueData);
        }

        private DialogueResult BuildResult(DialogueData dialogueData)
        {
            return _resolver.Build(dialogueData);
        }
    }
}
