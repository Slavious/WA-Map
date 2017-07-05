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
using System.Globalization;

namespace WindowsFormsApplication1
{
    public partial class WAWatcher : Form
    {
        private FileSystemWatcher watcher;
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
                watcher.NotifyFilter = NotifyFilters.Size;
                // Only watch text files.
                watcher.Filter = "";

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);
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
                        parseLine(sr.ReadToEnd());
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
                tbResults.AppendText("Running...\n");
                btnStart.Text = "Stop";
                running = true;
            }
            else
            {
                // Stop watching.
                watcher.EnableRaisingEvents = false;
                tbResults.AppendText("Stopped.\n");
                btnStart.Text = "Start";
                running = false;
            }
            
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // Clear the text box
            tbResults.Clear();
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
                        string subString = line.Substring(startIndex, i - startIndex + 1);
                        var ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                        var jsonObject = (IDictionary<string, object>)ser.DeserializeObject(subString);
                        if (jsonObject["eventName"].ToString() == "performanceReport")
                        {
                            var eventParams = (IDictionary<string, object>)jsonObject["eventParams"];
                            string result = "Time: " + jsonObject["eventTimestamp"].ToString() + " X: " + eventParams["playerXCoord"].ToString().Replace(',','.') +
                                            " Y: " + eventParams["playerYCoord"].ToString().Replace(',', '.') + " Z: " + eventParams["playerZCoord"].ToString().Replace(',', '.');
                            appendText(result + '\n');
                            if (soundOn)
                                System.Media.SystemSounds.Asterisk.Play();
                        }
                        startIndex = -1;
                    }
                }
            }
        }

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

        private void chkSound_CheckedChanged(object sender, EventArgs e)
        {
            soundOn = chkSound.Checked;
        }
    }
}
