using System.Reflection;
using System;
using System.Collections.Generic;

namespace AVGParser
{
    class TypeUtils
    {
        public static bool IsNumericType(Type type)
        {
            if (Type.GetTypeCode(type) >= TypeCode.SByte && Type.GetTypeCode(type) <= TypeCode.Decimal)
                return true;
            return false;
            //switch (Type.GetTypeCode(type))
            //{
            //    case TypeCode.Byte:
            //    case TypeCode.SByte:
            //    case TypeCode.UInt16:
            //    case TypeCode.UInt32:
            //    case TypeCode.UInt64:
            //    case TypeCode.Int16:
            //    case TypeCode.Int32:
            //    case TypeCode.Int64:
            //    case TypeCode.Decimal:
            //    case TypeCode.Double:
            //    case TypeCode.Single:
            //        return true;
            //    default:
            //        return false;
            //}
        }

        public static bool IsNumericTypeAndNullable(Type type)
        {
            return IsNumericType(type) || IsNumericType(Nullable.GetUnderlyingType(type));
        }

        public static bool IsGenericList(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)));
        }

        public static bool IsGenericDictionary(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>)));
        }
    }
}