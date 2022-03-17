using NewRelic.Api.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared_remoting_lib
{
    public class RemoteObject : MarshalByRefObject
    {
        private int callCount = 0;

        [Transaction]
        public int GetCount()
        {
            Console.WriteLine("GetCount has been called.");
            callCount++;
            return (callCount);
        }
    }
}
