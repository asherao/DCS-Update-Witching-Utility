using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;

/// <summary>
/// Welcome to DCS-UwU (Update Witching Utility). DCS-UwU contains the following features:
/// -Updates DCS when the update is available
/// -The ability to back up the Inputs folder
/// -The ability to clear the DCS fxo folder
/// -The ability to clear the DCS metashaders2 folder
/// -The ability to clear the DCS terrain shaders
/// -The ability to clear the DCS temp folder
/// </summary>
/// 

/// <Notes>
/// -Updates DCS when the update is available (reference G:\Games\DCS World OpenBeta\autoupdate.cfg)
/// -The ability to back up the Inputs folder (located at C:\Users\Bailey\Saved Games\DCS.openbeta\Config\Input)
/// -The ability to clear the DCS fxo folder (located at C:\Users\Bailey\Saved Games\DCS.openbeta\fxo)
/// -The ability to clear the DCS metashaders2 folder (located at C:\Users\Bailey\Saved Games\DCS.openbeta\metashaders2)
/// -The ability to clear the DCS terrain shaders (located at G:\Games\DCS World OpenBeta\Mods\terrains\Caucasus\misc\metacache\dcs)
/// -The ability to clear the DCS Temp Folder (located at C:\Users\Bailey\AppData\Local\Temp\DCS.openbeta)
/// </Notes>

//Resources
//https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.windowstyle?view=net-5.0


namespace DCS_Update_Witching_Utility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        //====GLOBALS====
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer imageRotationTimer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            //getVersionNumbers();
            GetInitialVersionNumbers();
            isDcsExeSelected = false;
            isOptionsLuaSelected = false;
            LoadSaveFile();

            textBlock_time.Text = "Last Checked: " + DateTime.Now.ToString();


            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 2, 0);//hours, minutes, seconds

            imageRotationTimer.Tick += ImageRotationTimer_Tick;
            imageRotationTimer.Interval = new TimeSpan(0, 0, 1);//hours, minutes, seconds
        }
        int rotationTracker = 0;
        private void ImageRotationTimer_Tick(object sender, EventArgs e)
        {
            //https://www.c-sharpcorner.com/uploadfile/mahesh/image-transformation-in-wpf/#:~:text=To%20rotate%20an%20image%2C%20we,as%20shown%20in%20below%20code.&text=The%20code%20listed%20in%20Listing,an%20image%20at%20run%2Dtime.&text=The%20ScaleTransform%20is%20used%20to%20scale%20an%20image.
            image_rotatingUpdate.IsEnabled = true;
            image_rotatingUpdate.Visibility = Visibility.Visible;
            RotateTransform transform = new RotateTransform(rotationTracker + 45);
            image_rotatingUpdate.RenderTransformOrigin = new Point(0.5, 0.5);

            image_rotatingUpdate.RenderTransform = transform;
            rotationTracker += 45;
        }

        //https://stackoverflow.com/questions/938421/getting-the-applications-directory-from-a-wpf-application
        string appPath = System.AppDomain.CurrentDomain.BaseDirectory;//gets the path of were the utility us running

        private void LoadSaveFile()
        {
            if (File.Exists(appPath + @"/DCS-Update-Witching-Utility-Settings/UwU-UserSettings.txt"))
            {
                //MessageBox.Show("Loading...");
                System.IO.StreamReader file = new System.IO.StreamReader(appPath + @"/DCS-Update-Witching-Utility-Settings/UwU-UserSettings.txt");
                //read the file line by line. Assumes the lines are properly formated via the saveSettings method
                //e.g. first line is the users dcs.exe location. The second line is the users options.lua location

                selected_selectDcsExe_string = file.ReadLine();//read the first line
                selected_selectOptionsLua_string = file.ReadLine();//read the second line
                file.Close();
                textBlock_selectDcsExe.Text = selected_selectDcsExe_string;//put the first line in the first box
                textBlock_selectOptionsLua.Text = selected_selectOptionsLua_string;//put the second line in the seecond box

                //figure out the rest of the filepaths
                GeneratePathsFromOptionsLuaPath();
                isOptionsLuaSelected = true;

                GeneratePathsFromDcsExePath();
                isDcsExeSelected = true;

                CheckIfDcsExeAndOptionsLuaHaveBeenSelected();//this should be true

            }
        }

        private void SaveUserSettings()
        {
            //export the 2 directories that the user choose to a .txt file

            //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-write-to-a-text-file
            //string[] exportLines = { fullPath_dcsExe, fullPath_optionsLua, customWidth, customHeight };
            Directory.CreateDirectory(appPath + "\\DCS-Update-Witching-Utility-Settings");//creates the save folder
            //File.WriteAllLines(appPath + "\\DCS-Resolution-Launcher-Settings\\DReLa-UserSettings.txt", exportLines);
            ////https://docs.microsoft.com/en-us/dotnet/api/system.io.streamwriter?redirectedfrom=MSDN&view=netcore-3.1
            using (StreamWriter sw = File.CreateText(appPath + "\\DCS-Update-Witching-Utility-Settings\\UwU-UserSettings.txt"))
            {
                sw.WriteLine(selected_selectDcsExe_string);
                sw.WriteLine(selected_selectOptionsLua_string);
                sw.Close();
            }
        }
        string htmlCode;
        private void GetInitialVersionNumbers()
        {
            DownloadHtmlFile();
            //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-read-a-text-file-one-line-at-a-time
            //int counter = 0;

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
                    string stableVersionNumber = htmlCode.Substring(indexOfWordStable, indexOfSlashAfterVersionNumber - indexOfWordStable);
                    //MessageBox.Show("Stable is version: " + stableVersionNumber);
                    //if you dont do this, the result will be something like "openbeta/2.3.4.12345.3"
                    stableVersionNumber = Regex.Replace(stableVersionNumber, "[^0-9.]", "");//https://stackoverflow.com/questions/3624332/how-do-you-remove-all-the-alphabetic-characters-from-a-string
                    textBlock_stableVersionNumber.Text = ("Latest stable version is: " + stableVersionNumber);

                }
                if (htmlCode.Contains("Current openbeta is"))
                {
                    int indexOfWordStable = htmlCode.IndexOf("changelog/") + 10;
                    int indexOfSlashAfterVersionNumber = htmlCode.IndexOf("/", indexOfWordStable) + 1;
                    indexOfSlashAfterVersionNumber = htmlCode.IndexOf("/", indexOfSlashAfterVersionNumber);
                    string openBetaVersionNumber = htmlCode.Substring(indexOfWordStable, indexOfSlashAfterVersionNumber - indexOfWordStable);
                    //MessageBox.Show("Stable is version: " + openBetaVersionNumber);
                    openBetaVersionNumber = Regex.Replace(openBetaVersionNumber, "[^0-9.]", "");
                    textBlock_openBetaVersionNumber.Text = ("Current openbeta is: " + openBetaVersionNumber);
                }
                //counter++;
            }

            reader.Close();
            //System.Console.WriteLine("There were {0} lines.", counter);
            // Suspend the screen.  
            //System.Console.ReadLine();
            //MessageBox.Show("Lines: " + counter);

        }

        private void DownloadHtmlFile()
        {
            using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
            {
                // Or you can get the file content without saving it
                //htmlCode = client.DownloadString("http://updates.digitalcombatsimulator.com/");
                //htmlCode = File.ReadAllText(@"E:\Downloads\[TEMP]\DCS World Updates2.html");//TODO: Remove this after testing is finished
                
                htmlCode = File.ReadAllText(@"D:\Downloads\DCS World Updates.html");//TODO: Remove this after testing is finished
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            // code goes here
            textBlock_time.Text = "Last Checked: " + DateTime.Now.ToString();
            GetVersionNumbers();
            
        }

        private void GetVersionNumbers()
        {
            DownloadHtmlFile();
            //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-read-a-text-file-one-line-at-a-time
            //int counter = 0;

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
                    if (textBlock_stableVersionNumber.Text.Equals("Latest stable version is: " + stableVersionNumberNew))//if the numbers where the same, there was no update
                    {
                        textBlock_stableVersionNumber.Text = ("Latest stable version is: " + stableVersionNumberNew);
                    }
                    else
                    { //the numbers were likely different
                        //this goes here so that it can be updated regardless if it was the one being monitored
                        textBlock_stableVersionNumber.Text = ("Latest stable version is: " + stableVersionNumberNew);
                        textBlock_stableVersionNumber.Background = new SolidColorBrush(Colors.LightGreen);
                        if (isStableAutoUpdateOn == true)
                        {
                            //MessageBox.Show(textBlock_stableVersionNumber.Text + "\r\n" +
                            //    stableVersionNumberNew);
                            
                            button_autoUpdateDcsViaStable.IsEnabled = false;
                            button_autoUpdateDcsViaOpenbeta.IsEnabled = false;
                            mediaPlayer.Play();
                            dispatcherTimer.Stop();
                            imageRotationTimer.Stop();
                            textBlock_time.Text = "Updated: " + DateTime.Now.ToString();
                            textBlock_time.Background = new SolidColorBrush(Colors.LightGreen);
                            Actions_UpdateDcsViaStableButton();//it presses the button to update stable branch
                        }
                    }


                }

                if (htmlCode.Contains("Current openbeta is"))
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
                //counter++;
            }

            reader.Close();
            //System.Console.WriteLine("There were {0} lines.", counter);
            // Suspend the screen.  
            //System.Console.ReadLine();
            //MessageBox.Show("Lines: " + counter);
        }

        private void Button_UpdateDCS_Click(object sender, RoutedEventArgs e)//done
        {
            Actions_updateButton();
        }

        private void Actions_updateButton()
        {
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            {
                //MessageBox.Show("You pressed the Update DCS Button");
                RunDcsUpdater();
            }
            else
            {
                //dont do anything. This condition is already caught.
            }
        }

        private void RunDcsUpdater()//done
        {
            string dcsUpdaterArguement = ("--quiet update");//this makes things easy

            ProcessStartInfo startInfo = new ProcessStartInfo(selected_selectDcsExe_string, dcsUpdaterArguement);//uses the "quiet update" arguement 
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden; //this hides the program entirely. not want i want
            //startInfo.WindowStyle = 
            Process.Start(startInfo);
        }

        string selected_selectDcsExe_string;
        bool isDcsExeSelected;
        bool isOptionsLuaSelected;
        private void Button_selectDcsExe_Click(object sender, RoutedEventArgs e)
        {

            //Have the select dialog pop up
            OpenFileDialog openFileDialog_selectDcsExe = new OpenFileDialog();
            openFileDialog_selectDcsExe.InitialDirectory = "c:\\Program Files\\Eagle Dynamics\\DCS World\\bin\\";//likely not necessary, but it may help
            openFileDialog_selectDcsExe.Filter = "Application files (*.exe)|*.exe";//pick an exe only
            openFileDialog_selectDcsExe.RestoreDirectory = true;
            openFileDialog_selectDcsExe.Title = "Select DCS.exe (Hint: C:\\Something\\Something2\\Something3\\bin\\DCS.exe";//TODO: Update this
            //the user picks their dcs.exe
            if (openFileDialog_selectDcsExe.ShowDialog() == true)
            {
                var selected_selectDcsExe = openFileDialog_selectDcsExe.FileName;
                selected_selectDcsExe_string = selected_selectDcsExe.ToString();
                if (selected_selectDcsExe.Contains("DCS_updater.exe"))
                {
                    //the user selected the correct correct file
                    //if the file is the correct one, try to make all of the other file paths that are related
                    //to that part of the folder system
                    GeneratePathsFromDcsExePath();
                    textBlock_selectDcsExe.Text = selected_selectDcsExe;
                    isDcsExeSelected = true;

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
        private void Button_selectOptionsLua_Click(object sender, RoutedEventArgs e)
        {
            //Have the select dialog pop up
            //the user picks their options.lua
            OpenFileDialog openFileDialog_selectOptionsLua = new OpenFileDialog();
            openFileDialog_selectOptionsLua.InitialDirectory = "c:\\Users";//gets closer to the location of the file
            openFileDialog_selectOptionsLua.Filter = "lua files (*.lua)|*.lua";//select lua files only
            openFileDialog_selectOptionsLua.RestoreDirectory = true;
            openFileDialog_selectOptionsLua.Title = "Select options.lua (Hint: C:\\Users\\YOURNAME\\Saved Games\\DCS\\Config\\options.lua";
            //the user picks their dcs.exe
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


        string terrainMetacacheLocation_Caucasus;
        string terrainMetacacheLocation_PersianGulf;
        string terrainMetacacheLocation_Syria;
        string terrainMetacacheLocation_TheChannel;
        string terrainMetacacheLocation_Normandy;
        string terrainMetacacheLocation_Nevada;
        string terrainMetacacheLocation_Falklands;
        string terrainMetacacheLocation_Mariana;
        string dcsInstallDirectory;

        private void GeneratePathsFromDcsExePath()
        {
            //generate paths here
            //you need a path for each map
            //\DCS World OpenBeta\Mods\terrains\Caucasus\misc\metacache\dcs
            //\DCS World OpenBeta\Mods\terrains\PersianGulf\misc\metacache\dcs
            //\DCS World OpenBeta\Mods\terrains\Syria\misc\metacache\dcs
            //\DCS World OpenBeta\Mods\terrains\Nevada\misc\metacache\dcs
            //\DCS World OpenBeta\Mods\terrains\TheChannel\misc\metacache\dcs
            //\DCS World OpenBeta\Mods\terrains\Normandy\misc\metacache\dcs

            //\DCS World OpenBeta\Mods\terrains\Mariana\misc\metacache\dcs TODO:Check to make sure this will be correct
            //\DCS World OpenBeta\Mods\terrains\Falklands\misc\metacache\dcs TODO:Check to make sure this will be correct

            dcsInstallDirectory = Path.GetFullPath(Path.Combine(selected_selectDcsExe_string, @"..\..\"));
            //MessageBox.Show(dcsInstallDirectory);//results in something like "C:/ProgramFiles/DCS"

            terrainMetacacheLocation_Caucasus = Path.Combine(dcsInstallDirectory, @"Mods\terrains\Caucasus\misc\metacache\dcs");
            terrainMetacacheLocation_PersianGulf = Path.Combine(dcsInstallDirectory, @"Mods\terrains\PersianGulf\misc\metacache\dcs");
            terrainMetacacheLocation_Syria = Path.Combine(dcsInstallDirectory, @"Mods\terrains\Syria\misc\metacache\dcs");
            terrainMetacacheLocation_Nevada = Path.Combine(dcsInstallDirectory, @"Mods\terrains\Nevada\misc\metacache\dcs");

            //MessageBox.Show(terrainMetacacheLocation_Caucasus + "\r\n"
            //    + terrainMetacacheLocation_PersianGulf + "\r\n"
            //    + terrainMetacacheLocation_Syria + "\r\n");//results in something like "C:/ProgramFiles/DCS"

            //TODO: Check these
            terrainMetacacheLocation_TheChannel = Path.Combine(dcsInstallDirectory, @"Mods\terrains\TheChannel\misc\metacache\dcs");
            terrainMetacacheLocation_Normandy = Path.Combine(dcsInstallDirectory, @"Mods\terrains\Normandy\misc\metacache\dcs");
            terrainMetacacheLocation_Mariana = Path.Combine(dcsInstallDirectory, @"Mods\terrains\Mariana\misc\metacache\dcs");
            terrainMetacacheLocation_Falklands = Path.Combine(dcsInstallDirectory, @"Mods\terrains\Falklands\misc\metacache\dcs");
        }

        string dcsSavedGamesDirectory;
        string userDirectory;
        string dcsInputsFolderPath;
        string dcsFxoFolderPath;
        string dcsMetashaders2FolderPath;
        string dcsTempFolderPathOpenbeta;
        string dcsTempFolderPathOpenalpha;
        string dcsTempFolderPathStable;
        string dcsConfigFolderPath;

        private void GeneratePathsFromOptionsLuaPath()
        {
            //generate paths here
            dcsSavedGamesDirectory = Path.GetFullPath(Path.Combine(selected_selectOptionsLua_string, @"..\..\"));
            //MessageBox.Show(dcsSavedGamesDirectory);//this results in "C:/Users/XXX/Saved Games/DCS.openbeta/"


            //make the filepaths
            dcsInputsFolderPath = Path.Combine(dcsSavedGamesDirectory, @"Config\Input");
            dcsConfigFolderPath = Path.Combine(dcsSavedGamesDirectory, @"Config");//used for the zip file
            dcsFxoFolderPath = Path.Combine(dcsSavedGamesDirectory, @"fxo");
            dcsMetashaders2FolderPath = Path.Combine(dcsSavedGamesDirectory, @"metashaders2");

            //MessageBox.Show(dcsSavedGamesDirectory + "\r\n"
            //    + dcsInputsFolderPath + "\r\n"
            //    + dcsFxoFolderPath + "\r\n"
            //    + dcsMetashaders2FolderPath + "\r\n");



            userDirectory = Path.GetFullPath(Path.Combine(selected_selectOptionsLua_string, @"..\..\..\..\"));
            //MessageBox.Show(userDirectory);//this results in "C:/Users/XXX"

            //make the filepaths
            //the final folder could be a few different names
            dcsTempFolderPathOpenbeta = Path.Combine(userDirectory, @"AppData\Local\Temp\DCS.openbeta");
            dcsTempFolderPathStable = Path.Combine(userDirectory, @"AppData\Local\Temp\DCS");
            dcsTempFolderPathOpenalpha = Path.Combine(userDirectory, @"AppData\Local\Temp\DCS.openalpha");

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
            //MessageBox.Show("Zipping " + dcsInputsFolderPath);
            {
                if (Directory.Exists(dcsInputsFolderPath))
                {
                    string zipFilePathAndName = Path.Combine(dcsConfigFolderPath, "Input-" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".zip");
                    ZipFile.CreateFromDirectory(dcsInputsFolderPath, zipFilePathAndName);
                    if (File.Exists(zipFilePathAndName))
                    {
                        MessageBox.Show("Zip was successful. It is located here: " + zipFilePathAndName);
                    }
                    else
                    {
                        MessageBox.Show("Zip was not successful.");
                    }
                }
                else
                {
                    MessageBox.Show("DCS-UwU could not zip the Input folder located at: " + dcsInputsFolderPath + ". Sorry!");
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

        private void DeleteAllFilesInTheDirectory()
        {
            //https://stackoverflow.com/questions/6452139/how-to-create-a-dialogbox-to-prompt-the-user-for-yes-no-option-in-wpf/6455754
            string sMessageBoxText = "You are about to delete all of the files located in: '" + directoryToDelete + "'\r\n" +
                "There is no 'undo'. " + "\r\n" +
                "Do you want to delete the files?";
            string sCaption = "READ THIS CAREFULLY";

            MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
            MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
          

            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
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

                case MessageBoxResult.No:
                    /* ... */
                    break;

                case MessageBoxResult.Cancel:
                    /* ... */
                    break;
            }
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

        private void Actions_clearTerrainShadersButton()
        {
            //clear the contents of the map terrain shaders folders
            //identifty the proper location
            //identify the installed maps
            //identify the folders that contain the shaders
            //check to see if the above file paths exist
            //delete the contents of the identified folders

            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();

            if (isGoodToProcess == true)
            {
                //=====Caucasus=====
                //check to see if the folder exists
                if (Directory.Exists(terrainMetacacheLocation_Caucasus))
                {
                    directoryToDelete = terrainMetacacheLocation_Caucasus;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + terrainMetacacheLocation_Caucasus + ". Sorry!");
                }

                //=====Syria=====
                //check to see if the folder exists
                if (Directory.Exists(terrainMetacacheLocation_Syria))
                {
                    directoryToDelete = terrainMetacacheLocation_Syria;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + terrainMetacacheLocation_Syria + ". Sorry!");
                }

                //=====theChannel=====
                //check to see if the folder exists
                if (Directory.Exists(terrainMetacacheLocation_TheChannel))
                {
                    directoryToDelete = terrainMetacacheLocation_TheChannel;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + terrainMetacacheLocation_TheChannel + ". Sorry!");
                }

                //=====Nevada=====
                //check to see if the folder exists
                if (Directory.Exists(terrainMetacacheLocation_Nevada))
                {
                    directoryToDelete = terrainMetacacheLocation_Nevada;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + terrainMetacacheLocation_Nevada + ". Sorry!");
                }

                //=====PersianGulf=====
                //check to see if the folder exists
                if (Directory.Exists(terrainMetacacheLocation_PersianGulf))
                {
                    directoryToDelete = terrainMetacacheLocation_PersianGulf;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + terrainMetacacheLocation_PersianGulf + ". Sorry!");
                }


                //=====Normandy=====
                //check to see if the folder exists
                if (Directory.Exists(terrainMetacacheLocation_Normandy))
                {
                    directoryToDelete = terrainMetacacheLocation_Normandy;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + terrainMetacacheLocation_Normandy + ". Sorry!");
                }


                //=====Falklands=====
                //check to see if the folder exists
                if (Directory.Exists(terrainMetacacheLocation_Falklands))
                {
                    directoryToDelete = terrainMetacacheLocation_Falklands;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + terrainMetacacheLocation_Falklands + ". Sorry!");
                }

                //=====Mariana=====
                //check to see if the folder exists
                if (Directory.Exists(terrainMetacacheLocation_Mariana))
                {
                    directoryToDelete = terrainMetacacheLocation_Mariana;
                    DeleteAllFilesInTheDirectory();
                }
                else
                {
                    //MessageBox.Show("DCS-UwU could not find the fxo folder at: " + terrainMetacacheLocation_Mariana + ". Sorry!");
                }
            }
        }

        bool isGoodToProcess;
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
            //clear the folder (check to make sure there isnt anything important in there)
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
            //have an "Are you sure?" dialog pop up
            //if yes:
            //backup the inputs folder
            //do all of the clearing 
            //and then run the updater in quiet mode

            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            {
                //https://stackoverflow.com/questions/6452139/how-to-create-a-dialogbox-to-prompt-the-user-for-yes-no-option-in-wpf/6455754
                string sMessageBoxText = "You are about to perform all 6 backup and clearing actions at once." + "\r\n" +
                    "There is no 'undo'. " + "\r\n" +
                    "Do you want to continue?";
                string sCaption = "READ THIS CAREFULLY";

                MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
                MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                switch (rsltMessageBox)
                {
                    case MessageBoxResult.Yes:
                        //do all the stuff here
                        Actions_backupInputFolderButton();
                        Actions_backupConfigFolderButton();
                        Actions_clearFxoFolderButton();
                        Actions_clearmetashaders2FolderButton();
                        Actions_clearTerrainShadersButton();
                        Actions_DcsTempFolderButton();
                        //actions_updateButton(); TODO:Remove This
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
                //dont do anything.This case is caught beforehand
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
                        MessageBox.Show("Zip was successful. It is located here: " + zipFilePathAndName);
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

        private void Actions_UpdateDcsViaStableButton()
        {
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            {
                string strCmdText;
                strCmdText = "/C \"" + selected_selectDcsExe_string + "\" --quiet update @release";
                //MessageBox.Show(strCmdText);
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

        private void Actions_UpdateDcsViaOpenbetaButton()
        {
            CheckIfDcsExeAndOptionsLuaHaveBeenSelected();
            if (isGoodToProcess == true)
            {
                string strCmdText;
                strCmdText = "/C \"" + selected_selectDcsExe_string + "\" --quiet update @openbeta";
                //MessageBox.Show(strCmdText);
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
                isStableAutoUpdateOn = true;
                dispatcherTimer.Start();
                imageRotationTimer.Start();
                button_autoUpdateDcsViaOpenbeta.IsEnabled = false;
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
                isOpenbetaAutoUpdateOn = true;
                dispatcherTimer.Start();
                imageRotationTimer.Start();
                button_autoUpdateDcsViaStable.IsEnabled = false;
            }
        }
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private void Button_pickAutoUpdateSound_Click(object sender, RoutedEventArgs e)
        {
            Actions_pickAutoUpdateSoundButton();

        }

        private void Actions_pickAutoUpdateSoundButton()
        {
            //https://www.wpf-tutorial.com/audio-video/playing-audio/
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
                //mediaPlayer.Play();//if this plays, it does not get triggered by the AutoUpdate
            }
        }

        private void Button_stopSound_Click(object sender, RoutedEventArgs e)
        {
            Actions_stopSoundButton();

        }

        private void Actions_stopSoundButton()
        {
            mediaPlayer.Stop();
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

        private void Button_selectDcsExe_rightUp(object sender, MouseButtonEventArgs e)
        {
            textBlock_selectDcsExe.Text = null;
        }

        private void Button_selectOptionsLua_rightUp(object sender, MouseButtonEventArgs e)
        {
            textBlock_selectOptionsLua.Text = null;
        }
    
        private void TextBlock_time_rightUp(object sender, MouseButtonEventArgs e)
        {
            //this does not work
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);//hours, minutes, seconds
            MessageBox.Show("Check interval is now 5 seconds.");
        }
       
        private void TextBlock_time_leftDown(object sender, MouseButtonEventArgs e)
        {
            //this does not work
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);//hours, minutes, seconds
            MessageBox.Show("Check interval is now 5 seconds.");
        }

        private void TextBlock_time_mouseWheel(object sender, MouseWheelEventArgs e)
        {
            //this does not work
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);//hours, minutes, seconds
            MessageBox.Show("Check interval is now 5 seconds.");
        }

       
        
        private void Button_WitchEverything_rightUp(object sender, MouseButtonEventArgs e)
        {
            //this one actually works
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);//hours, minutes, seconds
            //MessageBox.Show("Check interval is now 5 seconds.");
            textBlock_time.Foreground = new SolidColorBrush(Colors.Blue);

           
        }


    }
}
