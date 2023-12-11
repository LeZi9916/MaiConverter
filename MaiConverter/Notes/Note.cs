using System.Linq;
namespace MaiConverter.Notes
{
    public enum SensorArea
    {
        A,
        B,
        C,
        D,
        E
    }
    public enum NoteType
    {
        Tap,
        Hold,
        Star,
        Slide,
        Touch,
        TouchHold,
    }
    public class Note
    {
        /// <summary>
        /// Note的时间轴,384进制
        /// </summary>
        public required long Tick;
        /// <summary>
        /// Note的种类
        /// </summary>
        public required NoteType Type;
        /// <summary>
        /// 指示该Note的键位
        /// </summary>
        public required int Position;
        /// <summary>
        /// 指示该Note是否为ExNote
        /// </summary>
        public required bool ExNote;
        /// <summary>
        /// 指示该Note是否为Break
        /// </summary>
        public required bool Break;

        public static Note[] operator + (Note a,Note b) => new Note[] {a,b};
        public static Note[] operator + (Note a,IEnumerable<Note> array)
        {
            var b = array.ToList();
            b.Add(a);
            return b.ToArray();
        }
        public static Note[] operator + (IEnumerable<Note> array,Note a) => a + array;

    }
    public class NoteCollection
    {
        public List<long> Ticks = new();
        Dictionary<long,Note[]> Notes = new();
        public Dictionary<long,double> BpmList = new();

        public Note[]? this[long tick] => Ticks.Contains(tick) ? Notes[tick] : null;

        public Note[]? this[long tick,int position]
        {
            get
            {
                if(Ticks.Contains(tick))
                {
                    var notes = this[tick];
                    var result = from note in notes
                                where note.Position == position
                                select note;
                    return result.ToArray();
                }
                else
                    return null;
            }
        }
        public void Add(Note note)
        {
            var tick = note.Tick;
            if(Ticks.Contains(tick))
            {
                var notes = Notes[tick].ToList();
                notes.Add(note);
                Notes[tick] = notes.ToArray();
            }
            else
            {
                Ticks.Add(tick);
                Notes.Add(tick,new Note[]{note});
            }
        }
        public void AddRange(IEnumerable<Note> notes)
        {
            foreach(var note in notes)
                Add(note);
        }
        
    }
}