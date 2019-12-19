namespace MessageValidator.Helper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using MessageValidator.BusinessContext.DataType;
    using log4net;
    using MessageValidator.Domain;
    using System.Linq;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using GotDotNet.XInclude;
    using MessageValidator.BusinessContext;

    /// <summary>
    /// The helper.
    /// </summary>
    public static class MessageHelper
    {
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// gets the  dictionary for all the table numbers that is associated with a list of TablePairs
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>IDictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static Tables GetTableNumberDictionary()
        {
            Log.Debug("Begin - GetTableNumberDictionary");
            var tables = GetTableNumbersFromXmlFile();
            Log.Debug("End - GetTableNumberDictionary");
            return tables;
        }

        public static Message GetMessage(string messageContent)
        {
            MessageDefinition template = null;
            Log.Debug("Begin - GetMessage");

            int segmentSequenceNumber = 1;
            var segmentList = new List<Segment>();
            var found = false;

            const char FieldSeperatorValue = '|';
            const char ComponentseparatorValue = '^';
            const char RepetitionseparatorValue = '~';
            const char EscapeCharacterValue = '\\';
            const char SubComponentseparatorValue = '&';
            
            var lines = messageContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    if (found == false)
                    {
                        var error = "MSH has not been found as the first segment in the message. Validation Failed";
                        Log.ErrorFormat(error);
                        Log.Debug("End - BtnValidateMessage");
                        return null;
                    }

                    continue;
                }

                if (IsMshValid(line, ref found, FieldSeperatorValue, ComponentseparatorValue, RepetitionseparatorValue,
                    EscapeCharacterValue, SubComponentseparatorValue, ref template))
                {
                    // validate fields
                    var fieldList = GetAllFieldsFromTheLine(line, FieldSeperatorValue, RepetitionseparatorValue,
                        ComponentseparatorValue, SubComponentseparatorValue);
                    if (fieldList == null)
                    {
                        Log.Debug("End - BtnValidateMessage");
                        return null;
                    }

                    Log.InfoFormat("Adding segment to the list: {0}", line.Substring(0, 3));
                    segmentList.Add(new Segment(fieldList, segmentSequenceNumber, line.Substring(0, 3)));
                    ++segmentSequenceNumber;
                }
                else
                {
                    Log.Debug("End - BtnValidateMessage");
                    return null;
                }
            }

            var messageToValidate = new Message(segmentList, FieldSeperatorValue, ComponentseparatorValue, RepetitionseparatorValue, EscapeCharacterValue, SubComponentseparatorValue, template);
            return messageToValidate;
        }

        private static List<Field> GetAllFieldsFromTheLine(string line, char fieldSeperator, char repetitionseparator, char componentSeparator, char subComponentSeparator)
        {
            Log.DebugFormat("Begin - GetAllFieldsFromTheLine");
            var fieldSequenceNumber = 1;

            var fieldList = new List<Field>();

            // split after the segment ID 
            var fields = line.Substring(4).Split(fieldSeperator).ToList();

            if (line.Substring(0, 3).Equals("MSH"))
            {
                fields.Insert(0, fieldSeperator.ToString(CultureInfo.InvariantCulture));
            }

            for (var index = 0; index < fields.Count; index++)
            {
                // the component list for this field
                var componentList = new List<Component>();

                // repeating field to store.
                var tempFieldRepeatingListToStore = new List<Field>();

                // the foreach loop which contains the 1 field/many repeating field
                string[] repeatingFieldSplit = { " " };

                // check for any formatted text in the field to be replaced
                CheckForAnyCommandsToBeReplaced(fields, index);

                // Split the field value when it is repeating. 
                if (index == 1 && line.Substring(0, 3).Equals("MSH"))
                {
                    repeatingFieldSplit[0] = fields[index];
                }
                else
                {
                    repeatingFieldSplit = fields[index].Split(repetitionseparator);
                }

                // go through the repeats of the field.
                foreach (var field in repeatingFieldSplit)
                {
                    componentList.AddRange(GetAllComponentsFromFields(line, field, fields, componentSeparator, subComponentSeparator));

                    tempFieldRepeatingListToStore.Add(
                        new Field
                        {
                            Value = field,
                            SequenceNumber = fieldSequenceNumber,
                            Length = field.Length,
                            Components = componentList
                        });
                }

                // depend on repeating, add different value to field
                if (tempFieldRepeatingListToStore.Count > 1)
                {
                    // add the current field to the list as well as the repeating field list
                    fieldList.Add(
                        new Field
                        {
                            Value = fields[index],
                            SequenceNumber = fieldSequenceNumber,
                            Length = fields[index].Length,
                            Components = null,
                            RepeatingFields = tempFieldRepeatingListToStore
                        });
                }
                else
                {
                    // if no repeats. just append to the end of the field list.
                    fieldList.AddRange(tempFieldRepeatingListToStore);
                }

                ++fieldSequenceNumber;
            }

            Log.Debug("End - GetAllFieldsFromTheLine");
            return fieldList;
        }

        private static IEnumerable<Component> GetAllComponentsFromFields(string line, string field, List<string> fields, char componentseparator, char subComponentseparator)
        {
            // reset the component sequence number for this field iteration
            var componentSequenceNumber = 1;
            var componentListToReturn = new List<Component>();
            var subComponentList = new List<Component>();

            // prevent encoding characters from MSH field 2 being seperated into subcomponents and instead save the field
            if (!(line.Substring(0, 3).Equals("MSH") && field == fields[1]))
            {
                var componentsArray = field.Split(componentseparator);

                if (componentsArray.Length > 1)
                {
                    foreach (var component in componentsArray)
                    {
                        subComponentList.AddRange(GetAllSubComponentsFromComponents(component, subComponentseparator));
                        componentListToReturn.Add(new Component
                        {
                            Length = component.Length,
                            Value = component,
                            SequenceNumber = componentSequenceNumber,
                            SubComponents = subComponentList
                        });

                        ++componentSequenceNumber;

                        subComponentList = new List<Component>();
                    }
                }
                else
                {

                    subComponentList.AddRange(GetAllSubComponentsFromComponents(field, subComponentseparator));
                    var comp = new Component
                    {
                        Length = field.Length,
                        Value = field,
                        SequenceNumber = componentSequenceNumber,
                        SubComponents = subComponentList
                    };
                    componentListToReturn.Add(comp);
                }
            }
            else
            {
                // its the encoding character field
                var encodingCharactersComponent = field;
                componentListToReturn.Add(
                    new Component
                    {
                        Length = encodingCharactersComponent.Length,
                        SequenceNumber = componentSequenceNumber,
                        Value = encodingCharactersComponent,
                        SubComponents = new List<Component>()
                    });
            }

            return componentListToReturn;
        }

        private static IEnumerable<Component> GetAllSubComponentsFromComponents(string component, char subComponentseparator)
        {
            // reset the subcomponent for this component iteration
            var subComponentSequenceNumber = 1;
            var subComponentList = new List<Component>();

            // loop through subcomponents
            var subComponents = component.Split(subComponentseparator);

            // makes sure that string split method doesn't return itself
            if (subComponents.Count() > 1)
            {
                foreach (var subComponent in subComponents)
                {
                    subComponentList.Add(
                        new Component
                        {
                            Length = subComponent.Length,
                            SequenceNumber = subComponentSequenceNumber,
                            Value = subComponent,
                            SubComponents = new List<Component>()
                        });

                    ++subComponentSequenceNumber;
                }
            }
            else
            {
                // return a subcomponent that is the same as the component
                subComponentList.Add(new Component
                {
                    Length = component.Length,
                    Value = component,
                    SequenceNumber = subComponentSequenceNumber,
                    SubComponents = new List<Component>()
                });
            }

            return subComponentList;
        }

        private static void CheckForAnyCommandsToBeReplaced(List<string> array, int index)
        {
            Log.Debug("Begin - CheckForAnyCommandsToBeReplaced");

            // passes the match that matched the regex into this delegate function
            // match is the text that was found to correspond to regex engine
            var matchEvaluator = (MatchEvaluator)(x =>
            {
                var countGroup = x.Groups["Count"];
                var count = countGroup.Success ? int.Parse(countGroup.Value) : 1;

                // return the string which replaces the content 
                return string.Concat(Enumerable.Repeat(@"\.br\", count));
            });

            if (!array[index].Contains(@"^\.sp") && !array[index].Contains(@"\.ce\"))
            {
                Log.InfoFormat("No Commands to be replaced");
                Log.Debug("End - CheckForAnyCommandsToBeReplaced");
                return;
            }

            // check the 2 possibilities for sp
            if (array[index].Contains(@"^\.sp\"))
            {
                Log.InfoFormat(@"Replacing ^\.sp\ with \.br\");
                array[index] = array[index].Replace(@"^\.sp\", @"\.br\");
            }

            if (array[index].Contains(@"^\.sp"))
            {
                const string RegexToCheckFor = @"\^\\.sp\+(?<Count>[1-9]+[0-9]*)?\\";
                Log.InfoFormat(@"Replacing ^\.sp that matces the regex: {0} with \.br\", RegexToCheckFor);

                // keep a track of the number after the + sign
                array[index] = Regex.Replace(array[index], RegexToCheckFor, matchEvaluator);
            }

            // check for any .ce commands
            if (array[index].Contains(@"\.ce\"))
            {
                Log.InfoFormat(@"Replacing ^\.ce\ with \.br\");
                array[index] = array[index].Replace(@"\.ce\", @"\.br\");
            }

            Log.Debug("End - CheckForAnyCommandsToBeReplaced");
        }

        private static bool IsMshValid(string line, ref bool found, char fieldSeperator, char componentseparator, char repetitionseparator, char escapeCharacter, char subComponentseparator, ref MessageDefinition template)
        {
            Log.Debug("Begin - IsMshValid");

            // check if the segment MSH exists for the first time if so, read the characters in field 2 to get the encoding characters
            if (found == false && line.Substring(0, 3).Equals("MSH"))
            {
                found = true;

                var errors = TestSeparators(line, fieldSeperator, componentseparator, repetitionseparator, escapeCharacter, subComponentseparator);
                if (errors.Count > 0)
                {
                    var errorsOnEachLine = string.Join(Environment.NewLine, errors);
                    Log.ErrorFormat("Errors for Encoding field in MSH:2" + errorsOnEachLine);
                    return false;
                }

                var fields = line.Split(new[] { fieldSeperator });
                var encodingCharacters = fields[1];

                // repating check ?
                if (encodingCharacters.Length == 4)
                {
                    if (encodingCharacters.Distinct().Count() != 4)
                    {
                        Log.ErrorFormat("Encoding chaacters are not unique in field 2 of MSH.");
                        var error =
                            string.Format(
                            "The encoding characters must all be unique and stored in this order:{0}1. Component Seperator{0}2. Repetition Seperator{0}3. Escape character{0}4. Subcomponent Seperator",
                            Environment.NewLine);
                        Log.Debug("End - IsMshValid ");
                        return false;
                    }
                }
                else
                {
                    Log.ErrorFormat("Expected length of encoding characters: 4 \r\n Actual Length: {0}", encodingCharacters.Length);
                    var error = string.Format(
                            "The length of the encoding charcters must be 4 and stored in this order:{0}1. Component Seperator{0}2. Repetition Seperator{0}3. Escape character{0}4. Subcomponent Seperator",
                            Environment.NewLine);
                    Log.Debug("End - IsMshValid ");
                    return false;
                }
                
                if (!HasTemplateLoadedSuccessfuly(fields, componentseparator, ref template))
                {
                    Log.Debug("End - IsMshValid");
                    return false;
                }
            }

            // end the validation
            if (found == false)
            {
                Log.ErrorFormat("MSH does not exist in the message");
                Log.Debug("End - IsMshValid ");
                return false;
            }

            Log.Debug("End - IsMshValid ");
            return true;
        }

        private static bool HasTemplateLoadedSuccessfuly(string[] fields, char componentseparator, ref MessageDefinition template)
        {
            Log.Debug("Begin - HasTemplateLoadedSuccessfuly");
            
            var messageTypeField = GetArrayValue(fields.ToList(), 8);
            var versionField = GetArrayValue(fields.ToList(), 11);

            if (string.IsNullOrWhiteSpace(messageTypeField) && string.IsNullOrWhiteSpace(versionField))
            {
                Log.Error("There is no value for the message type field and the version ID field.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(messageTypeField))
            {
                Log.Error("There is no value for the message type field. Please state the message type and the trigger event (and an optional message structure field) and seperate them by the component separator.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(versionField))
            {
                Log.Error("there is no value for the Version ID field");
                return false;
            }

            // find template based on the message type
            var messageType = messageTypeField.Split(componentseparator);

            if (messageType.Length < 2)
            {
                Log.ErrorFormat(string.Format("the message template {0} could not be found", messageTypeField));
                return false;
            }

            var versionId = versionField.Split(componentseparator);
            string vid = string.Empty;
            var tablesList = GetTableNumberDictionary();

            foreach (var entry in tablesList.Table[34].Entry)
            {
                if (versionId[0].Equals(entry.value))
                {
                    vid = versionId[0];
                }
            }

            if (string.IsNullOrWhiteSpace(vid))
            {
                Log.ErrorFormat("The version ID for this message {0} could not be found", versionId[0]);
                return false;
            }

            var messageCode = string.Empty;
            var triggerEvent = string.Empty;
            var messageStructure = string.Empty;

            if (!string.IsNullOrWhiteSpace(messageType[0]))
            {
                messageCode = messageType[0];
            }

            if (!string.IsNullOrWhiteSpace(messageType[1]))
            {
                triggerEvent = messageType[1];
            }

            if (messageType.Length == 3 && !string.IsNullOrWhiteSpace(messageType[2]))
            {
                messageStructure = messageType[2];
            }

            var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\MessageValidator\XML Files/Version Id\" + vid + @"\MessageTypes\" + messageCode + @"\"
                   + messageCode + "_" + triggerEvent + ".XML");

            if (!File.Exists(templatePath))
            {
                return false;
            }

            try
            {
                
                var textreader = new XIncludingReader(templatePath);
                var serialiser = new XmlSerializer(typeof(MessageDefinition));
                template = (MessageDefinition)serialiser.Deserialize(textreader);
            }
            catch (InvalidOperationException ex)
            {
                if (messageType.Length == 3 && !string.IsNullOrEmpty(messageStructure))
                {
                    try
                    {
                        var textreader = new XIncludingReader(templatePath);
                        var serialiser = new XmlSerializer(typeof(MessageDefinition));
                        template = (MessageDefinition)serialiser.Deserialize(textreader);
                    }
                    catch (Exception ero)
                    {
                        Log.ErrorFormat("Exception had occured. Exception Detail: value that caused the error: \r\nValue: {0} \r\nException StackTract{1}", messageTypeField, ero);
                        Log.ErrorFormat("The message template {0} could not be found", messageTypeField);
                        Log.Debug("End - HasTemplateLoadedSuccessfuly");
                        return false;
                    }
                }

                Log.ErrorFormat(
                    "Exception had occured. Exception Detail: value that caused the error: \r\nValue: {0} \r\nException StackTract{1}",
                    messageTypeField,
                    ex);
                Log.ErrorFormat("The message template {0} could not be found", messageTypeField);
                Log.Debug("End - HasTemplateLoadedSuccessfuly");
                return false;
            }

            Log.Debug("End - HasTemplateLoadedSuccessfuly");
            return true;
        }

        private static List<string> TestSeparators(string line, char FieldSeperatorValue, char ComponentseparatorValue, char RepetitionseparatorValue, char EscapeCharacterValue, char SubComponentseparatorValue)
        {
            Log.Debug("Begin TestSeparators");
            var errorList = new List<string>();

            var fieldSeperator = GetArrayValue(line, 3);
            var componentseparator = GetArrayValue(line, 4);
            var repetitionseparator = GetArrayValue(line, 5);
            var escapeCharacter = GetArrayValue(line, 6);
            var subComponentseparator = GetArrayValue(line, 7);

            if (fieldSeperator != FieldSeperatorValue)
            {
                errorList.Add(string.Format("\r\nField Seperator does not equal to {0}", FieldSeperatorValue));
            }

            if (componentseparator != ComponentseparatorValue)
            {
                errorList.Add(string.Format("\r\nComponent Seperator does not equal to {0}", ComponentseparatorValue));
            }

            if (repetitionseparator != RepetitionseparatorValue)
            {
                errorList.Add(string.Format("\r\nRepetition Seperator does not equal to {0}", RepetitionseparatorValue));
            }

            if (escapeCharacter != EscapeCharacterValue)
            {
                errorList.Add(string.Format("\r\nEscape Character Seperator does not equal to {0}", EscapeCharacterValue));
            }

            if (subComponentseparator != SubComponentseparatorValue)
            {
                errorList.Add(string.Format("\r\nSubcomponent Seperator does not equal to {0}", SubComponentseparatorValue));
            }

            Log.Debug("End TestSeparators");
            return errorList;
        }

        /// <summary>
        /// The data type validator.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>IDictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IDictionary<DataTypes, IDataTypeValidator> DataTypeValidator()
        {
            Log.Debug("Begin - DataTypeValidator");
            return new Dictionary<DataTypes, IDataTypeValidator>
                       {
                           { DataTypes.DTM, new DTMValidator { DataType = DataTypes.DTM } },
                           { DataTypes.DT, new DTValidator { DataType = DataTypes.DT } },
                           { DataTypes.FT, new FTValidator { DataType = DataTypes.FT } },
                           { DataTypes.ID, new IDValidator { DataType = DataTypes.ID } },
                           { DataTypes.IS, new ISValidator { DataType = DataTypes.IS } },
                           { DataTypes.NM, new NMValidator { DataType = DataTypes.NM } },
                           { DataTypes.SI, new SIValidator { DataType = DataTypes.SI } },
                           { DataTypes.ST, new STValidator { DataType = DataTypes.ST } },
                           { DataTypes.TN, new TNValidator { DataType = DataTypes.TN } },
                           { DataTypes.TS, new TSValidator { DataType = DataTypes.TS } },
                           { DataTypes.TX, new TXValidator { DataType = DataTypes.TX } },
                           { DataTypes.VARIES, new VARIESValidator { DataType = DataTypes.VARIES } }
                       };
        }

        public static bool GetExceptionFilter(Exception e)
        {
            return e is ArgumentNullException || e is ConfigurationException || e is NullReferenceException;
        }

        /// <summary>
        /// The get dictionary from xml.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static Tables GetTableNumbersFromXmlFile()
        {
            Log.Debug("Begin - GetTableNumbersFromXmlFile");

            var FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\MessageValidator\XML Files\Table Numbers\TableNumbers.XML");

            if (!File.Exists(FilePath))
            {
                return null;
            }

            var reader = new StreamReader(FilePath);
            var serializer = new XmlSerializer(typeof(Tables));
            var tableNumberList = (Tables)serializer.Deserialize(reader);

            Log.Debug("End - GetTableNumbersFromXmlFile");
            return tableNumberList;
        }

        /// <summary>
        /// replaces all the specified characters 
        /// </summary>
        /// <param name="originalString">
        /// The string that will contain the original text
        /// </param>
        /// <param name="toReplaceList">
        /// replace all the occurrences of each character in the replace list to the specified string.
        /// </param>
        /// <returns>
        /// The string that doesn't contain the characters from replace list or an empty string
        /// </returns>
        public static string Replace(string originalString, params string[] toReplaceList)
        {
            var toReturn = originalString;
            foreach (var charToReplace in toReplaceList)
            {
                toReturn = toReturn.Replace(charToReplace, string.Empty);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the value from the array.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Catches this exception when the index is out of bounds from the array
        /// </exception>
        /// <param name="charArray">
        /// The array to look through
        /// </param>
        /// <param name="index">
        /// The index to find in the array
        /// </param>
        /// <returns>
        /// The <see cref="string"/> that belongs in the array or null of the index is out of bounds.
        /// </returns>
        public static char GetArrayValue(string charArray, int index)
        {
            if (index < 0 || index >= charArray.Length)
            {
                return '\0';
            }
            return charArray[index];
        }

        /// <summary>
        /// Gets the value from the array.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Catches this exception when the index is out of bounds from the array
        /// </exception>
        /// <param name="array">
        /// The array to look through
        /// </param>
        /// <param name="index">
        /// The index to find in the array
        /// </param>
        /// <returns>
        /// The <see cref="string"/> that belongs in the array or null of the index is out of bounds.
        /// </returns>
        public static string GetArrayValue(List<string> array, int index)
        {
            if (index < 0 || index >= array.Count)
            {
                return null;
            }

            return array[index];
        }

    }
}

