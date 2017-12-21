using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Cryptography
{
    class KeyGenerator
    {
        private const int generationRetries = 10;
        private readonly int _keysLength;

        public KeyGenerator(int keysLength)
        {
            _keysLength = keysLength;
        }

        public CryptoKeyPair GenerateKeys()
        {
            for (int i = 0; i < generationRetries; i++)
            {
                var keys = GenerateKeysPrivate();
                if (CheckKeys(keys)) return keys;
            }
            throw new Exception("Cant generate keys!");
        }

        private bool CheckKeys(CryptoKeyPair keys)
        {
            var testBytes = new byte[4];
            new Random().NextBytes(testBytes);
            var tryeBytes = keys.Decrypt(keys.Encrypt(testBytes));
            return testBytes.SequenceEqual(tryeBytes);
        }

        private CryptoKeyPair GenerateKeysPrivate()
        {
            var primes = new BigInteger[2];
            Parallel.Invoke(
                () => primes[0] = new Random().NextPrime(_keysLength),
                () => primes[1] = new Random().NextPrime(_keysLength));
            var phi = Phi(primes[0], primes[1]);
            var e = GetMutualyPrime();
            return new CryptoKeyPair
            {
                OpenKey = new OpenKey { Exponent = e, N = N(primes[0], primes[1]) },
                ClosedKey = new ClosedKey { Exponent = D(e, phi), N = N(primes[0], primes[1]) }
            };
        }

        private BigInteger Phi(BigInteger a, BigInteger b)
        {
            return (a - 1) * (b - 1);
        }

        private BigInteger N(BigInteger a, BigInteger b)
        {
            return a * b;
        }

        private BigInteger D(BigInteger a, BigInteger b)
        {
            var module = a > b ? a : b;
            BigInteger x = 0;
            BigInteger u = 1;
            while (a != 0)
            {
                var q = b / a;
                var r = b % a;
                var m = x - u * q;
                b = a;
                a = r;
                x = u;
                u = m;
            }

            return module - BigInteger.Abs(x);
        }

        private BigInteger GetMutualyPrime()
        {
            return 65537;
        }
    }


}
