// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TXValidator.cs" company="">
//   
// </copyright>
// <summary>
//   The tx validator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MessageValidator.BusinessContext.DataType
{

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The tx validator.
    /// </summary>
    public class TXValidator : IDataTypeValidator
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
            var errors = new List<ValidationError>();
            var formattedText = new FTValidator();

            // check the characters inside the text field
            errors.AddRange(new STValidator().Validate(segment, sequence, fieldName, component));

            // check if there are any escape seqeunces and validate them
            errors.AddRange(formattedText.CheckEscapeSequence(segment, sequence, fieldName, component));

            return errors.ToArray();
        }
    }
}
