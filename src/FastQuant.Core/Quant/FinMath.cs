// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace SmartQuant.Quant
{
    public enum EOptionPosition
    {
        InTheMoney,
        AtTheMoney,
        OutOfTheMoney
    }

    public enum EOptionPrice
    {
        BlackScholes,
        Binomial,
        Trinomial,
        MonteCarlo
    }

    public enum EOptionType
    {
        European,
        American,
        Exotic,
        Bermudian,
        Digial
    }

    public enum EPutCall
    {
        Call,
        Put
    }

    public class FinMath
    {
         
    }
}