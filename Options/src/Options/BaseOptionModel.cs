using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace OptionsSample.Options
{
    /// <summary>
    /// A base class for specifying options
    /// </summary>
    internal abstract class BaseOptionModel<T> where T : BaseOptionModel<T>, new()
    {
        private static AsyncLazy<T> _liveModel = new AsyncLazy<T>(CreateAsync, ThreadHelper.JoinableTaskFactory);
        private static AsyncLazy<ShellSettingsManager> _settingsManager = new AsyncLazy<ShellSettingsManager>(GetSettingsManagerAsync, ThreadHelper.JoinableTaskFactory);

        protected BaseOptionModel()
        { }

        /// <summary>
        /// A singleton instance of the options. MUST be called from UI thread only.
        /// </summary>
        /// <remarks>
        /// Call <see cref="GetLiveInstanceAsync()" /> instead if on a background thread or in an async context on the main thread.
        /// </remarks>
        public static T Instance
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

#pragma warning disable VSTHRD104 // Offer async methods
                return ThreadHelper.JoinableTaskFactory.Run(GetLiveInstanceAsync);
#pragma warning restore VSTHRD104 // Offer async methods
            }
        }

        /// <summary>
        /// Get the singleton instance of the options. Thread safe.
        /// </summary>
        public static Task<T> GetLiveInstanceAsync() => _liveModel.GetValueAsync();

        /// <summary>
        /// Creates a new instance of the options class and loads the values from the store. For internal use only
        /// </summary>
        /// <returns></returns>
        public static async Task<T> CreateAsync()
        {
            var instance = new T();
            await instance.LoadAsync();
            return instance;
        }

        /// <summary>
        /// The name of the options collection as stored in the registry.
        /// </summary>
        protected virtual string CollectionName { get; } = typeof(T).FullName;

        /// <summary>
        /// Hydrates the properties from the registry.
        /// </summary>
        public virtual void Load()
        {
            ThreadHelper.JoinableTaskFactory.Run(LoadAsync);
        }

        /// <summary>
        /// Hydrates the properties from the registry asyncronously.
        /// </summary>
        public virtual async Task LoadAsync()
        {
            ShellSettingsManager manager = await _settingsManager.GetValueAsync();
            SettingsStore settingsStore = manager.GetReadOnlySettingsStore(SettingsScope.UserSettings);

            if (!settingsStore.CollectionExists(CollectionName))
            {
                return;
            }

            foreach (PropertyInfo property in GetOptionProperties())
            {
                try
                {
                    string serializedProp = settingsStore.GetString(CollectionName, property.Name);
                    object value = DeserializeValue(serializedProp, property.PropertyType);
                    property.SetValue(this, value);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                }
            }
        }

        /// <summary>
        /// Saves the properties to the registry.
        /// </summary>
        public virtual void Save()
        {
            ThreadHelper.JoinableTaskFactory.Run(SaveAsync);
        }

        /// <summary>
        /// Saves the properties to the registry asyncronously.
        /// </summary>
        public virtual async Task SaveAsync()
        {
            ShellSettingsManager manager = await _settingsManager.GetValueAsync();
            WritableSettingsStore settingsStore = manager.GetWritableSettingsStore(SettingsScope.UserSettings);

            if (!settingsStore.CollectionExists(CollectionName))
            {
                settingsStore.CreateCollection(CollectionName);
            }

            foreach (PropertyInfo property in GetOptionProperties())
            {
                string output = SerializeValue(property.GetValue(this));
                settingsStore.SetString(CollectionName, property.Name, output);
            }

            T liveModel = await GetLiveInstanceAsync();

            if (this != liveModel)
            {
                await liveModel.LoadAsync();
            }
        }

        /// <summary>
        /// Serializes an object value to a string using the binary serializer.
        /// </summary>
        protected virtual string SerializeValue(object value)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, value);
                stream.Flush();
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        /// <summary>
        /// Deserializes a string to an object using the binary serializer.
        /// </summary>
        protected virtual object DeserializeValue(string value, Type type)
        {
            byte[] b = Convert.FromBase64String(value);

            using (var stream = new MemoryStream(b))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }
        }

        private static async Task<ShellSettingsManager> GetSettingsManagerAsync()
        {
#pragma warning disable VSTHRD010 
            // False-positive in Threading Analyzers. Bug tracked here https://github.com/Microsoft/vs-threading/issues/230
            var svc = await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SVsSettingsManager)) as IVsSettingsManager;
#pragma warning restore VSTHRD010 

            Assumes.Present(svc);

            return new ShellSettingsManager(svc);
        }

        private IEnumerable<PropertyInfo> GetOptionProperties()
        {
            return GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsSerializable && p.PropertyType.IsPublic);
        }
    }
}
