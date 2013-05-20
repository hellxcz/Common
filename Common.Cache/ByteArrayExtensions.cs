using System.IO;
using System.IO.Compression;
using System.Text;

namespace InMemoryCache
{
    public static class ByteArrayExtensions
    {
        public static string To0xString(this byte[] @this)
        {
            var sb = new StringBuilder(@this.Length * 2);
            foreach (var b in @this)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        public static byte[] Zip(this string str)
        {
            if (str == null)
            {
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(str);

            using (MemoryStream msi = new MemoryStream(bytes),
                                mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(this byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
}