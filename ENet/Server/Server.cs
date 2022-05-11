using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ENet;
using MelonLoader;
using DigitalRuby.Threading;
using GameSavvy.Byterizer;
using JetBrains.Annotations;
using LittleMultiplayer;
using Placemaker;
using UnityEngine;

namespace ENet
{
	public class Server
    {
	    public Host thisHost;
	    public Event netEvent;
	    public EZThread.EZThreadRunner incomingLoop;
		public Dictionary<uint, Server.Player> PlayerList;

		public void StartServer()
        {
			IP.Get();
			BootMaster.instance.worldMaster.maker.ClearUndoQueue();
			GlobalVars.serverPassword = Guid.NewGuid().ToString();

			Library.Initialize();			
			GlobalVars.isServer = true;
			Peer nullPeer = new Peer(); // very very careful with that, check for the server ID 6666 and NOT access it!!!! Danger Will Robinson!!!!

			PlayerList = new Dictionary<uint, Server.Player>();
			AddPlayer(6666, GlobalVars.playerName, GlobalVars.localIP, nullPeer, GlobalVars.playerColor, true);		
			LittleMultiplayer.GUI.myModSettings.AddPlayerButton(6666, GlobalVars.playerName, "Status", GlobalVars.playerColor, new Action(delegate { }));
			
			PlayerList[6666].isServer = true;
						
			thisHost = new Host();
		        Address address = new Address();
		        address.Port = (ushort)GlobalVars.localPort;
	        thisHost.Create(address, GlobalVars.maxPlayers);
			
			MelonLogger.Msg($"[Server] Started on port: " + GlobalVars.localPort);
			GlobalVars.myNetworkStatusWindow.statusText.text = "<color=\"green\">Online";
			GlobalVars.myNetworkStatusWindow.playerCount.text = "1";

			incomingLoop = EZThread.BeginThread(IncomingThreadLoop, false);
		}
		
        public void StopServer()
        {
	        OnScreenMessageManager.DisplayMessage("The server is now stopped!", 5, OnScreenMessageManager.MessageType.Error);
			ClearAllPlayer();
			GlobalVars.myNetworkStatusWindow.statusText.text = "<color=\"red\">Offline";
			GlobalVars.myNetworkStatusWindow.playerCount.text = "0";
			GlobalVars.isServer = false;		
			EZThread.EndThread(incomingLoop);
			thisHost.Dispose();
			Library.Deinitialize();
		}

		public void IncomingThreadLoop()
        {
	        bool polled = false;

	        while (!polled)
	        {
		        if (thisHost.CheckEvents(out netEvent) <= 0)
		        {
			        if (thisHost.Service(0, out netEvent) <= 0)
			        {
				        break;
			        }

			        polled = true;
		        }
				
				switch (netEvent.Type)
		        {
			        case EventType.None:
				        MelonLogger.Msg("no event");
						break;

			        case EventType.Connect:
				        MelonLogger.Msg("[New connection incoming] ID: " + netEvent.Peer.ID + " - IP: " + netEvent.Peer.IP);
						AddPlayer(netEvent.Peer.ID, "Guest", netEvent.Peer.IP, netEvent.Peer, new Color(0.5f,0.5f,0.5f), false);
						netEvent.Peer.Timeout(5, 500, 2000);
				        break;

			        case EventType.Disconnect:
				        MelonLogger.Msg("[Disconnected] IP: " + netEvent.Peer.IP);
				        OnScreenMessageManager.DisplayMessage(GlobalVars.activeServer.GetPlayer(netEvent.Peer.ID).playerName + " has left the game!", 5, OnScreenMessageManager.MessageType.Error);
						StandardSender.BroadcastLogout(netEvent.Peer);
						RemovePlayer(netEvent.Peer.ID);
						break;

			        case EventType.Timeout:
				        MelonLogger.Msg("[Connection lost] IP: " + netEvent.Peer.IP);
						OnScreenMessageManager.DisplayMessage("Someone has lost the connection!", 5, OnScreenMessageManager.MessageType.Error);
						StandardSender.BroadcastLogout(netEvent.Peer);
						RemovePlayer(netEvent.Peer.ID);
						break;

			        case EventType.Receive:				        
						StandardReceiver tmpStdReceiver = new StandardReceiver();
						tmpStdReceiver.IncomingMessage(ref netEvent);
						netEvent.Packet.Dispose();
				        break;
		        }
				
				thisHost.Flush();
		        polled = false;
	        }
        }

		public void SendReliable(Peer playerPeer, ByteStream outByteStream)
		{				
			var packet = default(Packet);
			packet.Create(outByteStream.ToArray(), PacketFlags.Reliable);
			playerPeer.Send(0, ref packet);
		}		

		public void SendUnreliable(ref Peer playerPeer, ByteStream outByteStream)
		{
			var packet = default(Packet);
			packet.Create(outByteStream.ToArray(), PacketFlags.None);
			playerPeer.Send(0, ref packet);
		}

		public void BroadcastReliable(ByteStream outByteStream)
		{
			var packet = default(Packet);
			packet.Create(outByteStream.ToArray(), PacketFlags.Reliable);
			thisHost.Broadcast(0, ref packet);
		}

		public void BroadcastReliableExcludePeer(ByteStream outByteStream, Peer peerToExclude)
		{			
			var packet = default(Packet);
			packet.Create(outByteStream.ToArray(), PacketFlags.Reliable);
			
			thisHost.Broadcast(0, ref packet, peerToExclude);
		}

		public void BroadcastUnreliable(ByteStream outByteStream)
		{
			var packet = default(Packet);
			packet.Create(outByteStream.ToArray(), PacketFlags.None);
			thisHost.Broadcast(0, ref packet);
		}				
	
		public bool AddPlayer(uint playerID, string playerName, string playerIP, Peer peer, Color playerColor, bool isLoggedIn)
		{
			if (!PlayerList.ContainsKey(playerID))
			{
				PlayerList.Add(playerID, new Player());
				PlayerList.TryGetValue(playerID, out Player player);

				player.playerID = playerID;
				player.playerName = playerName;
				player.playerIP = playerIP;
				player.isLoggedIn = false;
				player.playerPeer = peer;
				player.playerColor = playerColor;

				GlobalVars.myNetworkStatusWindow.playerCount.text = GlobalVars.activeServer.PlayerList.Count.ToString();		

				return true;
			}			
			return false;
		}

		public Player GetPlayer(uint playerID)
		{
			PlayerList.TryGetValue(playerID, out Player foundPlayer);

			return foundPlayer;
		}

		public bool PlayerExist(uint playerID)
		{
			if (PlayerList.ContainsKey(playerID))
			{
				return true;
			}
			return false;
		}

		public bool EditPlayerName(uint playerID, string playerName)
		{
			if (!PlayerList.ContainsKey(playerID))
			{
				PlayerList.TryGetValue(playerID, out Player player);
				player.playerName = playerName;
				return true;
			}
			return false;
		}			

		public bool RemovePlayer(uint playerID)
		{
			if (PlayerList.ContainsKey(playerID))
			{
				EZThread.ExecuteOnMainThread(new System.Action(delegate { LittleMultiplayer.GUI.myModSettings.RemovePlayerButton(playerID);}));
				PlayerList.Remove(playerID);
				GlobalVars.myNetworkStatusWindow.playerCount.text = GlobalVars.activeServer.PlayerList.Count.ToString();				
				return true;
			}
			return false;
		}

		public bool ClearAllPlayer()
		{
			PlayerList.Clear();
			EZThread.ExecuteOnMainThread(new System.Action(delegate { LittleMultiplayer.GUI.myModSettings.ClearPlayerButton(); }));
			GlobalVars.myNetworkStatusWindow.playerCount.text = GlobalVars.activeServer.PlayerList.Count.ToString();
			return true;
		}		

		public class Player
		{
			public uint playerID;
			public string playerName;
			public string playerIP;
			public bool isLoggedIn;
			public Peer playerPeer;
			public Color playerColor;
			public Material sparkMaterial;
			public ParticleSystemRenderer sparkRenderer;
			public GameObject playerSpark;
			public bool isServer = false;
		}
	}
}
