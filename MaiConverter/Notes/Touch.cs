
namespace MaiConverter.Notes
{
    public class Touch : Note
    {
        /// <summary>
        /// 表示该Touch所在的传感器编号
        /// </summary>
        public required SensorArea Sensor;
        /// <summary>
        /// 表示该Touch是否具有烟花效果
        /// </summary>
        public required bool Fireworks;
    }
}