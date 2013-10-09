// -----------------------------------------------------------------------
// <copyright file="SettingsBase.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Options
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// Base class providing a settings storage file.
    /// </summary>
    [Serializable]
    public abstract class SettingsBase
    {
        /// <summary>
        /// Extension for the settings file.
        /// </summary>
        protected const string Extension = "bbSettings";

        /// <summary>
        /// The path to the config file
        /// </summary>
        protected string configFilePath;

        /// <summary>
        /// Tries the find file.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>the path if the file is found.</returns>
        public static string TryFindFile(string[] paths)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Saves the config file.
        /// </summary>
        public void Save()
        {
            var serialiser = new XmlSerializer(this.GetType());
            using (TextWriter tw = new StreamWriter(this.configFilePath))
            {
                lock (this.configFilePath)
                {
                    serialiser.Serialize(tw, this);
                }
            }
        }

        /// <summary>
        /// Loads settings from a config file.
        /// </summary>
        /// <typeparam name="Tsettings">The type of the settings.</typeparam>
        /// <param name="configFileName">Name of the config file.</param>
        /// <returns>a BlinkboxSccOptions instance.</returns>
        internal static Tsettings LoadFromConfig<Tsettings>(string configFileName) where Tsettings : SettingsBase, new()
        {
            Tsettings options = null;

            if (File.Exists(configFileName))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(Tsettings));
                    using (TextReader tr = new StreamReader(configFileName))
                    {
                        options = (Tsettings)serializer.Deserialize(tr);
                    }
                }
                catch (Exception ex)
                {
                    NotificationService.DisplayException(ex, "Loading settings file " + configFileName + " failed");
                }
            }

            if (options == null)
            {
                options = new Tsettings();
            }

            options.configFilePath = configFileName;
            options.Init();

            return options;
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        protected abstract void Init();
    }
}
