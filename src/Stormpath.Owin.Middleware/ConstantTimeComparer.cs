using System.Text;

namespace Stormpath.Owin.Middleware
{
    public static class ConstantTimeComparer
    {
        public static bool Equals(string x, string y)
        {
            var xIsNull = x == null;
            var yIsNull = y == null;

            if (xIsNull && yIsNull)
            {
                return true;
            }

            if ((xIsNull && !yIsNull) || (!xIsNull && yIsNull))
            {
                return false;
            }

            if (x.Length != y.Length)
            {
                return false;
            }

            byte[] decodedCryptoBytes = Encoding.ASCII.GetBytes(x);
            byte[] decodedSignatureBytes = Encoding.ASCII.GetBytes(y);

            byte result = 0;
            for (int i = 0; i < x.Length; i++)
            {
                result |= (byte)(decodedCryptoBytes[i] ^ decodedSignatureBytes[i]);
            }

            return result == 0;
        }
    }
}
