namespace MessageValidator.BusinessContext.DataType
{
    /// <summary>
    /// The dtm validator.
    /// </summary>
    public class DTMValidator : IDataTypeValidator
    {
        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        public DataTypes DataType { get; set; }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="segment">
        /// The segment.
        /// </param>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="fieldName">
        /// The field name.
        /// </param>
        /// <param name="component">
        /// The component.
        /// </param>
        /// <returns>
        /// The <see cref="ValidationError"/>.
        /// </returns>
        public ValidationError[] Validate(string segment, string sequence, string fieldName, string component)
        {
            // has same validation as TS
            var ts = new TSValidator();
            return ts.Validate(segment, sequence, fieldName, component);
        }
    }
}
