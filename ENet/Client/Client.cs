using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DigitalRuby.Threading;
using UnityEngine;
using ENet;
using GameSavvy.Byterizer;
using LittleMultiplayer;
using MelonLoader;
using Placemaker;
using Random = UnityEngine.Random;

namespace ENet
{
	public class Client : MonoBehaviour
	{
		public Host thisClient;
		public Event netEvent;
		public EZThread.EZThreadRunner incomingLoop;
		public Peer serverPeer;
		public Dictionary<uint, Client.Player> playerList;
		
		public void StartClient()
		{
			ENet.Library.Initialize();
			BootMaster.instance.worldMaster.maker.ClearUndoQueue();
			
			playerList = new Dictionary<uint, Client.Player>();
						
			GlobalVars.isClient = true;

			thisClient = new Host();
				Address address = new Address();
				address.SetHost(GlobalVars.remoteIP);
				address.Port = (ushort)GlobalVars.remotePort;
			thisClient.Create();
			
			MelonLogger.Msg("[Connecting] IP: " + GlobalVars.remoteIP + ":" + GlobalVars.remotePort);
			OnScreenMessageManager.DisplayMessage("Connecting to server ...", 0, OnScreenMessageManager.MessageType.Info);

			thisClient.Connect(address);
			incomingLoop = EZThread.BeginThread(IncomingThreadLoop, false);
		}
		
		public void StopClient()
		{			
			GlobalVars.isClient = false;
			GlobalVars.isSynchronizing = false;
			GlobalVars.townIsSynchronized = false;
			GlobalVars.myNetworkStatusWindow.statusText.text = "<color=\"red\">Offline";
			GlobalVars.myNetworkStatusWindow.playerCount.text = "0";
			ClearAllPlayer();

			EZThread.EndThread(incomingLoop);
			thisClient.Dispose();
			Library.Deinitialize();
		}

		public void IncomingThreadLoop()
		{
			if (thisClient.CheckEvents(out netEvent) <= 0)
			{
				if (thisClient.Service(0, out netEvent) <= 0) // default timeout 15
					return;
			}

			serverPeer = netEvent.Peer;

			switch (netEvent.Type)
			{
				case ENet.EventType.None:
					break;

				case ENet.EventType.Connect:
					MelonLogger.Msg("[Connected]");
					GlobalVars.isSynchronizing = true;
					StandardSender.SendLogin(netEvent.Peer, false);
					netEvent.Peer.Timeout(5, 500, 2000);
					break;

				case ENet.EventType.Disconnect:
					MelonLogger.Msg("[Disconnected]");
					OnScreenMessageManager.DisplayMessage("You have left the server!", 5, OnScreenMessageManager.MessageType.Error);
					StopClient();
					break;

				case ENet.EventType.Timeout:
					MelonLogger.Msg("[Connection lost]");
					OnScreenMessageManager.DisplayMessage("You have left the server!", 5, OnScreenMessageManager.MessageType.Error);
					StopClient();
					break;

				case ENet.EventType.Receive:
					StandardReceiver tmpStdReceiver = new StandardReceiver();
					tmpStdReceiver.IncomingMessage(ref netEvent);
					netEvent.Packet.Dispose();
					break;
			}
		}
		
		public void SendReliable(ByteStream outByteStream)
		{
			BootMaster.instance.worldMaster.maker.ClearUndoQueue();

			var packet = default(Packet);
			packet.Create(outByteStream.ToArray(), PacketFlags.Reliable);
		
			serverPeer.Send((int)Protocol.ChannelID.Default, ref packet);
		}

		public void SendUnreliable(ByteStream outByteStream)
		{
			var packet = default(Packet);
			packet.Create(outByteStream.ToArray(), PacketFlags.None);
			
			serverPeer.Send((int)Protocol.ChannelID.Default, ref packet);
		}

		public bool AddPlayer(uint playerID, string playerName, Color playerColor)
		{			
			if (!playerList.ContainsKey(playerID))
			{
				playerList.Add(playerID, new Player());
				playerList.TryGetValue(playerID, out Player player);

				player.playerID = playerID;
				player.playerName = playerName;				
				player.playerColor = playerColor;								
				
				EZThread.ExecuteOnMainThread(new System.Action(delegate { LittleMultiplayer.GUI.myModSettings.AddPlayerButton(player.playerID, playerName, "Status", playerColor, new Action(delegate { })); }));
				GlobalVars.myNetworkStatusWindow.playerCount.text = GlobalVars.activeClient.playerList.Count.ToString();
				EZThread.ExecuteOnMainThread(new System.Action(delegate { OnScreenMessageManager.SetupPlayerSpark(playerID, playerColor); }));

				return true;
			}
			else
			{
				playerList.TryGetValue(playerID, out Player player);
				player.playerID = playerID;
				player.playerName = playerName;
				player.playerColor = playerColor;

				EZThread.ExecuteOnMainThread(new System.Action(delegate { OnScreenMessageManager.SetupPlayerSpark(playerID, playerColor); }));
			}		
			return false;
		}

		public Player GetPlayer(uint playerID)
		{
			playerList.TryGetValue(playerID, out Player foundPlayer);
			return foundPlayer;
		}

		public bool PlayerExist(uint playerID)
		{
			if (playerList.ContainsKey(playerID))
			{
				return true;
			}
			return false;
		}

		public bool RemovePlayer(uint playerID)
		{
			if (playerList.ContainsKey(playerID))
			{
				playerList.Remove(playerID);
				EZThread.ExecuteOnMainThread(new System.Action(delegate { LittleMultiplayer.GUI.myModSettings.RemovePlayerButton(playerID); }));
				GlobalVars.myNetworkStatusWindow.playerCount.text = GlobalVars.activeClient.playerList.Count.ToString();
				return true;
			}
			return false;
		}

		public bool ClearAllPlayer()
		{
			playerList.Clear();
			EZThread.ExecuteOnMainThread(new System.Action(delegate { LittleMultiplayer.GUI.myModSettings.ClearPlayerButton(); }));
			GlobalVars.myNetworkStatusWindow.playerCount.text = "0";
			return true;
		}

		public class Player
		{
			public uint playerID;
			public string playerName;
			public Color playerColor;
			public bool isServer = false;
			public GameObject playerSpark;
			public ParticleSystemRenderer sparkRenderer;
			public Material sparkMaterial;
		}
	}	
}
