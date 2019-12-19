namespace MessageValidator.Definition
{
    /// <summary>
    /// The CommonDefinition interface.
    /// </summary>
    public interface ICommonDefinition
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        string name { get; set; }

        /// <summary>
        /// Gets or sets the optional.
        /// </summary>
        string optional { get; set; }

        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        DataTypes dataType { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        int length { get; set; }

        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        int sequence { get; set; }

        /// <summary>
        /// Gets or sets the table number.
        /// </summary>
        string tableNumber { get; set; }
    }
}
