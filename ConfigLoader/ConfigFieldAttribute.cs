using System;

/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader
{
    /// <summary>
    /// Attribute used to mark fields and properties that can be loaded via ConfigObject
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ConfigFieldAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// If this config field is mandatory cor the loading process to correctly proceed
        /// </summary>
        public bool Mandatory { get; set; } = false;
        /// <summary>
        /// The name of the field in the config, if left blank, the name of the code field is used
        /// </summary>
        public string CustomName { get; set; } = null;
        #endregion
    }
}
