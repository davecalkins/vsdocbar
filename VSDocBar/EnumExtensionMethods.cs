using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSDocBar
{
    /// <summary>
    /// Helpers used by VSExtensionMethods when formatting doc fields info
    /// </summary>
    public static class EnumExtensionMethods
    {
        // my own version, this works and also shows applicable mask values when formatting
        // the string
        //public static string FormatFlagsEnumShowingIndividualValues(this Enum value, string separator)
        //{
        //    string result = "";

        //    result = String.Join("|", value.GetIndividualFlags());

        //    var valueI64 = Convert.ToInt64(value);

        //    var possibleValues = Enum.GetValues(value.GetType());
        //    foreach (var possibleVal in possibleValues)
        //    {
        //        var possibleValI64 = Convert.ToInt64(possibleVal);

        //        if ((valueI64 & possibleValI64) != 0)
        //        {
        //            if (result.Length > 0)
        //                result += separator;

        //            result += possibleVal.ToString();
        //        }
        //    }

        //    return result;
        //}

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        // taken from https://stackoverflow.com/questions/5542816/printing-flags-enum-as-separate-flags
        // answer by Jeff Mercado.  This does not show mask values only individual values which are set.
        // NOTE: had to replace ulong with long and ToUInt64 with ToInt64 to be able to handle enums with
        //       negative values
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        public static IEnumerable<Enum> GetFlags(this Enum value)
        {
            return GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());
        }

        public static IEnumerable<Enum> GetIndividualFlags(this Enum value)
        {
            return GetFlags(value, GetFlagValues(value.GetType()).ToArray());
        }

        private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
        {
            long bits = Convert.ToInt64(value);
            List<Enum> results = new List<Enum>();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                long mask = Convert.ToInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;
                if ((bits & mask) == mask)
                {
                    results.Add(values[i]);
                    bits -= mask;
                }
            }
            if (bits != 0L)
                return Enumerable.Empty<Enum>();
            if (Convert.ToInt64(value) != 0L)
                return results.Reverse<Enum>();
            if (bits == Convert.ToInt64(value) && values.Length > 0 && Convert.ToInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<Enum>();
        }

        private static IEnumerable<Enum> GetFlagValues(Type enumType)
        {
            long flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
            {
                long bits = Convert.ToInt64(value);
                if (bits == 0L)
                    //yield return value;
                    continue; // skip the zero value
                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////

    }
}
