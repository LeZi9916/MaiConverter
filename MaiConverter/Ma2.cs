using MaiConverter.Notes;
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
            enum ChartVersion
            {
                Normal,
                Festival
            }
            public static Chart Handle(string filePath)
            {
                Chart chart = new Chart();
                string version = ""; 
                string _chartStr = File.ReadAllText(filePath);
                var chartStr = _chartStr.Split("\n");
                int def = 384;
                string s = null;
                foreach (var line in chartStr) 
                {
                    var contents = line.Split("\t");
                    if (contents[0] == "VERSION")
                        version = contents[2];
                    else if (contents[0] == "CLK_DEF")
                        def = int.Parse(contents[1]);
                    else if (contents[0] == "COMPATIBLE_CODE")
                        s = "";
                    else if (contents[0].Contains("T_REC"))
                        break;
                    else if (s is not null)
                        s += line;                   
                }
                if (version == "1.04.00")
                    Decode(s, def, ChartVersion.Festival);
                else
                    Decode(s, def, ChartVersion.Normal);
            }
            static NoteCollection Decode(string chartStr,int def,ChartVersion version)
            {
                NoteCollection notes = new();
                var strArray = chartStr.Split("\n");
                
                for (int index = 0; index < strArray.Length; index++)
                {
                    var s = strArray[index];
                    var contents = s.Split("\t");

                    if (contents[0] is "BPM")
                    {
                        var tick = (long.Parse(contents[1]) * def) + long.Parse(contents[2]);
                        var bpm = double.Parse(contents[3]);
                        notes.AddBpm(tick, bpm);
                    }
                    else if (contents[0].Contains("TAP") || contents[0] == "BRK")
                        notes.Add(TapHandle(contents,def));

                }
            }
            static Tap TapHandle(string[] array,int def)
            {
                bool isBreak = false;
                bool isExNote = false;
                long tick;

                if (array[0].Contains("B"))
                    isBreak = true;
                if (array[0].Contains("X"))
                    isExNote = true;

                tick = (long.Parse(array[1]) * def) + long.Parse(array[2]);
                return new Tap()
                { 
                    Tick = tick,
                    Break = isBreak,
                    ExNote = isExNote,
                    Type = NoteType.Tap,
                    Position = int.Parse(array[3]) + 1
                };

            }
            static Hold HoldHandle(string[] array, int def)
            {

            }
        }
    }
}
