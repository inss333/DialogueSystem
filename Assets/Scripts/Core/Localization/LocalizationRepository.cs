namespace D1
{
    public enum LocalizationEntryType
    {
        Text,
        Voice
    }

    public class LocalizationRepository
    {
        public bool Has(LocalizationEntryType type, string key)
        {
            return !string.IsNullOrWhiteSpace(key) && Table.Instance.LocalizationDataByKey.ContainsKey(key);
        }

        public string Resolve(LocalizationEntryType type, string key, string fallback = "")
        {
            return GetLocalizedValue(
                Table.Instance.LocalizationDataByKey,
                key,
                data => LocalizationLanguage.ZhCN.GetValue(data),
                data => LocalizationLanguage.EnUS.GetValue(data),
                type == LocalizationEntryType.Text ? fallback : string.Empty,
                type == LocalizationEntryType.Text);
        }

        public string GetText(string key, string fallback)
        {
            return Resolve(LocalizationEntryType.Text, key, fallback);
        }

        public string GetVoicePath(string key)
        {
            return Resolve(LocalizationEntryType.Voice, key);
        }

        private static string GetLocalizedValue<T>(
            System.Collections.Generic.Dictionary<string, T> table,
            string key,
            System.Func<T, string> getZhCN,
            System.Func<T, string> getEnUS,
            string fallback,
            bool useCrossLanguageFallback)
        {
            if (string.IsNullOrWhiteSpace(key) || !table.TryGetValue(key, out var data))
            {
                return fallback ?? string.Empty;
            }

            var primary = LocalizationSettings.CurrentLanguage == LocalizationLanguage.EnUS
                ? getEnUS(data)
                : getZhCN(data);

            if (!string.IsNullOrWhiteSpace(primary))
            {
                return primary;
            }

            if (!useCrossLanguageFallback)
            {
                return fallback ?? string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(fallback))
            {
                return fallback;
            }

            var secondary = LocalizationSettings.CurrentLanguage == LocalizationLanguage.EnUS
                ? getZhCN(data)
                : getEnUS(data);

            return secondary ?? string.Empty;
        }
    }
}
