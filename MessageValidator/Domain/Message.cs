// $Id: Message.cs 13033 2018-07-31 13:16:12Z cse-servelec\asghar.mahmood $

namespace MessageValidator.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// The message.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="segments">
        /// The segments.
        /// </param>
        /// <param name="fieldDelimiter">
        /// The field delimiter.
        /// </param>
        /// <param name="componentDelimiter">
        /// The component delimiter.
        /// </param>
        /// <param name="repetitionDelimiter">
        /// The repetition delimiter.
        /// </param>
        /// <param name="escapeCharacter">
        /// The escape character.
        /// </param>
        /// <param name="subComponentDelimiter">
        /// The sub component delimiter.
        /// </param>
        public Message(IList<Segment> segments, char fieldDelimiter, char componentDelimiter, char repetitionDelimiter, char escapeCharacter, char subComponentDelimiter, MessageDefinition template)
        {
            this.FieldDelimiter = fieldDelimiter;
            this.ComponentDelimiter = componentDelimiter;
            this.RepetitionDelimiter = repetitionDelimiter;
            this.EscapeCharacter = escapeCharacter;
            this.SubComponentDelimiter = subComponentDelimiter;
            this.Segments = segments;
            this.template = template;
        }

        /// <summary>
        /// Gets or sets the field delimiter.
        /// </summary>
        public char FieldDelimiter { get; set; }

        /// <summary>
        /// Gets or sets the component delimiter.
        /// </summary>
        public char ComponentDelimiter { get; set; }

        /// <summary>
        /// Gets or sets the repetition delimiter.
        /// </summary>
        public char RepetitionDelimiter { get; set; }

        /// <summary>
        /// Gets or sets the escape character.
        /// </summary>
        public char EscapeCharacter { get; set; }

        /// <summary>
        /// Gets or sets the sub component delimiter.
        /// </summary>
        public char SubComponentDelimiter { get; set; } 

        /// <summary>
        /// Gets or sets the segments.
        /// </summary>
        public IList<Segment> Segments { get; set; }
        
        /// <summary>
        /// Gets or sets the template.
        /// </summary>
        public MessageDefinition template { get; set; }
    }
}
