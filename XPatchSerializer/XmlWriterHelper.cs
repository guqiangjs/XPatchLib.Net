﻿// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System.Xml;

namespace XPatchLib
{
    /// <summary>
    /// XML写入器帮助类。
    /// </summary>
    internal static class XmlWriterHelper
    {
        /// <summary>
        /// 写入 Action 特性。
        /// </summary>
        /// <param name="pWriter">XML写入器。</param>
        /// <param name="pAction">带写入的Action标记。</param>
        /// <remarks>
        /// 当 <paramref name="pAction" />==<see cref="Action" />时不写入。
        /// </remarks>
        internal static void WriteActionAttribute(this XmlWriter pWriter, Action pAction)
        {
            if (pAction != Action.Edit)
            {
                pWriter.WriteAttributeString(ConstValue.ACTION_NAME, pAction.ToString());
            }
        }
    }
}