using System;

namespace FastQuant
{
    public class Output : Event
    {
        public string Text { get; }

        public Output(DateTime dateTime, string text):base(dateTime)
        {
            Text = text;
        }

        public override byte TypeId => EventType.Output;
    }
}
