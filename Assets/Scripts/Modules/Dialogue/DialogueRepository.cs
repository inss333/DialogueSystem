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

        public bool TryGetCharacterData(int characterID, out CharacterData data)
        {
            return Table.Instance.CharacterDataByID.TryGetValue(characterID, out data);
        }

        public System.Collections.Generic.List<SelectData> GetSelectDataList(DialogueData dialogueData)
        {
            return dialogueData == null
                ? new System.Collections.Generic.List<SelectData>()
                : Table.Instance.DialogueSelectDataByDialogueID[dialogueData.ID];
        }
    }
}
