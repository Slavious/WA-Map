# WAWatcher
A program that catches events containing coordinates from the game Worlds Adrift.

### Is this hacking the game?
No, all WAWatcher does is read a text file that you can find yourself at '[UserFolder]\AppData\LocalLow\Bossa Studios\Worlds Adrift\ddsdk\events'. This file is used as a buffer for debug messages that are send to Bossa. Usually these files will be empty, as the messages are removed from it as soon as it's send. WAWatcher reads the file on every change and stores the coordinates found in a particular message. You could do this yourself by force quitting the game and opening the text file.
So in summary: this only reads a text file and not the game memory, and only automates something you could do manually already.

### So how do I use it?
- [download the installer here](https://github.com/Jerodar/WAWatcher/raw/master/WAWatcher.zip), unzip the archive, and then run the setup.
- You can start WAWatcher from your start menu. It doesn't matter if Worlds Adrift is already running.
- Before you click start, you can choose to enable a notification sound and if you want to write the coordinates to a file.
- After clicking start you'll see a new set of coordinates appear around once per minute if the game is running.

### Anything else I should know?
- The coordinates are lagging behind your actual location a bit. If you want a super accurate location, stand still in a spot until two different coordinate sets are the same.
- The Y coordinate is your altitude in the game, but compared to the ingame altimeter it's 2000 less and can become negative. So a y value of 0 is the same as 2000 meters on the altimeter, and a y value of -800 is 1200 ingame etc.
- If you enabled Write to file, you can find the file location by clicking Open Folder. The file is in CSV format so it can be imported into a spreadsheet if needed.
