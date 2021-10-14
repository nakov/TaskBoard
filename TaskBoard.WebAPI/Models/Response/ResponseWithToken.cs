using System;

namespace TaskBoard.WebAPI.Models.Response
{
    public class ResponseWithToken
    {
        public string Token { get; init; }
        public DateTime Expiration { get; init; }
    }
}
