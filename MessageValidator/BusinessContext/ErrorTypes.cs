namespace MessageValidator.BusinessContext
{
    /// <summary>
    /// The different error types
    /// </summary>
    public enum ErrorTypes
    {
        /// <summary>
        /// The error when the maximum length has been exceeded
        /// </summary>
        LengthError,

        /// <summary>
        /// The error when a Segment/field/Component/subcomponent/Group are missing.
        /// </summary>
        NotFoundError,

        /// <summary>
        /// The error when a value doesn't match any of the values in a specific table number.
        /// </summary>
        TableNumberError,

        /// <summary>
        /// The repeating error.
        /// </summary>
        RepeatingError,

        /// <summary>
        /// The unexpected error.
        /// </summary>
        UnexpectedError,

        /// <summary>
        /// The data type error.
        /// </summary>
        DataTypeError,
    }
}
