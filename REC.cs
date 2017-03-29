using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Data.SqlClient;

namespace REC
{
    ///<summary>
    /// The main class for the REC client, contains methods for downloading and exporting the data
    ///</summary>
    public class RECConn
    {
        public bool Loading = false;
        public const string API_LINK = "https://www.rec-registry.gov.au/rec-registry/app/api/public-register/certificate-actions?date=";
        private DateTime _from { get; set; }
        private DateTime _to { get; set; }
        public List<REC> Payload = new List<REC>();
        public RECConn(DateTime from, DateTime to)
        {
            _from = from;
            _to = to;
            Console.WriteLine($"{_from.ToString("yyyy-MM-dd")} to {_to.ToString("yyyy-MM-dd")}:");
        }
        public RECConn() : this(RECFormat.DateAUS("Enter start date (dd/mm/yyyy):"), RECFormat.DateAUS("Enter end date (non inclusive) (dd/mm/yyyy):)"))
        {
        }
        public async void ProcessRange()
        {
            Loading = true;
            DateTime current = _from;
            await Task.Run(async () =>
            {
                while (current != _to)
                {
                    var currentRec = await PullRec(current);
                    currentRec.Result.ForEach(r => Payload.Add(r));
                    current = current.AddDays(1);
                }
                SaveData();
            }
            );
            Loading = false;
            Console.WriteLine("Download finished, press any key to close the program");
        }
        public async Task<RECQuery> PullRec(DateTime day)
        {
            using (HttpClient client = new HttpClient())
            {
                string endpoint = $"{API_LINK}{day.ToString("yyyy-MM-dd")}";
                var result = JsonConvert.DeserializeObject<RECQuery>(await client.GetStringAsync(endpoint));
                Console.WriteLine($"Downloaded REC for {day.ToString("dd-MM-yyyy")}");
                return result;
            }
        }
        public void SaveData()
        {
            Console.WriteLine("Saving (This may take a while)...");
            string output = "";
            output += String.Join(",", Payload[0].CertificateRanges[0].Keys.ToList()) + ",certificateQuantity" + "\n";
            foreach (var req in Payload)
            {
                req.CertificateRanges.ForEach(c =>
                {
                    try
                    {
                        c["certificateQuantity"] = $"{(Convert.ToInt32(c["endSerialNumber"]) + 1) - Convert.ToInt32(c["startSerialNumber"])}";
                    }
                    catch
                    {
                        Console.WriteLine("Error Calculating Quantity, carrying on...");
                    }
                    output += String.Join(",", c.Values.ToList()) + "\n";
                });
            }
            Random rand = new Random();
            try
            {
                File.WriteAllText($"Output/{_from.ToString("dd-MM-yyyy")}_{_to.ToString("dd-MM-yyyy")}.csv", output);
            }
            catch
            {
                File.WriteAllText($"Output/{_from.ToString("dd-MM-yyyy")}_{_to.ToString("dd-MM-yyyy")}_{rand.Next(1, 100)}.csv", output);
            }
            Console.WriteLine("Saved...");
        }
    }
    public class RECQuery
    {
        public string Status { get; set; }
        public List<REC> Result { get; set; }
    }
    ///<summary>
    /// The model for the REC Certificate
    ///</summary>
    public class REC
    {
        public string ActionType { get; set; }
        public DateTime CompletedTime { get; set; }
        public List<Dictionary<string, string>> CertificateRanges { get; set; }

    }
    public class AccessConn
    {
        public AccessConn()
        {
            using (var connection = new SqlConnection(@"Provider = Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\Alex\Documents\Test.accdb;"))
            {
                SqlCommand comm = new SqlCommand("select * from TableNames;", connection);
                connection.Open();
                var reader = comm.ExecuteReader();
                while (reader.Read())
                    Console.WriteLine(JsonConvert.SerializeObject(reader.GetString(0)));
            }
        }
    }
    public static class RECFormat
    {
        ///<summary>
        /// Take a date as dd/mm/yy
        ///</summary>
        public static DateTime DateAUS(string prompt = "")
        {
            if (prompt != "")
                Console.WriteLine(prompt);
            string input = Console.ReadLine();
            string[] inputFields = input.Split('/');
            return new DateTime(Convert.ToInt32(inputFields[2]), Convert.ToInt32(inputFields[1]), Convert.ToInt32(inputFields[0]));
        }
    }
}