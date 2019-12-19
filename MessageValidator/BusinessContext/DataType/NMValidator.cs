// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NMValidator.cs" company="">
//   
// </copyright>
// <summary>
//   The nm validator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MessageValidator.BusinessContext.DataType
{
    using System.Linq;

    /// <summary>
    /// The nm validator.
    /// </summary>
    public class NMValidator : IDataTypeValidator
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
            double i;
            if (!double.TryParse(component, out i))
            {
                return new[]
                           {
                               new ValidationError(
                                   segment,
                                   sequence,
                                   fieldName,
                                   ErrorTypes.DataTypeError,
                                   "NM data type Invalid. There can be an optional leading + or - sign and an optional decimal point and contains no characters. An example is +10.5 ")
                           };
            }

            return Enumerable.Empty<ValidationError>().ToArray();
        }
    }
}
