using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BufferCashe.Buffer
{
    /// <summary>
    /// Buffer for cache many information and transfer
    /// </summary>
    /// <typeparam name="T1"> Single buffer key type</typeparam>
    /// <typeparam name="T2">Single buffer  type</typeparam>
    public class BufferGeneric<T1, T2> where T1 : BufferParametrs
    {
        private ConcurrentDictionary<string, ConcurrentQueue<T2>> _buffers { get; set; }

        private ConcurrentDictionary<string, T1> _internalBuffersInformation;

        public Dictionary<string, Action<IEnumerable<T2>>> DataFillEvents { get; private set; }


        public BufferGeneric(IEnumerable<T1> bufferParametrs)
        {
            _buffers = new ConcurrentDictionary<string, ConcurrentQueue<T2>>();
            _internalBuffersInformation = new ConcurrentDictionary<string, T1>();
            DataFillEvents = new Dictionary<string, Action<IEnumerable<T2>>>();
            InitializeBuffers(bufferParametrs);
        }


        private void InitializeBuffers(IEnumerable<T1> bufferParametrs)
        {
            foreach (var parametr in bufferParametrs)
            {
                ConcurrentQueue<T2> _queue = new ConcurrentQueue<T2>();
                _buffers.AddOrUpdate(parametr.Name, _queue, (e, s) => s);
                _internalBuffersInformation.AddOrUpdate(parametr.Name, parametr, (e, s) => s);
                DataFillEvents[parametr.Name] = (e => { Console.WriteLine(parametr.Name + "---" + "geted"); });
            }
        }

        public void TryFillBuffer(string key, T2 value)
        {
            _buffers[key].Enqueue(value);

            var _buffercount = _buffers[key].Count;

            if (_CheckBuffer(key, _buffercount))
            {
                _internalBuffersInformation[key].InProcess = true;

                ThreadPool.QueueUserWorkItem(
                    e =>
                    {
                        DataFillEvents[key]?.Invoke(TryGetTop(_buffercount, _buffers[key], _internalBuffersInformation[key]));
                    });

                _ResetTime(key);

            }

        }

        private bool _CheckBuffer(string key, int count)
        {
            var _t = _internalBuffersInformation[key];
            return (_t.CheckTimeOut() || _t.CheckSize(count)) && !_t.InProcess;
        }


        private void _ResetTime(string key)
        {
            _internalBuffersInformation[key]._startDate = DateTime.Now;
        }


        public IEnumerable<T2> TryGetTop(int x, ConcurrentQueue<T2> _queue, BufferParametrs parametrs)
        {
            int count = 0;
            var internalQueue = new List<T2>();

            while (x > count)
            {
                if (_queue.TryDequeue(out T2 item))
                {
                    internalQueue.Add(item);
                    count++;
                }
            }
            parametrs.InProcess = false;
            return internalQueue;
        }


    }





}
