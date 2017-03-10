﻿// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace XPatchLib
{
    /// <summary>
    ///     增量内容 XML 序列化器。
    /// </summary>
    /// <remarks>
    ///     <para> 在指定的两个同一类型的对象实例间增量的内容序列化为 XML 文档，也可以将增量的 XML 文档反序列化并附加至原始的对象实例上。 </para>
    ///     <para>
    ///         增量内容序列化是将两个同类型的对象实例的公共属性 (Property) 和字段之间的值差异进行比较后转换为序列格式（这里是指 XML）以便存储或传输的过程。
    ///         增量内容反序列化则是在原有对象实例的基础上附加 XML 输出的增量内容创建出序列化前的值相同的对象。
    ///     </para>
    ///     <para> 如果属性 (Property) 或字段返回一个复杂对象（如数组或类实例），则 XmlSerializer 将其转换为嵌套在主 XML 文档内的元素。 </para>
    ///     <para>
    ///         例如，以下代码中的第一个类返回第二个类的实例。
    ///         <code language="c#" source="..\..\XPatchLib.Example\CSharp\XmlSerializer\ClassRemark.cs" />
    ///         <para>
    ///             当原始对象的ObjectName的值为 "My String" ，更新后的对象将ObjectName的值设置为"My String New"时，序列化增量内容的 XML 输出如下所示：
    ///         </para>
    ///         <code language="xml" source="..\..\XPatchLib.Example\CSharp\XmlSerializer\ClassRemarkOutput.xml" />
    ///         <para> 当原始对象与更新后的对象的ObjectName的值相同时，输出空白内容。 </para>
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <para>
    ///         下面的示例包含两个主类：PurchaseOrder 和 Test。PurchaseOrder 类包含有关单个订单的信息。 Test
    ///         类包含创建两个不同内容的订单，对这两个订单之间进行内容比较，形成增量内容以及读取增量内容进行数据合并的方法。
    ///     </para>
    ///     <code language="c#" source="..\..\XPatchLib.Example\CSharp\XmlSerializer\ClassExample.cs" />
    ///     <para> 序列化增量内容的 XML 输出如下所示： </para>
    ///     <code language="xml" source="..\..\XPatchLib.Example\CSharp\XmlSerializer\ClassExampleOutPut.xml" />
    /// </example>
    public class XmlSerializer
    {
        #region Private Fields
        
        private readonly TypeExtend _type;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        ///     初始化 <c> XmlSerializer </c> 类的新实例。
        /// </summary>
        /// <param name="pType">
        ///     此 <see cref="XPatchLib.XmlSerializer" /> 可序列化的对象的类型。
        /// </param>
        /// <remarks>
        ///     默认在字符串与 System.DateTime 之间转换时，转换时应保留时区信息。
        ///     <para>
        ///         应用程序通常定义若干类。但是，XmlSerializer 只需知道一种类型，即表示 XML 根元素的类的类型。 XmlSerializer
        ///         自动序列化所有从属类的实例。 同样，反序列化仅需要 XML 根元素的类型。
        ///     </para>
        ///     <para> 默认不序列化默认值。 </para>
        /// </remarks>
        /// <example>
        ///     <para>
        ///         下面的示例构造 XmlSerializer，它在原始值为null和更新后名为 Widget 的简单对象之间产生增量内容。 该示例在调用 Divide 方法之前设置该对象的各种属性。
        ///     </para>
        ///     <code language="c#" source="..\..\XPatchLib.Example\CSharp\XmlSerializer\ConstructorExample.cs" />
        ///     <para> 序列化增量内容的 XML 输出如下所示： </para>
        ///     <code language="xml" source="..\..\XPatchLib.Example\CSharp\XmlSerializer\ConstructorExampleOutPut.xml" />
        /// </example>
        public XmlSerializer(Type pType)
        {
            TypeExtendContainer.Clear();
            _type = TypeExtendContainer.GetTypeExtend(pType, null);
        }

        #endregion Public Constructors

        #region Public Methods


        /// <summary>
        ///     反序列化指定 <see cref="System.Xml.XmlReader" /> 包含的 XML 增量文档，并与 原始对象 进行数据合并。
        /// </summary>
        /// <param name="pReader">
        ///     包含要反序列化的 XML 增量文档的 <see cref="System.Xml.XmlReader" />。
        /// </param>
        /// <param name="pOriValue">
        ///     待进行数据合并的原始对象。
        /// </param>
        /// <returns>
        ///     正被反序列化及合并后的 <see cref="System.Object" />。
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         <b> 默认不覆盖 <paramref name="pOriValue" /> 对象实例。 </b>
        ///     </para>
        ///     <para> 在反序列化及合并之前，必须使用待合并的对象的类型构造一个 <see cref="XPatchLib.XmlSerializer" /> 。 </para>
        /// </remarks>
        /// <example>
        ///     <para> 下面的示例使用 <c> TextReader </c> 对象反序列化增量内容，并附加至原始对象。 </para>
        ///     <code language="c#" source="..\..\XPatchLib.Example\CSharp\XmlSerializer\CombineXmlReaderExample.cs" />
        ///     <code language="xml" title="patch.xml"
        ///         source="..\..\XPatchLib.Example\CSharp\XmlSerializer\CombineXmlReaderExampleInPut.xml" />
        /// </example>
        public object Combine(ITextReader pReader, object pOriValue)
        {
            return Combine(pReader, pOriValue, false);
        }

        /// <summary>
        ///     以可指定是否覆盖原始对象的方式反序列化指定 <see cref="XmlReader" /> 包含的 XML 增量文档，并与 原始对象 进行数据合并。
        /// </summary>
        /// <param name="pReader">
        ///     包含要反序列化的 XML 增量文档的 <see cref="XmlReader" />。
        /// </param>
        /// <param name="pOriValue">
        ///     待进行数据合并的原始对象。
        /// </param>
        /// <param name="pOverride">
        ///     是否覆盖 <paramref name="pOriValue" /> 对象实例。
        /// </param>
        /// <returns>
        ///     正被反序列化及合并后的 <see cref="object" />。
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         设置参数 <paramref name="pOverride" /> 为 True 时，将比设置为 False 时，大幅提高序列化与反序列化性能。在无需保留 <paramref name="pOriValue" />
        ///         对象实例的情况下，建议使用 True 作为参数。
        ///     </para>
        ///     <para> 在反序列化及合并之前，必须使用待合并的对象的类型构造一个 <see cref="XmlSerializer" /> 。 </para>
        /// </remarks>
        /// <example>
        ///     <para> 下面的示例使用 <c> TextReader </c> 对象反序列化增量内容，并附加至原始对象。 </para>
        ///     <code language="c#"
        ///         source="..\..\XPatchLib.Example\CSharp\XmlSerializer\CombineXmlReaderOverrideExample.cs" />
        ///     <code language="xml" title="patch.xml"
        ///         source="..\..\XPatchLib.Example\CSharp\XmlSerializer\CombineXmlReaderExampleInPut.xml" />
        /// </example>
        [SuppressMessage("Microsoft.Usage", "CA2202:不要多次释放对象")]
        public object Combine(ITextReader pReader, object pOriValue, bool pOverride)
        {
            Guard.ArgumentNotNull(pReader, "pReader");

            object cloneObjValue = null;
            //当原始值不为Null时，需要先对原始值进行克隆，否则做数据合并时会侵入到原始数据
            if (pOriValue != null)
                if (pOverride)
                {
                    cloneObjValue = pOriValue;
                }
                else
                {
                    MemoryStream stream = null;
                    try
                    {
                        stream = new MemoryStream();
                        var settings = new XmlWriterSettings();
                        settings.ConformanceLevel = ConformanceLevel.Fragment;
                        settings.Indent = true;
                        settings.Encoding = Encoding.UTF8;
                        settings.OmitXmlDeclaration = false;
                        using (var xmlWriter = XmlWriter.Create(stream, settings))
                        {
                            ITextWriter writer = new XmlTextWriter(xmlWriter);
                            new DivideCore(writer, _type).Divide(_type.TypeFriendlyName, null, pOriValue);
                        }
#if DEBUG
                        stream.Position = 0;
                        XElement ele = XElement.Load(stream);
#endif
                        stream.Position = 0;
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            cloneObjValue = new CombineCore(_type).Combine(new XmlTextReader(reader), null, _type.TypeFriendlyName);
                        }
                    }
                    finally
                    {
                        if (stream != null)
                            stream.Dispose();
                    }
                }
            else
                cloneObjValue = _type.CreateInstance();

            //var ele = XElement.Load(pReader, LoadOptions.None);
            return new CombineCore(_type).Combine(pReader, cloneObjValue, _type.TypeFriendlyName);
        }

        public void Divide(ITextWriter pWriter, object pOriValue, object pRevValue)
        {
            Guard.ArgumentNotNull(pWriter, "pWriter");

            pWriter.WriteStartDocument();
            if (new DivideCore(pWriter, _type).Divide(_type.TypeFriendlyName,
                pOriValue, pRevValue))
                pWriter.WriteEndDocument();
            pWriter.Flush();
        }

        /// <summary>
        ///     向 <c> XmlSerializer </c> 注册类型与主键集合的键值对集合。
        /// </summary>
        /// <param name="pTypes">
        ///     类型与主键集合的键值对集合。
        /// </param>
        /// <remarks>
        ///     在无法修改类型定义，为其增加或修改 <see cref="XPatchLib.PrimaryKeyAttribute" /> 的情况下， 可以在调用
        ///     <c>
        ///         Divide
        ///     </c>
        ///     或 <c> Combine </c> 方法前，调用此方法，传入需要修改的Type及与其对应的主键名称集合。 系统在处理时会按照传入的设置进行处理。
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     当参数 <paramref name="pTypes" /> is null 时。
        /// </exception>
        /// <example>
        ///     <para>
        ///         下面的示例使用 RegisterTypes 方法向 <see cref="XPatchLib.XmlSerializer" /> 注册待处理的类型的主键信息 。
        ///     </para>
        ///     <code language="c#" source="..\..\XPatchLib.Example\CSharp\XmlSerializer\RegisteTypesExample.cs" />
        ///     <para> 序列化增量内容的 XML 输出如下所示： </para>
        ///     <code language="xml" title="patch.xml"
        ///         source="..\..\XPatchLib.Example\CSharp\XmlSerializer\RegisteTypesExampleOutPut.xml" />
        /// </example>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void RegisterTypes(IDictionary<Type, string[]> pTypes)
        {
            Guard.ArgumentNotNull(pTypes, "pTypes");

            ReflectionUtils.RegisterTypes(pTypes);
        }

        #endregion Public Methods
    }
}