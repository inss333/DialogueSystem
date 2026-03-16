using System.Collections.Generic;

namespace D1
{
    public class DialogueRepository
    {
        public bool TryGetDialogueData(int dialogueID, out DialogueData data)
        {
            return Table.Instance.DialogueDataByID.TryGetValue(dialogueID, out data);
        }

        public bool TryGetSelectData(int selectID, out SelectData data)
        {
            return Table.Instance.SelectDataByID.TryGetValue(selectID, out data);
        }

        public List<SelectData> GetSelectDataList(DialogueData dialogueData)
        {
            var result = new List<SelectData>();
            if (dialogueData == null)
            {
                return result;
            }

            foreach (var selectID in dialogueData.SelectList)
            {
                if (TryGetSelectData(selectID, out var selectData))
                {
                    result.Add(selectData);
                }
            }

            return result;
        }
    }
}