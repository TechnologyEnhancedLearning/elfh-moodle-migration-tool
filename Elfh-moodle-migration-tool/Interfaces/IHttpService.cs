﻿namespace Moodle_Migration.Interfaces
{
    public interface IHttpService
    {
        Task<string> Get(string url);
        Task<(string result,int resultValue)> Post(string url, Dictionary<string, string> parameters);
    }
}
