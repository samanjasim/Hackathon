using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using System.IO;
using System.Globalization;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Convert CSV to Lists");
            List<Hackathon> hackList =  ConvertToList(@"..\..\Files\data.csv");
            Console.WriteLine();
            Console.Write("Start Analyze Data........");
            Console.WriteLine();
            AnalyzeData(hackList);
        }

        #region Convert CSV to List
        public static List<Hackathon> ConvertToList(string Path)
        {
            return File.ReadAllLines(Path)
                    .Select(y => y.Split(','))
                       .Select(x => new
                       {
                           CallDate = DateTime.Parse(x[0]),
                           CallerId = Guid.Parse(x[1]),
                           EmployeeId = int.Parse(x[2]),
                           DurtionFromRing = int.Parse(x[3]),
                           DurtionFromTalk= int.Parse(x[4]),
                           Status = x[5]
                       }).Select(x=> new Hackathon(x.CallDate, x.CallerId, x.EmployeeId, x.DurtionFromRing, x.DurtionFromTalk, x.Status))
                           .ToList();

        }
        #endregion

        #region Analyze Data
        public static void AnalyzeData(List<Hackathon> hackList)
        {
            Dictionary<string, int> peakIncoming = new Dictionary<string, int>();
            Dictionary<string, int> peakSimultionDuration = new Dictionary<string, int>();
            Dictionary<string, int> relationDict = new Dictionary<string, int>();
            Dictionary<string, int> mostProdEmployeeDict = new Dictionary<string, int>();
            Dictionary<string, int> lessProdEmployeeDict = new Dictionary<string, int>();
            Dictionary<string, int> callerDict = new Dictionary<string, int>();
            
            hackList.ForEach(x =>
            {
                string whichTime = x.CallDate.ToString().Split(' ').ToList().Last();
                int EmployeeId = x.EmployeeId;
                Guid CallerId = x.CallerId;

                // peak Incoming Calling Muinte
                peakIncoming = AddPeakIncoming(peakIncoming, x.CallDate, whichTime);

                // peak Simultion Duration
                int Duration = x.DurtionFromRing;
                int WaitingTime = x.DurtionFromRing - x.DurtionFromTalk;
                string date = x.CallDate.Year + "/" + x.CallDate.Month + "/" + x.CallDate.Day + " " + x.CallDate.Hour + ":" + (x.CallDate.Minute + (WaitingTime > 59 ? 1 : 0)) + " " + whichTime;
                peakSimultionDuration = AddPeakSimultionCalls(peakSimultionDuration, date, Duration);

                // Relation Ship
                relationDict = CheckRelation(relationDict, EmployeeId, CallerId);
                
                // min & most Production Employee
                if(x.Status == "ANSWERED")
                {
                    mostProdEmployeeDict = AddProdEmployee(mostProdEmployeeDict, EmployeeId);
                }
                lessProdEmployeeDict = AddLessEmployee(lessProdEmployeeDict, EmployeeId);

                // Min & Most Client Caller
                callerDict = AddClient(callerDict, CallerId);

            });

            ///////
            int maxIncoming = peakIncoming.Values.Max();
            string maxIncomeDate = peakIncoming.Where(x => x.Value == maxIncoming).First().Key;
            Console.WriteLine();
            Console.Write("1: max Income Call Time= " + maxIncomeDate + " // Income Call Count= " + maxIncoming);
            Console.WriteLine();

            ///////
            int Durtaion = peakSimultionDuration.Values.Max();
            string maxSimultion = peakSimultionDuration.Where(x => x.Value == Durtaion).First().Key;
            Console.WriteLine();
            Console.Write("2: peak Simulation Date= " + maxSimultion ); //+ " // peak Simultion Duration= " + Durtaion
            Console.WriteLine();

            ///////
            int maxRelation = relationDict.Values.Max();
            string clientAndEmpId = relationDict.Where(x => x.Value == maxRelation).First().Key;
            Console.WriteLine();
            Console.Write("3: a Relation between " + clientAndEmpId + " // No. of Calling= " + maxRelation);
            Console.WriteLine();

            ///////
            int maxEmployeeNo = mostProdEmployeeDict.Values.Max();
            string maxEmpId = mostProdEmployeeDict.Where(x => x.Value == maxEmployeeNo).First().Key;
            Console.WriteLine();
            Console.Write("4: most Production EmployeeId with calls= " + maxEmpId + " // no. of call= " + maxEmployeeNo);
            Console.WriteLine();

            int minEmployeeNo = lessProdEmployeeDict.Values.Min();
            string minEmpId = lessProdEmployeeDict.Where(x => x.Value == minEmployeeNo).First().Key;
            Console.WriteLine();
            Console.Write("5: less Production EmployeeId with calls= " + minEmpId + " // no. of call= " + minEmployeeNo);
            Console.WriteLine();

            /////// 
            int maxClientCallsNo = callerDict.Values.Max();
            string maxCallerId = callerDict.Where(x => x.Value == maxClientCallsNo).First().Key;
            Console.WriteLine();
            Console.Write("6: max ClientId with calls= " + maxCallerId + " // no. of call= " + maxClientCallsNo);
            Console.WriteLine();
            //Console.ReadKey();

            int lessClientCallsNo = callerDict.Values.Min();
            string lessCallerId = callerDict.Where(x => x.Value == lessClientCallsNo).First().Key;
            Console.WriteLine();
            Console.Write("7: less ClientId with calls= " + lessCallerId + " // no. of call= " + lessClientCallsNo);
            Console.WriteLine();
            Console.ReadKey();
            // Console.Write("peakDate= " + peakDate);
        }
        #endregion

        #region CheckRelation
        public static Dictionary<string, int> CheckRelation(Dictionary<string, int>  hackDic,int EmployeeId,Guid CallerId)
        {
            if (!hackDic.ContainsKey("ClientId= " + CallerId + " / EmployeeId= " + EmployeeId))
            {
                hackDic.Add("ClientId= " + CallerId + " / EmployeeId= " + EmployeeId, 1);
            }
            else
            {
                hackDic["ClientId= " + CallerId + " / EmployeeId= " + EmployeeId] += 1;
            }
            return hackDic;
        }
        #endregion

        #region Employee
        public static Dictionary<string, int> AddProdEmployee(Dictionary<string, int> hackDic, int EmployeeId)
        {
            if (!hackDic.ContainsKey("" + EmployeeId))
            {
                hackDic.Add("" + EmployeeId, 1);
            }
            else
            {
                hackDic["" + EmployeeId] += 1;
            }
            return hackDic;
        }

        public static Dictionary<string, int> AddLessEmployee(Dictionary<string, int> hackDic, int EmployeeId)
        {
            if (!hackDic.ContainsKey("" + EmployeeId))
            {
                hackDic.Add("" + EmployeeId, 1);
            }
            else
            {
                hackDic["" + EmployeeId] += 1;
            }
            return hackDic;
        }
        #endregion

        #region AddClient
        public static Dictionary<string, int> AddClient(Dictionary<string, int> hackDic, Guid CallerId)
        {
            if (!hackDic.ContainsKey("" + CallerId))
            {
                hackDic.Add("" + CallerId, 1);
            }
            else
            {
                hackDic["" + CallerId] += 1;
            }
            return hackDic;
        }
        #endregion

        #region AddPeakIncoming
        public static Dictionary<string, int> AddPeakIncoming(Dictionary<string, int> hackDic, DateTime CallDate, string whichTime)
        {
            if (!hackDic.ContainsKey(CallDate.ToString("yyyy/MM/dd h:mm") + " " + whichTime))
            {
                hackDic.Add(CallDate.ToString("yyyy/MM/dd h:mm") + " " + whichTime, 1);
            }
            else
            {
                hackDic[CallDate.ToString("yyyy/MM/dd h:mm") + " " + whichTime] += 1;
            }
            return hackDic;
        }
        #endregion

        #region AddPeakSimultionCalls
        public static Dictionary<string, int> AddPeakSimultionCalls(Dictionary<string, int> hackDic, string date,int Duration)
        {
            if (!hackDic.ContainsKey(date))
            {
                hackDic.Add(date, Duration);

                // hackDic.Add(x.CallDate.ToString("yyyy/MM/dd h:mm") + " " + whichTime, Duration);
            }
            else
            {
                hackDic[date] += Duration;
            }
            return hackDic;
        }
        #endregion

    }



    class Hackathon :IComparable<Hackathon>
    {
        public DateTime CallDate { get; set; }
        public Guid CallerId { get; set; }
        public int EmployeeId  { get; set; }
        public int DurtionFromRing { get; set; }
        public int DurtionFromTalk { get; set; }
        public string Status { get; set; }
        public int Count { get; set; }


        public Hackathon(DateTime CallDate, Guid CallerId, int EmployeeId, int DurtionFromRing, int DurtionFromTalk, string Status)
        {
            this.CallDate = CallDate;
            this.CallerId = CallerId;
            this.EmployeeId = EmployeeId;
            this.DurtionFromRing = DurtionFromRing;
            this.DurtionFromTalk = DurtionFromTalk;
            this.Status = Status;
            this.Count = 0;
        }

        public Hackathon()
        {

        }
        public int CompareTo(Hackathon obj)
        {
            if(this.CallDate == obj.CallDate)
            {
                return this.CallDate.CompareTo(obj.CallDate);
            } 
            return obj.CallDate.CompareTo(this.CallDate);
        }
    }
}
