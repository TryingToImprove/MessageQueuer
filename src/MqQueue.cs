using System;

namespace MessageQueuer
{
    internal class MqQueue
    {
        public string Name { get; set; }

        public int Handlers { get; set; }

        public Type RecieverType { get; set; }
    }
}