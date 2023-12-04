
namespace MaiConverter.Notes
{
    public class Hold : Note
    {
        /// <summary>
        /// 表示该Hold的时值,当为0时，表示无长度Hold
        /// </summary>
        public required long Value;

        /// <summary>
        /// 特殊参数,用于修改该Note的BPM值
        /// </summary> 
        public float? Bpm;
    }
}