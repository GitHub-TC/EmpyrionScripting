using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EmpyrionScripting
{
    public static class SerializeHelper
    {
        static BinaryFormatter bf = new BinaryFormatter();

        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null) return null;

            using MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        // Convert a byte array to an Object
        public static T ByteArrayToObject<T>(byte[] arrBytes) where T:class
        {
            if (arrBytes == null) return default;

            using MemoryStream memStream = new MemoryStream();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            return bf.Deserialize(memStream) as T;
        }
    }
}
