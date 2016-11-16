using UnityEngine;

namespace Assets
{
    public struct IndividualSphereData
    {
        public string Id;
        public string Text;
        public Vector3 Position;
        public Color MaterialColor;
        public float SphereRadius;
        public string ObjectType;
		
        public IndividualSphereData(string id)
        {
            Id = id;
            Text = "";
            Position = new Vector3(0, 0, 0);
            MaterialColor = Color.white;
            SphereRadius = 1;
            ObjectType = "IndividualSphere";
        }
    }

}