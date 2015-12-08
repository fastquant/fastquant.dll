// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace SmartQuant
{
    public interface ICurrencyConverter
    {
        double Convert(double amount, byte fromCurrencyId, byte toCurrencyId);
    }

    public class CurrencyId
    {
        public const byte AED = 1;
        public const byte AFN = 2;
        public const byte ALL = 3;
        public const byte AMD = 4;
        public const byte ANG = 5;
        public const byte AOA = 6;
        public const byte ARS = 7;
        public const byte AUD = 8;
        public const byte AWG = 9;
        public const byte AZN = 10;
        public const byte BAM = 11;
        public const byte BBD = 12;
        public const byte BDT = 13;
        public const byte BGN = 14;
        public const byte BHD = 15;
        public const byte BIF = 16;
        public const byte BMD = 17;
        public const byte BND = 18;
        public const byte BOB = 19;
        public const byte BRL = 20;
        public const byte BSD = 21;
        public const byte BTN = 22;
        public const byte BWP = 23;
        public const byte BYR = 24;
        public const byte BZD = 25;
        public const byte CAD = 26;
        public const byte CDF = 27;
        public const byte CHF = 28;
        public const byte CLP = 29;
        public const byte CNY = 30;
        public const byte COP = 31;
        public const byte CRC = 32;
        public const byte CUC = 33;
        public const byte CUP = 34;
        public const byte CVE = 35;
        public const byte CZK = 36;
        public const byte DJF = 37;
        public const byte DKK = 38;
        public const byte DOP = 39;
        public const byte DZD = 40;
        public const byte EGP = 41;
        public const byte ERN = 42;
        public const byte ETB = 43;
        public const byte EUR = 44;
        public const byte FJD = 45;
        public const byte FKP = 46;
        public const byte GBP = 47;
        public const byte GEL = 48;
        public const byte GGP = 49;
        public const byte GHS = 50;
        public const byte GIP = 51;
        public const byte GMD = 52;
        public const byte GNF = 53;
        public const byte GTQ = 54;
        public const byte GYD = 55;
        public const byte HKD = 56;
        public const byte HNL = 57;
        public const byte HRK = 58;
        public const byte HTG = 59;
        public const byte HUF = 60;
        public const byte IDR = 61;
        public const byte ILS = 62;
        public const byte IMP = 63;
        public const byte INR = 64;
        public const byte IQD = 65;
        public const byte IRR = 66;
        public const byte ISK = 67;
        public const byte JEP = 68;
        public const byte JMD = 69;
        public const byte JOD = 70;
        public const byte JPY = 71;
        public const byte KES = 72;
        public const byte KGS = 73;
        public const byte KHR = 74;
        public const byte KMF = 75;
        public const byte KPW = 76;
        public const byte KRW = 77;
        public const byte KWD = 78;
        public const byte KYD = 79;
        public const byte KZT = 80;
        public const byte LAK = 81;
        public const byte LBP = 82;
        public const byte LKR = 83;
        public const byte LRD = 84;
        public const byte LSL = 85;
        public const byte LTL = 86;
        public const byte LYD = 87;
        public const byte MAD = 88;
        public const byte MDL = 89;
        public const byte MGA = 90;
        public const byte MKD = 91;
        public const byte MMK = 92;
        public const byte MNT = 93;
        public const byte MOP = 94;
        public const byte MRO = 95;
        public const byte MUR = 96;
        public const byte MVR = 97;
        public const byte MWK = 98;
        public const byte MXN = 99;
        public const byte MYR = 100;
        public const byte MZN = 101;
        public const byte NAD = 102;
        public const byte NGN = 103;
        public const byte NIO = 104;
        public const byte NOK = 105;
        public const byte NPR = 106;
        public const byte NZD = 107;
        public const byte OMR = 108;
        public const byte PAB = 109;
        public const byte PEN = 110;
        public const byte PGK = 111;
        public const byte PHP = 112;
        public const byte PKR = 113;
        public const byte PLN = 114;
        public const byte PYG = 115;
        public const byte QAR = 116;
        public const byte RON = 117;
        public const byte RSD = 118;
        public const byte RUB = 119;
        public const byte RWF = 120;
        public const byte SAR = 121;
        public const byte SBD = 122;
        public const byte SCR = 123;
        public const byte SDG = 124;
        public const byte SEK = 125;
        public const byte SGD = 126;
        public const byte SHP = 127;
        public const byte SLL = 128;
        public const byte SOS = 129;
        public const byte SPL = 130;
        public const byte SRD = 131;
        public const byte STD = 132;
        public const byte SVC = 133;
        public const byte SYP = 134;
        public const byte SZL = 135;
        public const byte THB = 136;
        public const byte TJS = 137;
        public const byte TMT = 138;
        public const byte TND = 139;
        public const byte TOP = 140;
        public const byte TRY = 141;
        public const byte TTD = 142;
        public const byte TVD = 143;
        public const byte TWD = 144;
        public const byte TZS = 145;
        public const byte UAH = 146;
        public const byte UGX = 147;
        public const byte USD = 148;
        public const byte UYU = 149;
        public const byte UZS = 150;
        public const byte VEF = 151;
        public const byte VND = 152;
        public const byte VUV = 153;
        public const byte WST = 154;
        public const byte XAF = 155;
        public const byte XCD = 156;
        public const byte XDR = 157;
        public const byte XOF = 158;
        public const byte XPF = 159;
        public const byte YER = 160;
        public const byte ZAR = 161;
        public const byte ZMW = 162;
        public const byte ZWD = 163;
        public const byte CNH = 201;

        private static Dictionary<byte, string> mapping;

        static CurrencyId()
        {
            mapping = new Dictionary<byte, string>();
            foreach (var info in typeof(CurrencyId).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (info.FieldType == typeof(byte))
                {
                    mapping.Add((byte)info.GetValue(null), info.Name);
                }
            }
        }

        public static byte GetId(string name)
        {
            var info = typeof(CurrencyId).GetField(name, BindingFlags.Static | BindingFlags.Public);
            return info != null ? (byte)info.GetValue(null) : (byte)0;
        }

        public static string GetName(byte id) => mapping[id] ?? id.ToString();
    }

    public class CurrencyConverter : ICurrencyConverter
    {
        public Framework Framework { get; private set; }

        public DataManager DataManager => Framework.DataManager;

        public CurrencyConverter(Framework framework)
        {
            Framework = framework;
        }

        public virtual double Convert(double amount, byte fromCurrencyId, byte toCurrencyId)
        {
            return amount;
        }
    }

    public class CurrencyConverterFX : ICurrencyConverter
    {
        private IdArray<IdArray<Instrument>> mappings = new IdArray<IdArray<Instrument>>(256);

        public DataManager DataManager => Framework.DataManager;

        public Framework Framework { get; }

        public CurrencyConverterFX(Framework framework)
        {
            Framework = framework;
        }

        public void Add(Instrument instrument)
        {
            if (instrument.CCY1 != 0 && instrument.CCY2 != 0)
            {
                if (this.mappings[instrument.CCY1] == null)
                    this.mappings[instrument.CCY1] = new IdArray<Instrument>(256);

                this.mappings[instrument.CCY1][instrument.CCY2] = instrument;
            }
        }

        public virtual double Convert(double amount, byte fromCurrencyId, byte toCurrencyId)
        {
            if (fromCurrencyId == toCurrencyId)
                return amount;

            var instrument = this.mappings[fromCurrencyId]?[toCurrencyId];
            if (instrument != null)
            {
                double price = GetPrice(instrument);
                return price != 0.0 ? amount*price : amount;
            }

            instrument = this.mappings[toCurrencyId]?[fromCurrencyId];
            if (instrument != null)
            {
                double price = GetPrice(instrument);
                return price != 0.0 ? amount/price : amount;
            }

            return amount;
        }

        private double GetPrice(Instrument instrument)
        {
            if (instrument.Bid != null)
                return instrument.Bid.Price;

            if (instrument.Ask != null)
                return instrument.Ask.Price;
          
            if (instrument.Trade != null)
                return instrument.Trade.Price;

            if (instrument.Bar != null)
                return instrument.Bar.Close;

            return 0;
        }
    }
}
