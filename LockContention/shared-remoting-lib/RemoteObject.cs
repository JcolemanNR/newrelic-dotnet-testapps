using NewRelic.Api.Agent;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;

namespace shared_remoting_lib
{
    public class RemoteObject : MarshalByRefObject
    {
        private PerformanceCounter _currentLogicalThreads = new PerformanceCounter(".NET CLR LocksAndThreads", "# of current logical Threads", "lock-consoleapp");
        private PerformanceCounter _contentionRatePerSecond = new PerformanceCounter(".NET CLR LocksAndThreads", "Contention Rate / sec", "lock-consoleapp");

        private int callCount = 0;

        [Transaction]
        public int GetCount()
        {
            Console.WriteLine("GetCount has been called.");
            callCount++;
            TrySendEvent(nameof(GetCount));
            return (callCount);
        }

        [Transaction]
        public string CreateFailedDbConnection()
        {
            Console.WriteLine("CreateFailedDbConnection has been called.");
            TrySendEvent(nameof(CreateFailedDbConnection));

            try
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = "Server=localhost;Database=NotGonnaExist;Trusted_Connection=true";
                    conn.Open();
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            return "somehow it worked???";
        }

        private void TrySendEvent(string name)
		{
            var data = new Dictionary<string, object>
            {
                { "MethodName", name},
                { "CurrentLogicalThreads", _currentLogicalThreads.NextValue()},
                { "ContentionRatePerSecond", _contentionRatePerSecond.NextValue()}
            };
            if (Convert.ToDouble(data["ContentionRatePerSecond"]) != 0)
            {
                NewRelic.Api.Agent.NewRelic.RecordCustomEvent("perfmon", data);
            }
        }
    }
}
