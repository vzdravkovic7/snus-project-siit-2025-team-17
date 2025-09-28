using System.Runtime.Serialization;

namespace SnusProject.Models
{
    [DataContract]
    public class RobotArmState
    {
        [DataMember]
        public int X { get; set; }

        [DataMember]
        public int Y { get; set; }

        [DataMember]
        public int Angle { get; set; }
    }
}