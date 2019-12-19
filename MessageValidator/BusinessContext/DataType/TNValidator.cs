// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TNValidator.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TNValidator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MessageValidator.BusinessContext.DataType
{
    using System;
    using System.Linq;

    /// <summary>
    /// The tn validator.
    /// </summary>
    public class TNValidator : IDataTypeValidator
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
        /// The field Name.
        /// </param>
        /// <param name="component">
        /// The component.
        /// </param>
        /// <returns>
        /// The <see cref="ValidationError"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public ValidationError[] Validate(string segment, string sequence, string fieldName, string component)
        {
            return new[]
                       {
                           !component.All(char.IsDigit)
                               ? new ValidationError(
                                     segment,
                                     sequence,
                                     fieldName,
                                     ErrorTypes.DataTypeError,
                                     "Telephone number must contain only digits 0-9")
                               : null
                       };
        }
    }
}
