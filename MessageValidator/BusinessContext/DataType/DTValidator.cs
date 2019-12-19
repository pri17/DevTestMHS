namespace MessageValidator.BusinessContext.DataType
{
    using System;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// The dt validator.
    /// </summary>
    public class DTValidator : IDataTypeValidator
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
            //// YYYY, YYYYMM, YYYYMMDD
            if (component.Length != 4 && component.Length != 6 && component.Length != 8)
            {
                return new[]
                           {
                               new ValidationError(
                                   segment,
                                   sequence,
                                   fieldName,
                                   ErrorTypes.LengthError,
                                   "The length must be: \r\n4 (YYYY)\r\n6 (YYYYMM)\r\n8 (YYYYMMDD)")
                           };
            }

            DateTime test;
            if (!DateTime.TryParseExact(
                    component,
                    this.GetFormatForDateTime(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out test))
            {
                return new[]
                           {
                               new ValidationError(
                                   segment,
                                   sequence,
                                   fieldName,
                                   ErrorTypes.DataTypeError,
                                   "Invalid date/format. The format must be \r\nYYYY \r\nYYYYMM \r\nYYYYMMDD")
                           };
            }

            return Enumerable.Empty<ValidationError>().ToArray();
        }

        /// <summary>
        /// The get format for date time.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>string[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        public string[] GetFormatForDateTime()
        {
            return new[] { "yyyy", "yyyyMM", "yyyyMMdd" };
        }
    }
}
