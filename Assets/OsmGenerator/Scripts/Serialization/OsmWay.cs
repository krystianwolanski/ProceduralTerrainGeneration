using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Assets.OsmGenerator.Scripts.Extensions;

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
        public OsmWay(XmlNode node)
        {
            NodeIDs = new List<ulong>();
            Height = 3.0f;
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
                    Height = 3.0f * t.GetAttribute<float>("v");

                }
                else if (key == "height")
                {
                    Height = 3.048f * t.GetAttribute<float>("v");
                }
                else if (key == "building")
                {
                    IsBuilding = t.GetAttribute<string>("v") == "yes";
                }
                else if (key == "highway")
                {
                    IsRoad = true;
                }
            }
        }
    }
}
