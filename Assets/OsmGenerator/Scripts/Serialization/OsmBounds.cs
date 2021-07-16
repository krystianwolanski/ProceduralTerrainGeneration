using System.Xml;
using Assets.OsmGenerator.Scripts.Extensions;
using UnityEngine;

namespace Assets.OsmGenerator.Scripts.Serialization
{
    public class OsmBounds
    {
        public float MinLat { get; }
        public float MaxLat { get; }
        public float MinLon { get; }
        public float MaxLon { get; }
        public Vector3 Centre { get; }


        public OsmBounds(XmlNode node)
        {
            MinLat = node.GetAttribute<float>("minlat");
            MaxLat = node.GetAttribute<float>("maxlat");
            MinLon = node.GetAttribute<float>("minlon");
            MaxLon = node.GetAttribute<float>("maxlon");

            float x = (float) ((MercatorProjection.lonToX(MaxLon) + MercatorProjection.lonToX(MinLon)) / 2);
            float y = (float)((MercatorProjection.latToY(MaxLat) + MercatorProjection.latToY(MinLat)) / 2);

            Centre = new Vector3(x,0,y);
        }
    }
}
