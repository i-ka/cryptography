using System;
using System.Numerics;

namespace Cryptography
{
    public static class Extentions
    {
        public static BigInteger NextBigInt(this Random random, int byteLenght)
        {
            var bytes = new byte[byteLenght];
            random.NextBytes(bytes);
            return new BigInteger(bytes);
        }

        public static BigInteger NextBigInt(this Random random, BigInteger low, BigInteger hight, int byteLenght)
        {
            var bytes = new byte[byteLenght];
            random.NextBytes(bytes);
            var rand = new BigInteger(bytes);
            return rand % (hight - low) + low;
        }

        public static BigInteger NextPrime(this Random random, int byteLength = 4)
        {
            while (true)
            {
                var candidate = random.NextBigInt(byteLength);
                var oddNum = candidate.IsEven ? candidate + 1 : candidate;
                if (MillerRabinTest(random, oddNum, 200, byteLength)) return oddNum;
            }

        }

        public static bool MillerRabinTest(Random random, BigInteger n, int k, int byteLength)
        {
            if (n < 2)
                return false;
            if (n == 2)
                return true;
            if (n % 2 == 0)
                return false;
            BigInteger r = 0, d = n - 1;
            while (d % 2 == 0)
            {
                d /= 2;
                r++;
            }
            
            for (int i = 0; i < k; i++)
            {
                BigInteger a = random.NextBigInt(2, n - 1, byteLength);
                BigInteger x = BigInteger.ModPow(a, d, n);
                if (x == 1 || x == n - 1)
                    continue;
                for (int j = 0; j < r - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == 1)
                        return false;
                    if (x == n - 1)
                        break;
                }
                if (x != n - 1)
                    return false;
            }
            return true;
        }

        //public static bool IsLucasPrime(this int p, Random random, int byteLength = 4)
        //{
        //    if (p % 2 == 0) return (p == 2);
        //    for (int i = 3; i <= (int)Math.Sqrt(p); i += 2)
        //        if (p % i == 0) return false; //not prime
        //    BigInteger m_p = FastPow(2, p) - 1;
        //    BigInteger s = 4;
        //    for (int i = 3; i <= p; i++)
        //        s = (s * s - 2) % m_p;
        //    return s == BigInteger.Zero;
        //}
    }
}