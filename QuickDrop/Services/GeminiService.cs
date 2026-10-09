using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using QuickDrop.Models;
using QuickDrop.Helpers;

namespace QuickDrop.Services
{
    public class GeminiService
    {
        private readonly string _endpoint;
        private readonly HttpClient _httpClient;

        public GeminiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
            _endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={Secrets.GeminiApiKey}";
        }

        public async Task<AiRecipeResult> GetRecipeAndProductsAsync(string userMessage, List<Product> allProducts)
        {
            var catalogBuilder = new StringBuilder();
            foreach (var p in allProducts)
            {
                catalogBuilder.AppendLine($"ID: {p.ProductId} | Name: {p.Name} | Price: ${p.Price}");
            }

            string prompt = $@"
            You are the head chef and recipe assistant for 'QuickDrop', a fast grocery delivery app.
            The user will tell you what they want to make.
            Your tasks:
            1. Create a step-by-step recipe in English (leave empty lines between steps).
            2. Write a short, helpful ""Chef Tip"" in English for this recipe.
            3. Find the required ingredients from the CATALOG below and include their product IDs in the JSON 'productIds' list.

            IMPORTANT - STRICT RULE: The recipe text (recipeText) and chef tip (chefTip) MUST NOT contain product IDs. Product IDs should never be displayed in the text to the user, use natural product names only.

            CATALOG:
            {catalogBuilder.ToString()}

            USER MESSAGE:
            {userMessage}

            YOUR RESPONSE FORMAT MUST BE STRICTLY THIS JSON:
            {{
              ""recipeText"": ""Step-by-step recipe in English"",
              ""chefTip"": ""Helpful chef tip in English"",
              ""productIds"": [1, 5, 12]
            }}
            ";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            string jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await _httpClient.PostAsync(_endpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        using var jsonDoc = JsonDocument.Parse(jsonResponse);
                        var root = jsonDoc.RootElement;

                        string aiRawText = root.GetProperty("candidates")[0]
                                               .GetProperty("content")
                                               .GetProperty("parts")[0]
                                               .GetProperty("text")
                                               .GetString()
                                               .Replace("```json", "")
                                               .Replace("```", "")
                                               .Trim();

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var geminiResponse = JsonSerializer.Deserialize<GeminiRecipeResponse>(aiRawText, options);

                        if (geminiResponse == null) return null;

                        var matchedProducts = allProducts.Where(p => geminiResponse.ProductIds.Contains(p.ProductId)).ToList();

                        return new AiRecipeResult
                        {
                            RecipeText = geminiResponse.RecipeText,
                            ChefTip = geminiResponse.ChefTip,
                            MatchedProducts = matchedProducts
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable || response.StatusCode == (System.Net.HttpStatusCode)429)
                    {
                        System.Diagnostics.Debug.WriteLine($"[API YOĞUNLUK VEYA KOTA] {i + 1}. deneme başarısız. Bekleniyor...");
                        await Task.Delay(2500);
                        continue;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"API Hatası: {await response.Content.ReadAsStringAsync()}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Sistem Hatası: {ex.Message}");
                    if (i == maxRetries - 1) return null;
                    await Task.Delay(2500);
                }
            }

            return null;
        }
    }
}