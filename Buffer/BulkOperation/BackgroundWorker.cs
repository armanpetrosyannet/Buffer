using BufferCashe.Buffer;
using Reciver.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Reciver.BulkOperation
{
    public class BackgroundWorker
    {
        private IEnumerable<Dictionary<ObjectTypeEnum, List<ObjectChange>>> AllData;
        private List<ObjectChange> _Added;
        private List<ObjectChange> _Deleted;
        private List<ObjectChange> _Updated;


        public BackgroundWorker(IEnumerable<Dictionary<ObjectTypeEnum, List<ObjectChange>>> list)
        {
            AllData = list;
            _Added = new List<ObjectChange>();
            _Deleted = new List<ObjectChange>();
            _Updated = new List<ObjectChange>();
        }

        private bool _Seperate()
        {
            try
            {
                var _all = AllData.SelectMany(e => e.Values).SelectMany(w => w).ToList();
                _Added = _all.Where(e => e.OperationKind == OperationKindEnum.Added).ToList();
                _Deleted = _all.Where(e => e.OperationKind == OperationKindEnum.Deleted).ToList();
                _Updated = _all.Where(e => e.OperationKind == OperationKindEnum.Modified).ToList();

            }
            catch (Exception e)
            {
                throw new Exception("Internal error " + e.Message);
            }
            return true;

        }

        public void ExecuteBulk()
        {
            _Seperate();

            if (_Added.Count > 0)
            {
                var name = Enum.GetName(typeof(ObjectTypeEnum), AllData.FirstOrDefault().Keys.FirstOrDefault());


                BulkInsertCommand command = new BulkInsertCommand(_Added, name);
                {
                    command.Run();
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Added--" + name + "---" + _Added.Count);
                Console.ResetColor();
            }

            if (_Updated.Count > 0)
            {
                var name = Enum.GetName(typeof(ObjectTypeEnum), AllData.FirstOrDefault().Keys.FirstOrDefault());

                BulkUbdateCommand command = new BulkUbdateCommand(_Updated, name);
                {
                    command.Run();
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Updated--" + name + "---" + _Updated.Count);
                Console.ResetColor();

            }

            if (_Deleted.Count > 0)
            {
                Console.WriteLine("Delete-----");

            }


        }


    }
}
