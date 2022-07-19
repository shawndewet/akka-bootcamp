using System;
using System.Collections.Generic;
using System.Text;

namespace WinTail.Messages
{

    #region Neutral/System messages

    /// <summary>
    /// Marker class to continue processing
    /// </summary>
    public class ContinueProcessing { }

    #endregion

    #region Success messages

    /// <summary>
    /// Base class for signalling that user input was valid
    /// </summary>
    public class InputSuccess
    {
        public InputSuccess(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; private set; }
    }

    #endregion

    #region Error messages

    /// <summary>
    /// Base class for signalling that user input was invalid
    /// </summary>
    public class InputError
    {
        public InputError(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; private set; }
    }

    public class NullInputError : InputError
    {
        public NullInputError(string reason) : base (reason)
        {

        }
    }

    public class ValidationError : InputError
    {
        public ValidationError(string reason) : base (reason)
        {

        }
    }

    #endregion



}
