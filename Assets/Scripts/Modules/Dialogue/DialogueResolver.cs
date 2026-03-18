using System.Collections.Generic;

namespace D1
{
    public class DialogueResolver
    {
        private readonly DialogueRepository _repository;
        private readonly LocalizationRepository _localizationRepository;

        public DialogueResolver(DialogueRepository repository, LocalizationRepository localizationRepository)
        {
            _repository = repository;
            _localizationRepository = localizationRepository;
        }

        public DialogueResult Build(DialogueData dialogueData)
        {
            if (dialogueData == null)
            {
                return new DialogueResult(
                    null,
                    null,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    new List<DialogueOptionResult>());
            }

            CharacterData character = null;
            if (dialogueData.SpeakerID != 0)
            {
                _repository.TryGetCharacterData(dialogueData.SpeakerID, out character);
            }

            return new DialogueResult(
                dialogueData,
                character,
                ResolveSpeakerName(character),
                _localizationRepository.Resolve(LocalizationEntryType.Text, dialogueData.TextKey, dialogueData.TextKey),
                _localizationRepository.Resolve(LocalizationEntryType.Voice, dialogueData.VoiceKey),
                ResolveOptions(dialogueData));
        }

        private List<DialogueOptionResult> ResolveOptions(DialogueData dialogueData)
        {
            var selectDataList = _repository.GetSelectDataList(dialogueData);
            var result = new List<DialogueOptionResult>(selectDataList.Count);

            foreach (var selectData in selectDataList)
            {
                result.Add(new DialogueOptionResult(
                    selectData.ID,
                    selectData.Goto,
                    _localizationRepository.Resolve(LocalizationEntryType.Text, selectData.TextKey, selectData.TextKey)));
            }

            return result;
        }

        private string ResolveSpeakerName(CharacterData character)
        {
            return character == null
                ? string.Empty
                : _localizationRepository.Resolve(LocalizationEntryType.Text, character.NameKey);
        }
    }
}
