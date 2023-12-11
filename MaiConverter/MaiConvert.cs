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
    public partial class MaiConvert
    {
        Chart chart;
        public MaiConvert(string filePath,ChartType chartFormat)
        {
            if(!File.Exists(filePath))
                throw new FileNotFoundException($"文件\"{filePath}\"不是有效的谱面文件");
            switch(chartFormat)
            {
                case ChartType.Simai:
                    chart = Simai.Handle(filePath);
                    break;
                case ChartType.Ma2:
                    break;
                case ChartType.Sdt:
                    break;
            }
        }
        
        
    }

}


