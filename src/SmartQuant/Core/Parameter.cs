// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SmartQuant
{
    public class Parameter
    {
        public string Name { get; }

        public object Value { get; }

        public string TypeName { get; }

        public Attribute[] Attributes { get; }

        public Parameter(string name, object value)
        {
            Name = name;
            Value = value;
            Attributes = new Attribute[0];
        }

        public Parameter(string name, object value, string typeName,  Attribute[] attributes)
        {
            Name = name;
            Value = value;
            TypeName = typeName;
            Attributes = attributes;
        }

        public override string ToString() => $"{Name} = {Value}";
    }

    public class ParameterAttribute : Attribute
    {
    }

    public class ParameterList : DataObject
    {
        private List<Parameter> parameters = new List<Parameter>();

        public List<string> Methods { get; } = new List<string>();

        public string Name { get; set; }

        public Parameter this[int index]
        {
            get
            {
                return this.parameters[index];
            }
            set
            {
                this.parameters[index] = value;
            }
        }

        public override byte TypeId => DataObjectType.ParameterList;

        public int Count() => this.parameters.Count;

        public void Add(Parameter parameter) => this.parameters.Add(parameter);

        public void Remove(Parameter parameter) => this.parameters.Remove(parameter);

        public void Clear() => this.parameters.Clear();

        public List<Parameter> Parameters() => this.parameters;

        public IEnumerator<Parameter> GetEnumerator() => this.parameters.GetEnumerator();
    }

    public class ParameterHelper
    {
        public ParameterList Aggregate(ParameterList list1, ParameterList list2)
        {
            var list = new ParameterList();
            list.Name = list1.Name;

            foreach (var p in list1.Parameters())
                list.Add(p);

            foreach (var p in list2.Parameters())
                list.Add(p);

            foreach (var m in list1.Methods)
                list.Methods.Add(m);

            foreach (var m in list2.Methods)
                list.Methods.Add(m);

            return list;
        }

        public object GetParameter(string parameterName, object obj)
        {
            var field = obj.GetType().GetField(parameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var property = obj.GetType().GetProperty(parameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(obj) ?? property?.GetValue(obj);
        }

        public ParameterList GetParameters(string name, object obj)
        {
            var parameterList = new ParameterList() {Name = name};
            var pred = new Predicate<Attribute>(a =>
            {
                var t = a.GetType();
                return t.FullName.Contains("ComponentModel") && t.Name != "PropertyTabAttribute" && t.Name != "ToolboxItemAttribute";
            });
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var array = properties;
            for (int i = 0; i < array.Length; i++)
            {
                PropertyInfo propertyInfo = array[i];
                if (propertyInfo.CanRead && propertyInfo.GetCustomAttribute(typeof(TypeConverterAttribute)) == null)
                {
                    List<Attribute> list = new List<Attribute>();
                    foreach (Attribute current in propertyInfo.GetCustomAttributes())
                    {
                        if (current.GetType().FullName.Contains("ComponentModel") && current.GetType().Name != "PropertyTabAttribute" && current.GetType().Name != "ToolboxItemAttribute")
                        {
                            list.Add(current);
                        }
                    }
                    if (!propertyInfo.CanWrite)
                    {
                        ReadOnlyAttribute item = new ReadOnlyAttribute(true);
                        if (!list.Contains(item))
                        {
                            list.Add(item);
                        }
                    }
                    string assemblyQualifiedName = propertyInfo.PropertyType.AssemblyQualifiedName;
                    parameterList.Add(new Parameter(propertyInfo.Name, propertyInfo.GetValue(obj), assemblyQualifiedName, list.ToArray()));
                }
            }
            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).TakeWhile(f => f.GetCustomAttribute(typeof(TypeConverterAttribute)) == null);
            foreach (var param in fields.Select(f => new Parameter(f.Name, f.GetValue(obj), f.FieldType.AssemblyQualifiedName, f.GetCustomAttributes().TakeWhile(a => pred(a)).ToArray())))
                parameterList.Add(param);

            return parameterList;
        }

        public object GetStrategyParameter(string parameterName, object obj)
        {
            var field = obj.GetType().GetField(parameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var property = obj.GetType().GetProperty(parameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field != null && field.GetCustomAttributes(typeof(ParameterAttribute), true).Any())
                return field.GetValue(obj);

            if (property != null && property.GetCustomAttributes(typeof(ParameterAttribute), true).Any())
                return property.GetValue(obj);

            return null;
        }

        public ParameterList GetStrategyParameters(string name, object obj)
        {
            ParameterList list = new ParameterList();
            list.Name = name;
            var pred = new Predicate<Attribute>(a =>
            {
                var t = a.GetType();
                return t.FullName.Contains("ComponentModel") && t.Name != "PropertyTabAttribute" && t.Name != "ToolboxItemAttribute";
            });
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).TakeWhile(p => p.GetCustomAttributes(typeof(ParameterAttribute), true).Any());
            foreach(var param in properties.Select(p => new Parameter(p.Name, p.GetValue(obj), p.PropertyType.AssemblyQualifiedName, p.GetCustomAttributes().TakeWhile(a => pred(a)).ToArray())))
                list.Add(param);

            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).TakeWhile(f => f.GetCustomAttributes(typeof(ParameterAttribute), true).Any());
            foreach (var param in fields.Select(f => new Parameter(f.Name, f.GetValue(obj), f.FieldType.AssemblyQualifiedName, f.GetCustomAttributes().TakeWhile(a => pred(a)).ToArray())))
                list.Add(param);

            var methods = obj.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).TakeWhile(m => m.GetCustomAttributes(typeof (StrategyMethodAttribute), true).Any() && m.GetParameters().Length == 0);
            foreach (var m in methods)
                list.Methods.Add(m.Name);

            return list;
        }

        public void SetParameter(string parameterName, object obj, object value)
        {
            var field = obj.GetType().GetField(parameterName, BindingFlags.Instance | BindingFlags.Public);
            var property = obj.GetType().GetProperty(parameterName, BindingFlags.Instance | BindingFlags.Public);
            if (field != null)
                field.SetValue(obj, value);
            else if (property != null && property.CanWrite)
                property.SetValue(obj, value);
        }

        public void SetParameters(ParameterList parameters, object obj)
        {
            foreach (Parameter p in parameters)
                SetParameter(p.Name, obj, p.Value);
        }

        public void SetStrategyParameter(string parameterName, object obj, object value)
        {
            var field = obj.GetType().GetField(parameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var property = obj.GetType().GetProperty(parameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.GetCustomAttributes(typeof(ParameterAttribute), true).Any())
                field.SetValue(obj, value);
            else if (property != null && property.GetCustomAttributes(typeof(ParameterAttribute), true).Any())
                property.SetValue(obj, value);
        }

        public void SetStrategyParameters(ParameterList parameters, object obj)
        {
            foreach (var p in parameters)
                SetStrategyParameter(p.Name, obj, p.Value);
        }
    }
}
