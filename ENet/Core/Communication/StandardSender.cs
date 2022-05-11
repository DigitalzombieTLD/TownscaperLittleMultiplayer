using GameSavvy.Byterizer;
using LittleMultiplayer;
using MelonLoader;
using Placemaker;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ENet
{
    public static class StandardSender
    {		
		public static void SendLogin(Peer thisPeer, bool loginPermitted)
		{
			ByteStream outgoingByteStream = new ByteStream();
		
			ByteStream outByteStream = new ByteStream();
			outByteStream.Append((byte)Protocol.MessageType.Login);
			outByteStream.Append((byte)Protocol.MessageCompression.None);
						
			if (GlobalVars.isClient)
			{				
				outByteStream.Append((uint)123);
				outByteStream.Append("Server");
				outByteStream.Append(GlobalVars.playerName);
				outByteStream.Append(GlobalVars.clientPassword);
				outByteStream.Append(GlobalVars.localServerAllowsBuilding);				
				outByteStream.Append(Protocol.ColorToVector3(GlobalVars.playerColor));

				GlobalVars.activeClient.SendReliable(outByteStream);
			}
			else if (GlobalVars.isServer)
			{				
				if (loginPermitted && GlobalVars.activeServer.PlayerExist(thisPeer.ID))
				{						
					outByteStream.Append(thisPeer.ID);					
					outByteStream.Append(GlobalVars.playerName);					
					outByteStream.Append(GlobalVars.activeServer.GetPlayer(thisPeer.ID).playerName);
					outByteStream.Append(GlobalVars.serverPassword);
					outByteStream.Append(GlobalVars.localServerAllowsBuilding);
					outByteStream.Append(Protocol.ColorToVector3(GlobalVars.playerColor));
					GlobalVars.activeServer.SendReliable(thisPeer, outByteStream);
				}
				else
				{					
					outByteStream.Append((uint)6666);
					outByteStream.Append("Server");
					outByteStream.Append(GlobalVars.playerName);
					outByteStream.Append(Guid.NewGuid().ToString());
					outByteStream.Append(GlobalVars.localServerAllowsBuilding);
					outByteStream.Append(Protocol.ColorToVector3(GlobalVars.playerColor));
					GlobalVars.activeServer.SendReliable(thisPeer, outByteStream);
					thisPeer.DisconnectLater(0);
				}				
			}			
		}

		public static void BroadcastLogin(Peer newPeer)
		{
			ByteStream outByteStream = new ByteStream();
			outByteStream.Append((byte)Protocol.MessageType.LoginBroadcast);
			outByteStream.Append((byte)Protocol.MessageCompression.None);
			outByteStream.Append(GlobalVars.activeServer.GetPlayer(newPeer.ID).playerID);
			outByteStream.Append(GlobalVars.activeServer.GetPlayer(newPeer.ID).playerName);			
			outByteStream.Append(Protocol.ColorToVector3(GlobalVars.activeServer.GetPlayer(newPeer.ID).playerColor));
			
			GlobalVars.activeServer.BroadcastReliableExcludePeer(outByteStream, newPeer);
		}

		public static void BroadcastLogout(Peer newPeer)
		{
			ByteStream outByteStream = new ByteStream();
			outByteStream.Append((byte)Protocol.MessageType.LogoutBroadcast);
			outByteStream.Append((byte)Protocol.MessageCompression.None);
			outByteStream.Append(GlobalVars.activeServer.GetPlayer(newPeer.ID).playerID);
			outByteStream.Append(GlobalVars.activeServer.GetPlayer(newPeer.ID).playerName);			

			GlobalVars.activeServer.BroadcastReliableExcludePeer(outByteStream, newPeer);
		}

		public static void SendPlayerlist(Peer newPeer)
		{
			ByteStream outByteStream = new ByteStream();
			outByteStream.Append((byte)Protocol.MessageType.PlayerlistBroadcast);
			outByteStream.Append((byte)Protocol.MessageCompression.None);
			
			uint[] playerIDs = new uint[GlobalVars.activeServer.PlayerList.Count];
			string[] playerNames = new string[GlobalVars.activeServer.PlayerList.Count];
			Vector3[] playerColors = new Vector3[GlobalVars.activeServer.PlayerList.Count];
			bool[] isServer = new bool[GlobalVars.activeServer.PlayerList.Count];

			int counter = 0;

			foreach(KeyValuePair<uint,Server.Player> singlePlayer in GlobalVars.activeServer.PlayerList)
			{				
				playerIDs[counter] = singlePlayer.Value.playerID;
				playerNames[counter] = singlePlayer.Value.playerName;
				playerColors[counter] = Protocol.ColorToVector3(singlePlayer.Value.playerColor);
				counter++;
			}

			outByteStream.AppendArray(playerIDs);
			outByteStream.AppendArray(playerNames);
			outByteStream.AppendArray(playerColors);
			outByteStream.AppendArray(isServer);

			GlobalVars.activeServer.SendReliable(newPeer, outByteStream);
		}

		public static void SendLogout(Peer thisPeer, bool broadcast)
		{
			ByteStream outByteStream = new ByteStream();
			outByteStream.Append((byte)Protocol.MessageType.Logout);
			outByteStream.Append((byte)Protocol.MessageCompression.None);

			if (GlobalVars.isClient)
			{
				GlobalVars.activeClient.SendReliable(outByteStream);
			}
			else if (GlobalVars.isServer)
			{
				if (broadcast)
				{
					GlobalVars.activeServer.BroadcastReliable(outByteStream);
				}
				else
				{
					GlobalVars.activeServer.SendReliable(thisPeer, outByteStream);
				}
			}
		}

		public static void SendText(Peer receiverPeer, string outgoingMessage, bool broadcast)
		{		
			ByteStream outByteStream = new ByteStream();
			outByteStream.Append((byte)Protocol.MessageType.ChatMessage);
			outByteStream.Append((byte)Protocol.MessageCompression.None);
			outByteStream.Append(outgoingMessage);

			if (GlobalVars.isClient)
			{
				GlobalVars.activeClient.SendReliable(outByteStream);
			}
			else if (GlobalVars.isServer)
			{
				if(broadcast)
				{
					GlobalVars.activeServer.BroadcastReliable(outByteStream);
				}
				else
				{
					GlobalVars.activeServer.SendReliable(receiverPeer, outByteStream);
				}				
			}
		}
	}
}
