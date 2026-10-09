using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuickDrop.Models
{
    public class GeminiRecipeResponse
    {
        [JsonPropertyName("recipeText")]
        public string RecipeText { get; set; }

        [JsonPropertyName("chefTip")]
        public string ChefTip { get; set; }

        [JsonPropertyName("productIds")]
        public List<int> ProductIds { get; set; }
    }

    public class AiRecipeResult
    {
        public string RecipeText { get; set; }
        public string ChefTip { get; set; }
        public List<Product> MatchedProducts { get; set; } = new List<Product>();
    }
}