﻿namespace Lunggo.ApCommon.Flight.Constant
{
    public enum Gender
    {
        Undefined = 0,
        Male = 1,
        Female = 2
    }

    internal class GenderCd
    {
        internal static string Mnemonic(Gender gender)
        {
            switch (gender)
            {
                case Gender.Male:
                    return "M";
                case Gender.Female:
                    return "F";
                default:
                    return null;
            }
        }

        internal static Gender Mnemonic(string gender)
        {
            switch (gender)
            {
                case "M":
                    return Gender.Male;
                case "F":
                    return Gender.Female;
                default:
                    return Gender.Undefined;
            }
        }
    }
}
