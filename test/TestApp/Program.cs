using System;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using Service.TimeLogger.Client;
using Service.TimeLogger.Grpc.Models;

namespace TestApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            Console.Write("Press enter to start");
            Console.ReadLine();

            var factory = new TimeLoggerClientFactory("http://localhost:5001");
            var client = factory.GetTimeLoggerService();

            //var resp = await  client.SayHelloAsync(new StartLoggingGrpcRequest(){Name = "Alex"});
            //Console.WriteLine(resp?.Message);

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
