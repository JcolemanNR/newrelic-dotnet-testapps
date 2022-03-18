using NewRelic.Api.Agent;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        [Transaction]
        public string CreateFailedDbConnection()
        {
            Console.WriteLine("CreateFailedDbConnection has been called.");
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
    }
}
