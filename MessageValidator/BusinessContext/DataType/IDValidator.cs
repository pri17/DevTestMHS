// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDValidator.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IDValidator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MessageValidator.BusinessContext.DataType
{
    using System.Linq;

    /// <summary>
    /// The id validator.
    /// </summary>
    public class IDValidator : IDataTypeValidator
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
            return Enumerable.Empty<ValidationError>().ToArray();
        }
    }
}
