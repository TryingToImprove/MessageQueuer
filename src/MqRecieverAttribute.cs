using System;

namespace MessageQueuer
{
    public class MqRecieverAttribute : Attribute
    {
        public string Name { get; set; }

        public int Handlers { get; set; }
    }
}