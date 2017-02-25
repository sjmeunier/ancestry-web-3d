using System.IO;
using UnityEngine;

namespace Assets
{
    public class IndividualSphereData
    {
        public string Id;
        public string Text;
        public Vector3 Position;
        public Color MaterialColor;
        public float SphereRadius;
        public string ObjectType;
        public string Summary;


        public IndividualSphereData(string id)
        {
            Id = id;
            Text = "";
            Position = new Vector3(0, 0, 0);
            MaterialColor = Color.white;
            SphereRadius = 1;
            ObjectType = "IndividualSphere";
            Summary = "";
        }

        public IndividualSphereData(BinaryReader reader)
        {
            ReadFromStream(reader);
        }

        public void WriteToStream(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Text);
            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write(Position.z);
            writer.Write(MaterialColor.r);
            writer.Write(MaterialColor.g);
            writer.Write(MaterialColor.b);
            writer.Write(MaterialColor.a);
            writer.Write(SphereRadius);
            writer.Write(ObjectType);
            writer.Write(Summary);
        }

        private void ReadFromStream(BinaryReader reader)
        {
            Id = reader.ReadString();
            Text = reader.ReadString();
            Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            MaterialColor = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            SphereRadius = reader.ReadSingle();
            ObjectType = reader.ReadString();
            Summary = reader.ReadString();
        }
    }

}