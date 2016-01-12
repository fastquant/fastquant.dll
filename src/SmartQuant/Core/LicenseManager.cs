// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace SmartQuant
{
    namespace Licensing
    {
        public enum License
        {
            Demo,
            Retail,
            Professional,
            Enterprise
        }
    }

    public class LicenseInfo
    {
        internal LicenseInfo()
        {
        }

        public bool EvaluationLockEnabled => false;

        public int EvaluationTime => int.MaxValue;

        public int EvaluationTimeCurrent => int.MinValue;

        public DateTime ExpirationDate => DateTime.MaxValue;

        public bool ExpirationDateLockEnabled => false;

        public IDictionary<string, string> Fields = new Dictionary<string, string>();

        public bool Licensed => true;
    }

    public class LicenseManager
    {
        public string GetHardwareID()
        {
            throw new NotImplementedException();
        }

        public LicenseInfo GetLicense() => new LicenseInfo();

        public void LoadLicense(byte[] license)
        {
            throw new NotImplementedException();
        }
    }
}