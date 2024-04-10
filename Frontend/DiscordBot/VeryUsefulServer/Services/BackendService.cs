/*using VeryUsefulServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static VeryUsefulServer.Services.DiscordClient;

namespace VeryUsefulServer.Services
{
    public class BackendService
    {
        UserStateManager _userStateManager;
        string baseUrl;


        public async Task getMyCreatedBlames(HttpClient client)
        {
            if (!string.IsNullOrEmpty(jwt))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);


                HttpResponseMessage response = await client.GetAsync(baseUrl + "/api/Blames/myBlames");
                    

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
**/