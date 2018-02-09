using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace Network
{
    public class Proto : IProto
    {
        public byte[] Bytes { get; private set; }
        public object Obj { get; private set; }

        public Proto(object obj)
        {
            Obj = obj;
        }

        public Proto(byte[] bytes)
        {
            Bytes = bytes;
        }

        public byte[] ToBytes()
        {
            return Bytes ?? Serialize(Obj);
        }

        public T ToObj<T>()
        {
            if (Obj != null) { return (T) Obj;}
            return Deserialize<T>(Bytes);
        }

        private byte[] Serialize<T>(T obj)
        {
            using (var steam = new MemoryStream())
            {
                Serializer.Serialize(steam, obj);
                return steam.ToArray();
            }
        }

        private T Deserialize<T>(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                var obj = Serializer.Deserialize<T>(stream);
                return obj;
            }
        }

    }

    public interface IProto
    {
        byte[] ToBytes();
        T ToObj<T>();
    }
}

