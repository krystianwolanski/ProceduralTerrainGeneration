using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Assets.OsmGenerator.Scripts.Serialization;
using UnityEngine;
using UnityEngine.Networking;

public class MapReader : MonoBehaviour
{
    [HideInInspector]
    public Dictionary<ulong, OsmNode> Nodes { get; set; }
    
    [HideInInspector]
    public List<OsmWay> Ways { get; set; }
    
    [HideInInspector]
    public OsmBounds Bounds { get; set; }
    
    public bool IsReady { get; private set; }
    private XmlDocument Doc { get; set; }

    [Tooltip("The resource file that contains the OSM map data")]
    public string resourceFile;
    
    public void Start()
    {
        var txtAsset = Resources.Load<TextAsset>(resourceFile);

        Doc = new XmlDocument();
        Doc.LoadXml(txtAsset.text);

        SetBounds();
        SetNodes();
        SetWays();

        IsReady = true;
    }

    void Update()
    {
        foreach (OsmWay osmWay in Ways)
        {
            if (osmWay.Visible)
            {
                Color c = Color.cyan; // for buildings
                if (!osmWay.IsBoundary)
                    c = Color.red; // for roads

                for (int i = 1; i < osmWay.NodeIDs.Count; i++)
                {
                    OsmNode p1 = Nodes[osmWay.NodeIDs[i - 1]];
                    OsmNode p2 = Nodes[osmWay.NodeIDs[i]];

                    Vector3 v1 = p1 - Bounds.Centre;
                    Vector3 v2 = p2 - Bounds.Centre;

                    Debug.DrawLine(v1,v2,c);
                }
            }
        }
    }

    private void SetWays()
    {
        var xmlWays = Doc.SelectNodes("/osm/way");
        if (xmlWays == null) return;

        Ways = new List<OsmWay>();
        foreach (XmlNode node in xmlWays)
        {
            OsmWay way = new OsmWay(node);
            Ways.Add(way);
        }
    }

    private void SetNodes()
    {
        var xmlNodes = Doc.SelectNodes("/osm/node");
        if (xmlNodes == null) return;

        Nodes = new Dictionary<ulong, OsmNode>();
        foreach (XmlNode n in xmlNodes)
        {
            OsmNode node = new OsmNode(n);
            Nodes[node.ID] = node;
        }
    }

    private void SetBounds()
    {
        var xmlBounds = Doc.SelectSingleNode("/osm/bounds");
        Bounds = new OsmBounds(xmlBounds);
    }
}
