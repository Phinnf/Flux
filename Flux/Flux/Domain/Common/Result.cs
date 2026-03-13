using System.Text.Json.Serialization;

namespace Flux.Domain.Common;

public class Result
{
    public bool IsSuccess { get; init; }
    public string Error { get; init; } = string.Empty;
    public bool IsFailure => !IsSuccess;

    [JsonConstructor]
    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    protected Result() { }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, string.Empty);
    public static Result<TValue> Failure<TValue>(string error) => new(default, false, error);
}

public class Result<TValue> : Result
{
    private TValue? _value;

    [JsonInclude]
    public TValue? Value 
    { 
        get => _value;
        private init => _value = value;
    }

    [JsonConstructor]
    public Result(TValue? value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    private Result() : base() { }

    // Explicitly named methods to resolve CS0266 and CS0109
    public static Result<TValue> CreateSuccess(TValue value) => new(value, true, string.Empty);
    public static Result<TValue> CreateFailure(string error) => new(default, false, error);
}
