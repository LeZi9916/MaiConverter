using MaiConverter.Exception;
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
            static Dictionary<long, Star> SlideList;//Ticks Slide
            static Dictionary<long, Star> SlideEndTick;//Ticks Slide
            public static Chart Handle(string filePath)
            {
                SlideList = new();
                SlideEndTick = new();
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
                if (s is null)
                    throw new FormatException($"\"{filePath}\"不是有效的的谱面文件");
                else
                    Decode(s, def);
            }
            static NoteCollection Decode(string chartStr,int def)
            {
                NoteCollection notes = new();
                var strArray = chartStr.Split("\n",StringSplitOptions.RemoveEmptyEntries);

                for (int index = 0; index < strArray.Length; index++)
                {
                    var s = strArray[index];
                    var contents = s.Split("\t");
                    string[] slideTypes = { "SI_","SCL","SCR","SV_","SLR","SLL","SUR","SUL","SXR","SXL","SSR","SSL","SF_" };
                    if (contents[0] is "BPM")
                    {
                        var tick = (long.Parse(contents[1]) * def) + long.Parse(contents[2]);
                        var bpm = double.Parse(contents[3]);
                        notes.AddBpm(tick, bpm);
                    }
                    else if (contents[0].Contains("TAP") || contents[0] == "BRK")
                        notes.Add(TapHandle(contents, def));
                    else if (contents[0].Contains("HLD") || contents[0].Contains("XHO") || contents[0].Contains("THO"))
                        notes.Add(HoldHandle(contents, def));
                    else if (contents[0].Contains("ST"))
                    {
                        
                    }
                    else if (slideTypes.Contains(contents[0].Contains("NM") || contents[0].Contains("CN") ? contents[0].Substring(2,3): contents[0]))
                    {
                        Func<string[], int> GetSlideStr = (array) =>
                        {
                            for (int i = index; i < array.Length; i++)
                            {
                                var contents = s.Split("\t");
                                if (contents[0].Contains("CN"))
                                    return i - 1;
                                if (contents[0].Contains("ST") || !slideTypes.Contains(contents[0].Replace("NM", "")))
                                    return i - 1;
                            }
                            return -1;
                        };
                        int endIndex = GetSlideStr(strArray);
                        if (endIndex == -1)
                            new UnknowNoteOrParametersException($"解释Slide时出错\n出错起始行:{index + 1}");
                        string[] slideStrArray = new string[endIndex - index + 1];
                        Array.Copy(strArray, index, slideStrArray, 0, endIndex - index + 1);
                        notes.Add(SlideHandle(string.Join('\n', slideStrArray)));
                        index = endIndex;
                    }
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
                bool isBreak = false;
                bool isExHold = false;
                bool isTouch = false;
                bool isFireworks = false;
                long tick = 0;
                long value = 0;

                if (array[0].Contains("B"))
                    isBreak = true;
                if (array[0].Contains("X"))
                    isExHold = true;
                if (array[0].Contains("T"))
                    isTouch = true;
                if (isTouch && array[6] is "1")
                    isFireworks = true;

                tick = long.Parse(array[1]) * def + long.Parse(array[2]);
                value = long.Parse(array[4]);

                if (isTouch)
                    return new TouchHold()
                    {
                        Tick = tick,
                        Value = value,
                        Position = int.Parse(array[3]) + 1,
                        Type = NoteType.TouchHold,
                        Fireworks = isFireworks,
                        Break = false,
                        ExNote = false
                    };
                else
                    return new Hold()
                    {
                        Tick = tick,
                        Value = value,
                        Position = int.Parse(array[3]) + 1,
                        Type = NoteType.Hold,
                        Break = false,
                        ExNote = false
                    };
            }
            static Star SlideHeadHandle(string slideStr)
            {
                var slideArray = slideStr.Split("\n");
            }
            static Slide SlideHandle(string slideStr)
            {

            }
            
        }
    }
}
