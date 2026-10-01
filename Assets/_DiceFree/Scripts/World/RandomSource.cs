using System;
namespace DiceFree.World
{
    public interface IRandomSource { double NextUnit(); }
    public sealed class SeededRandomSource : IRandomSource
    {
        private readonly Random random;
        public SeededRandomSource(int seed) => random = new Random(seed);
        public double NextUnit() => random.NextDouble();
    }
}
