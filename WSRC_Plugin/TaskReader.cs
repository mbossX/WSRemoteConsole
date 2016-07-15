using System.IO;
using System.Threading;

namespace WSRC_Plugin
{
    public class TaskReader : TextReader
    {
        private AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private TextReader _reader;
        private string _text;

        public void SendText(string text)
        {
            _text = text;
            _resetEvent?.Set();
        }

        public TaskReader(TextReader textReader)
        {
            _reader = textReader;
        }

        public override string ReadLine()
        {
            _resetEvent.WaitOne();
            return _text;
        }
    }
}
