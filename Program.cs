using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WIA;

namespace CamCopy
{
    public class Program
    {
        public static void NumberMode()
        {
            Console.WriteLine("Numbering connected cameras");
            List<string> ids = new List<string>();

            DeviceInfos dei = new DeviceManager().DeviceInfos;
            Console.WriteLine("Number of devices: " + dei.Count);
            Console.WriteLine();
            foreach (DeviceInfo info in dei)
            {
                try
                {
                    if (info.Type.Equals(WiaDeviceType.CameraDeviceType))
                    {
                        string prop = (string)info.Properties["Name"].get_Value();
                        Console.WriteLine("Name: " + prop);
                        prop = (string)info.Properties["Unique Device ID"].get_Value();
                        Console.WriteLine("Unique Device ID: " + prop);
                        prop = (string)info.Properties["PnP ID String"].get_Value();
                        Console.WriteLine("PnP ID String: " + prop);
                        ids.Add(CamID.GetUniqueID(info));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            ids.Sort();
            Console.WriteLine();
            Console.WriteLine("Numbers of connected cameras:");
            for (int i = 0; i < ids.Count; i++)
            {
                Console.WriteLine(i + "\t" + ids[i]);
            }

            Console.WriteLine();
            Console.WriteLine("Press 'q' to quit.");
            Console.WriteLine("Remove a camera and press any key to see which number it has.");
            Console.WriteLine();

            List<string> missingids = new List<string>(ids);

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.KeyChar == 'q')
                    break;

                dei = new DeviceManager().DeviceInfos;
                Console.WriteLine("Number of devices: " + dei.Count);
                foreach (DeviceInfo info in dei)
                {
                    try
                    {
                        if (info.Type.Equals(WiaDeviceType.CameraDeviceType))
                        {
                            string id = CamID.GetUniqueID(info);
                            missingids.Remove(id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                if (missingids.Count == 0)
                    Console.WriteLine("No cameras missing from original list.");
                else
                {
                    Console.WriteLine("Cameras that are missing now have following numbers:");
                    foreach (string s in missingids)
                    {
                        Console.WriteLine(ids.FindIndex(delegate(string str) { return s.Equals(str); }));
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Press 'q' to quit.");
                Console.WriteLine("Remove a camera and press any key to see which number it has.");
                Console.WriteLine();
            }
        }

        public static void DumpCamInfos()
        {
            DeviceInfos dei = new DeviceManager().DeviceInfos;
            Console.WriteLine("Number of connected devices: " + dei.Count);
            Console.WriteLine();

            foreach (DeviceInfo info in dei)
            {
                try
                {
                    if (info.Type.Equals(WiaDeviceType.CameraDeviceType))
                    {
                        CamCopy cc = new CamCopy(info);
                        CamCopy.PrintProperties(cc.Properties);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void Main(string[] args)
        {
            string folderArg = ".";
            bool delete = false;
            bool doCopy = true;
            bool crop = false;
            string position = "";
            string session = "";
            string fish = "";
            Random rnd = new Random();

            foreach (string arg in args)
            {
                if (arg.Length < 2)
                    continue;

                if (arg[0] == '-')
                {
                    if (arg == "-d" || arg == "-D")
                        delete = true;

                    if (arg == "-nocopy")
                        doCopy = false;
                    if (arg == "-crop")
                        crop = true;
                    if (arg == "-dump")
                    {
                        DumpCamInfos();
                        return;
                    }

                    if (arg[1] == 's' || arg[1] =='S')
                    {
                        int j = arg.Length;
                        session = "S" + arg.Substring(2, j - 2).PadLeft(4, '0') + "_";
                        folderArg = "S" + arg.Substring(2, j - 2).PadLeft(4, '0');
                    }

                    if (arg[1] == 'f' || arg[1] == 'F')
                    {
                        int j = arg.Length;
                        fish = "F" + arg.Substring(2, j - 2).PadLeft(4, '0') + "_";
                    }

                    if (arg[1] == 'p' || arg[1] == 'P')
                    {
                        int j = arg.Length;
                        position = "P" + arg.Substring(2, j - 2) + "_";
                    }

                }
            
                
                    else
                {
                    folderArg = arg;
                }
            }

            Console.WriteLine("Using image directory: " + folderArg);
            try
            {
                Directory.CreateDirectory(folderArg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not create image directory: " + folderArg);
                Console.WriteLine(ex.Message);
                return;
            }

            DeviceInfos dei = new DeviceManager().DeviceInfos;
            Console.WriteLine("Number of connected devices: " + dei.Count);
            CamID camid = new CamID(dei);
            Console.WriteLine();

            foreach (DeviceInfo info in dei)
            {
                try
                {
                    if (info.Type.Equals(WiaDeviceType.CameraDeviceType))
                    {
                        string id = CamID.GetUniqueID(info);
                        int num = camid.GetNumberForCamera(id);

                        if (num < 0)
                            continue;

                        string prop = (string)info.Properties["Name"].get_Value();
                        Console.WriteLine("Name: " + prop);
                        Console.WriteLine("Unique Device ID: " + id);
                        CamCopy cc = new CamCopy(info);
                        string leftRight = "empty";
                        if (num == 0) leftRight = "R";
                        if (num == 1) leftRight = "L";
                        if (num == 2) leftRight = "C";
                        string camera = leftRight + "_";
                        //string suffix = "CAM" + num.ToString("000") + "_";
                        Console.WriteLine("Using camera name: " + camera);

                        if (doCopy)
                            cc.SaveImages(folderArg, camera, position, session, fish, crop);

                        if (delete)
                            cc.DeleteImages();

                        Console.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
