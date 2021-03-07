using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;

//======IMPORTANT=================================================
///If you try to run the program and it just does nothing, make sure that you 
/// have selected the correct html code source. Aka, you have commented out the 
/// local source and uncommented the ED Updates site. Search "digital" to find it
/// in this code.
//======IMPORTANT=================================================

/// <summary>
/// Welcome to DCS Update Witching Utility. DCS-UwU contains the following features:
/// -Standalone and Steam Integration
/// -Backup Input Folder
/// -Backup Config Folder
/// -Clear Metashaders2 Folder
/// -Clear FXO Folder
/// -Clear DCS Temp Folder
/// -Clear Terrain Metacache Folder
/// -Update DCS via Stable or Openbeta (switch branches)
/// -Auto Update DCS Stable or Openbeta
/// -Pick Auto Update Sound (“Yay!” music)
/// </summary>

/// <Notes>
/// Steam users will have to do a workaround to use the backup and cleaning functions. It
/// is noted in the Readme.
/// </Notes>
/// 

/* Version Notes:
v1
-Initial Release
-Standalone and Steam Integration
-Backup Input Folder
-Backup Config Folder
-Clear Metashaders2 Folder
-Clear FXO Folder
-Clear DCS Temp Folder
-Clear Terrain Metacache Folder
-Update DCS via Stable or Openbeta (switch branches)
-Auto Update DCS Stable or Openbeta
-Pick Auto Update Sound (“Yay!” music)
v2
-Clear Tracks Folder
-Clear Tackview Tracks Folder
-Include File Counts
-Include Data estimation
 */

//Resources
//https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.windowstyle?view=net-5.0

namespace DCS_Update_Witching_Utility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        //====GLOBALS====
        //the dispatcher timer is for the check that goes to the dcs updates site
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        //the image rotation timer is to show the user that the utility is still checking between dispatch timer updates
        System.Windows.Threading.DispatcherTimer imageRotationTimer = new System.Windows.Threading.DispatcherTimer();

        //tracks if the files are selected
        bool isDcsExeSelected;
        bool isOptionsLuaSelected;

        public MainWindow()
        {
            InitializeComponent();
            //if for some readon the website check does not happen correctly
            //his will remain invisible and not look weird.
            textBlock_time.Visibility = Visibility.Hidden;
            GetInitialVersionNumbers();//used to get the version numbers on launch
            isDcsExeSelected = false;//the init for the value
            isOptionsLuaSelected = false;//the init for the value

            LoadSaveFile();//if the save file is present, this will load it
            
            dispatcherTimer.Tick += DispatcherTimer_Tick;//creates the tick even for this timer
            dispatcherTimer.Interval = new TimeSpan(0, 2, 0);//hours, minutes, seconds. As you can see, this will check the site ever 2 minutes

            imageRotationTimer.Tick += ImageRotationTimer_Tick;//creates the tick even for this timer
            imageRotationTimer.Interval = new TimeSpan(0, 0, 1);//hours, minutes, seconds. As you can see, this will rotate the image every 1 second
            //higher rate of rotation with lower angle rotations are possible, but they take more CPU
        }

        int rotationTracker = 0;//this is the init for the rotation tracker. the number goes up and  up and up...

        private void ImageRotationTimer_Tick(object sender, EventArgs e)//what happens every time the rotation timer ticks
        {
            //https://www.c-sharpcorner.com/uploadfile/mahesh/image-transformation-in-wpf/#:~:text=To%20rotate%20an%20image%2C%20we,as%20shown%20in%20below%20code.&text=The%20code%20listed%20in%20Listing,an%20image%20at%20run%2Dtime.&text=The%20ScaleTransform%20is%20used%20to%20scale%20an%20image.
            image_rotatingUpdate.IsEnabled = true;//make sure that the image is enabled (maybe not necessary)
            image_rotatingUpdate.Visibility = Visibility.Visible;//make sure that the image is visible
            RotateTransform transform = new RotateTransform(rotationTracker + 45);//add 45 to the previous rotation
            image_rotatingUpdate.RenderTransformOrigin = new Point(0.5, 0.5);//make sure the rotation happens in the middle

            image_rotatingUpdate.RenderTransform = transform;//this is the command that actually rotates the image
            rotationTracker += 45;//add 45 degrees to the tracker so that it can be used again for the next tick
        }

        //https://stackoverflow.com/questions/938421/getting-the-applications-directory-from-a-wpf-application
        string appPath = System.AppDomain.CurrentDomain.BaseDirectory;//gets the path of were the utility is running

        private void LoadSaveFile()
        {
            //if the save/load file is found
            if (File.Exists(appPath + @"/DCS-Update-Witching-Utility-Settings/UwU-UserSettings.txt"))
            {
                //MessageBox.Show("Loading...");//debugging
                //read the file line by line. Assumes the lines are properly formated via the saveSettings method
                //e.g. first line is the users dcs.exe location. The second line is the users options.lua location
                System.IO.StreamReader file = new System.IO.StreamReader(appPath + @"/DCS-Update-Witching-Utility-Settings/UwU-UserSettings.txt");
                
                selected_selectDcsExe_string = file.ReadLine();//read the first line
                selected_selectOptionsLua_string = file.ReadLine();//read the second line
                file.Close();//close the file because we are done with it
                textBlock_selectDcsExe.Text = selected_selectDcsExe_string;//put the first line in the first box
                textBlock_selectOptionsLua.Text = selected_selectOptionsLua_string;//put the second line in the seecond box

                //figure out the rest of the filepaths
                GeneratePathsFromDcsExePath();//makes the rest of the filepaths that concern this path
                isDcsExeSelected = true;
                textBlock_selectDcsExe.BorderBrush = Brushes.LightGreen;//visual feedback for the user

                GeneratePathsFromOptionsLuaPath();
                isOptionsLuaSelected = true;//makes the rest of the filepaths that concern this path
                textBlock_selectOptionsLua.BorderBrush = Brushes.LightGreen;//visual feedback for the user

                CheckIfDcsExeAndOptionsLuaHaveBeenSelected();//this should be true because we just set the stuff
            }
        }

        private void SaveUserSettings()
        {
            //you could use Json stuff, but we only need 2 lines in this utility.
            //export the 2 directories that the user choose to a .txt file
            //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-write-to-a-text-file
            Directory.CreateDirectory(appPath + "\\DCS-Update-Witching-Utility-Settings");//creates the save folder
            //https://docs.microsoft.com/en-us/dotnet/api/system.io.streamwriter?redirectedfrom=MSDN&view=netcore-3.1
            //write the following ot the text file
            using (StreamWriter sw = File.CreateText(appPath + "\\DCS-Update-Witching-Utility-Settings\\UwU-UserSettings.txt"))
            {
                sw.WriteLine(selected_selectDcsExe_string);
                sw.WriteLine(selected_selectOptionsLua_string);
                sw.Close();//close the file because we are done writing to it
            }
        }

        string htmlCode;//this will contain the html code that was downloaded from the site (or the file when debugging)

        private void GetInitialVersionNumbers()
        {
            DownloadHtmlFile();//downloads the fole from the location
            //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-read-a-text-file-one-line-at-a-time
            //int counter = 0;//debugging

            //Read the file line by line.  
            StringReader reader = new StringReader(htmlCode);
            while ((htmlCode = reader.ReadLine()) != null)//while you read the lines and havent got to the end of the file...
            {
                //System.Console.WriteLine(line);//debugging
                if (htmlCode.Contains("Latest stable version is"))//if the line contains this phrase
                {
                    int indexOfWordStable = htmlCode.IndexOf("changelog/") + 10;//count backwards to get the end of the phrase
                    int indexOfSlashAfterVersionNumber = htmlCode.IndexOf("/", indexOfWordStable) + 1;//this is here to skip the next index of the slash
                    indexOfSlashAfterVersionNumber = htmlCode.IndexOf("/", indexOfSlashAfterVersionNumber);//gets the index of the second slash
                    //the syntax for the version number string
                    string stableVersionNumber = htmlCode.Substring(indexOfWordStable, indexOfSlashAfterVersionNumber - indexOfWordStable);
                    //MessageBox.Show("Stable is version: " + stableVersionNumber);//debugging
                    //if you dont do this, the result will be something like "openbeta/2.3.4.12345.3"
                    stableVersionNumber = Regex.Replace(stableVersionNumber, "[^0-9.]", "");//https://stackoverflow.com/questions/3624332/how-do-you-remove-all-the-alphabetic-characters-from-a-string
                    textBlock_stableVersionNumber.Text = ("Latest stable version is: " + stableVersionNumber);
                }
                if (htmlCode.Contains("Current openbeta is"))//similar to what was done above
                {
                    int indexOfWordStable = htmlCode.IndexOf("changelog/") + 10;
                    int indexOfSlashAfterVersionNumber = htmlCode.IndexOf("/", indexOfWordStable) + 1;
                    indexOfSlashAfterVersionNumber = htmlCode.IndexOf("/", indexOfSlashAfterVersionNumber);
                    string openBetaVersionNumber = htmlCode.Substring(indexOfWordStable, indexOfSlashAfterVersionNumber - indexOfWordStable);
                    //MessageBox.Show("Stable is version: " + openBetaVersionNumber);//debugging
                    openBetaVersionNumber = Regex.Replace(openBetaVersionNumber, "[^0-9.]", "");
                    textBlock_openBetaVersionNumber.Text = ("Current openbeta is: " + openBetaVersionNumber);
                }
                //counter++;//debugging
            }

            reader.Close();//close the file because we are done using it
            //System.Console.WriteLine("There were {0} lines.", counter);//debugging
            //MessageBox.Show("Lines: " + counter);//debugging
            textBlock_time.Visibility = Visibility.Visible;
            textBlock_time.Text = "Last Checked: " + DateTime.Now.ToString();//updates the string
        }

        private void DownloadHtmlFile()//downloads the file
        {
            using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
            {
                htmlCode = client.DownloadString("http://updates.digitalcombatsimulator.com/");
                //htmlCode = File.ReadAllText(@"C:\Downloads\DCS World Updates.html");//debug testing location
                //htmlCode = File.ReadAllText(@"E:\Downloads\DCS World Updates.html");//debug testing location

            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            //every time this timer ticks, the version numbers get checked
            GetVersionNumbers();
        }

        private void GetVersionNumbers()
        {
            //this is almost exactly the same as the GetInitialVersionNumbers() method
            //The main differences are:
            //-stops timers if there was an appropiate change
            //-changes the color of the text backgrounds when there is a change
            DownloadHtmlFile();//downloads the info
            //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-read-a-text-file-one-line-at-a-time
            //int counter = 0;//debugging

            // Read the file and display it line by line.  
            StringReader reader = new StringReader(htmlCode);

            while ((htmlCode = reader.ReadLine()) != null)
            {
                //System.Console.WriteLine(line);
                if (htmlCode.Contains("Latest stable version is"))
                {
                    int indexOfWordStable = htmlCode.IndexOf("changelog/") + 10;
                    int indexOfSlashAfterVersionNumber = htmlCode.IndexOf("/", indexOfWordStable) + 1;
                    indexOfSlashAfterVersionNumber = htmlCode.IndexOf("/", indexOfSlashAfterVersionNumber);
                    string stableVersionNumberNew = htmlCode.Substring(indexOfWordStable, indexOfSlashAfterVersionNumber - indexOfWordStable);
                    //MessageBox.Show("Stable is version: " + stableVersionNumber);
                    //if you dont do this, the result will be something like "openbeta/2.3.4.12345.3"
                    stableVersionNumberNew = Regex.Replace(stableVersionNumberNew, "[^0-9.]", "");//https://stackoverflow.com/questions/3624332/how-do-you-remove-all-the-alphabetic-characters-from-a-string
                    //if the numbers where the same, there was no update
                    if (textBlock_stableVersionNumber.Text.Equals("Latest stable version is: " + stableVersionNumberNew))
                    {
                        textBlock_stableVersionNumber.Text = ("Latest stable version is: " + stableVersionNumberNew);
                    }
                    else
                    { //the numbers were likely different
                        //this goes here so that it can be updated regardless if it was the one being monitored
                        textBlock_stableVersionNumber.Text = ("Latest stable version is: " + stableVersionNumberNew);
                        textBlock_stableVersionNumber.Background = new SolidColorBrush(Colors.LightGreen);//color changed for user feedback
                        if (isStableAutoUpdateOn == true)
                        {
                            //MessageBox.Show(textBlock_stableVersionNumber.Text + "\r\n" +
                            //    stableVersionNumberNew);
                            //trigger all the stuff that signals that there was an appropiate change
                            button_autoUpdateDcsViaStable.IsEnabled = false;
                            button_autoUpdateDcsViaOpenbeta.IsEnabled = false;
                            mediaPlayer.Play();
                            dispatcherTimer.Stop();
                            imageRotationTimer.Stop();
                            textBlock_time.Text = "Updated: " + DateTime.Now.ToString();
                            textBlock_time.Background = new SolidColorBrush(Colors.LightGreen);//color changed for user feedback
                            Actions_UpdateDcsViaStableButton();//it presses the button to update stable branch
                        }
                    }


                }

                if (htmlCode.Contains("Current openbeta is"))//this is basically the same as the above, but for openbeta
                {
                    int indexOfWordStable = htmlCode.IndexOf("changelog/") + 10;
                    int indexOfSlashAfterVersionNumber = htmlCode.IndexOf("/", indexOfWordStable) + 1;
                    indexOfSlashAfterVersionNumber = htmlCode.IndexOf("/", indexOfSlashAfterVersionNumber);
                    string openBetaVersionNumberNew = htmlCode.Substring(indexOfWordStable, indexOfSlashAfterVersionNumber - indexOfWordStable);
                    //MessageBox.Show("Stable is version: " + openBetaVersionNumber);
                    openBetaVersionNumberNew = Regex.Replace(openBetaVersionNumberNew, "[^0-9.]", "");
                    if (textBlock_openBetaVersionNumber.Text.Equals("Current openbeta is: " + openBetaVersionNumberNew))//if the numbers where the same, there was no update
                    {
                        textBlock_openBetaVersionNumber.Text = ("Current openbeta is: " + openBetaVersionNumberNew);
                    }
                    else
                    { //the numbers were likely different
                        //this goes here so that it can be updated regardless if it was the one being monitored
                        textBlock_openBetaVersionNumber.Text = ("Current openbeta is: " + openBetaVersionNumberNew);
                        textBlock_openBetaVersionNumber.Background = new SolidColorBrush(Colors.LightGreen);
                        if (isOpenbetaAutoUpdateOn == true)
                        {
                            
                            //MessageBox.Show(textBlock_stableVersionNumber.Text + "\r\n" +
                            //    openBetaVersionNumberNew);
                            
                            button_autoUpdateDcsViaStable.IsEnabled = false;
                            button_autoUpdateDcsViaOpenbeta.IsEnabled = false;
                            mediaPlayer.Play();
                            dispatcherTimer.Stop();
                            imageRotationTimer.Stop();
                            textBlock_time.Text = "Updated: " +  DateTime.Now.ToString();
                            textBlock_time.Background = new SolidColorBrush(Colors.LightGreen);
                            Actions_UpdateDcsViaOpenbetaButton();//it presses the button to update stable branch
                            
                        }
                    }
                }
                //counter++;//debugging
            }

            reader.Close();
            //System.Console.WriteLine("There were {0} lines.", counter);//debugging
            //MessageBox.Show("Lines: " + counter);//debugging
            textBlock_time.Text = "Last Checked: " + DateTime.Now.ToString();
        }


        private void RunDcsUpdater()//This is old. Can stay here for reference
        {
            string dcsUpdaterArguement = ("--quiet update");//this makes things easier for the update itself. Requires a certian version of the dcs updater.
            ProcessStartInfo startInfo = new ProcessStartInfo(selected_selectDcsExe_string, dcsUpdaterArguement);//uses the "quiet update" arguement 
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden; //this hides the program entirely. not want i want
            //startInfo.WindowStyle = 
            Process.Start(startInfo);
        }

        string selected_selectDcsExe_string;

        private void Button_selectDcsExe_Click(object sender, RoutedEventArgs e)
        {
            //Have the select dialog pop up
            OpenFileDialog openFileDialog_selectDcsExe = new OpenFileDialog();
            openFileDialog_selectDcsExe.InitialDirectory = "C:\\Program Files\\Eagle Dynamics\\DCS World\\bin\\";//likely not necessary, but it may help
            openFileDialog_selectDcsExe.Filter = "Application files (*.exe)|*.exe";//pick an exe only
            openFileDialog_selectDcsExe.RestoreDirectory = true;//sure, but not necessary
            openFileDialog_selectDcsExe.Title = "Select DCS_updater.exe (Hint: C:\\Install Location\\bin\\DCS_updater.exe";//hints for all kinds of installs
            //the user picks their dcs-updater.exe
            if (openFileDialog_selectDcsExe.ShowDialog() == true)
            {
                var selected_selectDcsExe = openFileDialog_selectDcsExe.FileName;
                selected_selectDcsExe_string = selected_selectDcsExe.ToString();
                if (selected_selectDcsExe.Contains("DCS_updater.exe"))//check to make sure that the file they pick is the correct one
                {
                    //the user selected the correct correct file
                    //if the file is the correct one, try to make all of the other file paths that are related
                    //to that part of the folder system
                    GeneratePathsFromDcsExePath();
                    textBlock_selectDcsExe.Text = selected_selectDcsExe;
                    isDcsExeSelected = true;
                    textBlock_selectDcsExe.BorderBrush = Brushes.LightGreen;//visual feedback
                }
                else
                {
                    //the user did not select the correct file
                    MessageBox.Show("You picked: " + selected_selectDcsExe + ". This is not the correct file. Please try again.");
                    textBlock_selectDcsExe.Text = null;
                    isDcsExeSelected = false;
                }
            }
        }



        string selected_selectOptionsLua_string;

        //similar to Button_selectDcsExe_Click(), but for the options.lua
        private void Button_selectOptionsLua_Click(object sender, RoutedEventArgs e)
        {
            //Have the select dialog pop up
            //the user picks their options.lua
            OpenFileDialog openFileDialog_selectOptionsLua = new OpenFileDialog();
            openFileDialog_selectOptionsLua.InitialDirectory = "c:\\Users";//gets closer to the location of the file
            openFileDialog_selectOptionsLua.Filter = "lua files (*.lua)|*.lua";//select lua files only
            openFileDialog_selectOptionsLua.RestoreDirectory = true;//not exactly necessary
            openFileDialog_selectOptionsLua.Title = "Select options.lua (Hint: C:\\Users\\YOURNAME\\Saved Games\\DCS\\Config\\options.lua";
            //the user picks their options.lua
            if (openFileDialog_selectOptionsLua.ShowDialog() == true)
            {
                var selected_selectOptionsLua = openFileDialog_selectOptionsLua.FileName;
                selected_selectOptionsLua_string = selected_selectOptionsLua.ToString();
                if (selected_selectOptionsLua.Contains("options.lua"))
                {
                    //the user selected the correct correct file
                    //if the file is the correct one, try to make all of the other file paths that are related
                    //to that part of the folder system
                    GeneratePathsFromOptionsLuaPath();
                    textBlock_selectOptionsLua.Text = selected_selectOptionsLua;
                    isOptionsLuaSelected = true;
                    textBlock_selectOptionsLua.BorderBrush = Brushes.LightGreen;
                }
                else
                {
                    //the user did not select the correct file
                    MessageBox.Show("You picked: " + selected_selectOptionsLua + ". This is not the correct file. Please try again.");
                    textBlock_selectOptionsLua.Text = null;
                    isOptionsLuaSelected = false;
                }
            }
        }


        string terrainMetacacheLocation;
        string dcsInstallDirectory;
        string dcsBasicExe_string;

        private void GeneratePathsFromDcsExePath()
        {
            //generate paths here
            //init the strings first
            dcsInstallDirectory = Path.GetFullPath(Path.Combine(selected_selectDcsExe_string, @"..\..\"));
            dcsBasicExe_string = Path.Combine(dcsInstallDirectory, @"bin\dcs.exe");
            //MessageBox.Show(dcsInstallDirectory);//results in something like "C:/ProgramFiles/DCS"

            //this is used later in Actions_clearTerrainShadersButton()
            terrainMetacacheLocation = Path.Combine(dcsInstallDirectory, @"Mods\terrains");

            //get the exe version number
            if (File.Exists(dcsBasicExe_string))
            {
                textBlock_usersVersionNumber.Visibility = Visibility.Visible;
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(dcsBasicExe_string);
                textBlock_usersVersionNumber.Text = "Your DCS.exe Version: " + myFileVersionInfo.FileVersion;
                //MessageBox.Show("Your DCS.exe Version: " + myFileVersionInfo.FileVersion);//debugging
            }
        }

        //these are the strings that will be generated via the options.lua path
        string dcsSavedGamesDirectory;
        string userDirectory;
        string dcsInputsFolderPath;
        string dcsFxoFolderPath;
        string dcsMetashaders2FolderPath;
        string dcsTempFolderPathOpenbeta;
        string dcsTempFolderPathOpenalpha;
        string dcsTempFolderPathStable;
        string dcsConfigFolderPath;
        string dcsTracksFolderPath;
        string tacviewTracksFolderPath;//C:\Users\Bailey\Documents\Tacview

        private void GeneratePathsFromOptionsLuaPath()
        {
            //generate paths here
            dcsSavedGamesDirectory = Path.GetFullPath(Path.Combine(selected_selectOptionsLua_string, @"..\..\"));
            //MessageBox.Show(dcsSavedGamesDirectory);//this results in "C:/Users/XXX/Saved Games/DCS.openbeta/"



            //make the filepaths
            dcsInputsFolderPath = Path.Combine(dcsSavedGamesDirectory, @"Config\Input");//used for the zip file
            dcsConfigFolderPath = Path.Combine(dcsSavedGamesDirectory, @"Config");//used for the zip file
            dcsFxoFolderPath = Path.Combine(dcsSavedGamesDirectory, @"fxo");
            dcsMetashaders2FolderPath = Path.Combine(dcsSavedGamesDirectory, @"metashaders2");
            dcsTracksFolderPath = Path.Combine(dcsSavedGamesDirectory, @"Tracks");

            //MessageBox.Show(dcsSavedGamesDirectory + "\r\n"
            //    + dcsInputsFolderPath + "\r\n"
            //    + dcsFxoFolderPath + "\r\n"
            //    + dcsMetashaders2FolderPath + "\r\n");

            userDirectory = Path.GetFullPath(Path.Combine(selected_selectOptionsLua_string, @"..\..\..\..\"));
            //MessageBox.Show(userDirectory);//this results in "C:/Users/XXX"

            //make the TEMP filepaths
            //the final folder could be a few different names
            dcsTempFolderPathOpenbeta = Path.Combine(userDirectory, @"AppData\Local\Temp\DCS.openbeta");
            dcsTempFolderPathStable = Path.Combine(userDirectory, @"AppData\Local\Temp\DCS");
            dcsTempFolderPathOpenalpha = Path.Combine(userDirectory, @"AppData\Local\Temp\DCS.openalpha");
            tacviewTracksFolderPath = Path.Combine(userDirectory, @"Documents\Tacview");

            //MessageBox.Show(userDirectory + "\r\n"
            //    + dcsTempFolderPathOpenbeta + "\r\n"
            //    + dcsTempFolderPathStable + "\r\n"
            //    + dcsTempFolderPathOpenalpha + "\r\n");
        }


        private void Button_backupInputFolder_Click(object sender, RoutedEventArgs e)
        {
            Actions_backupInputFolderButton();
        }

        private void Actions_backupInputFolderButton()
        {
            //back up the input folder by zipping the contents and dating the folder with date and time
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            //MessageBox.Show("Zipping " + dcsInputsFolderPath);//debugging
            {
                if (Directory.Exists(dcsInputsFolderPath))
                    //check to make sure the file is actually there
                {
                    string zipFilePathAndName = Path.Combine(dcsConfigFolderPath, "Input-" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".zip");
                    ZipFile.CreateFromDirectory(dcsInputsFolderPath, zipFilePathAndName);
                    if (File.Exists(zipFilePathAndName))//if we can find the file we just zipped...
                    {
                        MessageBox.Show("Input folder zip was successful. It is located here: " + zipFilePathAndName);
                    }
                    else
                    {
                        MessageBox.Show("Zip was not successful.");
                    }
                }
                else//this can happen if the user had never made a change to the dcs controls
                {
                    MessageBox.Show("DCS-UwU could not find or zip the Input folder located at: " + dcsInputsFolderPath + ". Sorry!");
                }
            }
        }

        string directoryToDelete;

        private void Button_clearFxoFolder_Click(object sender, RoutedEventArgs e)
        {
            Actions_clearFxoFolderButton();
        }

        private void Actions_clearFxoFolderButton()
        {
            //clear the FXO folder
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            {
                //check to see if the folder exists
                if (Directory.Exists(dcsFxoFolderPath))
                {
                    directoryToDelete = dcsFxoFolderPath;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    MessageBox.Show("DCS-UwU could not find the fxo folder at: " + dcsFxoFolderPath + ". Sorry!");
                }
            }
        }
        //this handles all instances of deleting files
        //The folder location gets "passed" via a string from the priginal method
        private void DeleteAllFilesInTheDirectory()
        {

            CountTheFilesAndEstimateTheDataSize();
            //https://stackoverflow.com/questions/6452139/how-to-create-a-dialogbox-to-prompt-the-user-for-yes-no-option-in-wpf/6455754
            string sCaption = "READ THIS CAREFULLY";//the titlebar for the popup window
            string sMessageBoxText = "You are about to delete all of the files located in: '" + directoryToDelete + "'\r\n" +
                "Number of files: " + +numberOfFilesToDelete + "\r\n" +
                "Approximate Megabytes: " + sizeOfFilesToDelete +  "\r\n" +
                "There is no 'undo'. " + "\r\n" +
                "Do you want to delete the files?";



            //the three options. It could be two, but i hope that if the user panics, they only 
            //have a 1 in 3 chance of hitting the wrong button instead of a 1 in 2 chance
            MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
            MessageBoxImage icnMessageBox = MessageBoxImage.Warning;//the warning symbol on the left
           
          
            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes://if the user pressed YES, target the directory, get a list of the files, and then delete the files
                    System.IO.DirectoryInfo di = new DirectoryInfo(directoryToDelete);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                    break;

                case MessageBoxResult.No://user clicks no. dont do anything
                    /* ... */
                    break;

                case MessageBoxResult.Cancel://user clicks cancel. dont do anything
                    /* ... */
                    break;
            }
        }

        long numberOfFilesToDelete;
        long sizeOfFilesToDelete;
        private void CountTheFilesAndEstimateTheDataSize()
        {
            //https://stackoverflow.com/questions/2242564/file-count-from-a-folder
            numberOfFilesToDelete = Directory.GetFiles(directoryToDelete, "*", SearchOption.AllDirectories).Length;
            sizeOfFilesToDelete = GetDirectorySize(directoryToDelete);

        }
        //https://stackoverflow.com/questions/1118568/how-do-i-get-a-directory-size-files-in-the-directory-in-c
        private static long GetDirectorySize(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length) / 1024 / 1000;//the "/ 1024 / 1000" converts it to megabytes

        }


        private void Button_clearmetashaders2Folder_Click(object sender, RoutedEventArgs e)
        {
            Actions_clearmetashaders2FolderButton();
        }

        private void Actions_clearmetashaders2FolderButton()
        {
            //clear the metashaders folder
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            {
                //check to see if the folder exists
                if (Directory.Exists(dcsMetashaders2FolderPath))
                {
                    directoryToDelete = dcsMetashaders2FolderPath;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    MessageBox.Show("DCS-UwU could not find the fxo folder at: " + dcsMetashaders2FolderPath + ". Sorry!");
                }
            }
        }

        private void Button_clearTerrainShaders_Click(object sender, RoutedEventArgs e)
        {
            Actions_clearTerrainShadersButton();
        }

        string terrainMetacacheLocation_currentMapToClean;

        private void Actions_clearTerrainShadersButton()
        {

            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();

            if (isGoodToProcess == true)
            {
                //there is one of these for every map. I could make a map list that reads
                //the contents of the "G:\Games\DCS World OpenBeta\Mods\terrains" folder....acrtually,
                //that sounds like a good idea. TODO1:do that idea before more maps come out. remember to include
                //the creation of the map links in the "generation" phase.
                System.IO.DirectoryInfo terrainLoc = new DirectoryInfo(terrainMetacacheLocation);
                foreach (DirectoryInfo map in terrainLoc.GetDirectories()){

                    //MessageBox.Show(map.FullName.ToString());//this results in "C:\InstallLocation\DCS\Mods\terrains\Caucasus" or similar
                    //MessageBox.Show(map.Name.ToString());//this results in "Caucasus" or similar
                    
                    terrainMetacacheLocation_currentMapToClean = Path.Combine(map.FullName, @"misc\metacache\dcs");
                    //check to make sure thgat the directgory exists. It should, but just in case...
                    if (Directory.Exists(terrainMetacacheLocation_currentMapToClean))
                    {
                        //MessageBox.Show(terrainMetacacheLocation_currentMapToClean);//results in  "C:\InstallLocation\DCS\Mods\terrains\Caucasus\misc\metacache\dcs"
                        directoryToDelete = terrainMetacacheLocation_currentMapToClean;
                        DeleteAllFilesInTheDirectory();
                    }
                }
            }
        }

        bool isGoodToProcess;
        //this simply chects to make sure that the exe and lua are selected. otherwise, you cant do the other functions if you try
        private void CheckIfDcsExeAndOptionsLuaHaveBeenSelected()
        {
            if (isOptionsLuaSelected == true && isDcsExeSelected == true)
            {
                isGoodToProcess = true;
                SaveUserSettings();
            }
            else
            {
                MessageBox.Show("Please select your DCS_Updater.exe and Options.lua");
                isGoodToProcess = false;
            }
        }

        private void Button_DcsTempFolder_Click(object sender, RoutedEventArgs e)
        {
            Actions_DcsTempFolderButton();
        }

        private void Actions_DcsTempFolderButton()
        {
            //clears the dcs temp folder
            //identify the locaton of the folder
            //clear the folder
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            {
                //check to see if the folder exists
                if (Directory.Exists(dcsTempFolderPathStable))
                {
                    directoryToDelete = dcsTempFolderPathStable;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + dcsTempFolderPathStable + ". Sorry!");
                }

                if (Directory.Exists(dcsTempFolderPathOpenalpha))
                {
                    directoryToDelete = dcsTempFolderPathOpenalpha;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + dcsTempFolderPathOpenalpha + ". Sorry!");
                }

                if (Directory.Exists(dcsTempFolderPathOpenbeta))
                {
                    directoryToDelete = dcsTempFolderPathOpenbeta;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + dcsTempFolderPathOpenbeta + ". Sorry!");
                }
            }
        }

        private void Button_WitchEverything_Click(object sender, RoutedEventArgs e)
        {
            Actions_WitchEverythingButton();
        }

        private void Actions_WitchEverythingButton()
        {
            
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            //basically pressed the 2 backup buttons, and then the 4 Cleaning buttons for ya. Still confirms deletes
            if (isGoodToProcess == true)
            {
                //https://stackoverflow.com/questions/6452139/how-to-create-a-dialogbox-to-prompt-the-user-for-yes-no-option-in-wpf/6455754
                string sCaption = "READ THIS CAREFULLY";
                string sMessageBoxText = "You are about to perform all 6 backup and clearing actions at once." + "\r\n" +
                    "There is no 'undo'. " + "\r\n" +
                    "Do you want to continue?";

                MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
                MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                switch (rsltMessageBox)
                {
                    case MessageBoxResult.Yes:
                        //"Press" the buttons in this order
                        Actions_backupInputFolderButton();
                        Actions_backupConfigFolderButton();
                        Actions_clearFxoFolderButton();
                        Actions_clearmetashaders2FolderButton();
                        Actions_clearTerrainShadersButton();
                        Actions_DcsTempFolderButton();
                        Actions_clearDcsTracksFolderButton();
                        Actions_clearTacviewTracksFolderButton();
                        break;

                    case MessageBoxResult.No:
                        /* ... */
                        break;

                    case MessageBoxResult.Cancel:
                        /* ... */
                        break;
                }
            }
            else
            {
                //dont do anything. This case is caught beforehand
            }
        }

        private void Button_backupConfigFolder_Click(object sender, RoutedEventArgs e)
        {
            Actions_backupConfigFolderButton();
        }

        private void Actions_backupConfigFolderButton()
        {
            //back up the input folder by zipping the contents and dating the folder with date and time
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            //MessageBox.Show("Zipping " + dcsConfigFolderPath);
            {
                if (Directory.Exists(dcsConfigFolderPath))
                {
                    string zipFilePathAndName = Path.Combine(dcsSavedGamesDirectory, "Config-" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".zip");
                    ZipFile.CreateFromDirectory(dcsConfigFolderPath, zipFilePathAndName);
                    if (File.Exists(zipFilePathAndName))
                    {
                        MessageBox.Show("Config folder zip was successful. It is located here: " + zipFilePathAndName);
                    }
                    else
                    {
                        MessageBox.Show("Zip was not successful.");
                    }
                }
                else
                {
                    MessageBox.Show("DCS-UwU could not zip the Input folder located at: " + dcsConfigFolderPath + ". Sorry!");
                }
            }
        }

        private void Button_UpdateDcsViaStable_Click(object sender, RoutedEventArgs e)
        {
            Actions_UpdateDcsViaStableButton();
        }

        //uses a crafted command for cmd.exe to udpate dcs
        private void Actions_UpdateDcsViaStableButton()
        {
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            {
                string strCmdText;
                //"C" is for command
                //the string is the strng (duh)
                //everything after "--" are properties
                //"quiet" is a new way to update via the updater. less dialog boxes
                //"update @release" tell it to update to the stable branch version
                strCmdText = "/C \"" + selected_selectDcsExe_string + "\" --quiet update @release";
                //MessageBox.Show(strCmdText);//debugging
                System.Diagnostics.Process.Start("CMD.exe", strCmdText);
            }
            else
            {
                //MessageBox.Show("Please select your 'dcs_updater.exe' file. It is located in the 'bin' folder of where you installed DCS.");
            }
        }

        private void Button_UpdateDcsViaOpenbeta_Click(object sender, RoutedEventArgs e)
        {
            Actions_UpdateDcsViaOpenbetaButton();
        }

        //uses a crafted command for cmd.exe to udpate dcs
        private void Actions_UpdateDcsViaOpenbetaButton()
        {
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            {
                string strCmdText;
                //"C" is for command
                //the string is the strng (duh)
                //everything after "--" are properties
                //"quiet" is a new way to update via the updater. less dialog boxes
                //"update @openbeta" tell it to update to the penbeta branch version
                strCmdText = "/C \"" + selected_selectDcsExe_string + "\" --quiet update @openbeta";
                //MessageBox.Show(strCmdText);//debugging
                System.Diagnostics.Process.Start("CMD.exe", strCmdText);
            }
            else
            {
                //MessageBox.Show("Please select your 'dcs_updater.exe' file. It is located in the 'bin' folder of where you installed DCS.");
            }
        }

        private void Button_autoUpdateDcsViaStable_Click(object sender, RoutedEventArgs e)
        {
            Actions_autoUpdateDcsViaStableButton();
        }

        bool isStableAutoUpdateOn;
        bool isOpenbetaAutoUpdateOn;

        private void Actions_autoUpdateDcsViaStableButton()
        {
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            {
                isStableAutoUpdateOn = true;//this makes sure that the update is tripped on a stable update, not a openbeta update
                dispatcherTimer.Start();//starts the checking of the site
                imageRotationTimer.Start();//starts the image rotation
                button_autoUpdateDcsViaOpenbeta.IsEnabled = false;//visual feedback and prevents the user from selecting both
            }
        }

        private void Button_autoUpdateDcsViaOpenbeta_Click(object sender, RoutedEventArgs e)
        {
            Actions_autoUpdateDcsViaOpenbetaButton();
        }

        private void Actions_autoUpdateDcsViaOpenbetaButton()
        {

            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            {
                isOpenbetaAutoUpdateOn = true;//this makes sure that the update is tripped on a openbeta update, not a stable update
                dispatcherTimer.Start();//starts the checking of the site
                imageRotationTimer.Start();//starts the image rotation
                button_autoUpdateDcsViaStable.IsEnabled = false;//visual feedback and prevents the user from selecting both
            }
        }

        private MediaPlayer mediaPlayer = new MediaPlayer();

        private void Button_pickAutoUpdateSound_Click(object sender, RoutedEventArgs e)
        {
            Actions_pickAutoUpdateSoundButton();
        }

        //the user can pick a mp3 file to play when the update happens. 
        //can expand to other file formats upon request and availability
        private void Actions_pickAutoUpdateSoundButton()
        {
            //https://www.wpf-tutorial.com/audio-video/playing-audio/
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
                //mediaPlayer.Play();//if this plays to the end, it does not get triggered by the AutoUpdate later
            }
        }

        private void Button_stopSound_Click(object sender, RoutedEventArgs e)
        {
            Actions_stopSoundButton();
        }

        private void Actions_stopSoundButton()
        {
            mediaPlayer.Stop();//stops the sound. simple
        }

        //this does not work
        private void TextBlock_selectDcsExe_rightUp(object sender, MouseButtonEventArgs e)
        {
            textBlock_selectDcsExe.Text = null;
        }
        //this does not work
        private void TextBlock_selectOptionsLua_rightUp(object sender, MouseButtonEventArgs e)
        {
            textBlock_selectOptionsLua.Text = null;
        }

        //for screanshots and demos. Clears the text of the textbox
        private void Button_selectDcsExe_rightUp(object sender, MouseButtonEventArgs e)
        {
            textBlock_selectDcsExe.Text = null;
        }

        //for screanshots and demos. Clears the text of the textbox
        private void Button_selectOptionsLua_rightUp(object sender, MouseButtonEventArgs e)
        {
            textBlock_selectOptionsLua.Text = null;
        }

        //this does not work
        private void TextBlock_time_rightUp(object sender, MouseButtonEventArgs e)
        {
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);//hours, minutes, seconds
            MessageBox.Show("Check interval is now 5 seconds.");
        }

        //this does not work
        private void TextBlock_time_leftDown(object sender, MouseButtonEventArgs e)
        {
            
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);//hours, minutes, seconds
            MessageBox.Show("Check interval is now 5 seconds.");
        }

        //this does not work
        private void TextBlock_time_mouseWheel(object sender, MouseWheelEventArgs e)
        {
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);//hours, minutes, seconds
            MessageBox.Show("Check interval is now 5 seconds.");
        }

        //sets the update tick to 5 second intervals instead of the default
        private void Button_WitchEverything_rightUp(object sender, MouseButtonEventArgs e)
        {
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);//hours, minutes, seconds
            //MessageBox.Show("Check interval is now 5 seconds.");//debugging
            //visual feedback by changing the color of the "last updated" timer
            textBlock_time.Foreground = new SolidColorBrush(Colors.Blue);
        }

        private void Button_clearTacviewTracksFolder_Click(object sender, RoutedEventArgs e)
        {
            Actions_clearTacviewTracksFolderButton();
        }

      

        private void Actions_clearTacviewTracksFolderButton()
        {
            if (isGoodToProcess == true)
            {
                //check to see if the folder exists

                if (Directory.Exists(tacviewTracksFolderPath))
                {
                    directoryToDelete = tacviewTracksFolderPath;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + tacviewTracksFolderPath + ". Sorry!");
                }
            }
        }

        private void Button_clearDcsTracksFolder_Click(object sender, RoutedEventArgs e)
        {
            Actions_clearDcsTracksFolderButton();
        }

        private void Actions_clearDcsTracksFolderButton()
        {
            if (isGoodToProcess == true)
            {
                //check to see if the folder exists

                if (Directory.Exists(dcsTracksFolderPath))
                {
                    directoryToDelete = dcsTracksFolderPath;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + dcsTracksFolderPath + ". Sorry!");
                }
            }
        }
    }
}
