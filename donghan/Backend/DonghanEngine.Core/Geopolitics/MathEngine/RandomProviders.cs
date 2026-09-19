using System;
using System.Collections.Generic;
using DonghanEngine.Core.Geopolitics.Contracts;

namespace DonghanEngine.Core.Geopolitics.MathEngine;

public sealed class DefaultRandomProvider : IRandomProvider
{
    private readonly Random _rand = new();
    public int NextPercentage() => _rand.Next(0, 100);
    public int NextRange(int min, int max) => _rand.Next(min, max);
}

public sealed class SeededRandomProvider : IRandomProvider
{
    private readonly Queue<int> _predefinedPercentages = new();

    public void EnqueuePercentage(int percentage)
    {
        _predefinedPercentages.Enqueue(Math.Clamp(percentage, 0, 99));
    }

    public int NextPercentage()
    {
        return _predefinedPercentages.Count > 0 ? _predefinedPercentages.Dequeue() : 50;
    }

    public int NextRange(int min, int max) => min;
}
