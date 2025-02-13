﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CultistToolbox.DeterministicRandom;

public class DeterministicRandom
{
    private string _salt = "";
    private int _callIndex;

    public int CallIndex
    {
        get => _callIndex;
        set => _callIndex = value;
    }

    public DeterministicRandom(string salt = null)
    {
        _salt = salt ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
    }

    public bool WasCalledSinceReset()
    {
        return _callIndex != 0;
    }

    public void Reset(string newSalt)
    {
        this._salt = newSalt ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        this._callIndex = 0;
    }

    public int Range(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
        {
            throw new ArgumentException("maxExclusive must be greater than minInclusive");
        }

        IncrementCallIndex();
        ulong rangeSize = (ulong)(maxExclusive - minInclusive);
        var result = (int)(DeterministicallyAssignULong() % rangeSize) + minInclusive;

        return result;
    }

    public int GetRandomWeightedIndex(IEnumerable<float> weights)
    {
        if (weights == null)
            throw new ArgumentNullException("Weights cannot be null");
        double maxInclusive = 0.0f;
        foreach (float weight in weights)
        {
            if (weight < 0.0)
                throw new ArgumentException("Weight cannot be negative");
            maxInclusive += weight;
        }

        IncrementCallIndex();
        double num1 = DeterministicallyAssignDouble(0, maxInclusive);
        double num2 = 0.0f;
        int randomWeightedIndex = 0;
        foreach (float weight in weights)
        {
            num2 += weight;
            if (num1 <= num2)
                return randomWeightedIndex;
            ++randomWeightedIndex;
        }

        return -1;
    }

    public T GetRandomElement<T>(IEnumerable<T> collection)
    {
        if (collection == null || !collection.Any())
        {
            return default;
        }

        var selected = SortElementsByRandomPriority(collection)[0];

        return selected;
    }

    public List<T> SortElementsByRandomPriority<T>(IEnumerable<T> collection)
    {
        IncrementCallIndex();
        var list = collection
            .Select(element => new { Element = element, Priority = DeterministicallyAssignULong(element) })
            .OrderBy(e => e.Priority);

        return list.Select(pair => pair.Element).ToList();
    }

    private ulong DeterministicallyAssignULong<T>(T element)
    {
        return BitConverter.ToUInt64(ComputeHash(UniqueSalt.Of(element)), 0);
    }

    private ulong DeterministicallyAssignULong()
    {
        return BitConverter.ToUInt64(ComputeHash(), 0);
    }

    private double DeterministicallyAssignDouble(double min, double max)
    {
        uint value = BitConverter.ToUInt32(ComputeHash(), 0);
        return ((double)value / UInt32.MaxValue) * (max - min) + min;
    }

    private void IncrementCallIndex()
    {
        _callIndex++;
    }

    private byte[] ComputeHash(string extraInput = "")
    {
        return ComputeNonRandomHash(ComputeHashInput(extraInput));
    }

    private string ComputeHashInput(string extraInput = "")
    {
        return _salt + _callIndex + extraInput;
    }

    private static byte[] ComputeNonRandomHash(string input)
    {
        using var sha256 = SHA256.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        return sha256.ComputeHash(inputBytes);
    }
}