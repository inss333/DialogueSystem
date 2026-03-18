using UnityEngine;

namespace D1
{
    public class DialogueController : MonoBehaviour
    {
        private const string DialoguePrefabPath = "Perfab/DialogueUI";

        private Dialogue _dialogue;
        private DialogueRepository _repository;
        private GameObject _dialogueUI;
        private DialogueUI _dialogueUIComponent;
        private DialogueVoicePlayer _voicePlayer;
        private bool _isInitialized;

        public DialogueController Init()
        {
            if (_isInitialized)
            {
                return this;
            }

            _repository = new DialogueRepository();
            _dialogue = new Dialogue(_repository);
            _voicePlayer = gameObject.GetComponent<DialogueVoicePlayer>() ?? gameObject.AddComponent<DialogueVoicePlayer>();
            _voicePlayer.Init();

            var prefab = Resources.Load<GameObject>(DialoguePrefabPath);

            _dialogueUI = Instantiate(prefab);
            _dialogueUIComponent = _dialogueUI.GetComponent<DialogueUI>();

            _dialogueUIComponent.OnClickNextEvent += NextDialogue;
            _dialogueUIComponent.OnClickChoiceEvent += ChoiceDialogue;
            _dialogueUIComponent.OnLanguageChangedEvent += SetLanguage;
            _dialogueUIComponent.Init();
            _dialogueUIComponent.SetLanguageDropdown(LocalizationSettings.CurrentLanguage);
            _dialogueUI.SetActive(false);
            _isInitialized = true;
            return this;
        }

        public void RenderNode(int startID)
        {
            var result = _dialogue.StartDialogue(startID);
            ShowDialogueNode(result);
        }

        public void SetLanguage(LocalizationLanguage language)
        {
            LocalizationSettings.CurrentLanguage = language;

            if (!_isInitialized)
            {
                return;
            }

            var result = _dialogue.RefreshCurrentDialogue();
            if (result?.Node == null)
            {
                return;
            }

            _dialogueUIComponent?.SetLanguageDropdown(language);
            ShowDialogueNode(result);
        }

        private void ChoiceDialogue(int dialogueID)
        {
            var result = _dialogue.ChoiceDialogue(dialogueID);
            ShowDialogueNode(result);
        }

        private void NextDialogue()
        {
            var result = _dialogue.NextDialogue();
            if (result == null)
            {
                EndDialogue();
                return;
            }

            ShowDialogueNode(result);
        }

        private void ShowDialogueNode(DialogueResult result)
        {
            if (!_dialogueUI || !_dialogueUIComponent || result?.Node == null)
            {
                return;
            }

            _voicePlayer.Play(result.VoicePath);
            _dialogueUI.SetActive(true);
            _dialogueUIComponent.RenderDialogueNode(result);
        }

        private void EndDialogue()
        {
            _dialogue.EndDialogue();
            _voicePlayer.Stop();
            if (_dialogueUI)
            {
                _dialogueUI.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (!_dialogueUIComponent)
            {
                return;
            }

            _dialogueUIComponent.OnClickNextEvent -= NextDialogue;
            _dialogueUIComponent.OnClickChoiceEvent -= ChoiceDialogue;
            _dialogueUIComponent.OnLanguageChangedEvent -= SetLanguage;
            _dialogueUIComponent.Release();
        }
    }
}
