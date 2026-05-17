using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TippoDiscord.Models
{
    public class TippAnzeige
    {
        public string Heimteam { get; set; }
        public string Gastteam { get; set; }
        public int TippHeim { get; set; }
        public int TippGast { get; set; }
        public DateTime Anstosszeit { get; set; }
    }
}
