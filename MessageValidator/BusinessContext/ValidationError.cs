namespace MessageValidator.BusinessContext
{
    /// <summary>
    /// The validation error.
    /// </summary>
    public class ValidationError
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="segmentOrGroup">
        /// The SegmentOrGroup.
        /// </param>
        /// <param name="fieldNumber">
        /// The field number.
        /// </param>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="errorText">
        /// The error text.
        /// </param>
        public ValidationError(string segmentOrGroup, string fieldNumber, string field, ErrorTypes error, string errorText)
        {
            this.SegmentOrGroup = segmentOrGroup;
            this.FieldNumber = fieldNumber;
            this.Field = field;
            this.Error = error;
            this.ErrorText = errorText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// if the whole segment or group has an error (not found, required etc)
        /// </summary>
        /// <param name="segmentOrGroup">
        /// The SegmentOrGroup.
        /// </param>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="errorText">
        /// The error text.
        /// </param>
        public ValidationError(string segmentOrGroup, ErrorTypes error, string errorText)
        {
            this.SegmentOrGroup = segmentOrGroup;
            this.Error = error;
            this.ErrorText = errorText;
        }

        /// <summary>
        /// Gets or sets the SegmentOrGroup.
        /// </summary>
        public string SegmentOrGroup { get; set; }

        /// <summary>
        /// Gets or sets the field number.
        /// </summary>
        public string FieldNumber { get; set; }

        /// <summary>
        /// Gets or sets the field.
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        public ErrorTypes Error { get; set; }

        /// <summary>
        /// Gets or sets the error text.
        /// </summary>
        public string ErrorText { get; set; }
    }
}
