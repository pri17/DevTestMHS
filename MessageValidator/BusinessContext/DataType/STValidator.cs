namespace MessageValidator.BusinessContext.DataType
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The st validator.
    /// </summary>
    public class STValidator : IDataTypeValidator
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
            var formattedTextValidator = new FTValidator();

            errors.AddRange(this.CheckCharacterRange(segment, sequence, fieldName, component));

            // check if there are any escape seqeunces (field/component/subcomponent/repetition/escape seqeunce) and validate them
            errors.AddRange(formattedTextValidator.ValidateStructure(component, segment, sequence, fieldName, formattedTextValidator.GetPatternForEscapeSequence(Patterns.Fieldseparator), @"\F\"));
            errors.AddRange(formattedTextValidator.ValidateStructure(component, segment, sequence, fieldName, formattedTextValidator.GetPatternForEscapeSequence(Patterns.ComponentSeperator), @"\S\"));
            errors.AddRange(formattedTextValidator.ValidateStructure(component, segment, sequence, fieldName, formattedTextValidator.GetPatternForEscapeSequence(Patterns.Subcomponentseparator), @"\T\"));
            errors.AddRange(formattedTextValidator.ValidateStructure(component, segment, sequence, fieldName, formattedTextValidator.GetPatternForEscapeSequence(Patterns.Repetitionseparator), @"\R\"));
            errors.AddRange(formattedTextValidator.ValidateStructure(component, segment, sequence, fieldName, formattedTextValidator.GetPatternForEscapeSequence(Patterns.EscapeCharacter), @"\E\"));

            return errors.Any() ? errors.ToArray() : Enumerable.Empty<ValidationError>().ToArray();
        }

        /// <summary>
        /// Checks the character range for the component
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
        /// The <see cref="ValidationError[]"/>.
        /// </returns>
        public ValidationError[] CheckCharacterRange(string segment, string sequence, string fieldName, string component)
        {
            var errors = new List<ValidationError>();

            // check the characters inside the text field
            if (component.Any(character => character < 32 || character > 126 || character.Equals((char)13)))
            {
                errors.Add(new ValidationError(segment, sequence, fieldName, ErrorTypes.DataTypeError, "The character range must be between ASCII decimal value 32 and 126"));
            }

            return errors.Any() ? errors.ToArray() : Enumerable.Empty<ValidationError>().ToArray();
        }
    }
}
