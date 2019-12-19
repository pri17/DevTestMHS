namespace MessageValidator.Domain
{
    using System.Collections.Generic;
    using MessageValidator.Definition;

    public class Component : ICommonFields
    {
        /// <summary>
        /// Gets or sets the sub components.
        /// </summary>
        public IList<Component> SubComponents { get; set; }

        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the length of the component
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

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
            // for version 2.4
            if (definition.length == 0)
            {
                return true;
            }

            return this.Value.Length <= definition.length;
        }
    }
}
