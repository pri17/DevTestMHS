using MessageValidator.BusinessContext.DataType;
using System.Collections.Generic;
using MessageValidator.Domain;
using MessageValidator.BusinessContext;
using System.Linq;
using log4net;
using System.Globalization;
using MessageValidator.Helper;

namespace MessageValidator
{
    public class Validator
    {
        /// <summary>
        /// Data type dictionary to
        /// </summary>
        public static readonly IDictionary<DataTypes, IDataTypeValidator> DataTypeDictionary =
             MessageHelper.DataTypeValidator();

        /// <summary>
        /// The table number dictionary.
        /// </summary>
        public static readonly Tables TablesList = MessageHelper.GetTableNumberDictionary();

        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The sequence number to store for the error display
        /// </summary>
        private static string sequenceNumberToStore;

        /// <summary>
        /// Validates the messages
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IList</cref>
        ///     </see>
        ///     .
        /// </returns>
        public IList<ValidationError> Validate(Message message, MessageDefinition template)
        {
            Log.Debug("Begin - validate message");
            var errors = new List<ValidationError>();
            var queue = new Queue<Segment>(message.Segments);

            // order is validated first. if this is correct, then we know that 
            var isMessageValid = this.IsInValidOrder(message, template, errors);
            if (isMessageValid)
            {
                foreach (var s in template.Items)
                {
                    var messageItemTemplate = s;

                    if (messageItemTemplate is SegmentDefinition)
                    {
                        // the name of message item will be the segment name
                        var segmentTemplate = messageItemTemplate as SegmentDefinition;
                        var duplicateSegments = queue.TakeWhile(x => queue.Peek().Value == x.Value).ToList();
                        if (duplicateSegments.Any())
                        {
                            errors.AddRange(this.Validate(message, duplicateSegments, segmentTemplate));
                            // remove the correct amount from the queue
                            if (segmentTemplate.name == duplicateSegments[0].Value)
                            {
                                this.RemoveNumberOfSegmentsFromQueue(duplicateSegments.Count, queue);
                            }
                        }
                    }
                    else
                    {
                        var groupedSegmentTemplate = messageItemTemplate as GroupDefinition;
                        if (groupedSegmentTemplate != null)
                        {
                            errors.AddRange(this.Validate(message, queue, groupedSegmentTemplate));
                        }
                    }
                }
            }

            errors.RemoveAll(item => item == null);
            Log.Debug("End - validate message");
            return errors;
        }

        /// <summary>
        /// The remove number of segments from queue.
        /// </summary>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <param name="queue">
        /// The queue.
        /// </param>
        public void RemoveNumberOfSegmentsFromQueue(int count, Queue<Segment> queue)
        {
            Log.DebugFormat("Begin - RemoveNumberOfSegmentsFromQueue: {0}", count);

            for (var i = 0; i < count; i++)
            {
                Log.WarnFormat("Removing segment from queue: {0}", queue.Peek().Value);
                queue.Dequeue();
            }

            Log.Debug("End - RemoveNumberOfSegmentsFromQueue");
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="segmentsFromMessage">
        /// The segments from message.
        /// </param>
        /// <param name="currentGroupDefinition">
        /// The group definition
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public IEnumerable<ValidationError> Validate(Message message, Queue<Segment> segmentsFromMessage, GroupDefinition currentGroupDefinition)
        {
            Log.DebugFormat("Begin - Validate Group: {0}", currentGroupDefinition.name);

            var errors = new List<ValidationError>();

            // validate each field in each segment in all of the groups. the optionality and repeat check for the segment and the group will be already checked
            foreach (var itemDefintion in currentGroupDefinition.Items)
            {
                var childGroupDefintion = itemDefintion as GroupDefinition;
                if (childGroupDefintion != null)
                {
                    errors.AddRange(this.Validate(message, segmentsFromMessage, childGroupDefintion));
                    continue;
                }

                var segmentDefinition = itemDefintion as SegmentDefinition;
                var segment = segmentsFromMessage.Any() ? segmentsFromMessage.Peek() : null;
                if (segmentDefinition != null)
                {
                    if (segment != null && segmentDefinition.name == segment.Value)
                    {
                        var duplicateSegments = segmentsFromMessage.TakeWhile(x => segmentsFromMessage.Peek().Value == x.Value).ToList();
                        errors.AddRange(this.Validate(message, duplicateSegments, segmentDefinition));
                        this.RemoveNumberOfSegmentsFromQueue(duplicateSegments.Count, segmentsFromMessage);
                    }
                }
            }

            Log.DebugFormat("End - Validate Group: {0}", currentGroupDefinition.name);
            return errors;
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="segments">
        /// The lists of segments to validate (could be null or 1 or more).
        /// </param>
        /// <param name="segmentTemplate">
        /// The segment template.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public IEnumerable<ValidationError> Validate(Message message, List<Segment> segments, SegmentDefinition segmentTemplate)
        {
            Log.DebugFormat("Begin - Validate Segment {0} that is repeated {1} times", segments[0].Value, segments.Count - 1);

            var errors = new List<ValidationError>();
            if (segments[0].Value != segmentTemplate.name)
            {
                Log.InfoFormat("Segment in the queue: {0} did not match the current Segment Template: {1}", segments[0].Value, segmentTemplate.name);
                Log.Debug(string.Format("End - Validate Segment {0}", segments[0].Value));
                return errors;
            }

            // either a single segment or a list will be validated
            for (var index = 0; index < segments.Count; index++)
            {
                var segment = segments[index];
                Log.DebugFormat("Begin - Validate Segment at index {0}: {1}", index, segments[index].Value);
                var fieldCount = segment.Fields.Count();
                if (fieldCount > segmentTemplate.Field.Count())
                {
                    var errorText =
                        string.Format(
                            "The number of fields specified is higher than the Maximum fields for this segment \r\nCurrent number of Fields {0} \r\nRequired number of Fields {1}",
                            fieldCount,
                            segmentTemplate.Field.Count());

                    errors.Add(new ValidationError(segmentTemplate.name, ErrorTypes.LengthError, errorText));

                    Log.InfoFormat(
                        "Validation error: {0} \r\nCurrent number of fields: {1} have exceeded the maximum number of fields: {2}",
                        ErrorTypes.LengthError,
                        fieldCount,
                        segmentTemplate.Field.Count());
                }
                else
                {
                    // validate the fields only if the field count is correct
                    foreach (var fieldTemplate in segmentTemplate.Field)
                    {
                        var field = segment.Fields.FirstOrDefault(x => x.SequenceNumber == fieldTemplate.sequence);
                        errors.AddRange(this.Validate(message, field, segmentTemplate, fieldTemplate));
                    }
                }

                Log.DebugFormat("End - Validate Segment at index {0}: {1}", index, segments[index].Value);
            }

            Log.Debug(string.Format("End - Validate Segment {0}", segments[0].Value));
            return errors;
        }

        /// <summary>
        /// Validates the Field
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <param name="segmentTemplate">
        /// The segment Template.
        /// </param>
        /// <param name="fieldTemplate">
        /// The field template.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public IEnumerable<ValidationError> Validate(Message message, Field field, SegmentDefinition segmentTemplate, FieldDefinition fieldTemplate)
        {
            Log.DebugFormat("Begin - Validate Field: {0}", fieldTemplate.name);
            var errors = new List<ValidationError>();

            // check optionality
            if (!this.HasFieldGotValue(segmentTemplate, field))
            {
                if (fieldTemplate.optional == "R")
                {
                    errors.Add(new ValidationError(segmentTemplate.name, fieldTemplate.sequence.ToString(CultureInfo.InvariantCulture), fieldTemplate.name, ErrorTypes.NotFoundError, "This field is required"));
                    Log.InfoFormat("Validation error: {0} \r\nRequired Field: {1} \r\nField sequence number: {2} ", ErrorTypes.NotFoundError, fieldTemplate.name, fieldTemplate.sequence);
                }
            }
            else
            {
                // we know the field has a value and field is optional or required
                // check repeat only if the data type isn't TX
                if (fieldTemplate.dataType != DataTypes.TX)
                {
                    var canRepeat = this.CheckRepeat(field, message.RepetitionDelimiter, segmentTemplate, fieldTemplate);
                    if (!canRepeat && field.HasRepeated())
                    {
                        errors.Add(new ValidationError(segmentTemplate.name, fieldTemplate.sequence.ToString(CultureInfo.InvariantCulture), fieldTemplate.name, ErrorTypes.RepeatingError, "This field cannot be repeated"));
                        Log.InfoFormat("Validation error: {0} \r\nCurrent Field: {1} \r\nError detail: This field cannot be repeated", ErrorTypes.RepeatingError, fieldTemplate.name);
                        Log.DebugFormat("End - Validate Field: {0}", fieldTemplate.name);
                        return errors;
                    }

                    if (field.HasRepeated())
                    {
                        // validate each repeating field
                        foreach (var subfield in field.RepeatingFields)
                        {
                            errors.AddRange(this.Validate(message, subfield, segmentTemplate, fieldTemplate));
                        }

                        Log.DebugFormat("End - Validate Field: {0}", fieldTemplate.name);
                        return errors;
                    }
                }

                if (!field.IsLengthValid(fieldTemplate))
                {
                    errors.Add(
                    new ValidationError(
                        segmentTemplate.name,
                        sequenceNumberToStore,
                        fieldTemplate.name,
                        ErrorTypes.LengthError,
                        "Length for this field is " + field.Length + "\r\nExpected Length: " + fieldTemplate.length));
                    Log.InfoFormat("Validation error: {0} \r\nCurrent Field: {1} \r\nCurrent length: {2} \r\nExpected Length: {3}", ErrorTypes.LengthError, fieldTemplate.name, field.Length, fieldTemplate.length);
                }

                sequenceNumberToStore = field.SequenceNumber.ToString(CultureInfo.InvariantCulture);

                // check table number
                if (fieldTemplate.tableNumber != null)
                {
                    var tablePair = this.GetTableNumberPairs(fieldTemplate.tableNumber);
                    if (tablePair != null)
                    {
                        // contents for this table is not site defined
                        if (tablePair.Any(x => x.value == field.Value))
                        {
                            // move to next field
                            return errors;
                        }

                        // no matching value in the table number
                        var errorText = string.Format("Field Value: {0} doesn't match any of the values in the table number: {1}", field.Value, fieldTemplate.tableNumber);
                        errors.Add(new ValidationError(segmentTemplate.name, fieldTemplate.sequence.ToString(CultureInfo.InvariantCulture), fieldTemplate.name, ErrorTypes.TableNumberError, errorText));
                        Log.InfoFormat("Validation error: {0} \r\nCurrent Field: {1} \r\nError detail: {2} ", ErrorTypes.TableNumberError, fieldTemplate.name, errorText);
                    }
                }

                // table number is null or site defined, validate the data type if it's simple or validate the components
                if (fieldTemplate.dataTypeSpecified)
                {
                    if (fieldTemplate.Component != null && fieldTemplate.Component.Length > 0)
                    {
                        Log.ErrorFormat("Configuration Exception: \r\nCurrent Field: {0} \r\nSegment: {1} \r\nError detail: {2}", fieldTemplate.name, segmentTemplate.name, "You've defined a simple data type with subcomponents");
                        throw new ConfigurationException(string.Format("This field: {0} in segment: {1} is defined as a simple data type with components", fieldTemplate.name, segmentTemplate.name));
                    }

                    if (field.Components != null && field.Components.Count > 1)
                    {
                        errors.Add(
                            new ValidationError(
                                segmentTemplate.name,
                                sequenceNumberToStore,
                                fieldTemplate.name,
                                ErrorTypes.LengthError,
                                "This field is a simple data type and should not have any components"));
                        return errors;
                    }

                    errors.AddRange(this.ValidateDataType(segmentTemplate, fieldTemplate, fieldTemplate.dataType, field.Value));
                }
                else
                {
                    errors.AddRange(this.ValidateComponents(segmentTemplate, fieldTemplate, fieldTemplate.Component, field.Components));
                }
            }

            Log.DebugFormat("End - Validate Field: {0}", fieldTemplate.name);
            return errors;
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="component">
        /// The component.
        /// </param>
        /// <param name="segmentTemplate">
        /// The segment template.
        /// </param>
        /// <param name="fieldTemplate">
        /// The field template.
        /// </param>
        /// <param name="componentTemplate">
        /// The component template.
        /// </param>
        /// <exception cref="ConfigurationException">
        /// an Exception where the XML files are incorrect
        /// </exception>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public IEnumerable<ValidationError> Validate(Component component, SegmentDefinition segmentTemplate, FieldDefinition fieldTemplate, ComponentDefinition componentTemplate)
        {
            Log.DebugFormat("Begin - Validate Component: {0}", componentTemplate.name);

            var errors = new List<ValidationError>();
            if (!this.HasComponentGotValue(component))
            {
                if (componentTemplate.optional == "R")
                {
                    errors.Add(new ValidationError(segmentTemplate.name, fieldTemplate.sequence.ToString(CultureInfo.InvariantCulture), fieldTemplate.name, ErrorTypes.NotFoundError, "This Component is required"));
                    Log.InfoFormat("Validation error: {0} \r\nRequired Component: {1} \r\nComponent sequence number: {2} ", ErrorTypes.NotFoundError, componentTemplate.name, componentTemplate.sequence);
                }
            }
            else
            {
                sequenceNumberToStore = sequenceNumberToStore + "." + componentTemplate.sequence;
                if (!component.IsLengthValid(componentTemplate))
                {
                    errors.Add(
                    new ValidationError(
                        segmentTemplate.name,
                        sequenceNumberToStore,
                        fieldTemplate.name,
                        ErrorTypes.LengthError,
                        "Length for this component is " + component.Length + "\r\nExpected Length: " + componentTemplate.length));
                    Log.InfoFormat("Validation error: {0} \r\nCurrent Field: {1}\r\nCurrent Component: {2} \r\nCurrent length: {3} \r\nExpected Length: {4}", ErrorTypes.LengthError, fieldTemplate.name, componentTemplate.name, component.Length, componentTemplate.length);
                }

                // component isn't null and and is optional or required
                if (componentTemplate.tableNumber != null)
                {
                    var tablePair = this.GetTableNumberPairs(componentTemplate.tableNumber);
                    if (tablePair != null)
                    {
                        if (tablePair.Any(x => x.value == component.Value))
                        {
                            // move to next component/subcomponent
                            Log.DebugFormat("End - Validate Component: {0}", componentTemplate.name);
                            return errors;
                        }

                        // no matching value in the table number
                        var errorText = string.Format("Component Value: {0} doesn't match any of the values in the table number: {1}", component.Value, fieldTemplate.tableNumber);
                        errors.Add(new ValidationError(segmentTemplate.name, sequenceNumberToStore, fieldTemplate.name, ErrorTypes.TableNumberError, errorText));
                        sequenceNumberToStore = fieldTemplate.sequence.ToString(CultureInfo.InvariantCulture);
                        Log.InfoFormat("Validation error: {0} \r\nCurrent Component: {1} \r\nError detail: {2} ", ErrorTypes.TableNumberError, componentTemplate.name, errorText);
                    }
                    else
                    {
                        // tableNumber is site defined, validate the data type if it's simple or validate the subcomponents
                        if (componentTemplate.dataTypeSpecified)
                        {
                            // there should be no subcomponents
                            if (componentTemplate.Component != null && componentTemplate.Component.Length > 0)
                            {
                                Log.ErrorFormat("Configuration Exception: \r\nCurrent Component: {0} \r\nError detail: {1}", componentTemplate.name, "You've defined a simple data type with subcomponents");
                                throw new ConfigurationException("You've defined a simple data type with subcomponents");
                            }

                            if (component.SubComponents.Count > 1)
                            {
                                errors.Add(
                                    new ValidationError(
                                        segmentTemplate.name,
                                        sequenceNumberToStore,
                                        fieldTemplate.name,
                                        ErrorTypes.LengthError,
                                        "This component is a simple data type and should not have any subcomponents"));
                                return errors;
                            }

                            // validate the data type
                            errors.AddRange(this.ValidateDataType(segmentTemplate, fieldTemplate, componentTemplate.dataType, component.Value));
                        }
                        else
                        {
                            // validate the subcomponents
                            errors.AddRange(this.ValidateComponents(segmentTemplate, fieldTemplate, componentTemplate.Component, component.SubComponents));
                        }
                    }
                }
                else
                {
                    // table number is null, validate the data type if its simple or validate the subcomponents
                    if (componentTemplate.dataTypeSpecified)
                    {
                        // there should be no subcomponents
                        if (componentTemplate.Component != null && componentTemplate.Component.Length > 0)
                        {
                            Log.ErrorFormat("Configuration Exception: \r\nCurrent Component: {0} \r\nError detail: {1}", componentTemplate.name, "You've defined a simple data type with subcomponents");
                            throw new ConfigurationException("You've defined a simple data type with subcomponents");
                        }

                        if (component.SubComponents.Count > 1)
                        {
                            errors.Add(
                                new ValidationError(
                                    segmentTemplate.name,
                                    sequenceNumberToStore,
                                    fieldTemplate.name,
                                    ErrorTypes.LengthError,
                                    "This component is a simple data type and should not have any subcomponents"));
                            return errors;
                        }

                        // simple type, validate the data type
                        errors.AddRange(this.ValidateDataType(segmentTemplate, fieldTemplate, componentTemplate.dataType, component.Value));
                    }
                    else
                    {
                        // validate the subcomponents
                        errors.AddRange(this.ValidateComponents(segmentTemplate, fieldTemplate, componentTemplate.Component, component.SubComponents));
                    }
                }
            }

            Log.DebugFormat("End - Validate Component: {0}", componentTemplate.name);
            return errors;
        }

        /// <summary>
        /// check repetition of field (only fields can repeat)
        /// </summary>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <param name="repetitionDelimiter">
        /// The repetition Delimiter.
        /// </param>
        /// <param name="segmentTemplate">
        /// The segment template.
        /// </param>
        /// <param name="fieldTemplate">
        /// The field template.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CheckRepeat(ICommonFields field, char repetitionDelimiter, SegmentDefinition segmentTemplate, FieldDefinition fieldTemplate)
        {
            Log.Debug("Begin - CheckRepeat - true");

            if (fieldTemplate.repeat == "*")
            {
                Log.Debug("End - CheckRepeat - true");
                return true;
            }

            if (!field.Value.Contains(repetitionDelimiter))
            {
                Log.Debug("End - CheckRepeat - true");
                return true;
            }

            // prevent encoding characters from being an error
            if (segmentTemplate.name == "MSH" && fieldTemplate.sequence == 2)
            {
                Log.Debug("End - CheckRepeat - true");
                return true;
            }

            Log.Debug("End - CheckRepeat - false");
            return false;
        }

        /// <summary>
        /// The get table number pairs.
        /// </summary>
        /// <param name="tableNumber">
        /// The table number.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        private IEnumerable<TablePair> GetTableNumberPairs(string tableNumber)
        {
            Log.Debug("Begin - GetTableNumberPairs");

            foreach (var table in TablesList.Table)
            {
                if (table.number == tableNumber)
                {
                    return table.Entry;
                }
            }

            Log.Debug("End - GetTableNumberPairs");
            return null;
        }

        /// <summary>
        /// The validate data type.
        /// </summary>
        /// <param name="segmentTemplate">
        /// The segment template.
        /// </param>
        /// <param name="fieldTemplate">
        /// The field template.
        /// </param>
        /// <param name="dataType">
        /// The data type to validate the messageValue
        /// </param>
        /// <param name="messageValue">
        /// The message value.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>ValidationError[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        /// <exception cref="ConfigurationException">
        /// </exception>
        private IEnumerable<ValidationError> ValidateDataType(
            SegmentDefinition segmentTemplate,
            FieldDefinition fieldTemplate,
            DataTypes dataType,
            string messageValue)
        {
            Log.DebugFormat("Begin - ValidateDataType: {0}", dataType);

            var errors = new List<ValidationError>();

            // validate the data type
            IDataTypeValidator dataTypeValidor;

            DataTypeDictionary.TryGetValue(dataType, out dataTypeValidor);
            if (dataTypeValidor != null)
            {
                Log.DebugFormat("Begin - Validate DataType: {0} for this value: {1}", dataType, messageValue);
                var errorsFromDataType = dataTypeValidor.Validate(segmentTemplate.name, sequenceNumberToStore, fieldTemplate.name, messageValue);
                Log.DebugFormat("End - Validate DataType: {0}", dataType);
                if (errorsFromDataType != null)
                {
                    errors.AddRange(errorsFromDataType);
                    foreach (var validationError in errors)
                    {
                        if (validationError != null)
                        {
                            Log.InfoFormat(
                                "Data Type error: " + "{0} " + "\r\nCurrent Field: {1} "
                                + "\r\nField sequence number: {2} " + "\r\nError detail: {3}",
                                validationError.Error,
                                fieldTemplate.name,
                                sequenceNumberToStore,
                                validationError.ErrorText);
                        }
                    }
                }
            }
            else
            {
                Log.ErrorFormat("Invalid Data Type Defined: {0} \r\nSegment: {1} \r\nField Name: {2} \r\nField Sequence Number: {3}", fieldTemplate.dataType, segmentTemplate.name, fieldTemplate.name, fieldTemplate.sequence);
                throw new ConfigurationException("Invalid Data Type defined: " + fieldTemplate.dataType);
            }

            Log.DebugFormat("Begin - ValidateDataType: {0}", dataType);
            return errors.ToArray();
        }

        /// <summary>
        /// The validate components.
        /// </summary>
        /// <param name="segmentTemplate">
        /// The segment template.
        /// </param>
        /// <param name="fieldTemplate">
        /// The field template.
        /// </param>
        /// <param name="componentDefinitions">
        /// The component definitions.
        /// </param>
        /// <param name="componentsFromMessage">
        /// The components from message.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>ValidationError[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        /// <exception cref="ConfigurationException">
        /// </exception>
        private IEnumerable<ValidationError> ValidateComponents(SegmentDefinition segmentTemplate, FieldDefinition fieldTemplate, IEnumerable<ComponentDefinition> componentDefinitions, IEnumerable<Component> componentsFromMessage)
        {
            Log.Debug("Begin - Validate Components");
            var errors = new List<ValidationError>();

            // there must be components in the field template
            if (componentDefinitions == null || !componentDefinitions.Any())
            {
                Log.ErrorFormat("Configuration Exception: \r\nCurrent Segment: {0} \r\nCurrentField: {1}  \r\nError detail: {2}", segmentTemplate.name, fieldTemplate.name, "This field must have subcomponents");
                throw new ConfigurationException(string.Format("Components not present in the field {0} \r\nSegment {1}", fieldTemplate.name, segmentTemplate.name));
            }

            var componentDefintionList = componentDefinitions as IList<ComponentDefinition>;
            
            // check number of component in the field
            var fromMessage = componentsFromMessage as IList<Component> ?? new Component[0];
            var componentCount = fromMessage.Count();
            if (componentCount > componentDefintionList.Count())
            {
                var errorText = string.Format("The components are greater than the Maximum amount \r\nCurrent components {0} \r\nMaximum components {1}", componentCount, componentDefintionList.Count());
                errors.Add(
                    new ValidationError(
                        segmentTemplate.name,
                        sequenceNumberToStore,
                        fieldTemplate.name,
                        ErrorTypes.LengthError,
                        errorText));
                Log.InfoFormat("Validation error: {0} \r\nCurrent Field: {1} \r\nError detial: {2}", ErrorTypes.LengthError, fieldTemplate.name, errorText);
            }
            else
            {
                foreach (var componentDefinition in componentDefintionList)
                {
                    var temp = sequenceNumberToStore;
                    var component = fromMessage.FirstOrDefault(x => x.SequenceNumber == componentDefinition.sequence);
                    errors.AddRange(this.Validate(component, segmentTemplate, fieldTemplate, componentDefinition));
                    sequenceNumberToStore = temp;
                }
            }

            Log.Debug("End - Validate Components");
            return errors.ToArray();
        }

        /// <summary>
        /// if in valid order.
        /// </summary>
        /// <param name="messageToValidate">
        /// The message To Validate.
        /// </param>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="errors">
        /// The errors.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsInValidOrder(Message messageToValidate, MessageDefinition template, IList<ValidationError> errors)
        {
            Log.Debug("Begin - IsInValidOrder");

            // contains the segments inside the queue
            var queue = new Queue<Segment>(messageToValidate.Segments);
            foreach (var messageItemDefinition in template.Items)
            {
                // check segment one by one through template. if template segment is optional move on,
                // else throw error "expecting this template segment and got whatever on top of the queue" and exit the function
                if (messageItemDefinition is SegmentDefinition)
                {
                    // the name of message item will be the segment name
                    var segmentTemplate = messageItemDefinition as SegmentDefinition;
                    if (!this.ProcessSegment(segmentTemplate, queue, errors))
                    {
                        Log.Debug("End - IsInValidOrder - false");
                        return false;
                    }
                }
                else if (messageItemDefinition is GroupDefinition)
                {
                    // this is the parent group
                    var currentGroupDefinition = messageItemDefinition as GroupDefinition;
                    if (!this.ProcessGroup(queue, currentGroupDefinition, errors))
                    {
                        Log.Debug("End - IsInValidOrder - false");
                        return false;
                    }
                }
            }

            // if there are segments still inside queue, something is wrong
            if (queue.Count > 0)
            {
                // queue contains segments, return false, otherwise return true
                var errorText = "These segments have not been validated:";
                foreach (var segment in queue)
                {
                    errorText += string.Format("\r\n{0}", segment.Value);
                }

                errorText += string.Format("\r\n\r\n{0} segment may be unexpected at Line {1}", queue.Peek().Value, queue.Peek().SequenceNumber);

                var error = new ValidationError(string.Empty, ErrorTypes.UnexpectedError, errorText);
                errors.Add(error);
                Log.InfoFormat("Validation error: {0} \r\nError detail: {1}", ErrorTypes.UnexpectedError, errorText);
            }

            Log.Debug("End - IsInValidOrder");
            return queue.Count == 0;
        }

        /// <summary>
        /// The process group.
        /// </summary>
        /// <param name="segments">
        /// The segments.
        /// </param>
        /// <param name="definition">
        /// The definition.
        /// </param>
        /// <param name="errors">
        /// The errors.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ProcessGroup(Queue<Segment> segments, GroupDefinition definition, IList<ValidationError> errors)
        {
            Log.DebugFormat("Begin - ProcessGroup: {0}", definition.name);
            var groupRepeatCount = 0;

            // checks if any segment matches in the group so that it can repeat again
            bool groupMatched;

            do
            {
                groupMatched = false;

                foreach (var itemDefinition in definition.Items ?? new MessageItemDefinition[0])
                {
                    var itemRepeatCount = 0;

                    // checks if the segment or inner group matches so that either the segments or the inner groups can repeat
                    bool itemMatched;

                    // Validate group:
                    var childGroupDefinition = itemDefinition as GroupDefinition;

                    if (childGroupDefinition != null)
                    {
                        itemMatched = this.ProcessGroup(segments, childGroupDefinition, errors);

                        // will always return an error message if the child group hasn't been processed correctly
                        if (!itemMatched)
                        {
                            return false;
                        }

                        continue;
                    }

                    // Validate segment:
                    var segmentDefinition = (SegmentDefinition)itemDefinition;
                    Log.DebugFormat("Begin Procesing Segment Template: {0}", segmentDefinition.name);
                    var segment = segments.Count > 0 ? segments.Peek() : null;
                    do
                    {
                        itemMatched = false;

                        if (segment != null && segmentDefinition.name == segment.Value)
                        {
                            groupMatched = itemMatched = true;
                            segments.Dequeue();
                            segment = segments.Count > 0 ? segments.Peek() : null;
                            itemRepeatCount += 1;
                        }
                    }
                    while (itemMatched && itemDefinition.repeat == "*"); // individual segment/inner group definition

                    Log.DebugFormat("End Procesing Segment Template: {0}", segmentDefinition.name);

                    // when the repeat check is being done, if there are no elements that have matched the group, assume the repeat is finished
                    if (groupRepeatCount > 0 && groupMatched == false)
                    {
                        Log.DebugFormat("End - ProcessGroup: {0}", definition.name);
                        return true;
                    }

                    // moves to next group if no segment have been matched in an optional group when the first required segment is reached
                    if (!groupMatched && segmentDefinition.optional == "R")
                    {
                        if (definition.optional == "R")
                        {
                            // error.
                            errors.Add(
                                new ValidationError(
                                    definition.name, ErrorTypes.NotFoundError, "This group is required"));
                            Log.InfoFormat(
                                "Validation error: {0}. \r\nCurrent Group: {1} \r\nError datail: {2}",
                                ErrorTypes.NotFoundError,
                                definition.name,
                                string.Format("Expected {0}, Retrieved {1}", segmentDefinition.name, segment != null ? segment.Value : "Nothing"));

                            Log.DebugFormat("End - ProcessGroup: {0}", definition.name);
                            return false;
                        }

                        Log.DebugFormat("End - ProcessGroup: {0}", definition.name);
                        return true;
                    }

                    // if no items for this segment and the segment definition is required
                    if (itemRepeatCount == 0 && segmentDefinition.optional == "R")
                    {
                        errors.Add(
                            new ValidationError(
                                segmentDefinition.name,
                                ErrorTypes.NotFoundError,
                                string.Format("Expected {0}, Retrieved {1}", segmentDefinition.name, segment != null ? segment.Value : "Nothing")));
                        Log.InfoFormat("Validation error: {0}. \r\nCurrent Segment: {1} \r\nError datail: {2}", ErrorTypes.NotFoundError, segmentDefinition.name, string.Format("Expected {0}, Retrieved {1}", segmentDefinition.name, segment != null ? segment.Value : "Nothing"));
                        Log.DebugFormat("End - ProcessGroup: {0}", definition.name);
                        return false;
                    }
                }

                groupRepeatCount += 1;
            }
            while (groupMatched && definition.repeat == "*"); // group defintion

            Log.DebugFormat("End - ProcessGroup: {0}", definition.name);
            return true;
        }

        /// <summary>
        /// The process segment.
        /// </summary>
        /// <param name="segmentTemplate">
        /// The segment template.
        /// </param>
        /// <param name="queue">
        /// The queue.
        /// </param>
        /// <param name="errors">
        /// The errors.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ProcessSegment(SegmentDefinition segmentTemplate, Queue<Segment> queue, IList<ValidationError> errors)
        {
            Log.DebugFormat("Begin - ProcessSegment Template: {0}", segmentTemplate.name);
            var duplicateConsecutiveSegments = queue.TakeWhile(x => queue.Peek().Value == x.Value).ToList();
            foreach (var segment in duplicateConsecutiveSegments)
            {
                if (segment.Value != segmentTemplate.name)
                {
                    if (segmentTemplate.optional == "R")
                    {
                        errors.Add(new ValidationError(segmentTemplate.name, ErrorTypes.NotFoundError, string.Format("Expected {0}, Retrieved {1}", segmentTemplate.name, segment.Value ?? "Nothing")));
                        Log.InfoFormat("Validation error: {0}. \r\nCurrent Segment: {1} \r\nError detail: {2}", ErrorTypes.NotFoundError, segmentTemplate.name, string.Format("Expected {0}, Retrieved {1}", segmentTemplate.name, segment.Value ?? "Nothing"));
                        Log.DebugFormat("End - ProcessSegment Template: {0}", segmentTemplate.name);
                        return false;
                    }

                    // move on to the next segment leaving the queue as it is
                    continue;
                }

                // check null or empty
                if (string.IsNullOrEmpty(segment.Value) && segmentTemplate.optional == "R")
                {
                    errors.Add(new ValidationError(segmentTemplate.name, ErrorTypes.NotFoundError, "This segment is required"));
                    Log.InfoFormat("Validation error: {0}. \r\nRequired Segment: {1}", ErrorTypes.NotFoundError, segmentTemplate.name);
                    Log.DebugFormat("End - ProcessSegment Template: {0}", segmentTemplate.name);
                    return false;
                }

                // check repeat 
                if (segmentTemplate.repeat != "*" && duplicateConsecutiveSegments.Count > 1)
                {
                    errors.Add(new ValidationError(segmentTemplate.name, ErrorTypes.RepeatingError, "This segment at line" + segment.SequenceNumber + " that is outside the group cannot be duplicated"));
                    Log.InfoFormat("Validation error: {0}. \r\nCurrent Segment: {1} \r\nError detail: {2}", ErrorTypes.RepeatingError, segmentTemplate.name, "This segment at line" + segment.SequenceNumber + " that is outside the group cannot be duplicated");
                    Log.DebugFormat("End - ProcessSegment Template: {0}", segmentTemplate.name);
                    return false;
                }

                // remove the correct amount from the queue
                queue.Dequeue();
            }

            Log.DebugFormat("End - ProcessSegment Template: {0}", segmentTemplate.name);
            return true;
        }

        /// <summary>
        /// The check optionality for field.
        /// </summary>
        /// <param name="segmentDefinition">
        /// The segment Definition.
        /// </param>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool HasFieldGotValue(SegmentDefinition segmentDefinition, Field field)
        {
            if (field == null)
            {
                return false;
            }

            // Check for any repeating field 
            if (field.HasRepeated())
            {
                return field.RepeatingFields.Any(repeatingField => this.HasFieldOrComponentGotValue(segmentDefinition, repeatingField));
            }

            // check the individual field
            return this.HasFieldOrComponentGotValue(segmentDefinition, field);
        }

        /// <summary>
        /// Checks whether the component has got a value inside
        /// </summary>
        /// <param name="component">
        /// The component.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool HasComponentGotValue(ICommonFields component)
        {
            if (component == null)
            {
                return false;
            }

            var text = MessageHelper.Replace(component.Value, new[] { "&" });

            return text != string.Empty;
        }

        /// <summary>
        /// The has field or component got value.
        /// </summary>
        /// <param name="segmentDefinition">
        /// The segment Definition.
        /// </param>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool HasFieldOrComponentGotValue(SegmentDefinition segmentDefinition, Field field)
        {
            if (field == null)
            {
                return false;
            }

            if (field.SequenceNumber == 2 && segmentDefinition.name == "MSH")
            {
                return true;
            }

            // check whether there is any text besides the component and subcomponent seperator
            var text = MessageHelper.Replace(field.Value, new[] { "^", "&" });

            return text != string.Empty;
        }
    }
}
