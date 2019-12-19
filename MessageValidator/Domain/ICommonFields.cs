// $Id: ICommonFields.cs 13133 2018-09-05 12:40:42Z cse-servelec\asghar.mahmood $

namespace MessageValidator.Domain
{
    using MessageValidator.Definition;

    /// <summary>
    /// The CommonFields interface.
    /// </summary>
    public interface ICommonFields
    {
        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        int Length { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Checks if the length from the object implementing this interface is valid comparing it to the definition's length.
        /// </summary>
        /// <param name="definition">
        /// The definition.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool IsLengthValid(ICommonDefinition definition);
    }
}
