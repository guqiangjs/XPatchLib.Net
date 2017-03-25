﻿// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace XPatchLib
{
    internal static class ClrHelper
    {
        private static readonly Type[] GetValueParameterTypes = {typeof(object)};

        private static readonly Type[] SetValueParameterTypes = {typeof(object), typeof(object)};

        public static Func<Object> CreateInstanceFunc(Type pType)
        {
            ConstructorInfo emptyConstructor = pType.GetConstructor(Type.EmptyTypes);
            if (emptyConstructor == null)
                return null;
            var dynamicMethod = new DynamicMethod("CreateInstance", pType, Type.EmptyTypes, true);
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Newobj, emptyConstructor);
            ilGenerator.Emit(OpCodes.Ret);
            return (Func<object>) dynamicMethod.CreateDelegate(typeof(Func<object>));
        }

        /// <summary>
        /// </summary>
        /// <param name="pProperty">
        /// </param>
        /// <returns>
        /// </returns>
        public static Func<Object, Object> GetValueFunc(this PropertyInfo pProperty)
        {
            try
            {
                Debug.Assert(pProperty.ReflectedType != null, "pProperty.ReflectedType != null");
                if (!pProperty.PropertyType.IsPublic || !pProperty.ReflectedType.IsPublic)
                    return null;

                MethodInfo getMethod = pProperty.GetGetMethod(false);
                if (getMethod == null || !getMethod.IsPublic)
                    return null;

                DynamicMethod dynamicGetMethod = new DynamicMethod("GetValue", typeof(object), GetValueParameterTypes,
                    true);
                ILGenerator ilGenerator = dynamicGetMethod.GetILGenerator();
                ilGenerator.Emit(OpCodes.Ldarg_0);
                Debug.Assert(pProperty.DeclaringType != null, "pProperty.DeclaringType != null");
                ilGenerator.Emit(OpCodes.Castclass, pProperty.DeclaringType);
                ilGenerator.Emit(OpCodes.Callvirt, getMethod);
                if (pProperty.PropertyType.IsValueType)
                    ilGenerator.Emit(OpCodes.Box, pProperty.PropertyType);
                ilGenerator.Emit(OpCodes.Ret);
                return (Func<Object, Object>) dynamicGetMethod.CreateDelegate(typeof(Func<Object, Object>));
            }
            catch (InvalidProgramException)
            {
                return null;
            }
        }

        public static Func<Object, Object> GetValueFunc(this FieldInfo fieldInfo)
        {
            try
            {
                //if (!fieldInfo.IsPublic) return null;
                var instance = Expression.Parameter(typeof(Object), "i");
                Debug.Assert(fieldInfo.DeclaringType != null, "fieldInfo.DeclaringType != null");
                var castedInstance = Expression.ConvertChecked
                    (instance, fieldInfo.DeclaringType);
                var field = Expression.Field(castedInstance, fieldInfo);
                var convert = Expression.Convert(field, typeof(Object));
                var expression = Expression.Lambda(convert, instance);
                return (Func<Object, Object>) expression.Compile();
            }
            catch (InvalidProgramException)
            {
                return null;
            }
        }

        public static Action<object, object> SetValueFunc(this PropertyInfo pProperty)
        {
            try
            {
                Debug.Assert(pProperty.ReflectedType != null, "pProperty.ReflectedType != null");
                if (!pProperty.PropertyType.IsPublic || !pProperty.ReflectedType.IsPublic)
                    return null;

                MethodInfo setMethod = pProperty.GetSetMethod(false);
                if (setMethod == null || !setMethod.IsPublic)
                    return null;

                DynamicMethod dynamicSetMethod = new DynamicMethod("SetValue", typeof(void), SetValueParameterTypes);
                ILGenerator ilGenerator = dynamicSetMethod.GetILGenerator();
                ilGenerator.Emit(OpCodes.Ldarg_0);
                Debug.Assert(pProperty.DeclaringType != null, "pProperty.DeclaringType != null");
                ilGenerator.Emit(OpCodes.Castclass, pProperty.DeclaringType);
                ilGenerator.Emit(OpCodes.Ldarg_1);
                if (pProperty.PropertyType.IsValueType)
                    ilGenerator.Emit(OpCodes.Unbox_Any, pProperty.PropertyType);
                else
                    ilGenerator.Emit(OpCodes.Castclass, pProperty.PropertyType);
                ilGenerator.Emit(OpCodes.Callvirt, setMethod);
                ilGenerator.Emit(OpCodes.Ret);

                return (Action<object, object>) dynamicSetMethod.CreateDelegate(typeof(Action<object, object>));
            }
            catch (InvalidProgramException)
            {
                return null;
            }
        }

        public static Action<Object, Object> SetValueFunc(this FieldInfo fieldInfo)
        {
            try
            {
                if (!fieldInfo.IsPublic || fieldInfo.IsInitOnly) return null;
                var instance = Expression.Parameter(typeof(Object), "i");
                Debug.Assert(fieldInfo.DeclaringType != null, "fieldInfo.DeclaringType != null");
                var castedInstance = Expression.ConvertChecked
                    (instance, fieldInfo.DeclaringType);
                var argument = Expression.Parameter(typeof(Object), "a");
                var setter = Expression.Assign(
                    Expression.Field(castedInstance, fieldInfo),
                    Expression.Convert(argument, fieldInfo.FieldType));
                return Expression.Lambda<Action<object, Object>>
                    (setter, instance, argument).Compile();
            }
            catch (InvalidProgramException)
            {
                return null;
            }
        }
    }
}