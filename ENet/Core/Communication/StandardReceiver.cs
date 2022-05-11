using DigitalRuby.Threading;
using GameSavvy.Byterizer;
using LittleMultiplayer;
using MelonLoader;
using Placemaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ENet
{
	public class StandardReceiver
	{
		public void IncomingMessage(ref Event incomingMessage)
		{
			byte[] readBuffer = new byte[20480];
			byte[] relayBuffer = new byte[20480];

			ByteStream incomingByteStream;
			ByteStream relayByteStream;

			incomingMessage.Packet.CopyTo(readBuffer);
			incomingMessage.Packet.CopyTo(relayBuffer);

			incomingByteStream = new ByteStream(readBuffer);			

			Protocol.MessageType MessageCntrl = (Protocol.MessageType)incomingByteStream.PopByte();
			Protocol.MessageCompression MessageCompression = (Protocol.MessageCompression)incomingByteStream.PopByte();

			GameReceiver tmpGameReceiver = new GameReceiver();

			if (MessageCntrl == Protocol.MessageType.Login)
			{
				this.ReceiveLogin(incomingByteStream, incomingMessage.Peer);
			}			
			else if (MessageCntrl == Protocol.MessageType.LoginBroadcast)
			{
				if (GlobalVars.isClient)
				{
					this.ReceiveLoginBroadcast(incomingByteStream);
				}
			}
			else if (MessageCntrl == Protocol.MessageType.Logout)
			{				
				this.ReceiveLogoutRequest(incomingMessage.Peer);
			}
			else if (MessageCntrl == Protocol.MessageType.LogoutBroadcast)
			{
				this.ReceiveLogoutBroadcast(incomingByteStream);
			}
			else if (MessageCntrl == Protocol.MessageType.PlayerlistBroadcast)
			{
				this.ReceivePlayerlistBroadcast(incomingByteStream);
			}
			else if (MessageCntrl == Protocol.MessageType.ChatMessage)
			{
				ReceiveText(incomingByteStream, incomingMessage.Peer);
			}
			else if (MessageCntrl == Protocol.MessageType.BuildingAllowed)
			{
				if (GlobalVars.isClient)
				{
					tmpGameReceiver.ReceiveBuildingAllowed(incomingByteStream);
				}
			}
			else if (MessageCntrl == Protocol.MessageType.TownSync)
			{
				if (GlobalVars.isClient)
				{
					tmpGameReceiver.ReceiveTownstring(incomingByteStream);
				}
			}
			else if (MessageCntrl == Protocol.MessageType.AddVoxel)
			{				
				if (GlobalVars.isClient)
				{
					tmpGameReceiver.ReceiveAddVoxel(incomingByteStream);
				}
				else if(GlobalVars.isServer && GlobalVars.localServerAllowsBuilding)
				{
					relayByteStream = new ByteStream(relayBuffer);
					
					//ByteStream incomingBS = new ByteStream(incomingByteStream);
					tmpGameReceiver.ReceiveAddVoxel(incomingByteStream);
					
					GlobalVars.activeServer.BroadcastReliableExcludePeer(relayByteStream, incomingMessage.Peer);
				}
			}
			else if (MessageCntrl == Protocol.MessageType.RemoveVoxel)
			{
				if (GlobalVars.isClient)
				{
					tmpGameReceiver.ReceiveRemoveVoxel(incomingByteStream);
				}
				else if (GlobalVars.isServer && GlobalVars.localServerAllowsBuilding)
				{
					relayByteStream = new ByteStream(relayBuffer);
					//ByteStream outgoingByteStream = new ByteStream();
					//outgoingByteStream.Load(incomingByteStream);
					tmpGameReceiver.ReceiveRemoveVoxel(incomingByteStream);
					GlobalVars.activeServer.BroadcastReliableExcludePeer(relayByteStream, incomingMessage.Peer);
					//GlobalVars.activeServer.BroadcastReliableExcludePeer(ManInTheMiddlePeerChanger(relayByteStream, incomingMessage.Peer.ID), incomingMessage.Peer);
				}
			}
			else if (MessageCntrl == Protocol.MessageType.PaintVoxel)
			{
				if (GlobalVars.isClient)
				{
					tmpGameReceiver.ReceivePaintVoxel(incomingByteStream);
				}
				else if (GlobalVars.isServer && GlobalVars.localServerAllowsBuilding)
				{
					ByteStream outgoingByteStream = new ByteStream();
					outgoingByteStream.Load(incomingByteStream);
					tmpGameReceiver.ReceivePaintVoxel(incomingByteStream);
					GlobalVars.activeServer.BroadcastReliableExcludePeer(outgoingByteStream, incomingMessage.Peer);
				}
			}
		}

		public ByteStream ManInTheMiddlePeerChanger(ByteStream inByteStream, uint newPlayerID)
		{
			Protocol.MessageType MessageCntrl = (Protocol.MessageType)inByteStream.PopByte();
			Protocol.MessageCompression MessageCompression = (Protocol.MessageCompression)inByteStream.PopByte();
			uint playerID = inByteStream.PopUInt32();
			

			ByteStream outStream = new ByteStream(inByteStream);
			outStream.Prepend(newPlayerID);
			MelonLogger.Msg("Prepend playerID: " + newPlayerID);
			outStream.Prepend((byte)MessageCompression);
			MelonLogger.Msg("message compression " + MessageCompression.ToString());
			outStream.Prepend((byte)MessageCntrl);
			MelonLogger.Msg("message contrl: " + MessageCntrl.ToString());

			return outStream;
		}

		public void ReceiveLogin(ByteStream incomingByteStream, Peer incomingPeer)
		{			
			UInt32 tmpID = incomingByteStream.PopUInt32();
			string remoteServername = incomingByteStream.PopString();
			string playerName = incomingByteStream.PopString();
			string incomingPassword = incomingByteStream.PopString();
			//GlobalVars.remoteServerAllowsBuilding = incomingByteStream.PopBool();
			bool remoteServerAllowsBuilding = incomingByteStream.PopBool();
			Color playerColor = Protocol.Vector3ToColor(incomingByteStream.PopVector3());
			//PlayerManager.AddPlayer(incomingPeer.ID, playerName, incomingPeer.IP, incomingPeer);

			MelonLogger.Msg("Receiving login data ...");

			if (GlobalVars.isServer)
			{
				bool playernameAlreadyExist = false;

				foreach (KeyValuePair<uint, Server.Player> singlePlayer in GlobalVars.activeServer.PlayerList)
				{
					if (singlePlayer.Value.playerName == playerName)
					{
						playernameAlreadyExist = true;
					}

				}
				
				if (GlobalVars.serverPassword == incomingPassword && !playernameAlreadyExist)
				{
					OnScreenMessageManager.DisplayMessage("A new player joined the game!<br>[" + playerName +"]", 5, OnScreenMessageManager.MessageType.Info);
					MelonLogger.Msg("[New player] ID: " + incomingPeer.ID + " Player: " + playerName + " - IP: " + incomingPeer.IP);
										
					GlobalVars.activeServer.GetPlayer(incomingPeer.ID).isLoggedIn = true;
					GlobalVars.activeServer.GetPlayer(incomingPeer.ID).playerName = playerName;
										
					GlobalVars.activeServer.GetPlayer(incomingPeer.ID).playerColor = playerColor;
					EZThread.ExecuteOnMainThread(new System.Action(delegate { LittleMultiplayer.GUI.myModSettings.AddPlayerButton(incomingPeer.ID, playerName, "Status", playerColor, new Action(delegate { })); }));
					EZThread.ExecuteOnMainThread(new System.Action(delegate { OnScreenMessageManager.SetupPlayerSpark(incomingPeer.ID, playerColor); }));

					GlobalVars.myNetworkStatusWindow.statusText.text = "<color=\"green\">Connected";					
					GlobalVars.myNetworkStatusWindow.playerCount.text = GlobalVars.activeServer.PlayerList.Count.ToString();

					StandardSender.BroadcastLogin(incomingPeer);
					MelonCoroutines.Start(ExecuteAfterTime(1, incomingPeer));					
				}
				else
				{					
					MelonLogger.Msg("[Failed login!] Invitation expired or duplicate playername! " + incomingPeer.ID + "- Player: " + playerName + " - IP: " + incomingPeer.IP);					
					MelonCoroutines.Start(KickAfterTime(1, incomingPeer));					
				}


			}
			else if (GlobalVars.isClient)
			{
				if (GlobalVars.clientPassword == incomingPassword)
				{
					GlobalVars.remoteServerAllowsBuilding = remoteServerAllowsBuilding;
					GlobalVars.remoteServername = remoteServername;
					
					GlobalVars.remoteServerColor = playerColor;
					GlobalVars.myPeerID = tmpID;
					
					GlobalVars.activeClient.AddPlayer(6666, remoteServername, playerColor);
					GlobalVars.activeClient.GetPlayer(6666).isServer = true;
					EZThread.ExecuteOnMainThread(new System.Action(delegate { OnScreenMessageManager.SetupPlayerSpark(6666, playerColor); }));

					GlobalVars.myNetworkStatusWindow.statusText.text = "<color=\"green\">Connected";
					GlobalVars.myNetworkStatusWindow.playerCount.text = GlobalVars.activeClient.playerList.Count.ToString();

					MelonLogger.Msg("[Login successful!] Synchronizing Town ... ");
					OnScreenMessageManager.DisplayMessage("Login successful! Synchronizing Town ...", 0, OnScreenMessageManager.MessageType.Info);
					GlobalVars.isConnected = true;					
				}
				else
				{
					MelonLogger.Msg("[Failed login!] Invitation expired or your playername is already taken!");
					OnScreenMessageManager.DisplayMessage("[" + remoteServername + "]<br>Invitation expired!<br> Please ask for a new invitation code and/or change your playername.", 10, OnScreenMessageManager.MessageType.Error);
					GlobalVars.isConnected = false;
					GlobalVars.isClient = false;
					GlobalVars.activeClient.StopClient();
				}
			}
		}

		public void ReceiveLoginBroadcast(ByteStream incomingByteStream)
		{			
			uint newPlayerID = incomingByteStream.PopUInt32();
			string newPlayer = incomingByteStream.PopString();
		    Color newPlayerColor = Protocol.Vector3ToColor(incomingByteStream.PopVector3());
			
			if (GlobalVars.isClient)
			{
				MelonLogger.Msg(newPlayer + " has joined the game!");
				OnScreenMessageManager.DisplayMessage(newPlayer + " has joined the game! ", 5, OnScreenMessageManager.MessageType.Info);

				GlobalVars.activeClient.AddPlayer(newPlayerID, newPlayer, newPlayerColor);
				
			}
		}

		public void ReceivePlayerlistBroadcast(ByteStream incomingByteStream)
		{
			uint[] newPlayerIDs = incomingByteStream.PopUInt32Array();
			string[] newPlayers = incomingByteStream.PopStringArray();
			Vector3[] newPlayerColors = incomingByteStream.PopVector3Array();
			bool[] isServer = incomingByteStream.PopBoolArray();

			if (GlobalVars.isClient)
			{
				MelonLogger.Msg("Receiving playerlist ...");
				GlobalVars.activeClient.ClearAllPlayer();
				
				int counter = 0;

				foreach (uint singlePlayer in newPlayerIDs)
				{				
					GlobalVars.activeClient.AddPlayer(newPlayerIDs[counter], newPlayers[counter], Protocol.Vector3ToColor(newPlayerColors[counter]));					
					counter++;
				}
			}
		}

		public void ReceiveLogoutRequest(Peer incomingPeer)
		{
			if(GlobalVars.isServer)
			{
				MelonLogger.Msg(GlobalVars.activeServer.GetPlayer(incomingPeer.ID).playerName + " has left the game!");
				OnScreenMessageManager.DisplayMessage(GlobalVars.activeServer.GetPlayer(incomingPeer.ID).playerName + " has left the game!", 5, OnScreenMessageManager.MessageType.Error);
				incomingPeer.Reset();
				GlobalVars.activeServer.RemovePlayer(incomingPeer.ID);
			}
			else if(GlobalVars.isClient)
			{
				MelonLogger.Msg("The server has closed the connection!");
				OnScreenMessageManager.DisplayMessage("The server has closed the connection!", 5, OnScreenMessageManager.MessageType.Error);
				GlobalVars.activeClient.StopClient();
			}
		}

		public void ReceiveLogoutBroadcast(ByteStream incomingByteStream)
		{
			uint oldPlayerID = incomingByteStream.PopUInt32();
			string oldPlayerName = incomingByteStream.PopString();

			if (GlobalVars.isClient)
			{
				MelonLogger.Msg(oldPlayerName + " has left the game!");
				OnScreenMessageManager.DisplayMessage(oldPlayerName + " has left the game!", 5, OnScreenMessageManager.MessageType.Info);

				GlobalVars.activeClient.RemovePlayer(oldPlayerID);				
			}				
		}

		public void ReceiveText(ByteStream incomingByteStream, Peer incomingPeer)
		{
			string textMessage = incomingByteStream.PopString();
					
			MelonLogger.Msg("[Textmessage] ID: " + incomingPeer.ID + "- Player: " + GlobalVars.activeServer.GetPlayer(incomingPeer.ID).playerName + " - Message: " + textMessage);
			OnScreenMessageManager.DisplayMessage("[" + GlobalVars.activeServer.GetPlayer(incomingPeer.ID).playerName + "]<br>" + textMessage, 10, OnScreenMessageManager.MessageType.Chat);						
		}

		public static IEnumerator KickAfterTime(int timer, Peer peerToKick)
		{
			yield return new WaitForSeconds(timer);
			StandardSender.SendLogin(peerToKick, false);
			yield return new WaitForSeconds(2);
			peerToKick.DisconnectLater(0);
		}

		public static IEnumerator ExecuteAfterTime(int timer, Peer receivingPeer)
		{
			yield return new WaitForSeconds(timer);
			EZThread.ExecuteOnMainThread(new System.Action(delegate { StandardSender.SendLogin(receivingPeer, true); }));
			yield return new WaitForSeconds(timer);
			EZThread.ExecuteOnMainThread(new System.Action(delegate { GameSender.SendTownString(receivingPeer.ID, false, ""); }));
			yield return new WaitForSeconds(timer);
			EZThread.ExecuteOnMainThread(new System.Action(delegate { StandardSender.SendPlayerlist(receivingPeer); }));			
		}
	}
}
