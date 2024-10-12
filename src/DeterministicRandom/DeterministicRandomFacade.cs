using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;

namespace MoMEssentials.DeterministicRandom;

public class DeterministicRandomFacade
{
    private class Context
    {
        public DeterministicRandom DeterministicRandom = new();
        public int ActionCallIndex;
        public bool Simulating;
        public bool Closed = true;
    }

    private class ContextDisposer(IFsmStateAction action) : IDisposable
    {
        public void Dispose()
        {
            CloseContext(action);
        }
    }

    private static readonly ConditionalWeakTable<IFsmStateAction, Context> Contexts = new();
    private static Stack<Context> _contextStack = new();
    private static Context _defaultContext = new();


    public static IDisposable OpenContext(IFsmStateAction action)
    {
        OpenContext(action, simulating: false);
        return new ContextDisposer(action);
    }

    public static IDisposable OpenContextForSimulation(IFsmStateAction action)
    {
        OpenContext(action, simulating: true);
        return new ContextDisposer(action);
    }

    private static void OpenContext(IFsmStateAction action, bool simulating = false)
    {
        var context = Contexts.GetOrCreateValue(action);
        if (!context.Closed)
        {
            throw new InvalidOperationException("Context is already open fcr this action");
        }

        context.Closed = false;
        context.Simulating = simulating;
        context.DeterministicRandom.Reset(RuntimeHelpers.GetHashCode(action) + "@" + context.ActionCallIndex);
        _contextStack.Push(context);
    }

    public static void CloseContext(IFsmStateAction action)
    {
        var context = Contexts.GetOrCreateValue(action);
        if (context.Closed)
        {
            throw new InvalidOperationException("Context is already closed fcr this action");
        }

        if (_contextStack.Peek() != context)
        {
            throw new InvalidOperationException("Context for this action is not on top of the stack");
        }

        context.Closed = true;
        if (!context.Simulating)
        {
            context.ActionCallIndex++;
        }

        _contextStack.Pop();
    }

    private static Context CurrentContext()
    {
        return _contextStack.Count >= 1 ? _contextStack.Peek() : _defaultContext;
    }

    public static T GetRandomElement<T>(IEnumerable<T> collection)
    {
        return CurrentContext().DeterministicRandom.GetRandomElement(collection);
    }

    public static List<T> SortElementsByRandomPriority<T>(IEnumerable<T> collection)
    {
        return CurrentContext().DeterministicRandom.SortElementsByRandomPriority(collection);
    }

    public static T GetRandomElementAndRemove<T>(ICollection<T> collection)
    {
        var element = CurrentContext().DeterministicRandom.GetRandomElement(collection);
        collection.Remove(element);
        return element;
    }

    public static int GetRandomWeightedIndex(IEnumerable<float> weights)
    {
        return CurrentContext().DeterministicRandom.GetRandomWeightedIndex(weights);
    }

    public static int GetRandomWeightedIndexF(FsmFloat[] weights)
    {
        return CurrentContext().DeterministicRandom.GetRandomWeightedIndex(weights.Select(w => w.Value));
    }

    public static int Range(int minInclusive, int maxExclusive)
    {
        return CurrentContext().DeterministicRandom.Range(minInclusive, maxExclusive);
    }
}