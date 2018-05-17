using System;

namespace Leaf.xNet
{
    /// <summary>
    /// Класс-обёртка- для потокобезопасной генерации пресевдо-случайных чисел.
    /// Lazy-load singleton для ThreadStatic <see cref="Random"/>.
    /// </summary>
    public static class Randomizer
    {
        [ThreadStatic] private static Random _rand;
        public static Random Instance => _rand ?? (_rand = new Random());

    }
}
