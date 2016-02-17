// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace FastQuant
{
    public class NewsUrgency
    {
        public const byte Normal = 0;
        public const byte Flash = 1;
        public const byte Background = 2;
    }

    public class News : DataObject
    {
        internal int ProviderId { get; set; }

        internal int InstrumentId { get; set; }

        internal byte Urgency { get; set; }

        internal string Url { get; set; }

        internal  string Headline { get; set; }

        internal  string Text { get; set; }

        public override byte TypeId => DataObjectType.News;

        public News()
        {
        }

        public News(DateTime dateTime, int providerId, int instrumentId, byte urgency, string url, string headline, string text)
            : base(dateTime)
        {
            ProviderId = providerId;
            InstrumentId = instrumentId;
            Urgency = urgency;
            Url = url;
            Headline = headline;
            Text = text;
        }

        public override string ToString() => $"{Headline} : {Text}";
    }
}