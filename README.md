Townscaper - LittleMultiplayer - v1.0.0 by Digitalzombie
===========================================================

![Screenshot](https://github.com/DigitalzombieTLD/TownscaperLittleMultiplayer/raw/master/LittleMultiplayer.jpg)

How to install:
===============

1. Download "Melon Loader" by LavaGang:
https://github.com/LavaGang/MelonLoader/releases/latest/

2. Start the MelonLoader.Installer.exe

2.1. Click "Select" and navigate to your Townscaper folder and select the Townscaper.exe (usually: C:\Program Files(x86)\Steam\steamapps\common\Townscaper\Townscaper.exe)

2.2. **Untick the "Latest" checkbox and choose version 0.5.7**

2.3. Click install 

2.4. During the installation a "Mods" folder gets created it your game folder. MelonLoader does not(!) change any game files. 
	 You can uninstall anytime through the installer or by deleting the "version.dll" file.

3. Download the mod (latest release) from: https://github.com/DigitalzombieTLD/TownscaperLittleMultiplayer/releases/latest/

4. Extract the all files and folders from the "LittleMultiplayer_Release_1.0.0.zip" into your Townscaper folder.

- Place the "Byterizer.dll", "enet.dll" and "Open.Nat.dll" inside your Townscaper folder (next to the Townscaper.exe). Overwrite existing files and folders.
- Place the "LittleMultiplayer.dll", "LittleMultiplayer.unity3d" and the "_ModSettings" folder inside your Mods folder. Overwrite existing files and folders.

5. Download the utility mod "ModUI" (latest release) from: https://github.com/DigitalzombieTLD/TownscaperModUI/releases/latest/ and place it into your mods folder according to the readme.
Overwrite existing files.

6. Start the game! 

===========================
===========================


What does it do?
=================

It enable you to play Townscaper with your friends! There is no artificial player limit.


How to use it
==============

1. You can open the settings menu when hovering over the top left corner of your gamescreen with the mouse.

2. Set up your Playername and color. Hit "Apply" to save your settings.

3.  [Hosting a server]
3a. Click on "Create Server!" to start the server and allow incoming connections

3b. Click on "Copy Local Invite" if you want to play inside your local network

3c.  or click on "Copy Internet Invite" if you want to play over the internet

3d. An invite code will get copied to your clipboard

3e. Paste it (CTRL+V or Right-Mouse-> Paste) in Discord, send it via email or write it on a postcard to get it to your friends

4. [Joining a server]
4a. Get the invite code from Discord, email or type it from a postcard into a textfile

4b. Copy the code to your clipboard (CRTL+C or Right-Mouse-> Copy)

4c. Click on "Join Server!"

4d. After a few moments you will be connected and the town will be synchronized

5. Click "Close Connections" to stop Client and/or Server


Note:
One invite code is valid for many players. It is valid as long as no new code is created (by copying).

When a new code is created, current players will stay connected but new players need the new code.


Features
=================

- No artificial player limit
- Disable client interactions if you just want to show your town
- (Optional) graphical indicator in playercolors if anything is build/removed
- Automatically synchronizes clients if the server loads or creates a new town
- Duplicate playernames are not allowed
- Automatic port forwarding through firewall. No manual configuration necessary ... if everything goes right
- Manual configuration through ini file advanced users


What if there are problems?
===========================
A connection over the internet can fail if the mod can't communicate with your firewall/router to open up a port.

You've got two options:
1. "Forward the port" (router 8052 to your local pc 8052) in your router configuration manually
2. Use a VPN service like "Hamachi" and simulate a local game over the internet

[KNOW PROBLEMS]
Bigger towns can crash the game on synchronization. I can increase the limit on request and after the first testing phase.
In general, if something goes wrong, the game is mostly likely to crash on player or server side. 

Saving your town is highly recommended! Please report any problems you encounter (Discord link at the end or on YouTube)

Activity indicator for other players broken, because of changes in the game. Nothing I can do about that. Sorry.


Technical stuff for advanced users
===================================

LittleMultiPlayer runs on default UDP port 8052. On (internet) server start the port is opened locally, and a UPNP request is
sent to open the same port to forward it to the local machine. On game exit, a deletion request is sent.

The mod tries to aquire the external IP for the invite code through a thirdparty service. Right now thats me.
It's a simple PHP script that answers with the client IP on opening. It's included in the sourcecode for selfhosting.

The URL for the PHP script can be change in the INI file.
If an external self-hosting is not possible, a static public IP can be set there also. 

Same goes for manual configuration of the port or even completely disabling the internet access.
If the default values (127.0.0.1) are present, the settings are not used by the mod.



===========================
===========================

Keep yourself up to date on my projects on:
https://www.youtube.com/c/DigitalzombieDev

Support through the Townscaper Community Discord:
https://discord.gg/F46YJu8ZUJ

Thanks to:
LavaGang for MelonLoader


Townscaper LittleMultiplayer is licensed under GNU General Public License v3.0

===========================
===========================

Changelog:
==========
1.0.0 - Updated for the latest game version, lost the activity indicator

0.9.0 - First release!
