using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace WA_Map
{
    public partial class WAMap : Form
    {

        private Thread tChecker;
       // private ManualResetEvent mrse = new ManualResetEvent(false);
        private Boolean threadRunning = true;
        private string path = Environment.GetEnvironmentVariable("UserProfile") + "\\AppData\\LocalLow\\Bossa Studios\\Worlds Adrift\\ddsdk\\events";
        private string outputpath = Environment.GetEnvironmentVariable("AppData") + "\\WAWatcher";
        private System.IO.StreamWriter logFile;
        public int gX = 0;
        public int gY = 0;
      

        public WAMap()
        {
            
            InitializeComponent();
            timer1.Start();

            if (Directory.Exists(path))
            {
                // Start the worker thread
                tChecker = new Thread(FileChecker);
                tChecker.Start();
            }
            else
            {
                MessageBox.Show("Cannot find World Adrift folder, run the game at least once and try again.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                // I would force close app, but form is not loaded yet...
            }


        }

        private void FileChecker()
        {
            // Compare the last modified date of the file, and read if changed
            long lastA = new System.IO.FileInfo(path + "\\a").Length;
            long lastB = new System.IO.FileInfo(path + "\\b").Length;
            long newA = new System.IO.FileInfo(path + "\\a").Length;
            long newB = new System.IO.FileInfo(path + "\\b").Length;

            while (threadRunning)
            {
                newA = new System.IO.FileInfo(path + "\\a").Length;
                newB = new System.IO.FileInfo(path + "\\b").Length;
                if (newA != lastA)
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff") + ": File A size changed to " + newA.ToString() + ".");
                    UnlockedRead(path + "\\a");
                    lastA = newA;
                }
                if (newB != lastB)
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff") + ": File B size changed to " + newB.ToString() + ".");
                    UnlockedRead(path + "\\b");
                    lastB = newB;
                }
                Thread.Sleep(1);
            }
        }

        private void UnlockedRead(string path)
        {
            // Open the file without locking it
            try
            {
                using (FileStream fs = new FileStream(path,
                                      FileMode.Open,
                                      FileAccess.Read,
                                      FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        ParseLine(sr.ReadToEnd());
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ParseLine(string line)
        {
            // The strings contain a JSON object, however it is surrounded by semi random characters.
            // Find matching top level brackets to find the JSON object.
            int startIndex = -1;
            int nestLevel = 0;
            int i = -1;
            foreach (char c in line)
            {
                i++;
                if (c == '{')
                {   // If this is a top level opening bracket, record index
                    if (startIndex < 0)
                        startIndex = i;
                    nestLevel++;
                }
                if (c == '}')
                {   // If this is a top level closing bracket, create a substring from startIndex to here.
                    nestLevel--;
                    if (nestLevel == 0)
                    {
                        try
                        {
                            string subString = line.Substring(startIndex, i - startIndex + 1);
                            // Deserialise the JSON object within these brackets
                            var ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                            var jsonObject = (IDictionary<string, object>)ser.DeserializeObject(subString);

                            // Check if this object is a performanceReport
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff") + ": Found JSON object with eventName: " + jsonObject["eventName"].ToString() + " and time: " + jsonObject["eventTimestamp"].ToString() + ".");
                            if (jsonObject["eventName"].ToString() == "performanceReport")
                            {   // Parse the performanceReport and build a string with just the timestamp and coordinates
                                // I had to manually convert the decimal signs since these are dynamic objects and formatting does not work on them.
                                var eventParams = (IDictionary<string, object>)jsonObject["eventParams"];
                                string timestamp = jsonObject["eventTimestamp"].ToString();
                                string x = eventParams["playerXCoord"].ToString();
                                string y = eventParams["playerYCoord"].ToString().Replace(',', '.');
                                string z = eventParams["playerZCoord"].ToString();
                                if (x != null) gX = (int)float.Parse(x);
                                if (z != null) gY = (int)float.Parse(z);
                                //string result = "Time: " + timestamp + " X: " + x + " Y: " + y + " Z: " + z;
                                //AppendText(result + '\n');
                             }
                            startIndex = -1;
                        }
                        catch (System.ArgumentException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }
        }


        /*private void button1_Click(object sender, EventArgs e)
        {
            if (!Mark)
            {
                pictureBox1.Visible = true;
                Mark = true;
            }
            if (Mark)
            {
               pictureBox1.Location = new Point(Convert.ToInt32(this.Width / 2 + (float.Parse(Map.gX) * this.Width)/
                    36000),

                    Convert.ToInt32(this.Height-(this.Height/2+(float.Parse(Map.gY) * this.Height) /
                    36000)));
                
            }
        }
        */

        private void timer1_Tick(object sender, EventArgs e)
        {

            pictureBox1.Location = new Point(
                (this.ClientRectangle.Width / 2 + (gX * this.ClientRectangle.Width / 36000)-10),
                //(this.Height - (this.Height / 2 + (Map.gY * this.Height / 36000))));
                (this.ClientRectangle.Height / 2 - (gY * this.ClientRectangle.Height / 36000)-10));
        }
    }
}
