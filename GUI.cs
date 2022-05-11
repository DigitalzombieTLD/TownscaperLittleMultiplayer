using MelonLoader;
using UnityEngine;
using ModUI;
using System;
using ENet;
using TMPro;

namespace LittleMultiplayer
{
	public static class GUI
	{
		public static MelonMod myMod;
		public static ModSettings myModSettings;

		public static bool isInitialized;

		public static void Initialize(MelonMod thisMod)
		{			
			myModSettings = UIManager.Register(thisMod, new Color32(209, 54, 138, 255));

			myModSettings.AddInputField("Playername", "General", new Color32(239, 131, 135, 200), TMP_InputField.ContentType.Alphanumeric, "Player", new Action<string>(delegate (string value) { PlayerNameAction(value); }));
			myModSettings.GetValueString("Playername", "General", out GlobalVars.playerName);

			myModSettings.AddColorSlider("Playercolor", "General", GlobalVars.playerColor, 0.1f, 0.9f,  new Action<float>(delegate { PlayerColorChangeAction(); }));
			myModSettings.GetValueColor("Playercolor", out GlobalVars.playerColor);

			myModSettings.AddButton("Join Server!", "Client", new Color32(0, 186, 61, 200), new Action(delegate { JoinServerButtonAction(); }));
		
			myModSettings.AddButton("Create Server!", "Server", new Color32(0, 186, 61, 200), new Action(delegate { CreateServerButtonAction(); }));

			myModSettings.AddButton("Copy Local Invite", "Server", new Color32(84, 179, 227, 200), new Action(delegate { ClipboardMagic.CopyToClipboard(true); }));
			myModSettings.AddButton("Copy Internet Invite", "Server", new Color32(43, 135, 227, 200), new Action(delegate { ClipboardMagic.CopyToClipboard(false); }));
			
			myModSettings.AddToggle("Enable Guest Building", "Server", new Color32(222, 56, 133, 200), GlobalVars.localServerAllowsBuilding, new Action<bool>(delegate (bool value) { ToggleGuestBuilding(value); }));
			myModSettings.GetValueBool("Enable Guest Building", "Server", out GlobalVars.localServerAllowsBuilding);

			myModSettings.AddButton("Close Connections", "Client", new Color32(191, 0, 100, 200), new Action(delegate { CloseConnectionButtonAction(); }));

			myModSettings.AddToggle("Enable Interaction Indicator", "Misc", new Color32(209, 212, 0, 200), GlobalVars.displayInteractionIndicator, new Action<bool>(delegate (bool value) { ToggleInteractionIndicator(value); }));
			myModSettings.GetValueBool("Enable Interaction Indicator", "Misc", out GlobalVars.displayInteractionIndicator);


			bool doesInternetIPExist = myModSettings.GetValueString("MyInternetIP", "Server", out GlobalVars.localPublicIP);
			if (!doesInternetIPExist)
			{
				myModSettings.SetValueString("MyInternetIP", "Server", "127.0.0.1");
				GlobalVars.localPublicIP = "127.0.0.1";
			}

			bool doesLocalIPExist = myModSettings.GetValueString("MyLocalIP", "Server", out GlobalVars.localIP);
			if (!doesLocalIPExist)
			{
				myModSettings.SetValueString("MyLocalIP", "Server", "127.0.0.1");
				GlobalVars.localIP = "127.0.0.1";
			}

			bool doesLocalPortExist = myModSettings.GetValueInt("LocalPort", "Server", out GlobalVars.localPort);
			if (!doesLocalPortExist)
			{
				myModSettings.SetValueInt("LocalPort", "Server", 8052);
				GlobalVars.localPort = 8052;
			}

			bool localOnly = myModSettings.GetValueBool("LocalOnly", "Server", out GlobalVars.localOnlyServer);
			if (!localOnly)
			{
				myModSettings.SetValueBool("LocalOnly", "Server", false);
				GlobalVars.localOnlyServer = false;
			}

			bool doesIPURLExist = myModSettings.GetValueString("Get external IP service URL", "Server", out GlobalVars.requestInternetIP_URL);
			if (!doesIPURLExist)
			{
				myModSettings.SetValueString("Get external IP service URL", "Server", "https://townserver.digitalzombie.de/");
				GlobalVars.requestInternetIP_URL = "https://townserver.digitalzombie.de/";
			}

			bool doesMaxPlayersExist = myModSettings.GetValueInt("MaxPlayerAllowed", "Server", out GlobalVars.maxPlayers);
			if (!doesMaxPlayersExist)
			{
				myModSettings.SetValueInt("MaxPlayerAllowed", "Server", 10);
				//GlobalVars.maxPlayers = 4;
			}
						
			myModSettings.AddButton("Apply", "Save", new Color32(180, 200, 104, 200), new Action(delegate { SaveAll(); }));
			GlobalVars.myNetworkStatusWindow = LittleMultiplayer.GUI.myModSettings.AddNetworkStatus("StatusWindow", "Status", new Color32(180, 200, 104, 255));
			
			UpdateValues();
			isInitialized = true;
		}

		public static void UpdateValues()
		{
			myModSettings.GetValueString("Playername", "General", out GlobalVars.playerName);						
			myModSettings.GetValueKeyCode("MyKeybind", "Input", out MyInput.myKeycode);
			myModSettings.GetValueBool("Enable Guest Building", "Server", out GlobalVars.localServerAllowsBuilding);
			myModSettings.GetValueColor("Playercolor", out GlobalVars.playerColor);
			myModSettings.GetValueBool("Enable Interaction Indicator", "Misc", out GlobalVars.displayInteractionIndicator);
			
			//myModSettings.SaveToFile();
		}
		public static void DoNothing()
		{
		
		}

		public static void ToggleInteractionIndicator(bool value)
		{
			GlobalVars.displayInteractionIndicator = value;
		}
		
		public static void CreateServerButtonAction()
		{
			if (!GlobalVars.isClient && !GlobalVars.isServer)
			{
				OnScreenMessageManager.DisplayMessage("Server started! <br> Waiting for new players ...", 15, OnScreenMessageManager.MessageType.Info);				
				GlobalVars.activeServer = new Server();
				GlobalVars.activeServer.StartServer();
			}
		}

		public static void JoinServerButtonAction()
		{
			if (!GlobalVars.isServer)
			{
				if (!GlobalVars.isClient)
				{
					ClipboardMagic.ReadFromClipBoard();

					GlobalVars.activeClient = new Client();
					GlobalVars.activeClient.StartClient();
				}		
			}
		}

		public static void SaveAll()
		{
			UpdateValues();
			myModSettings.SaveToFile();
		}

		public static void CloseConnectionButtonAction()
		{
			if (GlobalVars.isClient)
			{
				OnScreenMessageManager.DisplayMessage("You left the server!", 5, OnScreenMessageManager.MessageType.Error);
				GlobalVars.activeClient.StopClient();
			}

			if (GlobalVars.isServer)
			{
				GlobalVars.activeServer.StopServer();
			}
		}

		public static void MySliderActionExample(float value)
		{
			
		}
		public static void MySliderActionExample(bool value)
		{
			
		}

		public static void PlayerNameAction(string value)
		{
			
			
		}

		public static void PlayerColorChangeAction()
		{
			myModSettings.GetValueColor("Playercolor", out GlobalVars.playerColor);
		}

		public static void ToggleGuestBuilding(bool value)
		{	
			GlobalVars.localServerAllowsBuilding = value;
			
			if (GlobalVars.isServer)
			{				
				GameSender.SendBuildingAllowed(0, true, value);
			}
		}
	}
}
