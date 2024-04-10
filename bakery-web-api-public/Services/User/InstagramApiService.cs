using bakery_web_api.Interfaces.User;
using bakery_web_api.Models.InstagramModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace bakery_web_api.Services.User;

public class InstagramApiService : IInstagramApi
{
    private readonly IConfiguration _configuration;

    public InstagramApiService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ActionResult> GetInstagramPosts()
    {
        try
        {
            var instagramKey = _configuration["MetaDev:instagramKey"];
            if (_configuration == null || instagramKey == null)
                throw new Exception("Brak klucza instagrama.");

            var client = new HttpClient();
            var response =
                await client.GetAsync(
                    $"https://graph.instagram.com/me/media?fields=media_url,media_type,permalink&access_token={instagramKey}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Błąd podczas pobierania postów z Instagrama.");

            var content = await response.Content.ReadAsStringAsync();
            var instagramResponse = JsonConvert.DeserializeObject<InstagramResponse>(content);

            if (instagramResponse == null)
                throw new Exception("Błąd podczas pobierania postów z Instagrama.");

            var posts = instagramResponse?.Data.Take(5).ToList();

            return new OkObjectResult(posts);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}