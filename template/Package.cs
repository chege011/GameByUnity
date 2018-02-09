using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{   
    public class Package
    {
        private static readonly ByteArray ByteBuffer = new ByteArray();
        public int Id { private set; get; }
        public byte[] Body { private set; get; }

        public Package(int id, byte[] body)
        {
            Id = id;
            Body = body;
        }

        public byte[] ToBytes()
        {
            ByteBuffer.Clear();

            ByteBuffer.WriteUnsignedShort((uint)Id);
            if (Body == null) return ByteBuffer.Bytes;
            ByteBuffer.WriteBytes(Body);
            return ByteBuffer.Bytes;
        }

        public static Package BytesToPackage(byte[] bytes)
        {
            ByteBuffer.Clear();
            ByteBuffer.WriteBytes(bytes);
            var id = (int)ByteBuffer.ReadUnsignedShort();
            return new Package(id, ByteBuffer.Bytes);
        }
    }
}

