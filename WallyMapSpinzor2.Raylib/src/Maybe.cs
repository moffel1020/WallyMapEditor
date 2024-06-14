using System;
using System.Diagnostics.CodeAnalysis;

namespace WallyMapSpinzor2.Raylib;

// simple optional type
public readonly struct Maybe<T>
{
    private readonly bool _hasValue = false;
    private readonly T _value = default!;

    public bool HasValue => _hasValue;
    public T Value => _hasValue ? _value : throw new InvalidOperationException("Attempt to get value of none type");
    public unsafe T ValueUnsafe => _value; // rust style shit

    public bool TryGetValue([NotNullWhen(true)] out T? t)
    {
        t = _hasValue ? _value : default;
        return _hasValue;
    }

    public T ValueOr(T t) => _hasValue ? _value : t;

    public Maybe<U> Map<U>(Func<T, U> operation) => _hasValue
        ? Maybe<U>.Some(operation(_value))
        : Maybe<U>.None;

    public void Do(Action<T> has, Action noHas)
    {
        if (_hasValue)
            has(_value);
        else
            noHas();
    }

    public void DoIfHas(Action<T> has)
    {
        if (_hasValue)
            has(_value);
    }

    public void DoIfNotHas(Action noHas)
    {
        if (!_hasValue)
            noHas();
    }

    public Maybe()
    {
        _hasValue = false;
        _value = default!;
    }

    public Maybe(T value)
    {
        _hasValue = true;
        _value = value;
    }

    public static Maybe<T> Some(T value) => new(value);
    public static Maybe<T> None => new();

    public static implicit operator Maybe<T>(T value) => new(value);
}