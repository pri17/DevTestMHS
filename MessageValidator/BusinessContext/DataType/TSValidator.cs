// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TSValidator.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TSValidator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MessageValidator.BusinessContext.DataType
{

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// The ts validator.
    /// </summary>
    public class TSValidator : IDataTypeValidator
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
            var errors = new List<ValidationError>();
            var length = component.Length;

            // check the minimum length (greater than 26 length will be checked already)
            if (length < 4)
            {
                errors.Add(new ValidationError(
                    segment,
                    sequence,
                    fieldName,
                    ErrorTypes.LengthError,
                    "Length must be between 4 and 24 \r\nFormat: YYYY[MM[DD[HH[MM[SS[.S[S[S[S]]]]]]]]][+/-ZZZZ]\r\nAtleast the Year must be present"));

                return errors.ToArray();
            }

            // validate the format
            DateTime test;
            if (!DateTime.TryParseExact(
                component,
                this.GetDateTimeFormats(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out test))
            {
                errors.Add(new ValidationError(segment, sequence, fieldName, ErrorTypes.DataTypeError, "Valid Timestamp/Format Required\r\nFormat: YYYY[MM[DD[HH[MM[SS[.S[S[S[S]]]]]]]]][+/-ZZZZ]\r\nAtleast the Year must be present"));
            }

            return errors.Any() ? errors.ToArray() : Enumerable.Empty<ValidationError>().ToArray();
        }

        /// <summary>
        /// The get date time formats.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>string[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        public string[] GetDateTimeFormats()
        {
            return new[]
                       {
                           "yyyy", "yyyyzzz", "yyyyMM", "yyyyMMzzz", "yyyyMMdd", "yyyyMMddzzz", "yyyyMMddHH",
                           "yyyyMMddHHzzz", "yyyyMMddHHmm", "yyyyMMddHHmmzzz", "yyyyMMddHHmmss", "yyyyMMddHmmsszzz",
                           "yyyyMMddHHmmss.f", "yyyyMMddHHmmss.fzzz", "yyyyMMddHHmmss.ff", "yyyyMMddHHmmss.ffzzz",
                           "yyyyMMddHHmmss.fff", "yyyyMMddHHmmss.fffzzz", "yyyyMMddHHmmss.ffff", "yyyyMMddHHmmss.ffffzzz"
                       };
        }
    }
}
