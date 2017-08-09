﻿// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
#if (NET_35_UP || NETSTANDARD)
using System.Linq;
#endif

namespace XPatchLib
{
    /// <summary>
    ///     字典类型增量内容产生类。
    /// </summary>
    /// <seealso cref="XPatchLib.DivideBase" />
    internal class DivideIDictionary : DivideBase
    {
        #region Internal Constructors

        /// <summary>
        ///     使用指定的类型初始化 <see cref="XPatchLib.DivideIEnumerable" /> 类的新实例。
        /// </summary>
        /// <param name="pWriter">写入器。</param>
        /// <param name="pType">指定的类型。</param>
        /// <exception cref="PrimaryKeyException">默认在字符串与System.DateTime 之间转换时，转换时应保留时区信息。</exception>
        /// <exception cref="ArgumentException">待处理的类型不是字典类型时。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="pType" /> 上无法获取元素类型时。</exception>
        internal DivideIDictionary(ITextWriter pWriter, TypeExtend pType)
            : base(pWriter, pType)
        {
            if (!Type.IsIDictionary)
                throw new ArgumentException("类型需要是字典类型");
            Type t = null;
            if (ReflectionUtils.TryGetIEnumerableGenericArgument(pType.OriType, out t))
                GenericArgumentType =
                    TypeExtendContainer.GetTypeExtend(pWriter.Setting, t, Writer.IgnoreAttributeType, pType);
            else
                throw new ArgumentOutOfRangeException(pType.OriType.FullName);
        }

        #endregion Internal Constructors

        #region Protected Properties

        /// <summary>
        ///     集合类型中元素的类型。
        /// </summary>
        protected TypeExtend GenericArgumentType { get; private set; }

        #endregion Protected Properties

        /// <summary>
        ///     产生增量内容的实际方法。
        /// </summary>
        /// <param name="pName">增量内容对象的名称。</param>
        /// <param name="pOriObject">原始对象。</param>
        /// <param name="pRevObject">更新后的对象。</param>
        /// <param name="pAttach">生成增量时可能用到的附件。</param>
        /// <returns>
        ///     返回是否成功写入内容。如果成功写入返回 <c>true</c> ，否则返回 <c>false</c> 。
        /// </returns>
        protected override bool DivideAction(string pName, object pOriObject, object pRevObject,
            DivideAttachment pAttach = null)
        {
            Boolean result;

            IEnumerable pOriItems = pOriObject as IEnumerable;
            IEnumerable pRevItems = pRevObject as IEnumerable;

            KeyValuesObject[] oriKey = Translate(pOriItems, Writer.Setting);
            KeyValuesObject[] revKey = Translate(pRevItems, Writer.Setting);
            if (pAttach == null)
                pAttach = new DivideAttachment();
            //将当前节点加入附件中，如果遇到子节点被写入前，会首先根据队列先进先出写入附件中的节点的开始标记
            pAttach.ParentQuere.Enqueue(new ParentObject(pName, pOriObject, Type));
            //顺序处理集合的删除、编辑、添加操作（顺序不能错）

            #region 处理删除

            bool removeItemsResult = DivideItems(oriKey, revKey, Action.Remove, pAttach);

            //只要有一个子节点写入成功，那么整个节点就是写入成功的
            result = removeItemsResult;

            #endregion 处理删除

            #region 处理编辑

            bool editItemsResult = DivideItems(oriKey, revKey, Action.Edit, pAttach);
            if (!result)
                result = editItemsResult;

            #endregion 处理编辑

            #region 处理新增

            bool addItemsResult = DivideItems(oriKey, revKey, Action.Add, pAttach);
            if (!result)
                result = addItemsResult;

            #endregion 处理新增

            return result;
        }

        #region Private Methods

        private static KeyValuesObject[] Translate(IEnumerable pValue,ISerializeSetting pSetting)
        {
            if (pValue != null)
            {
                Queue<KeyValuesObject> result = new Queue<KeyValuesObject>();

                IEnumerator enumerator = pValue.GetEnumerator();
                if (enumerator != null)
                    while (enumerator.MoveNext())
                    {
                        Object key =
                            enumerator.Current.GetType().GetProperty(ConstValue.KEY).GetValue(enumerator.Current, null);

                        result.Enqueue(new KeyValuesObject(enumerator.Current, key, pSetting));
                    }
                return result.ToArray();
            }
            return null;
        }

        /// <summary>
        ///     尝试比较原始集合和更新后的集合，找到被添加的元素集合。
        /// </summary>
        /// <param name="pOriItems">原始集合。</param>
        /// <param name="pRevItems">更新后的集合。</param>
        /// <param name="pFoundItems">找到的被添加的元素集合。</param>
        /// <returns>
        ///     当找到一个或多个被添加的元素时，返回 true 否则 返回 false 。
        /// </returns>
        private static Boolean TryGetAddedItems(KeyValuesObject[] pOriItems,
            KeyValuesObject[] pRevItems, out IEnumerable<KeyValuesObject> pFoundItems)
        {
            //查找存在于更新后的集合中但是不存在于原始集合中的元素。
            if (pOriItems == null)
                pFoundItems = pRevItems;
            else if (pRevItems == null)
                pFoundItems = null;
            else
                pFoundItems = pRevItems.Except(pOriItems, new KeyValuesObjectEqualityComparer());

            return pFoundItems != null;
        }

        /// <summary>
        ///     尝试比较原始集合和更新后的集合，找到可能被修改的元素集合。 (交集）
        /// </summary>
        /// <param name="pOriItems">原始集合。</param>
        /// <param name="pRevItems">更新后的集合。</param>
        /// <param name="pFoundItems">找到的被修改的元素集合。</param>
        /// <returns>
        ///     当找到一个或多个被修改的元素时，返回 true 否则 返回 false 。
        /// </returns>
        /// <remarks>
        ///     返回的集合是即存在于原始集合又存在于更新后集合的对象。
        /// </remarks>
        private static Boolean TryGetEditedItems(KeyValuesObject[] pOriItems,
            KeyValuesObject[] pRevItems, out IEnumerable<KeyValuesObject> pFoundItems)
        {
            pFoundItems = null;
            if (pOriItems != null && pRevItems != null)
                pFoundItems = pRevItems.Intersect(pOriItems, new KeyValuesObjectEqualityComparer());

            return pFoundItems != null;
        }

        /// <summary>
        ///     尝试比较原始集合和更新后的集合，找到被删除的元素集合。
        /// </summary>
        /// <param name="pOriItems">原始集合。</param>
        /// <param name="pRevItems">更新后的集合。</param>
        /// <param name="pFoundItems">找到的被删除的元素集合。</param>
        /// <returns>
        ///     当找到一个或多个被删除的元素时，返回 true 否则 返回 false 。
        /// </returns>
        private static Boolean TryGetRemovedItems(KeyValuesObject[] pOriItems,
            KeyValuesObject[] pRevItems, out IEnumerable<KeyValuesObject> pFoundItems)
        {
            //查找存在于原始集合中但是不存在于更新后的集合中的元素。
            //pFoundItems = pRevItems.Except(pOriItems, GenericArgumentType, GenericArgumentPrimaryKeys);
            if (pRevItems == null)
                pFoundItems = pOriItems;
            else if (pOriItems == null)
                pFoundItems = null;
            else
                pFoundItems = pOriItems.Except(pRevItems, new KeyValuesObjectEqualityComparer());
            return pFoundItems != null;
        }

        /// <summary>
        ///     按照传入的操作方式产生集合类型增量内容。
        /// </summary>
        /// <param name="pOriItems">原始集合。</param>
        /// <param name="pRevItems">更新后的集合。</param>
        /// <param name="pAction">操作方式。</param>
        /// <param name="pAttach">The p attach.</param>
        private Boolean DivideItems(KeyValuesObject[] pOriItems, KeyValuesObject[] pRevItems,
            Action pAction, DivideAttachment pAttach)
        {
            IEnumerable<KeyValuesObject> pFoundItems = null;

            //查找符合指定操作方式的元素。
            Boolean found = false;
            switch (pAction)
            {
                case Action.Add:
                    found = TryGetAddedItems(pOriItems, pRevItems, out pFoundItems);
                    break;

                case Action.Remove:
                    found = TryGetRemovedItems(pOriItems, pRevItems, out pFoundItems);
                    break;

                case Action.Edit:
                    found = TryGetEditedItems(pOriItems, pRevItems, out pFoundItems);
                    break;
            }

            Boolean result = false;

            //找到待处理的元素集合时
            if (found)
            {
                IEnumerator<KeyValuesObject> items = pFoundItems.GetEnumerator();

                //开始遍历待处理的元素集合中的所有元素

                //元素的类型未知，所以再次创建DivideCore实例，由此实例创建元素的增量结果。（递归方式）
                DivideKeyValuePair ser = new DivideKeyValuePair(Writer, GenericArgumentType);
                while (items.MoveNext())
                {
                    pAttach.CurrentAction = pAction;
                    //当前被处理的元素的增量内容数据对象
                    if (pAction == Action.Add)
                    {
                        //当前元素是新增操作时
                        //再次调用DivideCore.Divide的方法，传入空的原始对象，生成新增的增量节点。
                        KeyValuesObject obj = pRevItems.FirstOrDefault(x => x.Equals(items.Current));
                        bool itemResult = ser.Divide(GenericArgumentType.TypeFriendlyName, null, obj.OriValue,
                            pAttach);
                        if (!result)
                            result = itemResult;
                    }
                    else if (pAction == Action.Remove)
                    {
                        //当前元素是删除操作时
                        //再次调用DivideCore.Divide的方法，传入空的更新后对象，生成删除的增量节点。
                        KeyValuesObject obj = pOriItems.FirstOrDefault(x => x.Equals(items.Current));

                        if (pAttach == null)
                            pAttach = new DivideAttachment();
                        //pAttach.ParentQuere.Enqueue(new ParentObject(GenericArgumentType.TypeFriendlyName, GenericArgumentType) { Action = Action.Remove });
                        bool itemResult = ser.Divide(GenericArgumentType.TypeFriendlyName, obj.OriValue, null,
                            pAttach);
                        if (!result)
                            result = itemResult;
                    }
                    else if (pAction == Action.Edit)
                    {
                        //将此元素与当前正在遍历的元素作为参数调用序列化，看是否产生增量内容内容（如果没有产生增量内容内容则说明两个对象需要序列化的内容完全一样）
                        KeyValuesObject oldObj = pOriItems.FirstOrDefault(x => x.Equals(items.Current));
                        KeyValuesObject newObj = pRevItems.FirstOrDefault(x => x.Equals(items.Current));
                        bool itemResult = ser.Divide(GenericArgumentType.TypeFriendlyName, oldObj.OriValue,
                            newObj.OriValue,
                            pAttach);
                        if (!result)
                            result = itemResult;
                    }
                }
            }
            return result;
        }

        #endregion Private Methods
    }
}