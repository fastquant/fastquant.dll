// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System;
using System.Collections.Generic;

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

        private List<string> methods;

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

        public int Count()
        {
            return this.parameters.Count;
        }

        public void Add(Parameter parameter)
        {
            this.parameters.Add(parameter);
        }

        public void Remove(Parameter parameter)
        {
            this.parameters.Remove(parameter);
        }

        public void Clear()
        {
            this.parameters.Clear();
        }

        public List<Parameter> Parameters()
        {
            return this.parameters;
        }

        public IEnumerator<Parameter> GetEnumerator()
        {
            return this.parameters.GetEnumerator();
        }
    }
}
