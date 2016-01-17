using System;

namespace SmartQuant
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
