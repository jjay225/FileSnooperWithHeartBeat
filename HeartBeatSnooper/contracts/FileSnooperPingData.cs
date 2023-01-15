using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartBeatSnooper.contracts
{
    internal class FileSnooperPingData
    {
        public string Identifier { get; set; }
        public DateTime TimeSent { get; set; }
    }
}
