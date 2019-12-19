namespace MessageValidator.BusinessContext.DataType
{
    using System.Linq;

    /// <summary>
    /// The si validator.
    /// </summary>
    public class SIValidator : IDataTypeValidator
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
            int i;
            if (!int.TryParse(component, out i))
            {
                return new[]
                           {
                               new ValidationError(
                                   segment,
                                   sequence,
                                   fieldName,
                                   ErrorTypes.DataTypeError,
                                   "This Field must be an integer")
                           };
            }

            if (i < 0)
            {
                return new[]
                           {
                               new ValidationError(
                                   segment,
                                   sequence,
                                   fieldName,
                                   ErrorTypes.DataTypeError,
                                   "This field should not have a negative number")
                           };
            }

            return Enumerable.Empty<ValidationError>().ToArray();
        }
    }
}
