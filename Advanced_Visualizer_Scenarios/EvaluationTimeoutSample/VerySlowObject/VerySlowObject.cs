using System.Collections.Generic;
using System.Threading;

namespace CustomObjects
{
    public class SomeRandomObject
    {
        public override string ToString()
        {
            Thread.Sleep(500);
            return "Some Random Object's ToString value.";
        }
    }

    public class VerySlowObject
    {
        // More properties...

        // One of the properties we want to visualize.
        public List<SomeRandomObject>? VeryLongList { get; set; }

        // More properties...
    }
}
