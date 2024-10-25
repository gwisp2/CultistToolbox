﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace CultistToolbox.DeterministicRandom;

public class DeterministicRandom
{
    private string _salt = "";
    private int _callIndex;

    public DeterministicRandom(string salt = null)
    {
        _salt = salt ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
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
        return (int)(DeterministicallyAssignULong() % rangeSize) + minInclusive;
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

        return SortElementsByRandomPriority(collection)[0];
    }

    public List<T> SortElementsByRandomPriority<T>(IEnumerable<T> collection)
    {
        IncrementCallIndex();
        var list = collection
            .Select(element => new { Element = element, Priority = DeterministicallyAssignULong(element) }).ToList();
        list.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        return list.Select(pair => pair.Element).ToList();
    }

    private ulong DeterministicallyAssignULong<T>(T element)
    {
        return BitConverter.ToUInt64(ComputeHash(RuntimeHelpers.GetHashCode(element).ToString()), 0);
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
        var input = _salt + _callIndex + extraInput;
        using var sha256 = SHA256.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        return sha256.ComputeHash(inputBytes);
    }
}