using FrontEndShared.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using VeryUsefulServer.Models;
using FrontEndShared.Services;

namespace FrontEndShared
{
    public abstract class ClientInterface
    {
        private UserStateManager userStateManager;
        private readonly HttpClient client;
        string baseUrl;

        protected delegate Task AuthDelegate();
        protected delegate void HTTPResponseDelegate();
        protected delegate Task SendMessageDelegate(Object toSend);

        protected SendMessageDelegate SendMessage;
        
        protected ClientInterface(HttpClient client, SendMessageDelegate sendmsg) {
            this.SendMessage = sendmsg;
            this.client = client;
            baseUrl = baseUrl = "http://" + Environment.GetEnvironmentVariable("API_ENDPOINT");
        }

        protected abstract void write(InputMessage message);





        public async Task SetBlameComplete(InputMessage input)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", input.JWT);

            HttpResponseMessage response = await client.GetAsync(baseUrl + $"/api/Blames/blameBegone/{input.Message}");

            HTTPResponseDelegate hTTPResponseDelegate = async () =>
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                await SendMessage(responseBody);

            };

            await HTTPResponseWrapper(response, hTTPResponseDelegate);
        }

        public async Task<HttpResponseMessage> getMyCreatedBlames(string jwt)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            return await client.GetAsync(baseUrl + "/api/Blames/myBlames");

        }



        public async Task<HttpResponseMessage> getMyOpenBlames(string jwt)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            return await client.GetAsync(baseUrl + "/api/Blames/openBlames");
        }

        public async Task<HttpResponseMessage> getHelloWorld(string jwt)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            return await client.GetAsync(baseUrl + "/api/hello");
        }

        public async Task<HttpResponseMessage> getBlameShame(string jwt)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            return await client.GetAsync(baseUrl + "/api/Blames/blameShame");

        }

        public async Task<HttpResponseMessage> postNewBlame(string jwt, NewBlameRequest request)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            var jsonString = JsonSerializer.Serialize(request);
            StringContent? httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            return await client.PutAsync(baseUrl + "/api/Blames/newBlame", httpContent);

        }




        private async Task AuthWrapper(InputMessage message, AuthDelegate action)
        {
            if (userStateManager.userStateExists(message.d.author.id) && userStateManager.getUserState(message.d.author.id).currentState == State.LOGGED_IN)
            {
                await action();
            }
            else
            {
                await SendMessage("You are not authenticated, begone! (Please run /blame login)");

            }
        }

        private async Task HTTPResponseWrapper(HttpResponseMessage response, HTTPResponseDelegate responseDelegate)
        {
            if (response.IsSuccessStatusCode)
            {
                // Deserialize the JSON response into a list of Blame objects
                try
                {
                    responseDelegate();
                }
                catch
                {
                    await SendMessage("Something went wrong while communicating with the backend!");
                }


            }
            else
            {
                // Print out the detailed error reason if available
                string errorMessage = response.ReasonPhrase; // Default to the reason phrase
                if (response.Content != null)
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }
                await SendMessage($"\n**Error ({response.StatusCode})**: {errorMessage}");
                return;
            }
        }

    }
}
