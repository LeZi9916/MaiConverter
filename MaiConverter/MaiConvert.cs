using System;
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
                Dictionary<long,double> BpmList = new();//<Tick,BPM>
                double bpm = 0;
                int index = 1;
                long tick = 0;
                int step = 0;//步进
                const int a = 384;
                int i = 0;//拍号
                foreach(var s in strArray)
                {
                    tick += step * index;
                    if(s.Contains('(') && s.Contains(')'))
                    {
                        bpm = double.Parse(s.Replace("(","").Split(")")[0]);
                        BpmList.Add(tick,bpm);
                    }
                    if(s.Contains('{') && s.Contains('}'))
                    {
                        i = int.Parse(s.Replace("{","").Split("}")[0]);
                        index = 1;
                        step = 384 / i;
                    }
                    tick = tick == 0 ? step * index :tick;
                    var noteStr = s.Split(")")[1].Contains('}') ? s.Split(")")[1].Split("}")[1] : s.Split(")")[1];
                }
            }
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
        
        
    }

}


