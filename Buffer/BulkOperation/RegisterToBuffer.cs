using BufferCashe.Buffer;
using Reciver.Extensions;
using Reciver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Reciver.BulkOperation
{
    public class RegisterToBuffer
    {

        private BufferGeneric<BufferParametrs, Dictionary<ObjectTypeEnum, List<ObjectChange>>> _buffer;
        public RegisterToBuffer(BufferGeneric<BufferParametrs, Dictionary<ObjectTypeEnum, List<ObjectChange>>> buffer)
        {
            _buffer = buffer;
            Initialize();
        }

        public void Initialize()
        {
            var keys = BufferExtension.ReadParametrs().Select(e => e.Name).ToList();

            foreach (var item in keys)
            {
                _buffer.DataFillEvents[item] +=
                    (e =>
                    {
                        var _worker = new BackgroundWorker(e);
                        _worker.ExecuteBulk();

                    });
            }

        }




    }
}
