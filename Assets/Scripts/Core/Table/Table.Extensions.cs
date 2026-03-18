using System.Collections.Generic;

namespace D1
{
    public partial class Table
    {
        public Dictionary<int, List<SelectData>> DialogueSelectDataByDialogueID { get; } = new();

        public void Normalize()
        {
            DialogueSelectDataByDialogueID.Clear();

            foreach (var dialogueData in DialogueData)
            {
                var selectDataList = new List<SelectData>(dialogueData.SelectList.Count);
                foreach (var selectID in dialogueData.SelectList)
                {
                    if (SelectDataByID.TryGetValue(selectID, out var selectData))
                    {
                        selectDataList.Add(selectData);
                    }
                }

                DialogueSelectDataByDialogueID[dialogueData.ID] = selectDataList;
            }
        }
    }
}
