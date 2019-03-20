using System;
using System.Collections.Generic;
using System.Text;

namespace BufferCashe.Buffer
{
    /// <summary>
    /// Use for initialize single buffer
    /// </summary>    
    public abstract class BufferParametrs
    {
        /// <summary>
        /// Max count for Buffer
        /// </summary>
        public int MaxCount { get; set; }
        /// <summary>
        /// Name buffer
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Time out for buffer
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// assigme in buffer
        /// </summary>
        public DateTime _startDate;

        /// <summary>
        /// Chack
        /// </summary>
        /// <returns> True when timeout</returns>
        public bool CheckTimeOut()
        {
            var data = DateTime.Now;
            if ((data - _startDate).Seconds > TimeOut)
            {
#if DEBUG
                Console.WriteLine("Timeot" + "---" + Name);
#endif

                return true;
            }
            return false;
        }

        public bool CheckSize(int _size)
        {
            return MaxCount < _size;
        }

        public bool InProcess;

    }
}
