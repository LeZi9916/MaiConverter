using MaiConverter.Exception;
using MaiConverter.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaiConverter
{
    public partial class MaiConvert
    {
        public static class Simai
        {
            public static Chart Handle(string filePath)
            {
                string basicStr = "", advancedStr = "", expertStr = "", masterStr = "", reMasterStr = "";
                string _chartStr = File.ReadAllText(filePath);
                var chartStr = _chartStr.Split("&");
                Chart chart = new();
                foreach (var str in chartStr)
                {
                    if (str.Contains("title"))
                        chart.Title = str.Replace("title=", "");
                    else if (str.Contains("artist"))
                        chart.Artist = str.Replace("artist=", "");
                    else if (str.Contains("des"))
                        chart.Des = str.Replace("des=", "");
                    else if (str.Contains("first"))
                        chart.Offset = double.Parse(str.Replace("first=", ""));
                    else if (str.Contains("inote_2"))
                        basicStr = str.Replace("inote_2=", "");
                    else if (str.Contains("inote_3"))
                        advancedStr = str.Replace("inote_3=", "");
                    else if (str.Contains("inote_4"))
                        expertStr = str.Replace("inote_4=", "");
                    else if (str.Contains("inote_5"))
                        masterStr = str.Replace("inote_5=", "");
                    else if (str.Contains("inote_6"))
                        reMasterStr = str.Replace("inote_6=", "");
                }
                chart.Basic = Decode(basicStr, chart.Offset);
                chart.Advanced = Decode(advancedStr, chart.Offset);
                chart.Expert = Decode(expertStr, chart.Offset);
                chart.Master = Decode(masterStr, chart.Offset);
                chart.ReMaster = Decode(reMasterStr, chart.Offset);
                return chart;
            }
            static NoteCollection Decode(string chartStr, double offset)
            {
                var strArray = chartStr.Split(",");
                NoteCollection notes = new();
                //Dictionary<long, double> BpmList = new();//<Tick,BPM>
                bool applyOffset = false;
                /// <summary>
                /// 表示当前时间轴
                /// </summary>
                long tick = 0;
                /// <summary>
                /// 步进
                /// </summary>
                long step = 0;
                /// <summary>
                /// 表示当前拍号
                /// </summary>
                long meter = 0;
                /// <summary>
                /// 表示当前的BPM
                /// </summary>
                float bpm = 0;
                const int stepTick = 384;

                foreach (var s in strArray)
                {
                    var bpmStart = s.IndexOf("(");
                    var bpmEnd = s.IndexOf(")");
                    var meterStart = s.IndexOf("{");
                    var meterEnd = s.IndexOf("}");
                    var noteStart = Math.Max(bpmEnd, meterEnd) + 1;
                    var notesStr = s.Substring(noteStart, s.Length - noteStart).Split(new char[] { '/', '`' });

                    if ((bpmStart < 0 && bpmEnd >= 0) || (bpmStart >= 0 && bpmEnd < 0) || (meterStart < 0 && meterEnd >= 0) || (meterStart >= 0 && meterEnd < 0))
                        throw new UnknowNoteOrParametersException("\"{s}\"不是有效的Note、BPM或节拍数");
                    if (bpmStart >= 0 && bpmEnd >= 0)
                    {
                        if (float.TryParse(s.Substring(bpmStart + 1, bpmEnd - bpmStart - 1), out bpm))
                            notes.AddBpm(tick, bpm);
                        else
                            throw new UnknowBpmValueException($"\"{s.Substring(bpmStart + 1, bpmEnd - bpmStart - 1)}\"不是正确的BPM值");
                        if (!applyOffset)
                        {
                            var tickTime = 60 / bpm / 384;
                            var offsetTick = offset / tickTime;
                            tick += (long)offsetTick;
                            applyOffset = true;
                        }
                    }//BPM处理
                    if (meterStart >= 0 && meterEnd >= 0)//拍号处理
                        if (!long.TryParse(s.Substring(meterStart + 1, meterEnd - meterStart - 1), out meter))
                            throw new UnknowMeterValueException($"\"{s.Substring(meterStart + 1, meterEnd - meterStart - 1)}\"不是正确的拍号");

                    foreach (var noteStr in notesStr)
                    {
                        if (noteStr.Length == 1)
                            notes.Add(TapHandle(noteStr, tick));
                        else
                        {
                            if (noteStr.Contains("h"))
                                notes.Add(HoldHandle(noteStr, tick, bpm));
                            else if (noteStr.Split(new char[] { '-', '^', '>', '<', 'q', 'p', 'w', 's', 'z' }).Length != 1)
                                notes.Add(SlideHeadHandle(noteStr, tick, bpm));
                            else if (noteStr.Length == 2)
                            {
                                notes.Add(TapHandle(noteStr.Substring(0, 1), tick));
                                notes.Add(TapHandle(noteStr.Substring(1, 1), tick));
                            }
                            else
                                throw new UnknowNoteOrParametersException($"\"{noteStr}\"不是有效的Note");
                        }
                    }
                    step = stepTick / meter;
                    tick += step;
                }
                return notes;
            }
            /// <summary>
            /// Hold的处理
            /// </summary>
            /// <param name="s"></param>
            /// <param name="tick"></param>
            /// <param name="bpm"></param>
            /// <returns></returns>
            static Hold HoldHandle(string s, long tick, float bpm)
            {
                bool isBreak = false;
                bool isExNote = false;
                bool isFireworks = false;
                long value = 0;
                var pStart = s.IndexOf("[");
                var pEnd = s.IndexOf("]");
                var parameters = s.Substring(pStart + 1, pEnd - pStart - 1).Split('#', StringSplitOptions.RemoveEmptyEntries);

                if (parameters.Length == 1)
                    value = GetTimeTick(parameters[0], bpm);
                else if (parameters.Length == 2)
                    value = GetTimeTick(parameters[1], bpm, float.Parse(parameters[1]));

                if (s.Contains("b"))
                    isBreak = true;
                if (s.Contains("x"))
                    isExNote = true;
                if (s.Contains("f"))
                    isFireworks = true;
                if (s.Contains("C"))
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
                    Position = int.Parse(s.Substring(0, 1)),
                    ExNote = isExNote,
                    Break = isBreak,
                    Value = value

                };

            }
            /// <summary>
            /// Tap的处理
            /// </summary>
            /// <param name="s"></param>
            /// <param name="tick"></param>
            /// <returns></returns>
            static Tap TapHandle(string s, long tick)
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
            /// <summary>
            /// Slide头部的处理
            /// </summary>
            /// <param name="s"></param>
            /// <param name="tick"></param>
            /// <param name="bpm"></param>
            /// <returns></returns>
            static Star SlideHeadHandle(string s, long tick, float bpm)
            {
                bool isBreak = false;
                bool isExNote = false;
                var position = int.Parse(s.Substring(0, 1));
                var slideStr = s.Split("*", StringSplitOptions.RemoveEmptyEntries);
                List<Slide> slides = new();
                int group = 0;

                if (s.IndexOf("b") == 1)
                    isBreak = true;
                if (s.IndexOf("x") == 1)
                    isExNote = true;
                foreach (var a in slideStr)
                    slides.AddRange(SlideHandle(a, tick, bpm, position, group++));
                return new Star()
                {
                    Tick = tick,
                    Type = NoteType.Star,
                    Position = position,
                    ExNote = isExNote,
                    Break = isBreak,
                    Slides = slides.ToArray()
                };

            }
            /// <summary>
            /// Slide的处理
            /// </summary>
            /// <param name="s"></param>
            /// <param name="tick"></param>
            /// <param name="bpm"></param>
            /// <param name="position"></param>
            /// <param name="group"></param>
            /// <returns></returns>
            /// <exception cref="UnknowNoteOrParametersException"></exception>
            static Slide[] SlideHandle(string s, long tick, float bpm, int position, int group)
            {
                var pStart = s.IndexOf('[');
                var pEnd = s.IndexOf(']');
                var parameters = s.Substring(pStart + 1, pEnd - pStart - 1).Split('#', StringSplitOptions.RemoveEmptyEntries);
                long bpmTick = 96;
                long value = 0;
                bool isBreak = false;

                var body = s.Substring(0, pStart);//1-3
                var _body = SplitSlideBody(body);
                Func<string, string, string, string, Slide> Handle = (startStr, endStr, typeStr, relayStr) =>
                {
                    int start = int.Parse(startStr);
                    int end = int.Parse(endStr);
                    SlideType type;
                    if (typeStr is "V")
                        type = GetSlideType(typeStr, startStr, relayStr);
                    else
                        type = GetSlideType(typeStr, startStr, endStr);

                    return new Slide()
                    {
                        Tick = tick,
                        Type = NoteType.Slide,
                        Position = position,
                        ExNote = false,
                        Break = isBreak,
                        Start = start,
                        End = end,
                        Group = group,
                        Value = value,
                        SlideType = type,
                        BpmTick = bpmTick
                    };
                };
                List<Slide> slides = new();

                if (_body.Last() is "b")
                    isBreak = true;
                //参数处理
                if (parameters.Length == 1)// 8:1
                    value = GetTimeTick(parameters[0], bpm);
                else if (parameters.Length == 2)
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
                else if (parameters.Length == 3)//3##160#8:1
                {
                    var bpmSeconds = float.Parse(parameters[0]);
                    double tickTime = 60 / bpm / 384;
                    bpmTick = (long)(bpmSeconds / tickTime);
                    value = GetTimeTick(parameters[2], bpm, float.Parse(parameters[1]));
                }
                else
                    throw new UnknowNoteOrParametersException($"\"{s.Substring(pStart + 1, pEnd - pStart - 1)}\"不是有效的时值参数");

                if (_body.Length == 3)
                    slides.Add(Handle(_body[0], _body[1], _body[2], ""));
                else
                {
                    string[] types = { "-", "^", "<", ">", "q", "p", "z", "s", "v", "V", "w" };
                    for (int index = 0; index < _body.Length; index++)
                    {
                        if (types.Contains(_body[index]))
                        {
                            if (types.Contains(_body[index + 1]))
                                continue;
                            else if (types.Contains(_body[index - 1]))
                                slides.Add(Handle(_body[index - 2], _body[index] + _body[index - 1], _body[index + 1], ""));
                            else if (_body[index] == "V")
                                slides.Add(Handle(_body[index - 1], _body[index], _body[index + 2], _body[index + 1]));
                            else
                                slides.Add(Handle(_body[index - 1], _body[index], _body[index + 1], ""));
                        }
                    }
                }

                return slides.ToArray();

            }
            /// <summary>
            /// 判断Slide的类型
            /// </summary>
            /// <param name="typeStr"></param>
            /// <param name="start"></param>
            /// <param name="end"></param>
            /// <returns></returns>
            static SlideType GetSlideType(string typeStr, string start, string end)
            {
                string[] upperPart = { "7", "8", "1", "2" };
                string[] lowerPart = { "6", "5", "4", "3" };

                if (typeStr == "-")
                    return SlideType.Line;
                else if (typeStr == "^")
                {
                    List<int> leftList = new(3);
                    List<int> rightList = new(3);
                    int center = int.Parse(start);
                    int index = 1;
                    for (; index <= 3; index++)
                    {
                        int leftNum = center - index;
                        int rightNum = center + index;
                        if (leftNum <= 0)
                            leftList.Add(leftNum + 8);
                        else
                            leftList.Add(leftNum);

                        if (rightNum >= 9)
                            rightList.Add(rightNum - 8);
                        else
                            rightList.Add(rightNum);
                    }
                    if (leftList.Contains(int.Parse(end)))
                        return SlideType.L_Arc;
                    else
                        return SlideType.R_Arc;
                }
                else if (typeStr == "<")
                {
                    if (upperPart.Contains(start))
                        return SlideType.L_Arc;
                    else
                        return SlideType.R_Arc;
                }
                else if (typeStr == ">")
                {
                    if (upperPart.Contains(start))
                        return SlideType.R_Arc;
                    else
                        return SlideType.L_Arc;
                }
                else if (typeStr == "q")
                    return SlideType.R_Loop;
                else if (typeStr == "p")
                    return SlideType.L_Loop;
                else if (typeStr == "qq")
                    return SlideType.R_BigLoop;
                else if (typeStr == "pp")
                    return SlideType.L_BigLoop;
                else if (typeStr == "s")
                    return SlideType.L_Lightning;
                else if (typeStr == "z")
                    return SlideType.R_Lightning;
                else if (typeStr == "v")
                    return SlideType.Polyline;
                else if (typeStr == "V")
                {
                    List<int> leftList = new(3);
                    List<int> rightList = new(3);
                    int center = int.Parse(start);
                    int index = 1;
                    for (; index <= 3; index++)
                    {
                        int leftNum = center - index;
                        int rightNum = center + index;
                        if (leftNum <= 0)
                            leftList.Add(leftNum + 8);
                        else
                            leftList.Add(leftNum);

                        if (rightNum >= 9)
                            rightList.Add(rightNum - 8);
                        else
                            rightList.Add(rightNum);
                    }
                    if (leftList.Contains(int.Parse(end)))
                        return SlideType.L_BigPolyline;
                    else
                        return SlideType.R_BigPolyline;
                }
                else if (typeStr == "w")
                    return SlideType.WiFi;
                return SlideType.Line;
            }
            /// <summary>
            /// 分割Slide文本
            /// </summary>
            /// <param name="s"></param>
            /// <returns></returns>
            static string[] SplitSlideBody(string s)
            {
                char[] numChars = { '1', '2', '3', '4', '5', '6', '7', '8' };
                char[] types = { '-', '^', '<', '>', 'q', 'p', 's', 'z', 'w', 'v', 'V' };
                List<string> result = new();
                string tmp = "";
                foreach (var _s in s)
                {
                    if (numChars.Contains(_s))
                    {
                        if (tmp != "")
                        {
                            result.Add(tmp);
                            tmp = "";
                        }
                        result.Add(_s.ToString());
                    }
                    if (types.Contains(_s))
                        tmp += _s;
                }
                return result.ToArray();
            }
            static long GetTimeTick(string s, float bpm)
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
            static long GetTimeTick(string s, float bpm, float secondBpm) => (long)((bpm / secondBpm) * GetTimeTick(s, secondBpm));
        }
        public static T[] GetEqualElement<T>(IEnumerable<T> elements, T Key) where T : IEquatable<T> => elements.Where(value => value.Equals(Key)).ToArray();
        public static long GetEqualElementCount<T>(IEnumerable<T> elements, T Key) where T : IEquatable<T> => GetEqualElement(elements, Key).Length;
    }
}
