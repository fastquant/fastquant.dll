using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuant
{
    public class FillSeries : IEnumerable<Fill>
    {
        private List<Fill> fills = new List<Fill>();
        private Fill max;
        private Fill min;
        private string name;

        public Fill Max => this.max;

        public Fill Min => this.min;

        public int Count => this.fills.Count;

        public Fill this[int index] => this.fills[index];

        public FillSeries(string name = "")
        {
            this.name = name;
        }

        public void Add(Fill fill)
        {
            if (this.min == null)
            {
                this.min = fill;
            }
            else if (fill.Price < this.min.Price)
            {
                this.min = fill;
            }
            if (this.max == null)
            {
                this.max = fill;
            }
            else if (fill.Price > this.max.Price)
            {
                this.max = fill;
            }
            if (this.fills.Count != 0 && fill.DateTime < this.fills[this.fills.Count - 1].DateTime)
            {
                Console.WriteLine($"FillSeries::Add ({this.name} + incorrect fill order : {fill}");
            }
            this.fills.Add(fill);
        }

        public void Clear()
        {
            this.fills.Clear();
            this.max = this.min = null;
        }

        // TODO: rewrite it
        public int GetIndex(DateTime datetime, IndexOption option)
        {
            int num = 0;
            int num2 = 0;
            int num3 = this.fills.Count - 1;
            bool flag = true;
            while (flag)
            {
                if (num3 < num2)
                {
                    return -1;
                }
                num = (num2 + num3) / 2;
                switch (option)
                {
                    case IndexOption.Null:
                        if (this.fills[num].dateTime == datetime)
                        {
                            flag = false;
                        }
                        else if (this.fills[num].dateTime > datetime)
                        {
                            num3 = num - 1;
                        }
                        else if (this.fills[num].dateTime < datetime)
                        {
                            num2 = num + 1;
                        }
                        break;
                    case IndexOption.Next:
                        if (this.fills[num].dateTime >= datetime && (num == 0 || this.fills[num - 1].dateTime < datetime))
                        {
                            flag = false;
                        }
                        else if (this.fills[num].dateTime < datetime)
                        {
                            num2 = num + 1;
                        }
                        else
                        {
                            num3 = num - 1;
                        }
                        break;
                    case IndexOption.Prev:
                        if (this.fills[num].dateTime <= datetime && (num == this.fills.Count - 1 || this.fills[num + 1].dateTime > datetime))
                        {
                            flag = false;
                        }
                        else if (this.fills[num].dateTime > datetime)
                        {
                            num3 = num - 1;
                        }
                        else
                        {
                            num2 = num + 1;
                        }
                        break;
                }
            }
            return num;
        }

        public IEnumerator<Fill> GetEnumerator() => this.fills.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}