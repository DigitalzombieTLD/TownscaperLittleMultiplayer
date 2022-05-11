#define TRACE
using MelonLoader;
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using Open.Nat;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using System.Diagnostics;
using System.Threading;

namespace LittleMultiplayer
{
    public class IP
    {
		public static void Get()
		{
			if(GlobalVars.localIP == "127.0.0.1") // no ip overwrite set // default
			{				
				IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
				GlobalVars.localIP = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
			}

			if (GlobalVars.localPublicIP == "127.0.0.1" && !GlobalVars.localOnlyServer) // no ip overwrite set // default
			{			
				try
				{
					GlobalVars.localPublicIP = new System.Net.WebClient().DownloadString(GlobalVars.requestInternetIP_URL);
				}
				catch
				{
					MelonLogger.Msg("[ERROR] Couldn't contact external IP service. If you want to host a internet server, you need to set your public IP manually or swap out the IP service URL in the LittleMultiplayer.ini file.");
					MelonLogger.Msg("Falling back to local network IP ...");
					GlobalVars.localPublicIP = GlobalVars.localIP;
				}

				StartNATPortMapping();
			}
			else
			{
				GlobalVars.localPublicIP = GlobalVars.localIP;
			}
		}
		
		public static async Task StartNATPortMapping()
		{
			//NatDiscoverer.TraceSource.Switch.Level = SourceLevels.Verbose;
			//ConsoleTraceListener consoleTracer = new ConsoleTraceListener();
			//NatDiscoverer.TraceSource.Listeners.Add(consoleTracer);
			
			try
			{
				var discoverer = new NatDiscoverer();
				var device = await discoverer.DiscoverDeviceAsync();
				await device.CreatePortMapAsync(new Mapping(Protocol.Udp, GlobalVars.localPort, GlobalVars.localPort, "TownscaperLMP"));
			}
			catch (NatDeviceNotFoundException e)
			{
				MelonLogger.Msg("[ERROR] Couldn't open firewall port. You need to forward the incoming port " + GlobalVars.localPort + " manually in your router configuration. Or use an VPN like \"Hamachi\"");
				MelonLogger.Msg(e);
				GlobalVars.localPublicIP = GlobalVars.localIP;
			}			
		}

		public static async Task DeleteNATPortMapping()
		{
			var nat = new NatDiscoverer();
			var cts = new CancellationTokenSource(5000);
			var device = await nat.DiscoverDeviceAsync(PortMapper.Upnp, cts);

			foreach (var mapping in await device.GetAllMappingsAsync())
			{			
				if (mapping.Description.Contains("TownscaperLMP"))
				{					
					await device.DeletePortMapAsync(mapping);
				}
			}

		}

		public static IEnumerator SendTownAfterTime(string newTownString)
		{
			yield return new WaitForSeconds(3);
			GameSender.SendTownString(99, true, newTownString);
		}
	}
}
