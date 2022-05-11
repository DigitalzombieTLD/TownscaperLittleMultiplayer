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
	public class GameReceiver
	{
		public void ReceiveAddVoxel(ByteStream receivedVoxel)
		{			
			uint playerID = receivedVoxel.PopUInt32();
			bool ValidAdd = receivedVoxel.PopBool();			
			int DestinationHeight = receivedVoxel.PopInt32();
			float VertAngle = receivedVoxel.PopFloat();
			Vector2 HexPosReceived = receivedVoxel.PopVector2();
			VoxelType VoxelType = (VoxelType)receivedVoxel.PopByte();
			Vector2 PlanePosReceived = receivedVoxel.PopVector2();
			int2 HexPos = new int2((int)HexPosReceived.x, (int)HexPosReceived.y);
			float2 PlanePos = new float2(PlanePosReceived.x, PlanePosReceived.y);

			EZThread.ExecuteOnMainThread(new System.Action(delegate { AddClick(playerID, ValidAdd, DestinationHeight, VertAngle, HexPos, VoxelType, PlanePos); }));
		}

		public void ReceiveRemoveVoxel(ByteStream receivedVoxel)
		{
			uint playerID = receivedVoxel.PopUInt32();
			bool ValidRemove = receivedVoxel.PopBool();
			int DestinationHeight = receivedVoxel.PopInt32();
			float VertAngle = receivedVoxel.PopFloat();
			Vector2 HexPosReceived = receivedVoxel.PopVector2();					
			Vector2 PlanePosReceived = receivedVoxel.PopVector2();
			int2 HexPos = new int2((int)HexPosReceived.x, (int)HexPosReceived.y);
			float2 PlanePos = new float2(PlanePosReceived.x, PlanePosReceived.y);
			Vector3 voxelPosition = receivedVoxel.PopVector3();
			VoxelType voxelType = (VoxelType)receivedVoxel.PopByte(); // out

			EZThread.ExecuteOnMainThread(new System.Action(delegate { RemoveClick(playerID, ValidRemove, DestinationHeight, VertAngle, HexPos, PlanePos, voxelPosition, voxelType); }));
		}

		public void ReceivePaintVoxel(ByteStream receivedVoxel)
		{
			uint playerID = receivedVoxel.PopUInt32();
			bool ValidRemove = receivedVoxel.PopBool();
			int DestinationHeight = receivedVoxel.PopInt32();
			float VertAngle = receivedVoxel.PopFloat();
			Vector2 HexPosReceived = receivedVoxel.PopVector2();
			Vector2 PlanePosReceived = receivedVoxel.PopVector2();
			int2 HexPos = new int2((int)HexPosReceived.x, (int)HexPosReceived.y);
			float2 PlanePos = new float2(PlanePosReceived.x, PlanePosReceived.y);
			Vector3 voxelPosition = receivedVoxel.PopVector3();
			VoxelType voxelType = (VoxelType)receivedVoxel.PopByte(); // out

			EZThread.ExecuteOnMainThread(new System.Action(delegate { RemoveClick(playerID, ValidRemove, DestinationHeight, VertAngle, HexPos, PlanePos, voxelPosition, voxelType); }));
		}

		public void AddClick(uint playerID, bool ValidAdd, int DestinationHeight, float VertAngle, int2 HexPos, VoxelType VoxelType, float2 PlanePos)
		{			
			HoverData currentHoverData = GlobalVars.currentHoverData;

			if (ValidAdd)
			{
				var vert = new Vert()
				{
					angle = VertAngle,
					hexPos = HexPos
				};

				if (!vert.full)
					vert = currentHoverData.master.grid.GetVertOrIterate(HexPos, null);

				if (!vert.full || !currentHoverData.master.graph.IsCoordinateAllowed(vert.hexPos, DestinationHeight))
					return;


				currentHoverData.master.maker.BeginNewAction();		
				currentHoverData.master.maker.AddAction(vert.hexPos, (byte)DestinationHeight, VoxelType.Empty, VoxelType);
				var lastAddedVoxel = currentHoverData.master.graph.AddVoxel(vert.hexPos, (byte)DestinationHeight, VoxelType, true);
				currentHoverData.master.maker.lastAddedVoxel = lastAddedVoxel;				
				currentHoverData.master.maker.EndAction();
				currentHoverData.master.clickEffect.Click(true, PlanePos, DestinationHeight, VoxelType);
				currentHoverData.master.maker.ClearUndoQueue();
				
				OnScreenMessageManager.SparkParticle(playerID, lastAddedVoxel.gameObject.transform.position);				
			}
		}

		//public void RemoveClick(bool ValidAdd, int DestinationHeight, float2 PlanePos, int2 HexPos, Vector3 voxelPosition)
		public void RemoveClick(uint playerID, bool ValidRemove, int DestinationHeight, float VertAngle, int2 HexPos, float2 PlanePos, Vector3 voxelPosition, VoxelType voxelType)
		{
			HoverData currentHoverData = GlobalVars.currentHoverData;

			if (ValidRemove)
			{			
				foreach (var voxel in UnityEngine.Object.FindObjectsOfType<Voxel>())
				{					
					if (voxel.transform.position.Equals(voxelPosition))
					{
						if (voxel != null && voxel.type != VoxelType.Empty && voxel.type != VoxelType.Any && voxel.type != VoxelType.Water)
						{
							var vert = new Vert()
							{
								angle = VertAngle,
								hexPos = HexPos
							};

							OnScreenMessageManager.SparkParticle(playerID, voxelPosition);

							int2 hexPos = voxel.GetComponentInParent<Corner>().hexPos;
													
							currentHoverData.master.maker.BeginNewAction();													
							currentHoverData.master.maker.AddAction(hexPos, voxel.height, voxel.type, VoxelType.Empty);
							currentHoverData.master.graph.RemoveVoxel(voxel);
														
							currentHoverData.master.maker.EndAction();							
							currentHoverData.master.clickEffect.Click(false, PlanePos, DestinationHeight, VoxelType.Empty);													
						}
					}
				}
				currentHoverData.master.maker.ClearUndoQueue();
			}			
		}

		public void PaintClick(bool ValidRemove, int DestinationHeight, float VertAngle, int2 HexPos, float2 PlanePos, Vector3 voxelPosition, VoxelType voxelType)
		{
			HoverData currentHoverData = GlobalVars.currentHoverData;

			if (ValidRemove)
			{
				foreach (var voxel in UnityEngine.Object.FindObjectsOfType<Voxel>())
				{
					if (voxel.transform.position.Equals(voxelPosition))
					{

						if (voxel != null && voxel.type != VoxelType.Empty && voxel.type != VoxelType.Any && voxel.type != VoxelType.Water)
						{
							var vert = new Vert()
							{
								angle = VertAngle,
								hexPos = HexPos
							};

							int2 hexPos = voxel.GetComponentInParent<Corner>().hexPos;

							currentHoverData.master.maker.BeginNewAction();
							currentHoverData.master.maker.AddAction(HexPos, voxel.height, VoxelType.Empty, voxel.type);
							currentHoverData.master.graph.PaintVoxel(voxel, voxelType, null);
							currentHoverData.master.maker.EndAction();
							currentHoverData.master.clickEffect.Click(false, PlanePos, DestinationHeight, VoxelType.Empty);
						}
					}
				}
			}
		}

		public void ReceiveTownstring(ByteStream incomingByteStream)
		{			
			string townString = incomingByteStream.PopString();
			GlobalVars.isSynchronizing = true;

			if (GlobalVars.isClient)
			{
				EZThread.ExecuteOnMainThread(new System.Action(delegate { SynchronizeTown(townString); }));				
			}
		}

		public void SynchronizeTown(string townString)
		{
			if(!GlobalVars.townIsSynchronized)
			{
				GlobalVars.townIsSynchronized = true;
				//BootMaster.instance.worldMaster.SaveCurrentAndLoadAsNew(townString);
				//BootMaster.instance.worldMaster.ResetState();
				BootMaster.instance.worldMaster.Load(townString);
				GlobalVars.townIsSynchronized = false;
			}		

			GlobalVars.isSynchronizing = false;
			OnScreenMessageManager.DisplayMessage("Town is now synchronized!<br> Enjoy the game :)", 6, OnScreenMessageManager.MessageType.Info);
		}

		public void ReceiveBuildingAllowed(ByteStream incomingByteStream)
		{
			GlobalVars.remoteServerAllowsBuilding = incomingByteStream.PopBool();

			if (GlobalVars.isClient)
			{
				if (GlobalVars.remoteServerAllowsBuilding)
				{
					MelonLogger.Msg("Building is now allowed!");
					OnScreenMessageManager.DisplayMessage("[Building is now allowed!]", 7, OnScreenMessageManager.MessageType.Info);
				}
				else
				{
					MelonLogger.Msg("Building is now forbidden!");
					OnScreenMessageManager.DisplayMessage("[Building is now forbidden!", 7, OnScreenMessageManager.MessageType.Error);
				}
			}
		}	

	}
}
