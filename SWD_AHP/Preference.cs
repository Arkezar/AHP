using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD_AHP
{
    class Preference
    {
        public int id { get; set; }
        public string name { get; set; }
        public double val { get; set; }
        public Preference(int id, string name, double val)
        {
            this.id = id;
            this.name = name;
            this.val = val;
        }
        
        override
        public string ToString()
        {
            return id + " " + name + " " + val;
        }
        
    }
}
