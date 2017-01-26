using System;

namespace TinyDocDB
{
    public class TinyDocDB_UpdateEventArgs : EventArgs
    {
        public string updatedResourceOutput { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}
