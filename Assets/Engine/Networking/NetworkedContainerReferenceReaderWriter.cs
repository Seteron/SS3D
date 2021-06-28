using Mirror;
using SS3D.Engine.Inventory;
using UnityEngine;

namespace SS3D.Engine.Networking
{
    public static class NetworkedContainerReferenceReaderWriter
    {
        public static void WriteNetworkedContainerReference(this NetworkWriter writer, NetworkedContainerReference container)
        {
           NetworkWriterExtensions.WriteUInt(writer, container.SyncNetworkId);
           NetworkWriterExtensions.WriteUInt(writer, container.ContainerIndex);
        }
        
        public static NetworkedContainerReference ReadNetworkedContainerReference(this NetworkReader reader)
        {
            uint networkId = NetworkReaderExtensions.ReadUInt(reader);
            uint index = NetworkReaderExtensions.ReadUInt(reader);
            return new NetworkedContainerReference
            {
                SyncNetworkId = networkId,
                ContainerIndex = index
            };
        }
    }
}