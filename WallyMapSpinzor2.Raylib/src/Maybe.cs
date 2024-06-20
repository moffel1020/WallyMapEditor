using System;
using System.Diagnostics.CodeAnalysis;

namespace WallyMapSpinzor2.Raylib;

// simple optional type
public readonly struct Maybe<T> : IEquatable<T>, IEquatable<Maybe<T>>
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


    [MemberNotNullWhen(true, nameof(_value))]
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

    public Maybe<U> Map<U>(Func<T, U> operation) => _hasValue
        ? Maybe<U>.Some(operation(_value))
        : Maybe<U>.None;

    public Maybe<U> Bind<U>(Func<T, Maybe<U>> operation) => _hasValue
        ? operation(_value)
        : Maybe<U>.None;

    public Maybe<U> Cast<U>() => _hasValue
        ? Maybe<U>.Some((U)(object)_value!)
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

    public bool Equals(T? other)
    {
        if (!_hasValue)
            return false;
        return _value!.Equals(other);
    }

    public bool Equals(Maybe<T> other)
    {
        if (!_hasValue && !other._hasValue)
            return true;
        if (_hasValue && other._hasValue)
            return _value!.Equals(other._value);
        return false;
    }

    public override bool Equals(object? obj)
    {
        return
            (obj is Maybe<T> maybe && Equals(maybe)) ||
            (obj is T t && Equals(t));
    }

    public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);
    public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);
    public static bool operator ==(Maybe<T> left, T right) => left.Equals(right);
    public static bool operator !=(Maybe<T> left, T right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(_value, _hasValue);

    public static Maybe<T> Some(T value) => new(value);
    public static Maybe<T> None => new();
    public static implicit operator Maybe<T>(T value) => new(value);
}