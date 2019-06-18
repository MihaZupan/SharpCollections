using System;
using System.Diagnostics;

namespace SharpCollections.Helpers
{
    /// <summary>
    /// Inspired by CoreLib
    /// </summary>
    internal static class ThrowHelper
    {
        public static void ArgumentNullException(ExceptionArgument argument)
        {
            throw new ArgumentNullException(GetArgumentName(argument));
        }

        public static void ArgumentException(ExceptionArgument argument, ExceptionReason reason)
        {
            throw new ArgumentException(GetArgumentName(argument), GetExceptionReason(reason));
        }

        public static void ArgumentOutOfRangeException(ExceptionArgument argument, ExceptionReason reason)
        {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument), GetExceptionReason(reason));
        }

        public static void IndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

        public static void InvalidOperationException(ExceptionReason reason)
        {
            throw new InvalidOperationException(GetExceptionReason(reason));
        }

        private static string GetArgumentName(ExceptionArgument argument)
        {
            string name = null;

            switch (argument)
            {
                case ExceptionArgument.key:
                case ExceptionArgument.input:
                case ExceptionArgument.value:
                case ExceptionArgument.text:
                case ExceptionArgument.item:
                    name = argument.ToString();
                    break;

                case ExceptionArgument.offsetLength:
                    name = "offset and length";
                    break;
            }

            Debug.Assert(name != null, "The enum value is not defined, please check the ExceptionArgument Enum.");

            return name;
        }
        private static string GetExceptionReason(ExceptionReason reason)
        {
            string reasonString = null;

            switch (reason)
            {
                case ExceptionReason.String_Empty:
                    reasonString = "String must not be empty.";
                    break;

                case ExceptionReason.SmallCapacity:
                    reasonString = "Capacity was less than the current size.";
                    break;

                case ExceptionReason.InvalidOffsetLength:
                    reasonString = "Offset and length must refer to a position in the string.";
                    break;

                case ExceptionReason.DuplicateKey:
                    reasonString = "The given key is already present in the dictionary.";
                    break;

                case ExceptionReason.ContainerEmpty:
                    reasonString = "Container is empty.";
                    break;

                case ExceptionReason.MaximumCapacityReached:
                    reasonString = "Maximum capacity has been reached.";
                    break;
            }

            Debug.Assert(reasonString != null, "The enum value is not defined, please check the ExceptionReason Enum.");

            return reasonString;
        }
    }

    internal enum ExceptionArgument
    {
        key,
        input,
        value,
        offsetLength,
        text,
        item
    }

    internal enum ExceptionReason
    {
        String_Empty,
        SmallCapacity,
        InvalidOffsetLength,
        DuplicateKey,
        ContainerEmpty,
        MaximumCapacityReached,
    }
}
