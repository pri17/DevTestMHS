// $Id: Segment.cs 13002 2018-07-25 14:52:18Z cse-servelec\asghar.mahmood $

namespace MessageValidator.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// The segment.
    /// </summary>
    public class Segment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Segment"/> class.
        /// </summary>
        /// <param name="fields">
        /// The fields.
        /// </param>
        /// <param name="expectedSequenceNumber">
        /// The expected sequence number.
        /// </param>
        public Segment(List<Field> fields, int expectedSequenceNumber, string value)
        {
            this.Fields = fields;
            this.SequenceNumber = expectedSequenceNumber;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        public IList<Field> Fields { get; set; }

        /// <summary>
        /// Gets or sets the value (i.e the name of the segment)
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        public int SequenceNumber { get; set; }
    }
}
