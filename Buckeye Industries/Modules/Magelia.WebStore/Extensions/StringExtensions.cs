using System;
using System.Linq;

namespace Magelia.WebStore.Extensions
{
    public static class StringExtensions
    {
        public static Boolean EqualsInvariantCultureIgnoreCase(this String @string, params String[] compares)
        {
            return compares.Any(c => @string.Equals(c, StringComparison.InvariantCultureIgnoreCase)); //@string.Equals(compares, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}