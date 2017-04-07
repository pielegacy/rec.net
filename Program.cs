using System;
using System.IO;
using System.Threading.Tasks;
using REC;

namespace REC_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "REC Client";
            if (!Directory.Exists("Output"))
                Directory.CreateDirectory("Output");
#if DEBUG
            RECConn conn = new RECConn(new DateTime(2015, 12, 12), new DateTime(2015, 12, 18));
#else
            RECConn conn = new RECConn();
#endif
            Console.WriteLine("Beginning download, do not press any key until prompted to");
            conn.ProcessRange();
            while (conn.Loading)
                Console.ReadKey();
        }
    }
}
