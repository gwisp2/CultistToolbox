using System;
using System.Collections.Generic;
using System.Linq;
using CultistToolbox.Patch;
using HutongGames.PlayMaker;

namespace CultistToolbox.DeterministicRandom;

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

    private static string _seed;
    private static List<DeterministicRandomSave.ContextSave> _contextSaves;
    private static Dictionary<IFsmStateAction, Context> _contexts;
    private static Stack<Context> _contextStack;
    private static Context _defaultContext;

    static DeterministicRandomFacade()
    {
        Reset();
        HookScenarioLoadUnload.ScenarioShutdown += Reset;
    }

    private static void Reset()
    {
        _contextSaves = [];
        _contexts = new Dictionary<IFsmStateAction, Context>();
        _defaultContext = new();
        _seed = GenerateRandomSeed();
        _defaultContext.DeterministicRandom.Reset(_seed);
        _contextStack = new();
    }

    private static string GenerateRandomSeed()
    {
        const int length = 6;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static DeterministicRandomSave Save()
    {
        List<DeterministicRandomSave.ContextSave> newSavedContexts = _contexts
            .Select(kv => new DeterministicRandomSave.ContextSave()
                { ActionId = UniqueSalt.Of(kv.Key), ActionCallIndex = kv.Value.ActionCallIndex })
            .Where(s => s.ActionCallIndex != 0)
            .ToList();
        return new DeterministicRandomSave()
        {
            Seed = _seed,
            ActionContexts = [.._contextSaves, ..newSavedContexts],
            DefaultContextCallIndex = _defaultContext.DeterministicRandom.CallIndex
        };
    }

    public static void Load(DeterministicRandomSave save)
    {
        Reset();
        if (save == null) return;
        _seed = save.Seed;
        _defaultContext.DeterministicRandom.Reset(_seed);
        _defaultContext.DeterministicRandom.CallIndex = save.DefaultContextCallIndex;
        _contextSaves = save.ActionContexts; // used in GetContext()
    }

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

    private static void OpenContext(IFsmStateAction action, bool simulating)
    {
        var context = GetContext(action);
        if (!context.Closed)
        {
            throw new InvalidOperationException("Context is already open fcr this action");
        }

        context.Closed = false;
        context.Simulating = simulating;
        var actionId = UniqueSalt.Of(action);
        context.DeterministicRandom.Reset($"{_seed}@{actionId}@{context.ActionCallIndex}");
        _contextStack.Push(context);
    }

    public static void CloseContext(IFsmStateAction action)
    {
        var context = GetContext(action);
        if (context.Closed)
        {
            throw new InvalidOperationException("Context is already closed fcr this action");
        }

        if (_contextStack.Peek() != context)
        {
            throw new InvalidOperationException("Context for this action is not on top of the stack");
        }

        context.Closed = true;
        if (!context.Simulating && context.DeterministicRandom.WasCalledSinceReset())
        {
            context.ActionCallIndex++;
        }

        _contextStack.Pop();
    }

    private static Context GetContext(IFsmStateAction action)
    {
        if (_contexts.TryGetValue(action, out var context))
        {
            return context;
        }

        _contexts[action] = context = new();

        // Apply context from save (if present)
        var actionId = UniqueSalt.Of(action);
        var savedContext = _contextSaves.Find(c => c.ActionId == actionId);
        if (savedContext != null)
        {
            context.ActionCallIndex = savedContext.ActionCallIndex;
            _contextSaves.Remove(savedContext); // so it won't be saved again 
        }

        return context;
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