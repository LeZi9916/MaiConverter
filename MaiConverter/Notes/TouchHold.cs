
namespace MaiConverter.Notes
{
    public class TouchHold : Hold
    {
        /// <summary>
        /// 表示该Touch所在的传感器编号
        /// </summary>
        public readonly SensorArea Sensor = SensorArea.C;
        /// <summary>
        /// 表示该Touch是否具有烟花效果
        /// </summary>
        public required bool Fireworks;
    }
}