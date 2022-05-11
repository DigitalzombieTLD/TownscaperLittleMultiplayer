using System;
using System.IO;
using UnityEngine;

namespace ENet
{
    public class Protocol
    {
	    public enum MessageType : byte
        {
	        Login = 1,
	        Logout = 2,	       
	        LoginBroadcast = 3,
	        LogoutBroadcast = 4,
	        ChatMessage = 5,
			TownSync = 6,
			AddVoxel = 7,	
			BuildingAllowed = 9,
			RemoveVoxel = 10,
			PaintVoxel = 11,
			PlayerlistBroadcast = 12
		}

        public enum MessageCompression : byte
        {
	        None = 0,
			Enabled = 1,
	        Disabled = 2,
        }

        public enum ChannelID
        {
	        Default = 0,
	        One = 1,
	        Two = 2,
	        Three = 3
		}	
		

		public static Vector3 ColorToVector3(Color color)
		{
			return new Vector3(color.r, color.g, color.b);
		}

		public static Color Vector3ToColor(Vector3 vector3)
		{
			return new Color(vector3.x, vector3.y, vector3.z, 0.8f);
		}
	}

	
}
