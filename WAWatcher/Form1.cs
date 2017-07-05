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

        public WAWatcher()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string path = Environment.GetEnvironmentVariable("UserProfile") + "/AppData/LocalLow/Bossa Studios/Worlds Adrift/ddsdk/events/";
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
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            // Stop watching.
            watcher.EnableRaisingEvents = false;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // Clear the text box
            textBox1.Clear();
        }
    }
}
