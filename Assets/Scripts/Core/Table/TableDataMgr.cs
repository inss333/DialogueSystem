using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace D1
{
    public partial class Table
    {
        public static Table Instance { get; } = new();
    }

    public static class TableDataMgr
    {
        private const string TablePath = "LoadableAssets/Table";

        public static bool Init()
        {
            var listAsset = Resources.Load<TextAsset>($"{TablePath}/tablelist");
            var tableEntries = JsonArrayHelper.FromJson<string>(listAsset.text?.TrimStart('\uFEFF'));

            if (tableEntries == null || tableEntries.Length == 0)
            {
                Debug.LogError("tablelist.json has no table entries.");
                return false;
            }

            Table.Instance.ResetData();

            if ((from fileName in tableEntries
                    where !string.IsNullOrWhiteSpace(fileName)
                    select fileName.EndsWith(".bytes", StringComparison.OrdinalIgnoreCase)
                        ? fileName[..^6]
                        : fileName).Any(tableName => !Load(tableName)))
            {
                return false;
            }

            Table.Instance.Normalize();
            return true;
        }

        private static bool Load(string tableName)
        {
            var asset = Resources.Load<TextAsset>($"{TablePath}/{tableName}");
            using var stream = new MemoryStream(asset.bytes);
            var reader = new tabtoy.TableReader(stream) { ConvertNewLine = true };
            Table.Instance.Deserialize(reader);
            Table.Instance.IndexData(tableName);
            return true;
        }

        [Serializable]
        private class JsonArrayWrapper<T>
        {
            public T[] Items;
        }

        private static class JsonArrayHelper
        {
            public static T[] FromJson<T>(string json)
            {
                return JsonUtility.FromJson<JsonArrayWrapper<T>>("{\"Items\":" + json + "}").Items;
            }
        }
    }
}
