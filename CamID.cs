using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WIA;

namespace CamCopy
{
    public class CamID
    {
        public const string IDFILENAME = "camids.txt";
        private Dictionary<string, int> numbers = new Dictionary<string, int>();

        public static string GetUniqueID(DeviceInfo dei)
        {
            string prop = (string)dei.Properties["PnP ID String"].get_Value();
            string[] tokens = prop.Split('#');
            if (tokens.Length != 4)
            {
                Console.WriteLine("Invalid PnP ID String: " + prop);
                return prop;
            }
            return tokens[2];
        }

        public CamID(DeviceInfos infos)
        {
            if (File.Exists(IDFILENAME))
            {
                try
                {
                    ReadIDFile(IDFILENAME);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error trying to read camera id file:");
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                if (infos.Count > 0)
                {
                    int count = 0;
                    foreach (DeviceInfo dei in infos)
                    {
                        if (dei.Type.Equals(WiaDeviceType.CameraDeviceType))
                        {
                            string id = GetUniqueID(dei);
                            numbers.Add(id, count++);
                        }
                    }
                    try
                    {
                        WriteIDFile(IDFILENAME);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error trying to write camera id file:");
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            Console.WriteLine();

            List<int> nums = new List<int>(numbers.Values);
            nums.Sort();

            List<string> unknownIDs = new List<string>();
            List<int> okNums = new List<int>();
            foreach (DeviceInfo dei in infos)
            {
                string id = GetUniqueID(dei);
                int num;
                if ((num = GetNumberForCamera(id)) < 0)
                    unknownIDs.Add(id);
                else
                {
                    nums.Remove(num);
                    okNums.Add(num);
                }
            }
            if (unknownIDs.Count > 0)
            {
                Console.WriteLine("These cameras are connected but have no configured number:");
                foreach(string s in unknownIDs)
                    Console.WriteLine(s);
                Console.WriteLine();
            }
            if (nums.Count > 0)
            {
                Console.WriteLine("These numbers are configured but the camera is not connected:");
                foreach(int n in nums)
                    Console.Write(n + " ");
                Console.WriteLine();
            }
            if (okNums.Count > 0)
            {
                Console.WriteLine("Using these camera numbers: ");
                foreach (int n in okNums)
                    Console.Write(n + " ");
            }
            else
            {
                Console.WriteLine("No camera ids configured.");
            }
            Console.WriteLine();
        }

        private void ReadIDFile(string filename)
        {
            Console.WriteLine("Using file for camera ids: " + filename);
            StreamReader reader = new StreamReader(File.OpenRead(filename));
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("#"))
                    continue;

                string[] tokens = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 2)
                    continue;

                int num;
                if (!int.TryParse(tokens[0], out num) || num < 0)
                {
                    Console.WriteLine("Cannot parse this line:");
                    Console.WriteLine(line);
                    continue;
                }
                if (numbers.Values.Contains(num))
                    throw new Exception("The camera id file contains a duplicate number: " + num);
                if (numbers.ContainsKey(tokens[1]))
                    throw new Exception("The camera id file contains a duplicate id: " + tokens[1]);

                numbers.Add(tokens[1], num);
            }
            reader.Close();
        }

        private void WriteIDFile(string filename)
        {
            Console.WriteLine("Writing camera id file with connected cameras: " + filename);
            StreamWriter writer = new StreamWriter(File.OpenWrite(filename));
            List<KeyValuePair<string, int>> entryList = new List<KeyValuePair<string, int>>(numbers);
            entryList.Sort(delegate(KeyValuePair<string, int> entryA, KeyValuePair<string, int> entryB) 
            {
                return entryA.Value.CompareTo(entryB.Value);
            });

            foreach (KeyValuePair<string, int> entry in entryList)
            {
                writer.WriteLine(entry.Value + "\t" + entry.Key);
            }
            writer.Close();
        }

        public int GetNumberForCamera(string uniqueDeviceID)
        {
            int r = -1;
            numbers.TryGetValue(uniqueDeviceID, out r);
            return r;
        }
    }
}
