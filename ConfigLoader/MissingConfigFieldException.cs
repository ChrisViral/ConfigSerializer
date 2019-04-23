using System;

/* RealChute 2 is distributed under CC BY-NC-ND 4.0 INTL (https://creativecommons.org/licenses/by-nc-nd/4.0/)
 * The NoDerivs clause may be lifted and distribution of derivative works may be attributed to individuals on a case by case basis.
 * If you wish to obtain such rights, please contact the owner (Christophe Savard) directly through a private channel. */

namespace ConfigLoader
{
    /// <summary>
    /// Exception thrown when a config field is expected but not found
    /// </summary>
    public class MissingConfigFieldException : Exception
    {
        #region Constructors
        /// <summary>
        /// Creates a new MissingConfigException
        /// </summary>
        public MissingConfigFieldException() { }

        /// <summary>
        /// Creates a new MissingConfigFieldException with the given message
        /// </summary>
        /// <param name="message">Exception message</param>
        public MissingConfigFieldException(string message) : base(message) { }
        #endregion
    }
}
