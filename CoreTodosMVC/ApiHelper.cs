using CoreFlogger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreTodosMVC
{
    public static class ApiHelper
    {
        private const string ApiUrl = "https://localhost:44307/api";
        public static async Task<List<T>> GetListFromApiAsync<T>(string path,
            HttpContext context)
        {
            var client = await GetHttpClientWithBearerTokenAsync(context);

            var apiRequestPath = $"{ApiUrl}{path}";
            var response = await client.GetAsync(apiRequestPath);

            if (!response.IsSuccessStatusCode)
            {
                var ex = new Exception("The API call didn't work!!");
                ex.Data.Add("API Request path", apiRequestPath);
                ex.Data.Add("StatusCode", response.StatusCode);
                var error = JsonConvert.DeserializeObject<CustomErrorResponse>(
                    await response.Content.ReadAsStringAsync());
                ex.Data.Add("ApiErrorId", error.ErrorId);
                ex.Data.Add("ApiErrorMessage", error.Message);
                throw ex;
            }

            return JsonConvert.DeserializeObject<List<T>>(await response.Content
                .ReadAsStringAsync());
        }

        private static async Task<HttpClient> GetHttpClientWithBearerTokenAsync(HttpContext 
            context)
        {
            var token = await context.GetTokenAsync("access_token");

            var client = new HttpClient();
            client.SetBearerToken(token);
            return client;            
        }        
    }
}
