using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Assets.OsmGenerator.Scripts.Extensions;
using UnityEngine;

namespace Assets.OsmGenerator.Scripts.Serialization
{
    public class OsmWay
    {
        public ulong ID { get; }
        public bool Visible { get; }
        public List<ulong> NodeIDs { get; }
        public bool IsBoundary { get; }
        public float Height { get; }
        public bool IsBuilding { get; }
        public bool IsRoad { get; private set; }
        public string NaturalTypeName { get; private set; }
        public bool IsNatural { get; private set; }
        public string BuildingMaterialName { get; private set; }
        public string RoadTypeName { get; private set; }
        public bool IsBarrier { get; private set; }
        public string BarrierTypeName { get; private set; }
        public bool IsLanduse { get; private set; }
        public string LanduseTypeName { get; private set; }
        public bool IsParking { get; private set; }
        public bool HasLevels { get; private set; }
        public bool IsWaterWay { get; private set; }
        public string WaterWayTypeName { get; private set; }


        public bool Deleted { get; set; }
        public OsmWay(XmlNode node)
        {
            Deleted = false;
            NodeIDs = new List<ulong>();
            Height = 8.0f;
            ID = node.GetAttribute<ulong>("id");
            Visible = node.GetAttribute<bool>("visible");

            XmlNodeList nds = node.SelectNodes("nd");
            foreach (XmlNode n in nds)
            {
                ulong refNo = n.GetAttribute<ulong>("ref");
                NodeIDs.Add(refNo);
            }

            if (NodeIDs.Count > 1)
            {
                IsBoundary = NodeIDs[0] == NodeIDs[NodeIDs.Count - 1];
            }

            XmlNodeList tags = node.SelectNodes("tag");
             
            foreach (XmlNode t in tags)
            {
                string key = t.GetAttribute<string>("k");
                if (key == "building:levels")
                {
                    Height = t.GetAttribute<float>("v");
                    HasLevels = true;

                }
                else if (key == "height")
                {
                    Height = t.GetAttribute<float>("v");
                }
                else if (key == "highway")
                {
                    IsRoad = true;
                    RoadTypeName = t.GetAttribute<string>("v");
                }
                else if(key == "building:material")
                {
                    BuildingMaterialName = t.GetAttribute<string>("v");
                }
                else if(key == "natural")
                {
                    IsNatural = true;
                    NaturalTypeName = t.GetAttribute<string>("v");
                }
                else if(key == "barrier")
                {
                    IsBarrier = true;
                    BarrierTypeName = t.GetAttribute<string>("v");
                }
                else if(key == "landuse")
                {
                    IsLanduse = true;
                }
                else if(key == "waterway")
                {
                    IsWaterWay = true;
                    WaterWayTypeName = t.GetAttribute<string>("v");
                }


                if (key.Contains("building"))
                {
                    IsBuilding = true;
                    LanduseTypeName = t.GetAttribute<string>("v");
                }
            }
        }

        public bool HasBuildingMaterial()
        {
            return !string.IsNullOrEmpty(BuildingMaterialName);
        }
    }
}
