using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class WAWatcher : Form
    {
        private FileSystemWatcher watcher;
        private System.IO.StreamWriter logFile;
        private Boolean running = false;
        private Boolean soundOn = false;

        public WAWatcher()
        {
            InitializeComponent();

            string path = Environment.GetEnvironmentVariable("UserProfile") + "\\AppData\\LocalLow\\Bossa Studios\\Worlds Adrift\\ddsdk\\events";
            if (Directory.Exists(path))
            {
                // Create a new FileSystemWatcher and set its properties.
                watcher = new FileSystemWatcher();
                watcher.Path = path;
                /* Watch for changes in LastAccess and LastWrite times, and 
                   the renaming of files or directories. */
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                // Only watch text files.
                watcher.Filter = "";

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);

                // Create a log file
                logFile = new System.IO.StreamWriter("WAWatcherLog.txt", true);
            }
            else
            {
                MessageBox.Show("Cannot find World Adrift folder, run the game at least once and try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            
            // Open the file without locking it
            try
            {
                using (FileStream fs = new FileStream(e.FullPath,
                                      FileMode.Open,
                                      FileAccess.Read,
                                      FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (!sr.EndOfStream)
                        {
                            parseLine(sr.ReadLine());
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if(!running)
            {
                // Begin watching.
                watcher.EnableRaisingEvents = true;
                textBox1.AppendText("Running...\n");
                logFile.WriteLine("Running...");
                btnStart.Text = "Stop";
                running = true;
            }
            else
            {
                // Stop watching.
                watcher.EnableRaisingEvents = false;
                textBox1.AppendText("Stopped.\n");
                logFile.WriteLine("Stopped.");
                btnStart.Text = "Start";
                running = false;
            }
            
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // Clear the text box
            textBox1.Clear();
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
                {
                    if (startIndex < 0)
                        startIndex = i;
                    nestLevel++;
                }
                if (c == '}')
                {
                    nestLevel--;
                    if (nestLevel ==0)
                    {
                        string subString = line.Substring(startIndex, i - startIndex);
                        var ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                        var jsonObject = (IDictionary<string, object>)ser.DeserializeObject(subString);
                        if (jsonObject["eventName"].ToString() == "performanceReport")
                        {
                            var eventParams = (IDictionary<string, object>)jsonObject["eventParams"];
                            string result = "Time: " + jsonObject["eventTimestamp"].ToString() + ", X: " + eventParams["playerXCoord"].ToString() +
                                            ", Y: " + eventParams["playerYCoord"].ToString() + ", Z: " + eventParams["playerZCoord"].ToString();
                            textBox1.AppendText(result + '\n');
                            logFile.WriteLine(result);
                            if (soundOn)
                                System.Media.SystemSounds.Asterisk.Play();
                        }
                        startIndex = -1;
                    }
                }
            }
        }

        private void WAWatcher_FormClosing(object sender, FormClosingEventArgs e)
        {
            logFile.Close();
        }

        private void chkSound_CheckedChanged(object sender, EventArgs e)
        {
            soundOn = chkSound.Checked;
        }
    }
}
