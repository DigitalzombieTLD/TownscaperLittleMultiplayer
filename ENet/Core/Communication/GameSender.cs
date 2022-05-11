using DigitalRuby.Threading;
using ENet;
using GameSavvy.Byterizer;
using MelonLoader;
using Placemaker;
using Placemaker.Quads;
using Unity.Mathematics;
using UnityEngine;

using Harmony;
using Placemaker.Graphs;

namespace LittleMultiplayer
{
	public static class GameSender
	{	
		public static void AddVoxel(uint playerID, bool broadcast, bool ValidAdd, int DestinationHeight, float VertAngle, int2 HexPos, VoxelType VoxelType, float2 PlanePos)
		{			
			ByteStream outByteStream = new ByteStream();
			outByteStream.Append((byte)Protocol.MessageType.AddVoxel);
			outByteStream.Append((byte)Protocol.MessageCompression.None);
			outByteStream.Append(playerID);
			outByteStream.Append(ValidAdd);
			outByteStream.Append(DestinationHeight);
			outByteStream.Append(VertAngle);
			outByteStream.Append(new Vector2(HexPos.x,HexPos.y));
			outByteStream.Append((byte)VoxelType);
			outByteStream.Append(new Vector2(PlanePos.x, PlanePos.y));
			
			BootMaster.instance.worldMaster.maker.ClearUndoQueue();			

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
					GlobalVars.activeServer.SendReliable(GlobalVars.activeServer.GetPlayer(playerID).playerPeer, outByteStream);
				}
			}	
		}

		public static void RemoveVoxel(uint playerID, bool broadcast, bool validRemove, int DestinationHeight, float VertAngle, float2 PlanePos, int2 HexPos, Vector3 voxelPosition, VoxelType voxelType)
		{
			ByteStream outByteStream = new ByteStream();
			outByteStream.Append((byte)Protocol.MessageType.RemoveVoxel);
			outByteStream.Append((byte)Protocol.MessageCompression.None);
			outByteStream.Append(playerID);
			outByteStream.Append(validRemove);
			outByteStream.Append(DestinationHeight);
			outByteStream.Append(VertAngle);
			outByteStream.Append(new Vector2(HexPos.x, HexPos.y));			
			outByteStream.Append(new Vector2(PlanePos.x, PlanePos.y));
			outByteStream.Append(voxelPosition);
			outByteStream.Append((byte)voxelType); // last change

			BootMaster.instance.worldMaster.maker.ClearUndoQueue();

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
					GlobalVars.activeServer.SendReliable(GlobalVars.activeServer.GetPlayer(playerID).playerPeer, outByteStream);
				}
			}
		}

		public static void PaintVoxel(uint playerID, bool broadcast, bool validRemove, int DestinationHeight, float VertAngle, float2 PlanePos, int2 HexPos, Vector3 voxelPosition, VoxelType voxelType)
		{
			ByteStream outByteStream = new ByteStream();
			outByteStream.Append((byte)Protocol.MessageType.PaintVoxel);
			outByteStream.Append((byte)Protocol.MessageCompression.None);
			outByteStream.Append(playerID);
			outByteStream.Append(validRemove);
			outByteStream.Append(DestinationHeight);
			outByteStream.Append(VertAngle);
			outByteStream.Append(new Vector2(HexPos.x, HexPos.y));
			outByteStream.Append(new Vector2(PlanePos.x, PlanePos.y));
			outByteStream.Append(voxelPosition);
			outByteStream.Append((byte)voxelType); // in

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
					GlobalVars.activeServer.SendReliable(GlobalVars.activeServer.GetPlayer(playerID).playerPeer, outByteStream);
				}
			}
		}

		public static void SendBuildingAllowed(uint playerID, bool broadcast, bool buildingAllowed)
		{
			ByteStream outByteStream = new ByteStream();
			outByteStream.Append((byte)Protocol.MessageType.BuildingAllowed);
			outByteStream.Append((byte)Protocol.MessageCompression.None);
			outByteStream.Append(buildingAllowed);

			if (GlobalVars.isServer)
			{
				if (broadcast)
				{
					GlobalVars.activeServer.BroadcastReliable(outByteStream);
				}
				else
				{
					GlobalVars.activeServer.SendReliable(GlobalVars.activeServer.GetPlayer(playerID).playerPeer, outByteStream);
				}
			}
		}

		public static void SendTownString(uint playerID, bool broadcast, string newTownString)
		{
			string townstring = BootMaster.instance.worldMaster.GetSaveString();

			if (newTownString!="")
			{
				townstring = newTownString;
			}

			townstring = BootMaster.instance.worldMaster.GetSaveString();			
			
			//BootMaster.instance.worldMaster.ResetState();

			ByteStream outByteStream = new ByteStream();
			outByteStream.Append((byte)Protocol.MessageType.TownSync);
			outByteStream.Append((byte)Protocol.MessageCompression.None);
			outByteStream.Append(townstring);

			if (broadcast)
			{
				GlobalVars.activeServer.BroadcastReliable(outByteStream);
			}
			else
			{
				GlobalVars.activeServer.SendReliable(GlobalVars.activeServer.GetPlayer(playerID).playerPeer, outByteStream);
			}
		}
	}
}
