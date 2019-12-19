namespace MessageValidator.BusinessContext.DataType
{

    /// <summary>
    /// The DataTypeValidator interface.
    /// </summary>
    public interface IDataTypeValidator
    {
        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        DataTypes DataType { get; set; }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="segment">
        /// The segment.
        /// </param>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="comment">
        /// The comment.
        /// </param>
        /// <param name="errorText">
        /// The error text.
        /// </param>
        /// <returns>
        /// The <see cref="ValidationError"/>.
        /// </returns>
        ValidationError[] Validate(string segment, string sequence, string comment, string errorText);
    }
}
