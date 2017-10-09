using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WIA;
using System.Drawing;
using System.Drawing.Imaging;



namespace CamCopy
{
    public class CamCopy
    {
        private Device device;

        public CamCopy(DeviceInfo deviceInfo)
        {
            device = deviceInfo.Connect();
        }

        public Properties Properties { get { return device.Properties; } }

        public int ItemCount { get { return device.Items.Count; } }

        public void SaveImages(string folder, string camera, string position, string session, string fish, bool crop)
        {
            Random rnd = new Random();
            int count = 0;
            foreach (Item item in device.Items)
            {
                if (IsImageItem(item))
                {
                    WIA.ImageFile ifile2 = item.Transfer() as WIA.ImageFile;
                    byte[] imageBytes = (byte[])ifile2.FileData.get_BinaryData(); // <-- Converts the ImageFile to a byte array
                    MemoryStream ms = new MemoryStream(imageBytes);
                    Image image = Image.FromStream(ms);

                    PropertyItem propItem = image.GetPropertyItem(36867);
                    DateTime dtaken;
                    string sdate = Encoding.UTF8.GetString(propItem.Value).Trim();
                    string secondhalf = sdate.Substring(sdate.IndexOf(" "), (sdate.Length - sdate.IndexOf(" ")));
                    string firsthalf = sdate.Substring(0, 10);
                    firsthalf = firsthalf.Replace(":", "-");
                    sdate = firsthalf + secondhalf;
                    dtaken = DateTime.Parse(sdate);

                    string timestamp = dtaken.Year.ToString("0000") + "_" +
                                        dtaken.Month.ToString("00") + "_" +
                                        dtaken.Day.ToString("00") + "-" +
                                        dtaken.Hour.ToString("00") + "_" +
                                        dtaken.Minute.ToString("00") + "_" +
                                        dtaken.Second.ToString("00") + "_";

                    string imagenumber = "IMG" + (count + 1).ToString("0000")+"_" ;
                    string randomnumber = rnd.Next(99999999).ToString("00000000"); 

                    string itemName =   session + 
                                        camera +
                                        position +
                                        imagenumber + 
                                        fish  +  
                                        timestamp +
                                        randomnumber + 
                                        ".jpg"  ;

                    string fileName = itemName;

                    fileName = Path.ChangeExtension(fileName, "jpg");
                    fileName = Path.Combine(folder, fileName);

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    if (camera == "L_")
                    {
                        image.RotateFlip(RotateFlipType.Rotate180FlipY);
                    }

                    if (crop) //crop image to center third.
                    {
                        image = cropImage(image, new Rectangle(0, (int)image.Height/3, image.Width, (int)image.Height/3*2));
                    }

                    // ImageFile ifile = (ImageFile)item.Transfer();


                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 90L);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    //ifile.SaveFile(fileName);
                    var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    image.Save(fileName, encoder , myEncoderParameters );
                    Console.WriteLine("Copy: " + fileName);
                    count++;
                }
            }
            Console.WriteLine("Files saved: " + count);
        }

        public void DeleteImages()
        {
            int count = 0;
            foreach (Item item in device.Items)
            {
                if (IsImageItem(item))
                {
                    Delete(item);
                    count++;
                }
            }
            Console.WriteLine("Items deleted from camera: " + count);
        }

        public void Delete(Item item)
        {
            IWiaItem iwia = (IWiaItem)item.WiaItem;
            iwia.DeleteItem(0); // int parameter is not used
        }

        public static bool IsImageItem(Item item)
        {
            Property p = item.Properties["Item Flags"];
            int k = (int)p.get_Value();
            return (k & (int)WiaItemFlag.ImageItemFlag) != 0;
        }

        public static void PrintProperties(Properties props)
        {
            Console.WriteLine("Properties dump:");
            foreach (Property p in props)
            {
                Console.Write(p.Name);
                Console.WriteLine(":\t" + p.get_Value());
            }
            Console.WriteLine();
        }
        private static Image cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }
    }
}
