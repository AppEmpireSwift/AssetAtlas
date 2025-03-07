using System;
using MainScreen;
using UnityEngine;

namespace Assets
{
    [Serializable]
    public class AssetData
    {
        public AssetType Type;
        public int Value;
        public string Name;
        public string Description;

        public AssetData(AssetType type, int value, string name, string description)
        {
            Type = type;
            Value = value;
            Name = name;
            Description = description;
        }
    }
}