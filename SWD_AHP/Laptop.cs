using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD_AHP
{
    public class Laptop
    {
        public string manufacturer { get; set; }
        public string name { get; set; }
        public string cpu { get; set; }
        public string gpu { get; set; }
        public double ram { get; set; }
        public double price { get; set; }

        public Laptop(string manufacturer, string name, string cpu, string gpu, double ram, double price)
        {
            this.manufacturer = manufacturer;
            this.name = name;
            this.cpu = cpu;
            this.gpu = gpu;
            this.ram = ram;
            this.price = price;
        }

        public override string ToString()
        {
            return manufacturer + " " + name + "\n" + cpu+ "\n" +gpu+ "\n" +ram+ "\n" + price ;
        }
    }
}
