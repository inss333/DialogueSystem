using System.Collections.Generic;
using UnityEngine;

namespace D1
{
    public static class DialogueValidator
    {
        public static bool Validate(DialogueRepository repository)
        {
            var isValid = true;
            var seenDialogueIDs = new HashSet<int>();
            var seenSelectIDs = new HashSet<int>();

            foreach (var dialogue in Table.Instance.DialogueData)
            {
                if (!seenDialogueIDs.Add(dialogue.ID))
                {
                    Debug.LogError($"Duplicate dialogue ID: {dialogue.ID}");
                    isValid = false;
                }

                if (dialogue.NextID != 0 && !repository.TryGetDialogueData(dialogue.NextID, out _))
                {
                    Debug.LogError($"Dialogue {dialogue.ID} points to missing NextID {dialogue.NextID}");
                    isValid = false;
                }

                foreach (var selectID in dialogue.SelectList)
                {
                    if (!repository.TryGetSelectData(selectID, out var selectData))
                    {
                        Debug.LogError($"Dialogue {dialogue.ID} points to missing SelectID {selectID}");
                        isValid = false;
                        continue;
                    }

                    if (selectData.Goto != 0 && !repository.TryGetDialogueData(selectData.Goto, out _))
                    {
                        Debug.LogError($"Select {selectData.ID} points to missing dialogue {selectData.Goto}");
                        isValid = false;
                    }
                }
            }

            foreach (var select in Table.Instance.SelectData)
            {
                if (!seenSelectIDs.Add(select.ID))
                {
                    Debug.LogError($"Duplicate select ID: {select.ID}");
                    isValid = false;
                }
            }

            return isValid;
        }
    }
}