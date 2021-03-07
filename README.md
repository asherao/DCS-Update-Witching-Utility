# DCS-Update-Witching-Utility by Bailey

*“In folklore, the witching hour or devil's hour is a time of night, typically between 12 and 4 am, that is associated with supernatural events, whereby witches, demons and ghosts are thought to appear and be at their most powerful.”* – Wikipedia

## Introduction
Hello and welcome to DCS-Update Witching Utility. DCS-UwU provides several functions that will prepare your DCS install for a DCS update. Many people swear by these practices. Many do not use them at all. Both with various results. That is why I find the name of the utility quite appropriate. This is not an “everyday” utility. It is meant to be used sparingly, only when you need it. Happy Witching!!! 
*Note: This program has functions that deletes files. It has been tested on multiple systems to keep accidental occurrences to a minimum, aka, zero. If you are not willing to assume that risk or are not comfortable with that, do not use DCS-UwU!*

## Download and Install

1. Download DCS-UwU from the ED Userfiles. *Note: Your virus protection program might report a trojan or virus threat warning. See “Known or Possible Issues”. You may disregard the warning.*
2. Unzip the downloaded file with Winrar, Winzip, or another program of your choice. Make sure to unzip it into its own folder.
3. Read this Readme.
4. Double click the program to start it.


![DCS-UwU-03](https://i.imgur.com/pAnywU4.jpg)  
*DCS-UwU before and after an update*

## How To Use DCS-UwU

**Click the “Select DCS Updater.exe” button.** Select your DCS_updater.exe file. By default, it is normally installed in:
“C:/Program Files/Eagle Dynamics/DCS/bin”. If you are using DCS via Steam, look at the Known and Possible Issues section now.

**Click the “Select Options.lua” button.** Select your options.lua file. By default, it is normally located in: “C:\Users\YOURNAME\Saved Games\DCS\Config”.

You can now access the Witching utilities. When any of the following buttons are clicked, your dcs_updater.exe and options.lua file locations will be saved so that you will not have to locate them the next time you use the program.

**Backup Input Folder** – Zips your DCS Input folder. This is useful for hardware changes, updates, before installing new modules, after installing new modules, migrating DCS installs, or any other time you would like to backup your keyboard and controller bindings.

**Backup Config Folder** – Zips your DCS Config folder. This is useful for similar reasons as you read above,
but covers more things like general settings, monitor settings, custom views, and anything else you have
in that folder, which includes the Input folder.

**Clear Metashaders2 Folder, FXO Folder, Terrain Metacache Folders** – Cleaning these folders is reported
to improve FPS for many people in both flatscreen and VR. Specifically, these folders are the ones you
should want to clear if you are installing or re-installing the VR FPS mod. The next time you run DCS, it
will take longer to load because new shaders are being created.

**Clear DCS Temp Folder** – DCS uses this folder to temporally stage information for missions while you
play DCS, such as briefing images and audio sounds. Unfortunately, DCS sometimes forgets to clear this
folder when the game is closed. Some people have found gigabytes of unnecessary temporary files in
this folder. Clearing them will make the hard drive space available for more important and useful things
(like more DCS Modules!).

**Update DCS via Stable** – Updates DCS to the newest Stable Branch Version. Will also switch from
Openbeta to Stable.

**Update DCS via Openbeta** – Updates DCS to the newest Openbeta Branch Version. Will also switch from
Stable to Openbeta.

**Auto Update DCS Stable** – When this is clicked, DCS-UwU will monitor the DCS World Updates site
(http://updates.digitalcombatsimulator.com/) for changes. If the Stable Version number changes, DCSUwU
will see the change, and automatically update DCS for you. This is useful for when you want DCS to
update as soon as the update is available. It is also useful to give your poor F5 key a break! See “Known
or Possible Issues” for more information.

**Auto Update DCS Openbeta** – Similar to “Auto Update DCS Stable”, but for Openbeta.

**Pick Auto Update Sound** – You can pick an MP3 file to play when an Auto Update for your branch is
detected.

**Stop Sound** – Stops playing the picked sound.

**Witch Everything!!!** – Performs both Backup functions and then performs the four Clearing functions.
This button basically presses the 6 other buttons for you. You will still need to confirm the deletion of
the files (for your safety and sanity).

## How It Works

“Select” buttons – After you have picked your dcs_updater.exe and options.lua file locations, the
program then finds the associated folders to do the functions that are in DCS-UwU.

“Clear” functions – You will have the option to cancel any operation that has the possibility of deleting
any file.

“Update DCS via” buttons – These use a CMD.exe command to update DCS to the respective branch.

“Auto Update” buttons – These will monitor and check the code in the DCS World Updates site at a certain interval. If there is a change to the chosen branch, DCS-UwU will use CMD.exe to update DCS.

## Known or Possible Issues

- If you have an irregular DCS Installs such as symbolic references or customized file locations (you know who you are…), DCS-UwU may not work for you. Feel free to contact me with your specific situation and I will see if it can be accommodated.
- Eagle Dynamics sometimes updates the DCS World Updates site in an unpredictable manner. The “Auto Update DCS” feature may not work correctly if ED updates the site in an unpredictable manner.
- When downloading DCS-UwU, you may get a warning about malicious software from your virus protection program. I have not included malicious software or code.
- Workaround for DCS Steam Installs:
	1. Right-click DCS Word Steam Edition in your Steam Library. Select Manage. Select Browse Local Files.
	2. Double-click the “bin” folder.
	3. Locate the “DCS.exe” file. Copy and paste it in the same location. You should see the new file as “DCS - Copy.exe”.
	4. Rename “DCS - Copy.exe” to “DCS_updater.exe”. It is case sensitive.
	5. Use “DCS_updater.exe” for DCS-UwU. You will not be able to use the “Update” or “Autoupdate” features (because you already have them, silly!)

## Acknowledgments and Credits

Thanks to all of those who make mods and dev for DCS.  
Thanks to Eagle Dynamics.  
Thanks to everyone who has spread the word about my mods and addons for DCS.  
Thanks to JCofDI for being the first to be Witched :D  

-Please join us in the Discord server: https://discord.gg/PbYgC5e

-Please feel free to donate. All donations go back into DCS to create more free mods, just like this one: https://www.paypal.com/paypalme/asherao

-Check out my other Mods, Utilities, and VoiceAttack profiles here: https://www.digitalcombatsimulator.com/en/files/filter/user-is-asherao/apply/?PER_PAGE=100

-Check out DCS-UwU on the DCS forums

-If you would like to contribute to the development of DCS-UwU, check out the code, or if you like spaghetti, the DCS-UwU Github is located here: https://github.com/asherao/DCS-Update-Witching-Utility

Feel free to contact me on the ED forums (Bailey) or better yet on Discord (Bailey#6230). Remember that comments, questions, critiques, and requests are always welcome! Enjoy!

~Bailey
MAR2021

## Version Notes:
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
