using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaiConverter
{
    public  partial class MaiConvert
    {
        public static class Ma2
        {
            public Chart Handle(string filePath)
            {
                Chart chart = new Chart();
                string version = ""; 
                string _chartStr = File.ReadAllText(filePath);
                var chartStr = _chartStr.Split("\n");
                int def = 384;
                foreach (var line in chartStr) 
                {
                    var contents = line.Split("\t");
                    if (contents[0] == "VERSION")
                        version = contents[2];
                    else if (contents[0] == "CLK_DEF")
                        def = int.Parse(contents[1]);
                }
            }
        }
    }
}
