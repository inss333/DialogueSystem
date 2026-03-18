using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace D1
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private Image bg;
        [SerializeField] private Image inputImg;
        [SerializeField] private Image characterImg;

        [SerializeField] private Button choice1;
        [SerializeField] private TextMeshProUGUI text1;

        [SerializeField] private Button choice2;
        [SerializeField] private TextMeshProUGUI text2;

        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI diaText;
        [SerializeField] private float charactersPerSecond = 15f;
        [SerializeField] private TMP_Dropdown speakerDropdown;

        private readonly List<Button> _choiceButtons = new ();
        private readonly List<TextMeshProUGUI> _choiceTexts = new ();
        private readonly List<int> _choiceTargets = new ();
        private readonly Dictionary<string, Sprite> _portraitCache = new ();
        private bool _isInitialized;
        private bool _allowBackgroundNext;
        private bool _isTyping;
        private Coroutine _typingCoroutine;
        private DialogueResult _currentResult;

        public event Action OnClickNextEvent;
        public event Action<int> OnClickChoiceEvent;
        public event Action<LocalizationLanguage> OnLanguageChangedEvent;

        public void Init()
        {
            if (_isInitialized)
            {
                return;
            }

            RegisterChoiceButton(choice1, text1);
            RegisterChoiceButton(choice2, text2);
            ConfigureLanguageDropdown();
            _isInitialized = true;
        }

        public void Release()
        {
            if (!_isInitialized)
            {
                return;
            }

            foreach (var button in _choiceButtons)
            {
                button.onClick.RemoveAllListeners();
            }
            
            speakerDropdown.onValueChanged.RemoveListener(OnLanguageDropdownValueChanged);
            
            _choiceButtons.Clear();
            _choiceTexts.Clear();
            _choiceTargets.Clear();
            StopTyping();
            _isInitialized = false;
        }

        public void RenderDialogueNode(DialogueResult result)
        {
            if (result?.Node == null)
            {
                return;
            }

            _currentResult = result;
            StopTyping();
            _allowBackgroundNext = false;
            RefreshSpeakerName(result.SpeakerName);
            diaText.text = result.Text ?? string.Empty;
            diaText.maxVisibleCharacters = 0;
            RefreshPortrait(result.Character);
            UpdateSelectButtons(Array.Empty<DialogueOptionResult>());
            _typingCoroutine = StartCoroutine(TypeDialogueText());
        }

        public void OnClickNext()
        {
            if (_isTyping)
            {
                CompleteTyping();
                return;
            }

            if (!_allowBackgroundNext)
            {
                return;
            }

            OnClickNextEvent?.Invoke();
        }

        public void SetLanguageDropdown(LocalizationLanguage language)
        {
            speakerDropdown.SetValueWithoutNotify((int)language);
        }

        private void RegisterChoiceButton(Button button, TextMeshProUGUI text)
        {
            var index = _choiceButtons.Count;
            _choiceButtons.Add(button);
            _choiceTexts.Add(text);
            button.onClick.AddListener(() => OnChoice(index));
        }

        private void ConfigureLanguageDropdown()
        {
            if (speakerDropdown.options.Count < 2)
            {
                speakerDropdown.ClearOptions();
                speakerDropdown.AddOptions(new List<string>
                {
                    LocalizationLanguage.ZhCN.DisplayName(),
                    LocalizationLanguage.EnUS.DisplayName()
                });
            }

            speakerDropdown.onValueChanged.AddListener(OnLanguageDropdownValueChanged);
        }

        private void EnsureChoiceButtonPool(int requiredCount)
        {
            while (_choiceButtons.Count < requiredCount)
            {
                var templateButton = choice1;
                var templateText = text1;
                var clone = Instantiate(templateButton.gameObject, templateButton.transform.parent);
                clone.name = $"Choice{_choiceButtons.Count + 1}";
                clone.SetActive(true);

                var button = clone.GetComponent<Button>();
                var text = clone.GetComponentInChildren<TextMeshProUGUI>(true);
                if (button == null || text == null)
                {
                    Destroy(clone);
                    break;
                }

                var rectTransform = clone.GetComponent<RectTransform>();
                var previousRectTransform = _choiceButtons[^1].GetComponent<RectTransform>();
                if (rectTransform && previousRectTransform)
                {
                    rectTransform.anchoredPosition = previousRectTransform.anchoredPosition + new Vector2(0f, 50f);
                }

                text.text = templateText.text;
                RegisterChoiceButton(button, text);
            }
        }

        private void UpdateSelectButtons(IReadOnlyList<DialogueOptionResult> selectDataList)
        {
            var optionCount = selectDataList?.Count ?? 0;
            _choiceTargets.Clear();
            EnsureChoiceButtonPool(optionCount);

            for (var i = 0; i < _choiceButtons.Count; i++)
            {
                var isVisible = i < optionCount;
                _choiceButtons[i].gameObject.SetActive(isVisible);
                if (!isVisible)
                {
                    continue;
                }

                _choiceTexts[i].text = selectDataList[i].Text;
                _choiceTargets.Add(selectDataList[i].Goto);
            }
        }

        private void OnChoice(int index)
        {
            if (_isTyping)
            {
                CompleteTyping();
                return;
            }

            if (index < 0 || index >= _choiceTargets.Count)
            {
                return;
            }

            OnClickChoiceEvent?.Invoke(_choiceTargets[index]);
        }

        private void OnLanguageDropdownValueChanged(int value)
        {
            var language = LocalizationLanguageExtensions.FromDropdownValue(value);
            OnLanguageChangedEvent?.Invoke(language);
        }

        private IEnumerator TypeDialogueText()
        {
            _isTyping = true;
            diaText.ForceMeshUpdate();
            var totalCharacters = diaText.textInfo.characterCount;
            if (totalCharacters <= 0)
            {
                FinishTyping();
                yield break;
            }

            var visibleCharacters = 0;
            var interval = charactersPerSecond > 0f ? 1f / charactersPerSecond : 0f;
            var elapsed = 0f;

            while (visibleCharacters < totalCharacters)
            {
                if (interval <= 0f)
                {
                    visibleCharacters = totalCharacters;
                }
                else
                {
                    elapsed += Time.deltaTime;
                    var targetVisibleCharacters = Mathf.FloorToInt(elapsed / interval);
                    if (targetVisibleCharacters <= visibleCharacters)
                    {
                        yield return null;
                        continue;
                    }

                    visibleCharacters = Mathf.Min(targetVisibleCharacters, totalCharacters);
                }

                diaText.maxVisibleCharacters = visibleCharacters;
                yield return null;
            }

            FinishTyping();
        }

        private void CompleteTyping()
        {
            if (_currentResult?.Node == null)
            {
                return;
            }

            StopTyping();
            diaText.ForceMeshUpdate();
            diaText.maxVisibleCharacters = diaText.textInfo.characterCount;
            FinishTyping();
        }

        private void FinishTyping()
        {
            _isTyping = false;
            _typingCoroutine = null;

            var options = _currentResult?.Options;
            UpdateSelectButtons(options ?? Array.Empty<DialogueOptionResult>());
            _allowBackgroundNext = options == null || options.Count == 0;
        }

        private void StopTyping()
        {
            _isTyping = false;
            if (_typingCoroutine == null)
            {
                return;
            }

            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        private void RefreshPortrait(CharacterData character)
        {
            var portrait = LoadPortrait(character?.ImagePath);
            characterImg.sprite = portrait;
            characterImg.enabled = portrait != null;
        }

        private void RefreshSpeakerName(string speakerName)
        {
            var hasName = !string.IsNullOrWhiteSpace(speakerName);
            speakerNameText.text = hasName ? $"{speakerName}:" : string.Empty;
            speakerNameText.gameObject.SetActive(hasName);
        }
        
        private Sprite LoadPortrait(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            if (_portraitCache.TryGetValue(imagePath, out var sprite))
            {
                return sprite;
            }

            sprite = Resources.Load<Sprite>(imagePath);
            _portraitCache[imagePath] = sprite;
            return sprite;
        }
    }
}
