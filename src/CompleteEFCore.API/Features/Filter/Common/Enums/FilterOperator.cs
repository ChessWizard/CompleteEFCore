namespace CompleteEFCore.API.Features.Filter.Common.Enums;

public enum FilterOperator
{
    Equals = 1,// =
    GreaterThan = 2,// >
    LessThan = 3,// <
    GreaterThanOrEqualTo = 4,// >=
    LessThanOrEqualTo = 5,// <=
    Includes = 6,// +
    Excludes = 7,// -
    Contains = 8,// ~= (Text contains value)
    StartsWith = 9,// ^= (Text starts with value)
    EndsWith = 10,// $= (Text ends with value)
    Between = 11,// .. (Value is between a range)
    Not = 12,// != (Value is not equal)
    Regex = 13// /  (Matches regex pattern)
}