using System;
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

        [SerializeField] private TextMeshProUGUI diaText;

        private readonly List<Button> _choiceButtons = new List<Button>();
        private readonly List<TextMeshProUGUI> _choiceTexts = new List<TextMeshProUGUI>();
        private readonly List<int> _choiceTargets = new List<int>();
        private bool _isInitialized;
        private bool _allowBackgroundNext;

        public event Action OnClickNextEvent;
        public event Action<int> OnClickChoiceEvent;

        public void Init()
        {
            if (_isInitialized)
            {
                return;
            }

            RegisterChoiceButton(choice1, text1);
            RegisterChoiceButton(choice2, text2);
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

            _choiceButtons.Clear();
            _choiceTexts.Clear();
            _choiceTargets.Clear();
            _isInitialized = false;
        }

        public void RenderDialogueNode(DialogueResult result)
        {
            if (result?.Node == null)
            {
                return;
            }

            _allowBackgroundNext = result.Options.Count == 0;
            diaText.text = result.Node.Text;
            UpdateSelectButtons(result.Options);
        }

        public void OnClickNext()
        {
            if (!_allowBackgroundNext)
            {
                return;
            }

            OnClickNextEvent?.Invoke();
        }

        private void RegisterChoiceButton(Button button, TextMeshProUGUI text)
        {
            var index = _choiceButtons.Count;
            _choiceButtons.Add(button);
            _choiceTexts.Add(text);
            button.onClick.AddListener(() => OnChoice(index));
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

        private void UpdateSelectButtons(IReadOnlyList<SelectData> selectDataList)
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
            if (index < 0 || index >= _choiceTargets.Count)
            {
                return;
            }

            OnClickChoiceEvent?.Invoke(_choiceTargets[index]);
        }
    }
}
