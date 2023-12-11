using System;
using System.Numerics;
using MaiConverter.Exception;
using MaiConverter.Notes;

namespace MaiConverter
{
    /// <summary>
    /// 表示输入的谱面类型
    /// </summary> 
    public enum ChartType
    {
        Simai,
        Ma2,
        Sdt
    }
    public struct Chart
    {
        public string Title,Artist,Des;
        public double Offset;
        public NoteCollection Basic;
        public NoteCollection Advanced;
        public NoteCollection Expert;
        public NoteCollection Master;
        public NoteCollection ReMaster;

    }
    public class MaiConvert
    {
        static class Simai
        {
            static void Handle(string filePath)
            {
                string easyStr = "",advancedStr = "",expertStr = "",masterStr = "",reMasterStr = "";
                string _chartStr = File.ReadAllText(filePath);
                var chartStr = _chartStr.Split("&");
                Chart chart = new();
                foreach(var str in chartStr)
                {
                    if (str.Contains("title"))
                        chart.Title = str.Replace("title=","");
                    else if (str.Contains("artist"))
                        chart.Artist = str.Replace("artist=","");
                    else if (str.Contains("des"))
                        chart.Des = str.Replace("des=","");
                    else if (str.Contains("first"))
                        chart.Offset = double.Parse(str.Replace("first=",""));
                    else if(str.Contains("inote_2"))
                        easyStr = str.Replace("inote_2=","");
                    else if(str.Contains("inote_3"))
                        advancedStr = str.Replace("inote_3=","");
                    else if(str.Contains("inote_4"))
                        expertStr = str.Replace("inote_4=","");
                    else if(str.Contains("inote_5"))
                    masterStr = str.Replace("inote_5=","");
                    else if(str.Contains("inote_6"))
                        reMasterStr = str.Replace("inote_6=","");
                }
            }
            static void Decode(string chartStr)
            {
                var strArray = chartStr.Split(",");
                NoteCollection notes = new();
                Dictionary<long,double> BpmList = new();//<Tick,BPM>
                /// <summary>
                /// 表示当前时间轴
                /// </summary>
                long tick = 0;
                /// <summary>
                /// 步进
                /// </summary>
                int step = 0;
                /// <summary>
                /// 表示当前拍号
                /// </summary>
                float meter = 0;
                /// <summary>
                /// 表示当前的BPM
                /// </summary>
                float bpm = 0;
                const int stepTick = 384;

                foreach(var s in strArray)
                {
                    var bpmStart = s.IndexOf("(");
                    var bpmEnd = s.IndexOf(")");
                    var meterStart = s.IndexOf("{");
                    var meterEnd = s.IndexOf("}");
                    var noteStart = Math.Max(bpmEnd,meterEnd) + 1;
                    var notesStr = s.Substring(noteStart,s.Length - noteStart).Split(new char[] {'/','`'});

                    if( (bpmStart < 0 && bpmEnd >= 0) || (bpmStart >= 0 && bpmEnd < 0) || (meterStart < 0 && meterEnd >= 0) || (meterStart >= 0 && meterEnd < 0))
                        throw new UnknowNoteOrParametersException("\"{s}\"不是有效的Note、BPM或节拍数");
                    if(bpmStart >= 0 && bpmEnd >= 0)//BPM处理
                    {
                        if(float.TryParse(s.Substring(bpmStart + 1,bpmEnd - bpmStart - 1),out bpm))
                            BpmList.Add(tick,bpm);
                        else
                            throw new UnknowBpmValueException($"\"{s.Substring(bpmStart + 1,bpmEnd - bpmStart - 1)}\"不是正确的BPM值");
                    }
                    if(meterStart >= 0 && meterEnd >= 0)//拍号处理
                    {
                        float _meter;
                        if(float.TryParse(s.Substring(meterStart + 1,meterEnd - meterStart - 1),out _meter))
                            meter = _meter;
                        else
                            throw new UnknowMeterValueException($"\"{s.Substring(meterStart + 1,meterEnd - meterStart - 1)}\"不是正确的拍号");
                    }
                    
                    foreach(var noteStr in notesStr)
                    {
                        if(noteStr.Length == 1)
                            notes.Add(TapHandle(noteStr,tick));
                        else
                        {
                            if(noteStr.Contains("h"))
                                notes.Add(HoldHandle(noteStr,tick,bpm));
                            else if(noteStr.Split(new char[]{'-','^','>','<','q','p','w','s','z'}).Length != 1)
                                notes.Add(SlideHeadHandle(noteStr,tick,bpm));
                            else if(noteStr.Length == 2)
                            {
                                notes.Add(TapHandle(noteStr.Substring(0,1),tick));
                                notes.Add(TapHandle(noteStr.Substring(1,1),tick));
                            }
                            else
                                throw new UnknowNoteOrParametersException($"\"{noteStr}\"不是有效的Note");
                        }
                    }

                }
            }
            static Hold HoldHandle(string s,long tick,float bpm)
            {
                bool isBreak = false;
                bool isExNote = false;
                bool isFireworks = false;
                long value = 0;
                var pStart = s.IndexOf("[");
                var pEnd = s.IndexOf("]");
                var parameters = s.Substring(pStart + 1,pEnd - pStart - 1).Split('#',StringSplitOptions.RemoveEmptyEntries);

                if(parameters.Length == 1)
                    value = GetTimeTick(parameters[0], bpm);
                else if(parameters.Length == 2)
                    value = GetTimeTick(parameters[1], bpm, float.Parse(parameters[1]));

                if (s.Contains("b"))
                    isBreak = true;
                if (s.Contains("x"))
                    isExNote = true;
                if(s.Contains("f"))
                    isFireworks = true;
                if(s.Contains("C"))
                {
                    return new TouchHold()
                    {
                        Tick = tick,
                        Type = NoteType.TouchHold,
                        Position = 0,
                        ExNote = isExNote,
                        Break = false,
                        Value = value,
                        Fireworks = isFireworks
                    };
                }
                return new Hold()
                {
                    Tick = tick,
                    Type = NoteType.Hold,
                    Position = int.Parse(s.Substring(0,1)),
                    ExNote = isExNote,
                    Break = isBreak,
                    Value = value

                };
                
            }
            static Tap TapHandle(string s,long tick)
            {
                bool isBreak = false;
                bool isExNote = false;

                if (s.Contains("b"))
                    isBreak = true;
                if (s.Contains("x"))
                    isExNote = true;

                return new Tap()
                {
                    Tick = tick,
                    Position = int.Parse(s),
                    Type = NoteType.Tap,
                    ExNote = isExNote,
                    Break = isBreak
                };
            }
            static Star SlideHeadHandle(string s,long tick,float bpm)
            {
                bool isBreak = false;
                bool isExNote = false;
                var position = int.Parse(s.Substring(0,1));
                var slideStr = s.Split("*",StringSplitOptions.RemoveEmptyEntries);
                Slide[] slides = new Slide[1024];
                int group = 0;
                long value = 0;
                long numerator = 0;
                long denominator = 0;

                if(s.IndexOf("b") == 1)
                    isBreak = true;
                if(s.IndexOf("x") == 1)
                    isExNote = true;
                foreach(var a in slideStr)
                    slides += SlideHandle(a,tick,bpm,position,group++);
                return new Star()
                {
                    Tick = tick,
                    Type = NoteType.Star,
                    Position = position,
                    ExNote = isExNote,
                    Break = isBreak,
                    Slides = slides 
                };
                
            }
            static Slide SlideHandle(string s,long tick,float bpm,int position,int group)
            {
                var pStart = s.IndexOf('[');
                var pEnd = s.IndexOf(']');
                var parameters = s.Substring(pStart + 1,pEnd - pStart - 1).Split('#',StringSplitOptions.RemoveEmptyEntries);
                long bpmTick = 96;
                long value = 0;

                var body = s.Substring(0, pStart);
                string[] upperPart = {"7","8","1","2"}; 
                string[] lowerPart = {"6","5","4","3"};
                

                if(parameters.Length == 1 )// 8:1
                    value = GetTimeTick(parameters[0], bpm);
                else if(parameters.Length == 2)
                {
                    if (GetEqualElementCount(s.Substring(pStart + 1, pEnd - pStart - 1), '#') == 2) // 3##1.5或3##8:1
                    {
                        var bpmSeconds = float.Parse(parameters[0]);
                        double tickTime = 60 / bpm / 384;
                        bpmTick = (long)(bpmSeconds / tickTime);
                        value = GetTimeTick(parameters[1], bpm);
                    }
                    else// 160#8:1或160#2
                    {
                        var secondBpm = float.Parse(parameters[0]);
                        bpmTick = (long)(bpmTick * bpm / secondBpm);
                        if (parameters[1].Contains(':'))
                            value = GetTimeTick(parameters[1], bpm, secondBpm);
                        else
                            value = GetTimeTick(parameters[1], bpm);
                    }
                }
                else if(parameters.Length ==3)//3##160#8:1
                {
                    var bpmSeconds = float.Parse(parameters[0]);
                    double tickTime = 60 / bpm / 384;
                    bpmTick = (long)(bpmSeconds / tickTime);
                    value = GetTimeTick(parameters[2], bpm, float.Parse(parameters[1]));
                }
                else
                    throw new UnknowNoteOrParametersException($"\"{s.Substring(pStart + 1, pEnd - pStart - 1)}\"不是有效的时值参数");




            }
            static long GetTimeTick(string s,float bpm)
            {
                long numerator = 0;
                long denominator = 0;
                if (s.Contains(":"))
                {
                    var _value = s.Split(':');
                    if (!long.TryParse(_value[0], out denominator) && !long.TryParse(_value[1], out numerator))
                        throw new UnknowNoteOrParametersException($"\"{s}\"不是有效的时值参数");
                    return 384 * (numerator * (1 / denominator));
                }
                else
                {
                    double tickTime = 60 / bpm / 384;
                    long seconds = long.Parse(s);
                    return (long)(seconds / tickTime);
                }
            }
            static long GetTimeTick(string s, float bpm, float secondBpm) => (long)(( bpm / secondBpm ) * GetTimeTick(s, secondBpm));
        } 
        public MaiConvert(string filePath,ChartType chartFormat)
        {
            if(!File.Exists(filePath))
                throw new FileNotFoundException($"文件\"{filePath}\"不是有效的谱面文件");
            switch(chartFormat)
            {
                case ChartType.Simai:
                    break;
                case ChartType.Ma2:
                    break;
                case ChartType.Sdt:
                    break;
            }
        }
        
        public static T[] GetEqualElement<T>(IEnumerable<T> elements,T Key) where T: IEquatable<T> => elements.Where(value => value.Equals(Key)).ToArray();
        public static long GetEqualElementCount<T>(IEnumerable<T> elements,T Key) where T: IEquatable<T> => GetEqualElement(elements,Key).Length;
    }

}


