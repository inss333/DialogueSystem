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
        private bool _isInitialized;

        public DialogueController Init()
        {
            if (_isInitialized)
            {
                return this;
            }

            _repository = new DialogueRepository();
            if (!DialogueValidator.Validate(_repository))
            {
                Debug.LogError("Dialogue validation failed.");
                return null;
            }

            _dialogue = new Dialogue(_repository);

            var prefab = Resources.Load<GameObject>(DialoguePrefabPath);
            if (!prefab)
            {
                Debug.LogError("DialogueUI.prefab load failed, expected at Assets/Resources/Perfab/DialogueUI.prefab");
                return null;
            }

            _dialogueUI = Instantiate(prefab);
            _dialogueUIComponent = _dialogueUI.GetComponent<DialogueUI>();
            if (!_dialogueUIComponent)
            {
                Debug.LogError("DialogueUI component missing on DialogueUI prefab.");
                Destroy(_dialogueUI);
                return null;
            }

            _dialogueUIComponent.OnClickNextEvent += NextDialogue;
            _dialogueUIComponent.OnClickChoiceEvent += ChoiceDialogue;
            _dialogueUIComponent.Init();
            _dialogueUI.SetActive(false);
            _isInitialized = true;
            return this;
        }

        public void RenderNode(int startID)
        {
            if (!_isInitialized)
            {
                Debug.LogError("DialogueController is not initialized.");
                return;
            }

            var result = _dialogue.StartDialogue(startID);
            if (result == null)
            {
                Debug.LogError($"Dialogue start failed, id={startID}");
                EndDialogue();
                return;
            }

            ShowDialogueNode(result);
        }

        private void ChoiceDialogue(int dialogueID)
        {
            var result = _dialogue.ChoiceDialogue(dialogueID);
            if (result == null)
            {
                Debug.LogError($"Dialogue choice target not found, id={dialogueID}");
                EndDialogue();
                return;
            }

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

            _dialogueUI.SetActive(true);
            _dialogueUIComponent.RenderDialogueNode(result);
        }

        private void EndDialogue()
        {
            _dialogue.EndDialogue();
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
            _dialogueUIComponent.Release();
        }
    }
}
