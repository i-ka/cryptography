using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Cryptography
{
    abstract class CryptoKey
    {
        public BigInteger Exponent { get; set; }
        public BigInteger N { get; set; }

        public override string ToString()
        {
            return $"ClosedKey{{Exponent = {Exponent}, N = {N}}}";
        }
    }

    class OpenKey : CryptoKey
    {
        public byte[] Encode(byte[] bytes)
        {
            return BigInteger.ModPow(new BigInteger(bytes), Exponent, N).ToByteArray();
        }
    }

    class ClosedKey : CryptoKey
    {
        public byte[] Decode(byte[] bytes)
        {
            return BigInteger.ModPow(new BigInteger(bytes), Exponent, N).ToByteArray();
        }
    }

    class CryptoKeyPair
    {
        public OpenKey OpenKey { get; set; }
        public ClosedKey ClosedKey { get; set; }

        public byte[] Encrypt(byte[] bytes) { return OpenKey.Encode(bytes); }
        public byte[] Decrypt(byte[] bytes) { return ClosedKey.Decode(bytes); }

        public override string ToString()
        {
            return $"OpenKey {OpenKey}, ClosedKey {ClosedKey}";
        }
    }
}