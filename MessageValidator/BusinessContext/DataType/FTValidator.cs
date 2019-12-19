namespace MessageValidator.BusinessContext.DataType
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The FT validator.
    /// </summary>
    public class FTValidator : IDataTypeValidator
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

            // check the character range and see if it's valid or not
            errors.AddRange(new STValidator().CheckCharacterRange(segment, sequence, fieldName, component));

            errors.AddRange(this.CheckFormattedText(component, segment, sequence, fieldName));
            errors.AddRange(this.CheckEscapeSequence(segment, sequence, fieldName, component));

            return errors.Any() ? errors.ToArray() : Enumerable.Empty<ValidationError>().ToArray();
        }

        /// <summary>
        /// Checks if the escape sequences (if any) have been written correctly
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
        /// The <see cref="string"/>.
        /// </returns>
        public ValidationError[] CheckEscapeSequence(string segment, string sequence, string fieldName, string component)
        {
            var errors = new List<ValidationError>();

            // data characters can appear before highlight, in middle of highlight and after highlight in which this can repeat
            var highlightPattern = this.GetPatternForEscapeSequence(Patterns.Highlight);
            errors.AddRange(this.ValidateStructure(component, segment, sequence, fieldName, highlightPattern, @"\H\"));

            var fieldSeperatorPattern = this.GetPatternForEscapeSequence(Patterns.Fieldseparator);
            errors.AddRange(this.ValidateStructure(component, segment, sequence, fieldName, fieldSeperatorPattern, @"\F\"));

            var escapeCharacterPattern = this.GetPatternForEscapeSequence(Patterns.EscapeCharacter);
            errors.AddRange(this.ValidateStructure(component, segment, sequence, fieldName, escapeCharacterPattern, @"\E\"));

            var componentSeperatorPattern = this.GetPatternForEscapeSequence(Patterns.ComponentSeperator);
            errors.AddRange(this.ValidateStructure(component, segment, sequence, fieldName, componentSeperatorPattern, @"\S\"));

            var repetitionSeperatorPattern = this.GetPatternForEscapeSequence(Patterns.Repetitionseparator);
            errors.AddRange(this.ValidateStructure(component, segment, sequence, fieldName, repetitionSeperatorPattern, @"\R\"));

            var subComponentSeperatorPattern = this.GetPatternForEscapeSequence(Patterns.Subcomponentseparator);
            errors.AddRange(this.ValidateStructure(component, segment, sequence, fieldName, subComponentSeperatorPattern, @"\T\"));

            return errors.Any() ? errors.ToArray() : Enumerable.Empty<ValidationError>().ToArray();
        }

        /// <summary>
        /// Checks if there are any formatted texts and validates them.
        /// </summary>
        /// <param name="stringToTest">
        /// The string to test.
        /// </param>
        /// <param name="segment">
        /// The segment.
        /// </param>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="fieldName">
        /// The field Name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public ValidationError[] CheckFormattedText(string stringToTest, string segment, string sequence, string fieldName)
        {
            var errors = new List<ValidationError>();

            // begin new output line
            const string BrPattern = @"^((([^\\](?!\\\.br))|(\\(?!\.br)))*.?(\\\.br\\|$))*$";
            errors.AddRange(this.ValidateStructure(stringToTest, segment, sequence, fieldName, BrPattern, @"\.br"));

            const string CePattern = @"^((([^\\](?!\\\.ce))|(\\(?!\.ce)))*.?(\\\.ce\\|$))*$";
            errors.AddRange(this.ValidateStructure(stringToTest, segment, sequence, fieldName, CePattern, @"\.ce"));

            // begin wrap mode
            const string FiPattern = @"^((([^\\](?!\\\.fi))|(\\(?!\.fi)))*.?(\\\.fi\\|$))*$";
            errors.AddRange(this.ValidateStructure(stringToTest, segment, sequence, fieldName, FiPattern, @"\.fi"));
            
            // begin no wrap mode
            const string NfPattern = @"^((([^\\](?!\\\.nf))|(\\(?!\.nf)))*.?(\\\.nf\\|$))*$";
            errors.AddRange(this.ValidateStructure(stringToTest, segment, sequence, fieldName, NfPattern, @"\.nf"));

            // allow positive integer or absent
            const string SkPatternStructure = @"^((([^\\](?!\\\.sk))|(\\(?!\.sk)))*.?(\\\.sk(\+)([1-9]+[0-9]*)\\|$))*$";
            errors.AddRange(this.ValidateStructure(stringToTest, segment, sequence, fieldName, SkPatternStructure,  @"\.sk"));

            // allow positive or negative integer. must be before the first printable character of a line
            const string InPatternStructure = @"(\\\.)(in)(\+|\-)([1-9]+([0-9])*)\\";
            const string InvalidPatternForIn = @"((\s*(\w+|[\p{P}\p{S}-[\\]])\s*)\\\.in|\s*\\\.in[\+|\-][1-9]+[0-9]*\\(\w|\s)*(?<!\\\.br\\)\s*\\\.in)";
            errors.AddRange(this.Validate(stringToTest, segment, sequence, fieldName, InPatternStructure, InvalidPatternForIn, @"\.in", false));

            const string TiPatternStructure = @"(\\\.)(ti)(\+|\-)([1-9]+([0-9])*)\\";
            const string InvalidPatternForTi = @"((\s*(\w+|[\p{P}\p{S}-[\\]])\s*)\\\.ti|\s*\\\.ti[\+|\-][1-9]+[0-9]*\\(\w|\s)*(?<!\\\.br\\)\s*\\\.ti)";
            errors.AddRange(this.Validate(stringToTest, segment, sequence, fieldName, TiPatternStructure, InvalidPatternForTi, @"\.ti", false));

            return errors.Any() ? errors.ToArray() : Enumerable.Empty<ValidationError>().ToArray();
        }

        /// <summary>
        /// This validates the structure at any part of the string.
        /// </summary>
        /// <param name="stringToTest">
        /// The string to test.
        /// </param>
        /// <param name="segment">
        /// The segment.
        /// </param>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="fieldName">
        /// The field name.
        /// </param>
        /// <param name="patternStructure">
        /// The pattern Structure.
        /// </param>
        /// <param name="patternToCheck">
        /// The pattern to check.
        /// </param>
        /// <param name="formattedCommand">
        /// The formatted Command.
        /// </param>
        /// <param name="testForValidPattern">
        /// The test for valid pattern.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>ValidationError[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        public ValidationError[] Validate(string stringToTest, string segment, string sequence, string fieldName, string patternStructure, string patternToCheck, string formattedCommand, bool testForValidPattern)
        {
            var errors = new List<ValidationError>();

            if (!stringToTest.Contains(formattedCommand))
            {
                return Enumerable.Empty<ValidationError>().ToArray();
            }

            // the formatting command is present
            var lines = stringToTest.Split(new[] { @"\.br\" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (testForValidPattern)
                {
                    // if the line does match the valid pattern
                    if (Regex.IsMatch(line, patternToCheck))
                    {
                        // check the structure is correct
                        errors.AddRange(this.ValidateStructure(line, segment, sequence, fieldName, patternStructure, patternToCheck));
                        continue;
                    }
                }
                else
                {
                    // if the line doesn't match the invalid pattern 
                    if (!Regex.IsMatch(line, patternToCheck))
                    {
                        // check the structure is correct
                        errors.AddRange(this.ValidateStructure(line, segment, sequence, fieldName, patternStructure, formattedCommand));
                        continue;
                    }
                }

                // line is invalid
                var errorPartOfTheLine = Regex.Match(line, patternToCheck).Value;
                var errorText = string.Format("Field has an invalid formatting command of {0} in this part of the line: \r\n{1}", formattedCommand, errorPartOfTheLine);
                errorText += this.AdditionalErrorTextForFormattedText(formattedCommand);
                errors.Add(new ValidationError(segment, sequence, fieldName, ErrorTypes.DataTypeError, errorText));
            }

            return errors.Any() ? errors.ToArray() : Enumerable.Empty<ValidationError>().ToArray();
        }

        /// <summary>
        /// The validate pattern.
        /// </summary>
        /// <param name="stringToTest">
        /// The string to test.
        /// </param>
        /// <param name="segment">
        /// The segment.
        /// </param>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="fieldName">
        /// The field name.
        /// </param>
        /// <param name="patternStructure">
        /// The pattern structure.
        /// </param>
        /// <param name="formattedCommand">
        /// The formatted Command.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>ValidationError[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        public ValidationError[] ValidateStructure(
            string stringToTest,
            string segment,
            string sequence,
            string fieldName,
            string patternStructure,
            string formattedCommand)
        {
            var errors = new List<ValidationError>();

            if (!stringToTest.Contains(formattedCommand))
            {
                return Enumerable.Empty<ValidationError>().ToArray();
            }

            var match = Regex.Match(stringToTest, patternStructure);
            if (match.Success)
            {
                return errors.ToArray();
            }
            
            var errorText = string.Format(
                "Match Failed  with the text: \r\n{0} \r\nFor this formatting command: \r\n{1}",
                stringToTest,
                formattedCommand);

            errorText += this.AdditionalErrorTextForFormattedText(formattedCommand);
            errors.Add(new ValidationError(segment, sequence, fieldName, ErrorTypes.DataTypeError, errorText));

            return errors.ToArray();
        }

        /// <summary>
        /// The additional text for formatting command.
        /// </summary>
        /// <param name="command">
        /// The command that is being validated
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string AdditionalErrorTextForFormattedText(string command)
        {
            var toReturn = string.Empty;

            switch (command)
            {
                case @"\H\":
                    toReturn = "\r\nThe format for highlighting text is \\H\\TextHere\\N\\ \r\nNo escape sequence may contain a nested escape sequence";
                    break;
                case @"\F\":
                    toReturn = "\r\nThe format for the field seperator is \\F\\TextHere\\F\\ \r\nNo escape sequence may contain a nested escape sequence";
                    break;
                case @"\S\":
                    toReturn = "\r\nThe format for the component seperator is \\S\\TextHere\\S\\ \r\nNo escape sequence may contain a nested escape sequence";
                    break;
                case @"\T\":
                    toReturn = "\r\nThe format for the subcomponent seperator is \\T\\TextHere\\T\\ \r\nNo escape sequence may contain a nested escape sequence";
                    break;
                case @"\R\":
                    toReturn = "\r\nThe format for the repetition seperator is \\R\\TextHere\\R\\ \r\nNo escape sequence may contain a nested escape sequence";
                    break;
                case @"\E\":
                    toReturn = "\r\nThe format for the escape sequence is \\E\\TextHere\\E\\ \r\nNo escape sequence may contain a nested escape sequence";
                    break;
                case @"\.br\":
                    toReturn = "\r\nFormat must be: \\.br\\ or ^\\.sp<number>\\ where number is postivie or absent and corresponds to each newline or \\.ce\\";
                    break;
                case @"\.fi\":
                    toReturn = "\r\nFormat must be: \\.fi\\ for begin wrap mode";
                    break;
                case @"\.nf\":
                    toReturn = "\r\nFormat must be: \\.nf\\ for no wrap mode";
                    break;
                case @"\.in\":
                    toReturn = "\r\nFormat must be: \\.in<number>\\ where number is postive or negative for indenting number of spaces. "
                               + "\r\nThis command cannot appear after the first printable character of a line.";
                    break;
                case @"\.ti\":
                    toReturn = "\r\nFormat must be: \\.ti<number>\\ where number is postive or negative for temporarily indenting number of spaces. "
                               + "\r\nThis command cannot appear after the first printable character of a line.";
                    break;
                case @"\.sk\":
                    toReturn = "\r\nFormat must be: \\.sk<number>\\ to skip number of spaces to the right"; 
                    break;
            }

            return toReturn;
        }

        /// <summary>
        /// The get pattern for escape sequence.
        /// </summary>
        /// <param name="patternToRetrieve">
        /// The pattern to retrieve.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetPatternForEscapeSequence(Patterns patternToRetrieve)
        {
            switch (patternToRetrieve)
            {
                case Patterns.ComponentSeperator:
                    {
                        return @"^((([^\\](?!\\S))|(\\(?!S)))*.?((\\S\\(\w*|\d*|[\p{P}\p{S}-[\\]]{0,})\\S\\)|$))*$";
                    }

                case Patterns.EscapeCharacter:
                    {
                        return @"^((([^\\](?!\\E))|(\\(?!E)))*.?((\\E\\(\w*|\d*|[\p{P}\p{S}-[\\]]{0,})\\E\\)|$))*$";
                    }

                case Patterns.Fieldseparator:
                    {
                        return @"^((([^\\](?!\\F))|(\\(?!F)))*.?((\\F\\(\w*|\d*|[\p{P}\p{S}-[\\]]{0,})\\F\\)|$))*$";
                    }

                case Patterns.Highlight:
                    {
                        return @"^((([^\\](?!\\H))|(\\(?!H)))*.?((\\H\\(\w*|\d*|[\p{P}\p{S}-[\\]]{0,})\\N\\)|$))*$";
                    }

                case Patterns.Repetitionseparator:
                    {
                        return @"^((([^\\](?!\\R))|(\\(?!R)))*.?((\\R\\(\w*|\d*|[\p{P}\p{S}-[\\]]{0,})\\R\\)|$))*$";
                    }

                case Patterns.Subcomponentseparator:
                    {
                        return @"^((([^\\](?!\\T))|(\\(?!T)))*.?((\\T\\(\w*|\d*|[\p{P}\p{S}-[\\]]{0,})\\T\\)|$))*$";
                    }

                default:
                    {
                        return null;
                    }
            }
        }
    }
}
