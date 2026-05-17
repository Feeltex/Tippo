using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TippoDiscord
{
    public class Spiel
    {
        public int spielId { get; set; }
        public string heimteam { get; set; }
        public string gastteam { get; set; }
        public DateTime anstosszeit { get; set; }
        public int spieltag { get; set; }
    }
}
