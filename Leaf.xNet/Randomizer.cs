using System;
using System.Security.Cryptography;

namespace Leaf.xNet
{
    /// <summary>
    /// Класс-обёртка- для потокобезопасной генерации псевдослучайных чисел.
    /// Lazy-load singleton для ThreadStatic <see cref="Random"/>.
    /// </summary>
    public static class Randomizer
    {
        private static readonly RNGCryptoServiceProvider generator = new RNGCryptoServiceProvider();

        private static Random Generate()
        {
            byte[] buffer = new byte[4];
            generator.GetBytes(buffer);
            return new Random(BitConverter.ToInt32(buffer, 0));
        }

        public static Random Instance => _rand ?? (_rand = Generate());
        [ThreadStatic] private static Random _rand;
    }
}
