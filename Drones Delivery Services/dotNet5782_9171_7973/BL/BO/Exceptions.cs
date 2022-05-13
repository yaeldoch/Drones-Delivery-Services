﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BO
{
    /// <summary>
    /// An exception that occours when there is a trial to make an invalid action 
    /// in the BL layer
    /// </summary>
    [Serializable]
    public class InvalidActionException : Exception
    {
        public InvalidActionException() : base() { }
        public InvalidActionException(string message) : base(message) { }
        public InvalidActionException(string message, Exception inner) : base(message, inner) { }
        protected InvalidActionException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override string ToString()
        {
            return Message;
        }
    }

    /// <summary>
    /// An exception that occours when the wanted item does not exist
    /// </summary>
    [Serializable]
    public class ObjectNotFoundException : Exception
    {
        public ObjectNotFoundException(string message) : base(message) { }

        public ObjectNotFoundException(string message, Exception inner) : base(message, inner) { }

        protected ObjectNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public ObjectNotFoundException(Type type, Exception inner) : base($"{type.Name} not found", inner) { }

        public ObjectNotFoundException(Type type) : base($"{type.Name} not found") { }

    }

    /// <summary>
    /// An exception that occours when there is a try to add an object with existing id
    /// </summary>
    [Serializable]
    public class IdAlreadyExistsException : Exception
    {
        public IdAlreadyExistsException(string message) : base(message) { }

        public IdAlreadyExistsException(string message, Exception inner) : base(message, inner) { }

        protected IdAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public IdAlreadyExistsException(Type type, Exception inner) : base($"{type.Name} not found", inner) { }

        public IdAlreadyExistsException(Type type, int id) : base($"{type.Name} #{id} already exists") { }
    }

    /// <summary>
    /// An exception that occours when there is a try to add an object with existing id
    /// </summary>
    [Serializable]
    public class InvalidPropertyValueException : Exception
    {
        public string PropertyName { get; set; }

        public object Value { get; set; }

        public InvalidPropertyValueException(string message) : base(message) { }

        public InvalidPropertyValueException(string message, Exception inner) : base(message, inner) { }

        protected InvalidPropertyValueException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public InvalidPropertyValueException(object value, [CallerMemberName] string propName = "")
        {
            PropertyName = propName;
            Value = value;
        }
    }

}
