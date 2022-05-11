using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Windows;
using ENet;
using GameSavvy.Byterizer;
using Il2CppSystem.Net;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;

namespace LittleMultiplayer
{
	public static class ClipboardMagic
	{
		public static string outgoingString;
		public static string incomingString;
		
		public static void Deserialize(string clipboardContent)
		{
			string playerName;		
			
			try
			{
				ByteStream deserialize = new ByteStream(Convert.FromBase64String(clipboardContent));
				if (deserialize.Length>0)
				{					
					playerName = deserialize.PopString();					
					GlobalVars.remoteIP = deserialize.PopString();
					GlobalVars.remotePort = deserialize.PopInt32();
					GlobalVars.clientPassword = deserialize.PopString();
					GlobalVars.remoteServerAllowsBuilding = deserialize.PopBool();
				}
			}
			catch
			{
				MelonLogger.Msg("Pasted connnection details faulty!");
				OnScreenMessageManager.DisplayMessage("The pasted invitation code is faulty!", 10, OnScreenMessageManager.MessageType.Error);
			}
		}

		public static void ReadFromClipBoard()
		{
			if (GUIUtility.systemCopyBuffer != null)
			{
				string systemCopyBuffer = GUIUtility.systemCopyBuffer;
				Deserialize(systemCopyBuffer);
			}
		}

		public static void CopyToClipboard(bool local)
		{
			if (GlobalVars.isServer)
			{
				GlobalVars.serverPassword = Guid.NewGuid().ToString();
				ByteStream Serialize = ClipboardMagic.Serialize(local);
				GUIUtility.systemCopyBuffer = Convert.ToBase64String(Serialize.ToArray());

				OnScreenMessageManager.DisplayMessage("Invitation copied to clipboard!<br>Send it to a friend", 10, OnScreenMessageManager.MessageType.Info);

				if(local)
				{
					MelonLogger.Msg("Invitation copied to clipboard. Connection " + GlobalVars.localIP + ":" + GlobalVars.localPort);
				}
				else
				{
					MelonLogger.Msg("Invitation copied to clipboard. Connection " + GlobalVars.localPublicIP + ":" + GlobalVars.localPort);
				}
			}
		}

		public static ByteStream Serialize(bool local)
		{		
			ByteStream outByteStream = new ByteStream();
			outByteStream.Append(GlobalVars.playerName);

			if (local)
			{
				outByteStream.Append(GlobalVars.localIP);
			}
			else
			{
				outByteStream.Append(GlobalVars.localPublicIP);
			}

			outByteStream.Append(GlobalVars.localPort);			
			outByteStream.Append(GlobalVars.serverPassword);
			outByteStream.Append(GlobalVars.localServerAllowsBuilding);

			return outByteStream;
		}
	}
}
