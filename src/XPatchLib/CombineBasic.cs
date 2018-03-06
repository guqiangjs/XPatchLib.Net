﻿// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Xml;

namespace XPatchLib
{
    /// <summary>
    ///     基础类型增量内容合并类。
    /// </summary>
    /// <seealso cref="XPatchLib.CombineBase" />
    internal class CombineBasic : CombineBase
    {
        /// <summary>
        ///     合并数据。
        /// </summary>
        /// <param name="pTypeCode">合并后的数据类型。</param>
        /// <param name="pIsGuid">是否为<see cref="Guid" />类型。</param>
        /// <param name="pMode">指定在字符串与 <see cref="T:System.DateTime" /> 之间转换时，如何处理时间值。</param>
        /// <param name="pValue">待合并的数据值。</param>
        /// <returns>返回合并后的数据。</returns>
        internal static Object CombineAction(TypeCode pTypeCode, bool pIsGuid, DateTimeSerializationMode pMode,
            string pValue)
        {
            switch (pTypeCode)
            {
                case TypeCode.Boolean:
                    return CombineBoolean(pValue);

                case TypeCode.Char:
                    return CombineChar(pValue);

                case TypeCode.SByte:
                    return CombineByte(pValue);

                case TypeCode.Byte:
                    return CombineUnsignedByte(pValue);

                case TypeCode.Int16:
                    return CombineShort(pValue);

                case TypeCode.UInt16:
                    return CombineUnsignedShort(pValue);

                case TypeCode.Int32:
                    return CombineInt32(pValue);

                case TypeCode.UInt32:
                    return CombineUnsignedInt32(pValue);

                case TypeCode.Int64:
                    return CombineLong(pValue);

                case TypeCode.UInt64:
                    return CombineUnsignedLong(pValue);

                case TypeCode.Single:
                    return CombineFloat(pValue);

                case TypeCode.Double:
                    return CombineDouble(pValue);

                case TypeCode.Decimal:
                    return CombineDecimal(pValue);

                case TypeCode.DateTime:
                    return CombineDateTime(pValue, pMode);

                case TypeCode.String:
                    return CombineString(pValue);

                case TypeCode.TimeSpan:
                    return CombineTimeSpan(pValue);

                case TypeCode.DateTimeOffset:
                    return CombineDateTimeOffset(pValue);

#if NET_40_UP || NETSTANDARD_1_1_UP
                case TypeCode.BigInteger:
                    return CombineBigInteger(pValue);
#endif

                case TypeCode.Uri:
                    return CombineUri(pValue);
            }
            if (pIsGuid)
                return CombineGuid(pValue);
            return null;
        }

        /// <summary>
        ///     根据增量内容创建基础类型实例。
        /// </summary>
        /// <param name="pReader">XML读取器。</param>
        /// <param name="pOriObject">现有待合并数据的对象。</param>
        /// <param name="pName">当前读取的内容名称。</param>
        /// <returns></returns>
        protected override object CombineAction(ITextReader pReader, object pOriObject, string pName)
        {
            return CombineAction(Type.TypeCode, Type.IsGuid, pReader.Setting.Mode, pReader.GetValue());
        }

#region Internal Constructors

        /// <summary>
        ///     使用指定的类型初始化 <see cref="XPatchLib.CombineBasic" /> 类的新实例。
        /// </summary>
        /// <param name="pType">
        ///     指定的类型。
        /// </param>
        internal CombineBasic(TypeExtend pType)
            : base(pType)
        {
        }

#endregion Internal Constructors

#region Private Methods

        private static object CombineBoolean(string pValue)
        {
            return bool.Parse(pValue);
            //return XmlConvert.ToBoolean(pValue);
        }

        private static object CombineByte(string pValue)
        {
            return sbyte.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return XmlConvert.ToSByte(pValue);
        }

        private static object CombineChar(string pValue)
        {
            return (char) ushort.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return (char)XmlConvert.ToUInt16(pValue);
        }

        private static object CombineDateTime(string pValue, DateTimeSerializationMode pMode)
        {
            return XmlConvert.ToDateTime(pValue, pMode.Convert());
        }

        private static object CombineDecimal(string pValue)
        {
            return decimal.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return XmlConvert.ToDecimal(pValue);
        }

        private static object CombineDouble(string pValue)
        {
            return double.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return XmlConvert.ToDouble(pValue);
        }

        private static object CombineFloat(string pValue)
        {
            return float.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return XmlConvert.ToSingle(pValue);
        }

        private static object CombineGuid(string pValue)
        {
#if (NET_40_UP || NETSTANDARD)
            return Guid.Parse(pValue);
#else
            return ConvertHelper.ConvertGuidFromString(pValue);
#endif
            //return XmlConvert.ToGuid(pValue);
        }

        private static object CombineInt32(string pValue)
        {
            return int.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return XmlConvert.ToInt32(pValue);
        }

        private static object CombineLong(string pValue)
        {
            return long.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return XmlConvert.ToInt64(pValue);
        }

        private static object CombineShort(string pValue)
        {
            return short.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return XmlConvert.ToInt16(pValue);
        }

        private static object CombineString(string pValue)
        {
            //return pValue.Replace("\n", "\r\n");
            return pValue;
        }

        private static object CombineUnsignedByte(string pValue)
        {
            return byte.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return XmlConvert.ToByte(pValue);
        }

        private static object CombineUnsignedInt32(string pValue)
        {
            return uint.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return XmlConvert.ToUInt32(pValue);
        }

        private static object CombineUnsignedLong(string pValue)
        {
            return ulong.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return XmlConvert.ToUInt64(pValue);
        }

        private static object CombineUnsignedShort(string pValue)
        {
            return ushort.Parse(pValue, NumberFormatInfo.InvariantInfo);
            //return XmlConvert.ToUInt16(pValue);
        }

        private static object CombineTimeSpan(string pValue)
        {
            return TimeSpan.Parse(pValue);
        }

        private static object CombineDateTimeOffset(string pValue)
        {
            return XmlConvert.ToDateTimeOffset(pValue);
        }

#if NET_40_UP || NETSTANDARD_1_1_UP
        private static object CombineBigInteger(string pValue)
        {
            return System.Numerics.BigInteger.Parse(pValue, CultureInfo.InvariantCulture);
        }
#endif

        private static object CombineUri(string pValue)
        {
            return new Uri(pValue);
        }

#endregion Private Methods
    }
}