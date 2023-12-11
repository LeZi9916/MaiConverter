

namespace MaiConverter.Notes
{
    public enum SlideType
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
    public class Star : Note
    {
        /// <summary>
        /// 表示该Slide的路径
        /// </summary>
        public required Slide[] Slides;
    }
    public class Slide : Note
    {
        /// <summary>
        /// 表示该Slide的起点
        /// </summary>
        public required int Start;
        /// <summary>
        /// 表示该Slide的终点
        /// </summary>
        public required int End;
        /// <summary>
        /// 表示该Slide所属组别
        /// </summary> 
        public required int Group;
        /// <summary>
        /// 用于Polayline类型的Slide
        /// </summary>
        public int? Relay;

        ///  <summary>
        /// 表示该Slide的时值
        /// </summary>
        public required long Value;

        /// <summary>
        /// 特殊参数,用于修改该Note的BPM值
        /// </summary> 
        public float? Bpm;
        /// <summary>
        /// 表示Slide的类型
        /// </summary>      
        public required SlideType SlideType;

        public static Slide[] operator + (Slide a,Slide b) => new Slide[] {a,b};
        public static Slide[] operator + (Slide a,IEnumerable<Slide> array)
        {
            var b = array.ToList();
            b.Add(a);
            return b.ToArray();
        }
        public static Slide[] operator + (IEnumerable<Slide> array,Slide a) => a + array;
    }
}