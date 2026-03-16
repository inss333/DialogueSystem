using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace D1
{
    public partial class Table 
    {
        public static Table Instance { get; } = new ();
    }

    [Serializable]
    internal class TableListData
    {
        public string[] Tables;
        public string[] tables;
    }

    public static class TableDataMgr
    {
        private const string TablePath = "LoadableAssets/Table";

        public static bool Init()
        {
            var listAsset = Resources.Load<TextAsset>($"{TablePath}/tablelist");
            var listJson = listAsset.text?.TrimStart('\uFEFF');
            var tableList = JsonUtility.FromJson<TableListData>(listJson);
            var tableEntries = tableList?.Tables ?? tableList?.tables;

            if (tableEntries == null || tableEntries.Length == 0)
            {
                Debug.LogError("tablelist.json has no table entries.");
                return false;
            }

            Table.Instance.ResetData();

            if ((from fileName in tableEntries 
                    where !string.IsNullOrWhiteSpace(fileName) 
                    select fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                    ? fileName[..^5]
                    : fileName).Any(tableName => !Load(tableName)))
            {
                return false;
            }

            TryNormalize();
            return true;
        }

        private static bool Load(string tableName)
        {
            var asset = Resources.Load<TextAsset>($"{TablePath}/{tableName}");
            var tableJson = asset.text?.TrimStart('\uFEFF');
            JsonUtility.FromJsonOverwrite(tableJson, Table.Instance);
            BuildIndexForTable(tableName);
            return true;
        }

        //构建Dic，后续查找的时间复杂度为 O(1)。
        private static void BuildIndexForTable(string tableName)
        {
            var tableType = typeof(Table);
            var listField = tableType.GetField(tableName, BindingFlags.Instance | BindingFlags.Public);
            var dictField = tableType.GetField($"{tableName}ByID", BindingFlags.Instance | BindingFlags.Public);
            if (listField == null || dictField == null)
            {
                return;
            }

            if (dictField.GetValue(Table.Instance) is not IDictionary dict)
            {
                return;
            }

            dict.Clear();

            if (listField.GetValue(Table.Instance) is not IEnumerable list)
            {
                return;
            }

            foreach (var row in list)
            {
                if (row == null)
                {
                    continue;
                }

                var idField = row.GetType().GetField("ID", BindingFlags.Instance | BindingFlags.Public);
                if (idField == null)
                {
                    continue;
                }

                var idValue = idField.GetValue(row);
                if (idValue == null)
                {
                    continue;
                }

                dict[idValue] = row;
            }
        }

        private static void TryNormalize()
        {
            var normalize = typeof(Table).GetMethod("Normalize", BindingFlags.Instance | BindingFlags.Public);
            normalize?.Invoke(Table.Instance, null);
        }
    }
}
