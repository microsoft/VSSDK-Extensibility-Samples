using Microsoft.VisualStudio.Workspace.Settings;
using Microsoft.VisualStudio.Workspace;

namespace OpenFolderExtensibility.SettingsSample
{
    /// <summary>
    /// Settings for word count action.
    /// See WordCountActionProviderFactory.cs for how this is used.
    /// </summary>
    struct WordCountSettings
    {
        /// <summary>
        /// Root settings key.
        /// </summary>
        public const string Key = "WordCountSettings";

        /// <summary>
        /// Enumeration of supported word count types.
        /// </summary>
        public enum WordCountType
        {
            WordCount,
            LineCount
        }

        /// <summary>
        /// Type of word count to perform.
        /// </summary>
        public WordCountType CountType;

        /// <summary>
        /// Return the word count settings from an IWorkspace
        /// </summary>
        /// <param name="workspaceContext">The workspace context</param>
        /// <returns>Word count settings structure</returns>
        public static WordCountSettings GetSettings(IWorkspace workspaceContext)
        {
            var settings = workspaceContext.GetSettingsManager().GetAggregatedSettings(SettingsTypes.Generic);
            return settings.Property<WordCountSettings>(
                WordCountSettings.Key,
                new WordCountSettings() { CountType = WordCountType.WordCount });
        }

        /// <summary>
        /// Store word count settings to an IWorkspace's local settings file
        /// </summary>
        /// <param name="workspaceContext">The workspace context</param>
        /// <param name="settings">Word count settings structure to store</param>
        public async static void StoreSettings(IWorkspace workspaceContext, WordCountSettings settings)
        {
            using (var persistance = await workspaceContext.GetSettingsManager().GetPersistanceAsync(true))
            {
                var perUser = await persistance.GetWriter(SettingsTypes.Generic);
                perUser.SetProperty(Key, settings);
            }
        }
    }
}
