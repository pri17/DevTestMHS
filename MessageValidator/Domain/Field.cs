// $Id: Field.cs 13133 2018-09-05 12:40:42Z cse-servelec\asghar.mahmood $

namespace MessageValidator.Domain
{
    using System.Collections.Generic;
    using MessageValidator.Definition;

    /// <summary>
    /// The field.
    /// </summary>
    public class Field : ICommonFields
    {
        /// <summary>
        /// Gets or sets the components.
        /// </summary>
        public IList<Component> Components { get; set; }

        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the repeating fields.
        /// </summary>
        public IList<Field> RepeatingFields { get; set; } 

        /// <summary>
        /// The has repeated.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool HasRepeated()
        {
            return this.RepeatingFields != null;
        }

        /// <summary>
        /// compares the length between the definition and it's value
        /// </summary>
        /// <param name="definition">
        /// The definition to compare the length to
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsLengthValid(ICommonDefinition definition)
        {
            return this.Value.Length <= definition.length;
        }
    }
}
