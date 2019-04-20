using System;

/* RealChute 2 is distributed under CC BY-NC-ND 4.0 INTL (https://creativecommons.org/licenses/by-nc-nd/4.0/)
 * The NoDerivs clause may be lifted and distribution of derivative works may be attributed to individuals on a case by case basis.
 * If you wish to obtain such rights, please contact the owner (Christophe Savard) directly through a private channel. */

namespace RealChute.ConfigLoader
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
