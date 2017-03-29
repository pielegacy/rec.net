using System;
using System.IO;
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
            RECConn conn = new RECConn();
            Console.WriteLine("Beginning download, do not press any key until prompted to");
            conn.ProcessRange();
            while (conn.Loading)
                Console.ReadKey();
        }
    }
}
