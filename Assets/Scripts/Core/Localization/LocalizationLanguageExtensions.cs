namespace D1
{
    public static class LocalizationLanguageExtensions
    {
        public static string DisplayName(this LocalizationLanguage language)
        {
            return language switch
            {
                LocalizationLanguage.EnUS => "English",
                _ => "简体中文"
            };
        }

        public static string GetValue(this LocalizationLanguage language, LocalizationData data)
        {
            return language switch
            {
                LocalizationLanguage.EnUS => data.EnUS,
                _ => data.ZhCN
            };
        }

        public static LocalizationLanguage FromDropdownValue(int value)
        {
            return value == (int)LocalizationLanguage.EnUS
                ? LocalizationLanguage.EnUS
                : LocalizationLanguage.ZhCN;
        }
    }
}
