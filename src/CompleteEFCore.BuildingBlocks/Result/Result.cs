﻿namespace CompleteEFCore.BuildingBlocks.Result
{
    public class Result<TData> : BaseResult<TData>
    {
        public static Result<TData> Success(TData data, int statusCode)
        {
            return new Result<TData> { Data = data, HttpStatusCode = statusCode };
        }

        public static Result<TData> Success(string message, int statusCode)
        {
            return new Result<TData> { Message = message, HttpStatusCode = statusCode };
        }

        public static Result<TData> Success(string message, TData data, int statusCode)
        {
            return new Result<TData> { Message = message, Data = data, HttpStatusCode = statusCode };
        }

        public static Result<TData> Success(int statusCode)
        {
            return new Result<TData> { Data = default, HttpStatusCode = statusCode };
        }

        public static Result<TData> Error(ErrorResult errorDto, int statusCode)
        {
            return new Result<TData> { ErrorDto = errorDto, HttpStatusCode = statusCode, IsSuccessful = false };
        }

        public static Result<TData> Error(string errorMessage, int statusCode, bool isShow = true)
        {
            return new Result<TData>
            {
                Data = default,
                ErrorDto = new(errorMessage, isShow),
                HttpStatusCode = statusCode,
                IsSuccessful = false
            };
        }
    }
}
