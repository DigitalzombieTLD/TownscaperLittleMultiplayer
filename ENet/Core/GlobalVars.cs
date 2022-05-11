using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using MelonLoader;
using Mono.Cecil;
using TMPro;
using UnityEngine;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using DigitalRuby.Threading;
using ENet;
using HarmonyLib;
using Placemaker;
using UnhollowerRuntimeLib;
using ModUI;

namespace LittleMultiplayer
{
    public static class GlobalVars
    {
		public static string playerName = "Player";
		public static string clientPassword = "";
		public static string serverPassword = "1234";
		public static bool localOnlyServer = false;

		public static NetworkStatus myNetworkStatusWindow;

		public static Color playerColor = new Color(0.5f,0.6f,0.7f,0.7f);
		public static Color remoteServerColor = new Color(0.5f, 0.6f, 0.7f, 0.7f);
		public static uint myPeerID;

		public static string remoteIP = "127.0.0.1";
		public static int remotePort = 8052;
		public static string remoteServername = "Server";

		public static string requestInternetIP_URL = "https://townserver.digitalzombie.de/";

		public static string localIP = "127.0.0.1";
		public static string localPublicIP = "127.0.0.1";

		public static int localPort = 8052;
		public static bool isConnected = false;

		public static int maxPlayers = 4;

		public static bool displayInteractionIndicator;

		public static bool remoteServerAllowsBuilding = true;
		public static bool localServerAllowsBuilding = true;

		public static Server activeServer;
		public static Client activeClient;

		public static bool isServer = false;
		public static bool isClient = false;

		public static bool townIsSynchronized = false;
		public static bool isSynchronizing = false;

		public static HoverData currentHoverData;
    }
}
