

namespace MaiConverter.Notes
{
    public enum SlidePathType
    {
        Line,// -
        Polyline,// v&V
        L_Arc,// ^
        R_Arc,
        L_Loop,// p&q
        R_Loop,
        L_BigLoop,// pp&qq
        R_BigLoop,
        L_Lightning,// s&z
        R_Lightning,
        WiFi// w
    }
    public class Slide : Note
    {
        /// <summary>
        /// 表示该Slide的时值,当为0时，表示无长度Hold
        /// </summary>
        public required long Value;

        /// <summary>
        /// 特殊参数,用于修改该Note的BPM值
        /// </summary> 
        public float? Bpm;
        /// <summary>
        /// 表示该Slide的路径
        /// </summary>
        public required SlidePath[] Paths;
    }
    public class SlidePath
    {
        /// <summary>
        /// 表示该Path的起点
        /// </summary>
        public required int Start;
        /// <summary>
        /// 表示该Path的终点
        /// </summary>
        public required int End;
        /// <summary>
        /// 用于Polayline类型的Path
        /// </summary>
        public int? Relay;
        /// <summary>
        /// 表示Path的类型
        /// </summary>
        public required SlidePathType Type;
    }
}