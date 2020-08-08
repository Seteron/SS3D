using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
namespace SS3D.Engine.Utilities
{
    [ExecuteAlways]
    public class AssetDataParser : MonoBehaviour
    {
        public bool run;
        public TextAsset assetDataJson;
        public DataNode Root;
        // Start is called before the first frame update
        void Start()
        {
            AssetDataParse(assetDataJson);
        }

        // Update is called once per frame
        void Update()
        {
            if(run)
            {
                run = !run;
                AssetDataParse(assetDataJson);
            }
        }

        void AssetDataParse(TextAsset assetData)
        {
            JSONNode value = JSON.Parse(assetData.text);
            Root = new DataNode(value, "Root", "Root");
        }

    }
    public class DataNode
    {
        public JSONNode value;
        [HideInInspector]
        public string name;
        public string valueString;
        public List<DataNode> SubNodes;
        public bool expanded;

        public DataNode(JSONNode dataValue, string _name, string _valueString)
        {
            value = dataValue;
            valueString = _valueString;
            name = _name;

            SubNodes = new List<DataNode>();
            if(value.IsObject)
            {
                foreach(KeyValuePair<string, SimpleJSON.JSONNode> subNode in value)
                {
                    SubNodes.Add(new DataNode(subNode, subNode.Key, subNode.Value));
                }
            }
        }
    }
}
