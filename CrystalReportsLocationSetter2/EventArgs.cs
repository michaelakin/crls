using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrystalReportLocationSetter2
{
    public class EventArgs<T> : EventArgs
    {
        public T Data { get; set; }
    }
}
