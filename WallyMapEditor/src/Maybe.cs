using System;
using System.Diagnostics.CodeAnalysis;

namespace WallyMapEditor;

// simple optional type
public readonly struct Maybe<T>
{
    private readonly bool _hasValue = false;
    private readonly T _value = default!;

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

    public bool HasValue => _hasValue;

    public T Value => _hasValue ? _value : throw new InvalidOperationException("Attempt to get value of none type");
    public T ValueOrThrow(Exception e) => _hasValue ? _value : throw e;
    public unsafe T ValueUnsafe => _value; // rust style shit

    public bool TryGetValue([NotNullWhen(true)] out T? t)
    {
        t = _hasValue ? _value : default;
        return _hasValue;
    }

    public T ValueOr(T t) => _hasValue ? _value : t;
    public T ValueOr(Func<T> func) => _hasValue ? _value : func();

    public Maybe<U> Map<U>(Func<T, U> operation) =>
    _hasValue
        ? operation(_value)
        : Maybe<U>.None;

    public Maybe<U> Bind<U>(Func<T, Maybe<U>> operation) =>
    _hasValue
        ? operation(_value)
        : Maybe<U>.None;

    public Maybe<R> Combine<U, R>(Maybe<U> other, Func<T, U, R> operation) =>
    _hasValue && other._hasValue
        ? operation(_value, other._value)
        : Maybe<R>.None;

    public Maybe<R> CombineBind<U, R>(Maybe<U> other, Func<T, U, Maybe<R>> operation) =>
    _hasValue && other._hasValue
        ? operation(_value, other._value)
        : Maybe<R>.None;

    public Maybe<U> Cast<U>() =>
    _hasValue
        ? (U)(object)_value!
        : Maybe<U>.None;

    public void Do(Action<T> ifSome, Action ifNone)
    {
        if (_hasValue)
            ifSome(_value);
        else
            ifNone();
    }

    public void DoIfSome(Action<T> ifSome)
    {
        if (_hasValue)
            ifSome(_value);
    }

    public void DoIfNone(Action ifNone)
    {
        if (!_hasValue)
            ifNone();
    }

    public static Maybe<T> Some(T value) => new(value);
    public static Maybe<T> None => new();
    public static implicit operator Maybe<T>(T value) => new(value);
}

public static class Maybe
{
    public static Maybe<T> Some<T>(T value) => Maybe<T>.Some(value);
    public static Maybe<T> None<T>() => Maybe<T>.None;
}