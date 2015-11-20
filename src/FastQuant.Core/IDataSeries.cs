// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public enum SearchOption
    {
        Next,
        Prev,
        ExactFirst,
        ExactLast
    }

    public interface IDataSeries
    {
        long Count { get; }

        string Name { get; }

        string Description { get; }

        DateTime DateTime1 { get; }

        DateTime DateTime2 { get; }

        DataObject this[long index] { get; }

        long GetIndex(DateTime dateTime, SearchOption option = SearchOption.Prev);

        bool Contains(DateTime dateTime);

        void Add(DataObject obj);

        void Remove(long index);

        void Clear();
    }
}

