﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Lunggo.Framework.Util
{
    public static class TypeUtil
    {
        public static dynamic ToAnonymousType(Object source)
        {
            return CreateExpandableObject(source); 
        }

        public static dynamic ToAnonymousType(Object source, Func<PropertyInfo, bool> typeFilter)
        {
            return CreateExpandableObject(source, typeFilter);
        }

        public static dynamic ToAnonymousType(Object source, IEnumerable<string> propertyNames)
        {
            return CreateExpandableObject(source, propertyNames);
        }

        private static ExpandoObject CreateExpandableObject(Object source, Func<PropertyInfo, bool> typeFilter = null)
        {
            ExpandoObject expandableObject = new ExpandoObject();
            FillExpandableObject(expandableObject, source,typeFilter);
            return expandableObject;
        }

        private static ExpandoObject CreateExpandableObject(Object source, IEnumerable<string> propertyNames)
        {
            ExpandoObject expandableObject = new ExpandoObject();
            FillExpandableObject(expandableObject, source, propertyNames);
            return expandableObject;
        }

        private static void FillExpandableObject(ExpandoObject expandableObject, Object source, IEnumerable<string> propertyNames)
        {
            IDictionary<string, object> expandableObjectAsDictionary = expandableObject;
            foreach(var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName);
                expandableObjectAsDictionary.Add(property.Name, property.GetValue(source));
            }
        }

        private static void FillExpandableObject(ExpandoObject expandableObject, Object source, Func<PropertyInfo, bool> typeFilter)
        {
            IDictionary<string, object> expandableObjectAsDictionary = expandableObject;
            foreach (var property in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if(typeFilter != null)
                {
                    if(typeFilter(property))
                    {
                        expandableObjectAsDictionary.Add(property.Name, property.GetValue(source));
                    }
                }
                else
                {
                    expandableObjectAsDictionary.Add(property.Name, property.GetValue(source));
                }
            }
        }
    }
}
