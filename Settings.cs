using System.Text.Json;

namespace WBImport
{
    public sealed class MSSettings
    {
        #region Properties

        public string AccessToken { get; set; }
        public string SalesChannelId { get; set; }

        #endregion Properties
    }

    public sealed class Settings
    {
        public static Settings Default { internal set; get; }

        #region Properties

        public MSSettings MoySklad { get; set; }
        public WBSettings Wildberries { get; set; }

        #endregion Properties

        #region Methods

        public static async Task<Settings> FromFileAsync(string fileName)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(fileName, nameof(fileName));

            if (!File.Exists(fileName))
                throw new InvalidOperationException($"Файл настроек для пути \"{fileName}\" не найден.");

            using var fileStream = File.OpenRead(fileName);

            return await JsonSerializer.DeserializeAsync<Settings>(fileStream);
        }

        #endregion Methods
    }

    public sealed class WBSettings
    {
        #region Properties

        public string AccessToken { get; set; }

        #endregion Properties
    }
}