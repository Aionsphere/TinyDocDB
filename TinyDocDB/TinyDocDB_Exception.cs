using System;

namespace TinyDocDB
{
    class TinyDocDB_Exception : Exception
    {
        public TinyDocDB_Exception()
        {
        }

        public TinyDocDB_Exception(string message, Exception ex) : base(message, ex)
        {
        }

        public TinyDocDB_Exception(string message): base(message)
        {
        }
    }
}
