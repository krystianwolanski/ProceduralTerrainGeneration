using System.Xml;
using Assets.OsmGenerator.Scripts.Extensions;
using UnityEngine;

namespace Assets.OsmGenerator.Scripts.Serialization
{
    public class OsmNode
    {
        public ulong ID { get; }
        public float Latitude { get; }
        public float Longitude { get; }
        public float X { get; }
        public float Y { get; }

        public static implicit operator Vector3(OsmNode node)
        {
            return new Vector3(node.X,0,node.Y);
        }

        public OsmNode(XmlNode node)
        {
            ID = node.GetAttribute<ulong>("id");
            Latitude = node.GetAttribute<float>("lat");
            Longitude = node.GetAttribute<float>("lon");

            X = (float)MercatorProjection.lonToX(Longitude);
            Y = (float) MercatorProjection.latToY(Latitude);
        }
    }
}
