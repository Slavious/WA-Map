using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class WAWatcher : Form
    {
        private Thread tChecker;
        private ManualResetEvent mrse = new ManualResetEvent(false);
        private Boolean threadRunning = true;
        private Boolean running = false;
        private Boolean soundOn = false;
        private Boolean loggingOn = false;
        private string path = Environment.GetEnvironmentVariable("UserProfile") + "\\AppData\\LocalLow\\Bossa Studios\\Worlds Adrift\\ddsdk\\events";
        private string outputpath = Environment.GetEnvironmentVariable("AppData") + "\\WAWatcher";
        private System.IO.StreamWriter logFile;

        public WAWatcher()
        {
            InitializeComponent();

            if (Directory.Exists(path))
            {
                // Start the worker thread
                tChecker = new Thread(fileChecker);
                tChecker.Start();
            }
            else
            {
                MessageBox.Show("Cannot find World Adrift folder, run the game at least once and try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                // I would force close app, but form is not loaded yet...
            }
        }
        // UI events
        private void btnStart_Click(object sender, EventArgs e)
        {
            // Toggle state of start / stop button
            if (!running)
            {
                // Begin watching.
                tbResults.AppendText("Running...\n");
                btnStart.Text = "Stop";
                running = true;

                if(loggingOn)
                {
                    logFile = new System.IO.StreamWriter(outputpath + "\\log_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv");
                }

                mrse.Set();
            }
            else
            {
                // Stop watching.
                tbResults.AppendText("Stopped.\n");
                btnStart.Text = "Start";
                running = false;

                if (loggingOn)
                {
                    logFile.Close();
                }

                mrse.Reset();
            }

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // Clear the text box
            tbResults.Clear();
        }

        private void chkSound_CheckedChanged(object sender, EventArgs e)
        {
            // if enabled, a sound will play on each coordinate found
            soundOn = chkSound.Checked;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            // Opens the folder that contains the log file
            System.Diagnostics.Process.Start(@outputpath);
        }

        private void chkFile_CheckedChanged(object sender, EventArgs e)
        {
            // if enabled, a csv file will be created with all the recorded data
            loggingOn = chkFile.Checked;
        }

        private void WAWatcher_FormClosed(object sender, FormClosedEventArgs e)
        {   
            // Makes sure that the threads are closed
            threadRunning = false;
            mrse.Set();
        }

        private void fileChecker()
        {
            // Compare the last modified date of the file, and read if changed
            long lastA = new System.IO.FileInfo(path + "\\a").Length;
            long lastB = new System.IO.FileInfo(path + "\\b").Length;
            long newA = new System.IO.FileInfo(path + "\\a").Length;
            long newB = new System.IO.FileInfo(path + "\\b").Length;

            while (threadRunning)
            {
                mrse.WaitOne();
                newA = new System.IO.FileInfo(path + "\\a").Length;
                newB = new System.IO.FileInfo(path + "\\b").Length;
                if (newA != lastA)
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff") + ": File A size changed to " + newA.ToString() + ".");
                    unlockedRead(path + "\\a");
                    lastA = newA;
                }
                if (newB != lastB)
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff") + ": File B size changed to " + newB.ToString() + ".");
                    unlockedRead(path + "\\b");
                    lastB = newB;
                }
                Thread.Sleep(1);
            }
        }

        // Other functions
        private void unlockedRead(string path)
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
                        parseLine(sr.ReadToEnd());
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void parseLine(string line)
        {
            // The strings contain a JSON object, however it is surrounded by semi random characters.
            // Find matching top level brackets to find the JSON object.
            int startIndex = -1;
            int nestLevel = 0;
            int i = -1;
            foreach (char c in line)
            {
                i++;
                if (c=='{')
                {   // If this is a top level opening bracket, record index
                    if (startIndex < 0)
                        startIndex = i;
                    nestLevel++;
                }
                if (c == '}')
                {   // If this is a top level closing bracket, create a substring from startIndex to here.
                    nestLevel--;
                    if (nestLevel ==0)
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
                            string x = eventParams["playerXCoord"].ToString().Replace(',', '.');
                            string y = eventParams["playerYCoord"].ToString().Replace(',', '.');
                            string z = eventParams["playerZCoord"].ToString().Replace(',', '.');

                            string result = "Time: " + timestamp + " X: " + x + " Y: " + y + " Z: " + z;
                            appendText(result + '\n');
                            if(loggingOn)
                            {
                                // Write the string to a file.
                                logFile.WriteLine(timestamp + ", " + x + ", " + y + ", " + z);
                            }
                            if (soundOn) // Play a sound if the option is enabled
                                System.Media.SystemSounds.Asterisk.Play();
                        }
                        startIndex = -1;
                    }
                }
            }
        }

        // Delegate function to ensure this runs on interface thread
        private void appendText(string text)
        {
            if (this.tbResults.InvokeRequired)
            {
                AppendTextCallback d = new AppendTextCallback(appendText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.tbResults.AppendText(text);
            }
        }

        delegate void AppendTextCallback(string result);
    }
}
