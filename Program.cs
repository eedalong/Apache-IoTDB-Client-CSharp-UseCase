using System;
using Apache.IoTDB;
using Apache.IoTDB.DataStructure;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace iotdb_test
{
    class Program
    {
        static string host = "localhost";
        static int port = 6667;
        static string user = "root";
        static string passwd = "root";
        static int fetch_size = 50;
        static int processed_size = 4;
        static bool debug = false;
        static int pool_size = 1;
        static async Task TestInsertRecord()
        {
            
            var session_pool = new SessionPool(host, port, pool_size);
            int status;
            await session_pool.Open(false);
            if (debug) session_pool.OpenDebugMode();

            System.Diagnostics.Debug.Assert(session_pool.IsOpen());
            status = await session_pool.DeleteStorageGroupAsync("root.97209_TEST_CSHARP_CLIENT_GROUP");

            status = await session_pool.CreateTimeSeries(
                "root.97209_TEST_CSHARP_CLIENT_GROUP.TEST_CSHARP_CLIENT_DEVICE.TEST_CSHARP_CLIENT_TS1", TSDataType.TEXT,
                TSEncoding.PLAIN, Compressor.UNCOMPRESSED);

            System.Diagnostics.Debug.Assert(status == 0);
            status = await session_pool.CreateTimeSeries(
                "root.97209_TEST_CSHARP_CLIENT_GROUP.TEST_CSHARP_CLIENT_DEVICE.TEST_CSHARP_CLIENT_TS2",
                TSDataType.BOOLEAN, TSEncoding.PLAIN, Compressor.UNCOMPRESSED);
            System.Diagnostics.Debug.Assert(status == 0);
            status = await session_pool.CreateTimeSeries(
                "root.97209_TEST_CSHARP_CLIENT_GROUP.TEST_CSHARP_CLIENT_DEVICE.TEST_CSHARP_CLIENT_TS3",
                TSDataType.INT32, TSEncoding.PLAIN, Compressor.UNCOMPRESSED);
            System.Diagnostics.Debug.Assert(status == 0);
            var measures = new List<string>
                {"TEST_CSHARP_CLIENT_TS1", "TEST_CSHARP_CLIENT_TS2", "TEST_CSHARP_CLIENT_TS3"};
            var values = new List<object> {"test_text", true, (int) 123};
            var tasks = new List<Task<int>>();
            var start_ms = DateTime.Now.Ticks / 10000;
            for (var timestamp = 1; timestamp <= fetch_size * processed_size; timestamp++)
            {
                var rowRecord = new RowRecord(timestamp, values, measures);
                var task = session_pool.InsertRecordAsync(
                    "root.97209_TEST_CSHARP_CLIENT_GROUP.TEST_CSHARP_CLIENT_DEVICE", rowRecord);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            var end_ms = DateTime.Now.Ticks / 10000;
            Console.WriteLine(string.Format("total insert record time is {0}", end_ms - start_ms));
            status = await session_pool.DeleteStorageGroupAsync("root.97209_TEST_CSHARP_CLIENT_GROUP");
            await session_pool.Close();
            Console.WriteLine("TestInsertRecordAsync Passed");
        }
        static void Main(string[] args)
        {
            var task = TestInsertRecord();
            task.Wait();
           
        }
    }
}
