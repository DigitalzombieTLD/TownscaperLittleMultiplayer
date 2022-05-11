using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using ENet;
using MelonLoader;
using Placemaker.Ui;
using UnityEngine;


namespace LittleMultiplayer
{
	public static class MyInput
	{
		public static MelonMod thisMod;
		public static KeyCode myKeycode = KeyCode.None;

		public static void GetInput()
		{
			// Don't accept input before keycodes are loaded from settings
			if(GUI.isInitialized)
			{
			/*
				if (UnityEngine.Input.GetKeyDown(myKeycode))
				{					
					if (GlobalVars.isClient)
					{
						StandardSender.SendText(GlobalVars.activeClient.serverPeer, "the client says hello!", false);

						foreach (KeyValuePair<uint, Client.Player> singlePlayer in GlobalVars.activeClient.playerList)
						{
							MelonLogger.Msg("ID: " + singlePlayer.Value.playerID + " | Name: " + singlePlayer.Value.playerName + " | Color: " + ColorUtility.ToHtmlStringRGBA(singlePlayer.Value.playerColor) + " isServer: " + singlePlayer.Value.isServer);
						}
					}
					else if (GlobalVars.isServer)
					{
						//StandardSender.SendText(GlobalVars.activeClient.serverPeer, "the server says hello!", true);						

						foreach (KeyValuePair<uint, Server.Player> singlePlayer in GlobalVars.activeServer.PlayerList)
						{
							MelonLogger.Msg("ID: " + singlePlayer.Value.playerID + " | Name: " + singlePlayer.Value.playerName + " | IP: " + singlePlayer.Value.playerIP + " | Logged in: " + singlePlayer.Value.isLoggedIn + " | Color: " + ColorUtility.ToHtmlStringRGBA(singlePlayer.Value.playerColor) + " isServer: " + singlePlayer.Value.isServer);							
						}
					}				
				}
				*/
			}
		}
	}
}
