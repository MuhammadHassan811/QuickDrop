using QuickDrop.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuickDrop.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _db;

        public async Task InitAsync()
        {
            if (_db is not null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "QuickDrop.db3");
            _db = new SQLiteAsyncConnection(databasePath);

            await _db.CreateTableAsync<Category>();
            await _db.CreateTableAsync<Product>();
            await _db.CreateTableAsync<CategoryFilter>();
            await _db.CreateTableAsync<User>();
            await _db.CreateTableAsync<CartItem>();
            await _db.CreateTableAsync<Discount>();
            await _db.CreateTableAsync<Order>();
            await _db.CreateTableAsync<OrderItem>();
            await _db.CreateTableAsync<Address>();
            await _db.CreateTableAsync<PaymentMethod>();
            await _db.CreateTableAsync<Favorite>();
            await _db.CreateTableAsync<FavoriteProduct>();
            await _db.CreateTableAsync<Campaign>();
            await _db.CreateTableAsync<Review>();

            await SeedDatabaseAsync();
        }

        private async Task SeedDatabaseAsync()
        {
            await SeedDefaultUserAsync();
            await SeedCategoriesAsync();
            await SeedProductsAsync();
            await SeedFiltersAsync();
            await SeedCampaignsAsync();
            await SeedDiscountsAsync();
            await SeedAddressesAsync();
            await SeedPaymentMethodsAsync();
            await SeedFavoritesAsync();
            await SeedReviewsAsync();
        }

        private async Task SeedDefaultUserAsync()
        {
            var demoUser = await _db.Table<User>().Where(u => u.Email == "demo@quickdrop.com").FirstOrDefaultAsync();
            if (demoUser == null)
            {
                await _db.InsertAsync(new User
                {
                    FullName = "Demo User",
                    Email = "demo@quickdrop.com",
                    Phone = "+1 555-0199",
                    Password = "123456",
                    Role = "Customer",
                    ProfileImage = "default_avatar.png"
                });
            }

            var adminUser = await _db.Table<User>().Where(u => u.Email == "admin@quickdrop.com").FirstOrDefaultAsync();
            if (adminUser == null)
            {
                await _db.InsertAsync(new User
                {
                    FullName = "Super Admin",
                    Email = "admin@quickdrop.com",
                    Phone = "+1 555-0999",
                    Password = "admin123",
                    Role = "Admin",
                    ProfileImage = "default_avatar.png"
                });
            }

            var vendorUser = await _db.Table<User>().Where(u => u.Email == "vendor@quickdrop.com").FirstOrDefaultAsync();
            if (vendorUser == null)
            {
                await _db.InsertAsync(new User
                {
                    FullName = "QuickDrop Official Vendor",
                    Email = "vendor@quickdrop.com",
                    Phone = "+1 555-0888",
                    Password = "vendor123",
                    Role = "Vendor",
                    ProfileImage = "default_avatar.png"
                });
            }

            var riderUser = await _db.Table<User>().Where(u => u.Email == "rider@quickdrop.com").FirstOrDefaultAsync();
            if (riderUser == null)
            {
                await _db.InsertAsync(new User
                {
                    FullName = "Alex Rivers (Speed Rider)",
                    Email = "rider@quickdrop.com",
                    Phone = "+1 555-0777",
                    Password = "rider123",
                    Role = "Rider",
                    ProfileImage = "default_avatar.png"
                });
            }
        }

        private async Task SeedCategoriesAsync()
        {
            var count = await _db.Table<Category>().CountAsync();
            if (count == 0)
            {
                var categories = new List<Category>
                {
                    new Category { CategoryId = 1, Name = "Fruits & Vegetables", Image = "category_fruit.jpg", IconCode = "\ue541", Description = "Fresh fruits, vegetables, and greens" },
                    new Category { CategoryId = 2, Name = "Dairy & Breakfast", Image = "category_milk.jpg", IconCode = "\ue56c", Description = "Milk, cheese, eggs, and breakfast items" },
                    new Category { CategoryId = 3, Name = "Snacks & Sweets", Image = "category_atistirmalik.jpg", IconCode = "\ue540", Description = "Chips, chocolates, biscuits, and treats" },
                    new Category { CategoryId = 4, Name = "Beverages", Image = "category_icecek.jpg", IconCode = "\ue570", Description = "Water, sodas, juices, and cold drinks" },
                    new Category { CategoryId = 5, Name = "Bakery", Image = "category_firin.jpg", IconCode = "\ue555", Description = "Bread, pastries, rolls, and bagels" },
                    new Category { CategoryId = 6, Name = "Meat, Fish & Poultry", Image = "category_et.jpg", IconCode = "\ue532", Description = "Fresh meat, chicken, and seafood" },
                    new Category { CategoryId = 7, Name = "Cleaning & Household", Image = "category_temizlik.jpg", IconCode = "\ue8b8", Description = "Laundry, dishwashing, and home care" },
                    new Category { CategoryId = 8, Name = "Pantry & Staples", Image = "category_temel.jpg", IconCode = "\ue88a", Description = "Pasta, rice, flour, oil, and spices" }
                };
                await _db.InsertAllAsync(categories);
            }
        }

        private async Task SeedProductsAsync()
        {
            var count = await _db.Table<Product>().CountAsync();
            if (count == 0)
            {
                var products = new List<Product>
                {
                    // Flash Deals (IDs 101, 102, 103)
                    new Product
                    {
                        ProductId = 101,
                        CategoryId = 3,
                        Brand = "Ferrero",
                        Name = "Rocher Chocolate",
                        Description = "Delicious whole hazelnut wrapped in rich creamy chocolate and crispy wafer.",
                        Price = 82.50m,
                        OldPrice = 110.00m,
                        DiscountPercentage = 25,
                        Unit = "200g",
                        ImageUrl = "rocher.png",
                        Stock = 50,
                        IsFavorite = true,
                        NutritionEnergy = "579 kcal",
                        NutritionFat = "42.7g",
                        NutritionProtein = "8.2g",
                        NutritionCarbs = "44.4g"
                    },
                    new Product
                    {
                        ProductId = 102,
                        CategoryId = 4,
                        Brand = "Nescafe",
                        Name = "Gold Coffee",
                        Description = "Rich aroma and smooth taste freeze-dried instant coffee.",
                        Price = 101.50m,
                        OldPrice = 145.00m,
                        DiscountPercentage = 30,
                        Unit = "100g Jar",
                        ImageUrl = "nescafe.png",
                        Stock = 40,
                        IsFavorite = true,
                        NutritionEnergy = "67 kcal",
                        NutritionFat = "0.2g",
                        NutritionProtein = "7.0g",
                        NutritionCarbs = "9.0g"
                    },
                    new Product
                    {
                        ProductId = 103,
                        CategoryId = 3,
                        Brand = "Pringles",
                        Name = "Sour Cream & Onion",
                        Description = "Crispy potato crisps bursting with savory sour cream and herb flavor.",
                        Price = 60.00m,
                        OldPrice = 75.00m,
                        DiscountPercentage = 20,
                        Unit = "165g",
                        ImageUrl = "pringles.png",
                        Stock = 60,
                        IsFavorite = false,
                        NutritionEnergy = "515 kcal",
                        NutritionFat = "32g",
                        NutritionProtein = "4.2g",
                        NutritionCarbs = "51g"
                    },

                    // Fruits & Vegetables (CategoryId: 1)
                    new Product
                    {
                        ProductId = 1,
                        CategoryId = 1,
                        Brand = "FreshFarm",
                        Name = "Organic Bananas",
                        Description = "Sweet and nutritious fresh organic bananas sourced from premium farms.",
                        Price = 29.90m,
                        OldPrice = 36.00m,
                        DiscountPercentage = 17,
                        Unit = "1 kg",
                        ImageUrl = "muz1.png,muz.jpg",
                        Stock = 100,
                        IsFavorite = true,
                        NutritionEnergy = "89 kcal",
                        NutritionFat = "0.3g",
                        NutritionProtein = "1.1g",
                        NutritionCarbs = "22.8g",
                        StorageConditions = "Keep at room temperature away from direct sunlight."
                    },
                    new Product
                    {
                        ProductId = 2,
                        CategoryId = 1,
                        Brand = "FreshFarm",
                        Name = "Crisp Red Apples",
                        Description = "Crispy, sweet and juicy red apples freshly harvested from orchards.",
                        Price = 22.50m,
                        OldPrice = 28.00m,
                        DiscountPercentage = 20,
                        Unit = "1 kg",
                        ImageUrl = "elma.png,amasya_elma.jpg",
                        Stock = 80,
                        IsFavorite = false,
                        NutritionEnergy = "52 kcal",
                        NutritionFat = "0.2g",
                        NutritionProtein = "0.3g",
                        NutritionCarbs = "13.8g"
                    },
                    new Product
                    {
                        ProductId = 3,
                        CategoryId = 1,
                        Brand = "GreenValley",
                        Name = "Vine Tomatoes",
                        Description = "Firm, flavorful, and juicy vine-ripened fresh red tomatoes.",
                        Price = 18.90m,
                        OldPrice = 24.00m,
                        DiscountPercentage = 21,
                        Unit = "1 kg",
                        ImageUrl = "domates.png,salkim_domates.jpg",
                        Stock = 90,
                        IsFavorite = false,
                        NutritionEnergy = "18 kcal",
                        NutritionFat = "0.2g",
                        NutritionProtein = "0.9g",
                        NutritionCarbs = "3.9g"
                    },
                    new Product
                    {
                        ProductId = 4,
                        CategoryId = 1,
                        Brand = "GreenValley",
                        Name = "Fresh Cucumbers",
                        Description = "Crisp and refreshing locally grown salad cucumbers.",
                        Price = 16.50m,
                        OldPrice = 20.00m,
                        DiscountPercentage = 18,
                        Unit = "1 kg",
                        ImageUrl = "salatalik.png",
                        Stock = 85,
                        IsFavorite = false,
                        NutritionEnergy = "15 kcal",
                        NutritionFat = "0.1g",
                        NutritionProtein = "0.7g",
                        NutritionCarbs = "3.6g"
                    },
                    new Product
                    {
                        ProductId = 5,
                        CategoryId = 1,
                        Brand = "FreshFarm",
                        Name = "Golden Potatoes",
                        Description = "Versatile golden potatoes ideal for roasting, boiling, and mashing.",
                        Price = 14.00m,
                        OldPrice = 18.00m,
                        DiscountPercentage = 22,
                        Unit = "2 kg",
                        ImageUrl = "patates.png",
                        Stock = 120,
                        IsFavorite = false,
                        NutritionEnergy = "77 kcal",
                        NutritionFat = "0.1g",
                        NutritionProtein = "2.0g",
                        NutritionCarbs = "17.5g"
                    },
                    new Product
                    {
                        ProductId = 6,
                        CategoryId = 1,
                        Brand = "FreshFarm",
                        Name = "Yellow Onions",
                        Description = "Pungent and flavorful cooking onions essential for every kitchen.",
                        Price = 12.50m,
                        OldPrice = null,
                        DiscountPercentage = 0,
                        Unit = "1 kg",
                        ImageUrl = "kuru_sogan.png",
                        Stock = 110,
                        IsFavorite = false,
                        NutritionEnergy = "40 kcal",
                        NutritionFat = "0.1g",
                        NutritionProtein = "1.1g",
                        NutritionCarbs = "9.3g"
                    },
                    new Product
                    {
                        ProductId = 7,
                        CategoryId = 1,
                        Brand = "SunCitrus",
                        Name = "Fresh Lemons",
                        Description = "Zesty, fragrant, and juicy fresh yellow lemons.",
                        Price = 19.90m,
                        OldPrice = 24.90m,
                        DiscountPercentage = 20,
                        Unit = "500g",
                        ImageUrl = "limon.png",
                        Stock = 75,
                        IsFavorite = false,
                        NutritionEnergy = "29 kcal",
                        NutritionFat = "0.3g",
                        NutritionProtein = "1.1g",
                        NutritionCarbs = "9.3g"
                    },
                    new Product
                    {
                        ProductId = 8,
                        CategoryId = 1,
                        Brand = "GreenValley",
                        Name = "Crisp Lettuce",
                        Description = "Crisp, green garden lettuce, perfect for daily fresh salads.",
                        Price = 15.00m,
                        OldPrice = null,
                        DiscountPercentage = 0,
                        Unit = "1 Piece",
                        ImageUrl = "marul.png",
                        Stock = 60,
                        IsFavorite = false,
                        NutritionEnergy = "14 kcal",
                        NutritionFat = "0.2g",
                        NutritionProtein = "1.4g",
                        NutritionCarbs = "2.9g"
                    },

                    // Dairy & Breakfast (CategoryId: 2)
                    new Product
                    {
                        ProductId = 9,
                        CategoryId = 2,
                        Brand = "DailyDairy",
                        Name = "Fresh Whole Milk",
                        Description = "100% pure pasteurized whole farm milk rich in calcium and vitamin D.",
                        Price = 32.50m,
                        OldPrice = 38.00m,
                        DiscountPercentage = 14,
                        Unit = "1 L",
                        ImageUrl = "sut1.png,sut.jpg",
                        Stock = 95,
                        IsFavorite = true,
                        NutritionEnergy = "64 kcal",
                        NutritionFat = "3.5g",
                        NutritionProtein = "3.2g",
                        NutritionCarbs = "4.8g",
                        NutritionCalcium = "120mg"
                    },
                    new Product
                    {
                        ProductId = 10,
                        CategoryId = 2,
                        Brand = "HappyHen",
                        Name = "Farm Fresh Eggs",
                        Description = "Grade-A free-range farm fresh large eggs packed with natural protein.",
                        Price = 48.00m,
                        OldPrice = 55.00m,
                        DiscountPercentage = 13,
                        Unit = "15 Pack",
                        ImageUrl = "yumurta1.png,yumurta.jpg",
                        Stock = 80,
                        IsFavorite = true,
                        NutritionEnergy = "143 kcal",
                        NutritionFat = "9.5g",
                        NutritionProtein = "12.6g",
                        NutritionCarbs = "0.7g"
                    },
                    new Product
                    {
                        ProductId = 11,
                        CategoryId = 2,
                        Brand = "ArtisanCheese",
                        Name = "Mature Cheddar",
                        Description = "Aged creamy cheddar cheese block made from premium whole milk.",
                        Price = 85.00m,
                        OldPrice = 105.00m,
                        DiscountPercentage = 19,
                        Unit = "400g",
                        ImageUrl = "kasar2.png,kasar_peyniri.jpg",
                        Stock = 50,
                        IsFavorite = false,
                        NutritionEnergy = "403 kcal",
                        NutritionFat = "33g",
                        NutritionProtein = "25g",
                        NutritionCarbs = "1.3g"
                    },
                    new Product
                    {
                        ProductId = 12,
                        CategoryId = 2,
                        Brand = "Creamery",
                        Name = "Pure Farm Butter",
                        Description = "Rich and creamy churned traditional butter made from fresh sweet cream.",
                        Price = 72.00m,
                        OldPrice = 88.00m,
                        DiscountPercentage = 18,
                        Unit = "250g",
                        ImageUrl = "tereyag.png,dogal_tereyagi.png",
                        Stock = 45,
                        IsFavorite = false,
                        NutritionEnergy = "717 kcal",
                        NutritionFat = "81g",
                        NutritionProtein = "0.9g",
                        NutritionCarbs = "0.1g"
                    },
                    new Product
                    {
                        ProductId = 13,
                        CategoryId = 2,
                        Brand = "DailyDairy",
                        Name = "Strained Greek Yogurt",
                        Description = "Thick, creamy, and velvety strained probiotic yogurt.",
                        Price = 44.00m,
                        OldPrice = null,
                        DiscountPercentage = 0,
                        Unit = "900g",
                        ImageUrl = "suzme_yogurt.png",
                        Stock = 60,
                        IsFavorite = false,
                        NutritionEnergy = "115 kcal",
                        NutritionFat = "5g",
                        NutritionProtein = "9g",
                        NutritionCarbs = "4g"
                    },
                    new Product
                    {
                        ProductId = 14,
                        CategoryId = 2,
                        Brand = "OliveGrove",
                        Name = "Classic Black Olives",
                        Description = "Naturally cured premium black olives in light brine with olive oil.",
                        Price = 68.50m,
                        OldPrice = 82.00m,
                        DiscountPercentage = 16,
                        Unit = "500g",
                        ImageUrl = "siyah_zeytin.png",
                        Stock = 55,
                        IsFavorite = false,
                        NutritionEnergy = "115 kcal",
                        NutritionFat = "11g",
                        NutritionProtein = "0.8g",
                        NutritionCarbs = "6g"
                    },
                    new Product
                    {
                        ProductId = 15,
                        CategoryId = 2,
                        Brand = "HoneyBee",
                        Name = "Pure Blossom Honey",
                        Description = "100% natural pure floral nectar harvested from mountain meadows.",
                        Price = 95.00m,
                        OldPrice = 120.00m,
                        DiscountPercentage = 21,
                        Unit = "460g Jar",
                        ImageUrl = "bal.png,suzme_cicek_bali.png",
                        Stock = 35,
                        IsFavorite = false,
                        NutritionEnergy = "304 kcal",
                        NutritionFat = "0g",
                        NutritionProtein = "0.3g",
                        NutritionCarbs = "82g"
                    },

                    // Snacks & Sweets (CategoryId: 3)
                    new Product
                    {
                        ProductId = 16,
                        CategoryId = 3,
                        Brand = "Nutella",
                        Name = "Hazelnut Spread",
                        Description = "World-famous rich chocolate hazelnut spread for toast and crepes.",
                        Price = 89.90m,
                        OldPrice = 110.00m,
                        DiscountPercentage = 18,
                        Unit = "400g Jar",
                        ImageUrl = "nutella.png",
                        Stock = 70,
                        IsFavorite = false,
                        NutritionEnergy = "539 kcal",
                        NutritionFat = "30.9g",
                        NutritionProtein = "6.3g",
                        NutritionCarbs = "57.5g"
                    },
                    new Product
                    {
                        ProductId = 17,
                        CategoryId = 3,
                        Brand = "Oreo",
                        Name = "Original Cookies",
                        Description = "Rich dark chocolate cookies filled with smooth sweet vanilla cream.",
                        Price = 26.50m,
                        OldPrice = 32.00m,
                        DiscountPercentage = 17,
                        Unit = "154g",
                        ImageUrl = "oreo.png",
                        Stock = 85,
                        IsFavorite = false,
                        NutritionEnergy = "471 kcal",
                        NutritionFat = "20g",
                        NutritionProtein = "5g",
                        NutritionCarbs = "68g"
                    },
                    new Product
                    {
                        ProductId = 18,
                        CategoryId = 3,
                        Brand = "Doritos",
                        Name = "Nacho Cheese Chips",
                        Description = "Crispy corn tortilla chips coated in bold and savory nacho cheese seasoning.",
                        Price = 28.00m,
                        OldPrice = 35.00m,
                        DiscountPercentage = 20,
                        Unit = "130g",
                        ImageUrl = "doritos.png",
                        Stock = 90,
                        IsFavorite = false,
                        NutritionEnergy = "498 kcal",
                        NutritionFat = "25.6g",
                        NutritionProtein = "6.5g",
                        NutritionCarbs = "58.4g"
                    },
                    new Product
                    {
                        ProductId = 19,
                        CategoryId = 3,
                        Brand = "Magnum",
                        Name = "Almond Ice Cream Bar",
                        Description = "Smooth vanilla ice cream coated in thick milk chocolate and toasted almonds.",
                        Price = 38.00m,
                        OldPrice = 45.00m,
                        DiscountPercentage = 15,
                        Unit = "100ml",
                        ImageUrl = "magnum.png",
                        Stock = 65,
                        IsFavorite = false,
                        NutritionEnergy = "272 kcal",
                        NutritionFat = "16g",
                        NutritionProtein = "3.8g",
                        NutritionCarbs = "27g"
                    },
                    new Product
                    {
                        ProductId = 20,
                        CategoryId = 3,
                        Brand = "ChocoArt",
                        Name = "Dark Chocolate 70%",
                        Description = "Exquisite 70% dark cocoa chocolate bar with intense aromatic notes.",
                        Price = 24.00m,
                        OldPrice = null,
                        DiscountPercentage = 0,
                        Unit = "80g",
                        ImageUrl = "bitter_cikolata.png,cikolata.png",
                        Stock = 75,
                        IsFavorite = false,
                        NutritionEnergy = "566 kcal",
                        NutritionFat = "41g",
                        NutritionProtein = "8.5g",
                        NutritionCarbs = "34g"
                    },

                    // Beverages (CategoryId: 4)
                    new Product
                    {
                        ProductId = 21,
                        CategoryId = 4,
                        Brand = "Classic Cola",
                        Name = "Original Cola",
                        Description = "Refreshing, crisp, and bubbly iconic cola drink.",
                        Price = 28.00m,
                        OldPrice = 34.00m,
                        DiscountPercentage = 18,
                        Unit = "1 L Bottle",
                        ImageUrl = "orijinal_kola.png",
                        Stock = 120,
                        IsFavorite = false,
                        NutritionEnergy = "42 kcal",
                        NutritionFat = "0g",
                        NutritionProtein = "0g",
                        NutritionCarbs = "10.6g"
                    },
                    new Product
                    {
                        ProductId = 22,
                        CategoryId = 4,
                        Brand = "Classic Cola",
                        Name = "Zero Sugar Cola",
                        Description = "Crisp and classic cola taste with absolutely zero sugar and zero calories.",
                        Price = 28.00m,
                        OldPrice = 34.00m,
                        DiscountPercentage = 18,
                        Unit = "1 L Bottle",
                        ImageUrl = "sekersiz_kola.png",
                        Stock = 110,
                        IsFavorite = false,
                        NutritionEnergy = "0.3 kcal",
                        NutritionFat = "0g",
                        NutritionProtein = "0g",
                        NutritionCarbs = "0g"
                    },
                    new Product
                    {
                        ProductId = 23,
                        CategoryId = 4,
                        Brand = "FuseTea",
                        Name = "Peach Iced Tea",
                        Description = "Slow-brewed black tea blended with delicious peach juice.",
                        Price = 22.00m,
                        OldPrice = 26.00m,
                        DiscountPercentage = 15,
                        Unit = "1 L Bottle",
                        ImageUrl = "fusetea.png,soguk_cay_seftali.png",
                        Stock = 80,
                        IsFavorite = false,
                        NutritionEnergy = "31 kcal",
                        NutritionFat = "0g",
                        NutritionProtein = "0g",
                        NutritionCarbs = "7.5g"
                    },
                    new Product
                    {
                        ProductId = 24,
                        CategoryId = 4,
                        Brand = "AquaPure",
                        Name = "Sparkling Mineral Water",
                        Description = "Refreshing natural mineral water rich in essential minerals and bubbles.",
                        Price = 12.00m,
                        OldPrice = 15.00m,
                        DiscountPercentage = 20,
                        Unit = "6 x 200ml",
                        ImageUrl = "maden_suyu.png,dogal_maden_suyu.png",
                        Stock = 100,
                        IsFavorite = false,
                        NutritionEnergy = "0 kcal",
                        NutritionFat = "0g",
                        NutritionProtein = "0g",
                        NutritionCarbs = "0g"
                    },
                    new Product
                    {
                        ProductId = 25,
                        CategoryId = 4,
                        Brand = "AquaPure",
                        Name = "Natural Spring Water",
                        Description = "Pristine, clean, and refreshing natural spring bottled water.",
                        Price = 10.00m,
                        OldPrice = null,
                        DiscountPercentage = 0,
                        Unit = "5 L",
                        ImageUrl = "dogal_kaynak_suyu.png",
                        Stock = 150,
                        IsFavorite = false,
                        NutritionEnergy = "0 kcal",
                        NutritionFat = "0g",
                        NutritionProtein = "0g",
                        NutritionCarbs = "0g"
                    },
                    new Product
                    {
                        ProductId = 26,
                        CategoryId = 4,
                        Brand = "SunCitrus",
                        Name = "Fresh Orange Juice",
                        Description = "100% pure squeezed orange juice with rich natural vitamin C.",
                        Price = 35.00m,
                        OldPrice = 42.00m,
                        DiscountPercentage = 17,
                        Unit = "1 L",
                        ImageUrl = "sikmalik_portakal.png",
                        Stock = 70,
                        IsFavorite = false,
                        NutritionEnergy = "45 kcal",
                        NutritionFat = "0.2g",
                        NutritionProtein = "0.7g",
                        NutritionCarbs = "10.4g"
                    },

                    // Bakery (CategoryId: 5)
                    new Product
                    {
                        ProductId = 27,
                        CategoryId = 5,
                        Brand = "BakeHouse",
                        Name = "Artisan Sourdough Loaf",
                        Description = "Slow-fermented artisan sourdough bread with a crispy crust and soft crumb.",
                        Price = 24.00m,
                        OldPrice = 30.00m,
                        DiscountPercentage = 20,
                        Unit = "500g",
                        ImageUrl = "ekmek.jpg",
                        Stock = 60,
                        IsFavorite = false,
                        NutritionEnergy = "244 kcal",
                        NutritionFat = "1.1g",
                        NutritionProtein = "8.4g",
                        NutritionCarbs = "50.6g"
                    },
                    new Product
                    {
                        ProductId = 28,
                        CategoryId = 5,
                        Brand = "BakeHouse",
                        Name = "Sesame Bagel (Simit)",
                        Description = "Crispy golden exterior coated in toasted sesame seeds with a soft chewy center.",
                        Price = 12.00m,
                        OldPrice = null,
                        DiscountPercentage = 0,
                        Unit = "1 Piece",
                        ImageUrl = "simit.jpg,sokak_simidi.png",
                        Stock = 80,
                        IsFavorite = false,
                        NutritionEnergy = "275 kcal",
                        NutritionFat = "3.8g",
                        NutritionProtein = "9.2g",
                        NutritionCarbs = "52.4g"
                    },
                    new Product
                    {
                        ProductId = 29,
                        CategoryId = 5,
                        Brand = "BakeHouse",
                        Name = "Whole Wheat Sliced Bread",
                        Description = "Hearty, fiber-rich 100% whole grain sliced sandwich bread.",
                        Price = 26.00m,
                        OldPrice = 32.00m,
                        DiscountPercentage = 19,
                        Unit = "450g",
                        ImageUrl = "kepekli_ekmek.png,dilimli_tost_ekmegi.png",
                        Stock = 50,
                        IsFavorite = false,
                        NutritionEnergy = "228 kcal",
                        NutritionFat = "1.5g",
                        NutritionProtein = "9.8g",
                        NutritionCarbs = "43.2g"
                    },
                    new Product
                    {
                        ProductId = 30,
                        CategoryId = 5,
                        Brand = "BakeHouse",
                        Name = "Butter Croissants",
                        Description = "Golden, flaky, and layered traditional French breakfast pastry.",
                        Price = 22.00m,
                        OldPrice = 28.00m,
                        DiscountPercentage = 21,
                        Unit = "2 Pack",
                        ImageUrl = "tereyagli_kruvasan.png",
                        Stock = 40,
                        IsFavorite = false,
                        NutritionEnergy = "406 kcal",
                        NutritionFat = "21g",
                        NutritionProtein = "8.2g",
                        NutritionCarbs = "45.8g"
                    },

                    // Meat, Fish & Poultry (CategoryId: 6)
                    new Product
                    {
                        ProductId = 31,
                        CategoryId = 6,
                        Brand = "FarmFresh",
                        Name = "Chicken Breast Fillet",
                        Description = "Tender, skinless and boneless chicken breast fillets loaded with protein.",
                        Price = 135.00m,
                        OldPrice = 160.00m,
                        DiscountPercentage = 16,
                        Unit = "1 kg",
                        ImageUrl = "pilic_fileto_gogus.png,pilic_baget.jpg",
                        Stock = 50,
                        IsFavorite = false,
                        NutritionEnergy = "120 kcal",
                        NutritionFat = "1.5g",
                        NutritionProtein = "26g",
                        NutritionCarbs = "0g"
                    },
                    new Product
                    {
                        ProductId = 32,
                        CategoryId = 6,
                        Brand = "PrimeButcher",
                        Name = "Ground Beef (Mince)",
                        Description = "Fresh 85/15 lean ground prime beef mince for delicious meatballs and sauces.",
                        Price = 245.00m,
                        OldPrice = 280.00m,
                        DiscountPercentage = 12,
                        Unit = "500g",
                        ImageUrl = "dana_kiyma.png",
                        Stock = 45,
                        IsFavorite = false,
                        NutritionEnergy = "215 kcal",
                        NutritionFat = "15g",
                        NutritionProtein = "20g",
                        NutritionCarbs = "0g"
                    },
                    new Product
                    {
                        ProductId = 33,
                        CategoryId = 6,
                        Brand = "PrimeButcher",
                        Name = "Prime Beef Cubes",
                        Description = "Hand-trimmed tender prime beef cubes perfect for stews and slow cooking.",
                        Price = 265.00m,
                        OldPrice = 295.00m,
                        DiscountPercentage = 10,
                        Unit = "500g",
                        ImageUrl = "dana_kusbasi.png",
                        Stock = 40,
                        IsFavorite = false,
                        NutritionEnergy = "190 kcal",
                        NutritionFat = "11g",
                        NutritionProtein = "22g",
                        NutritionCarbs = "0g"
                    },
                    new Product
                    {
                        ProductId = 34,
                        CategoryId = 6,
                        Brand = "PrimeButcher",
                        Name = "Spiced Beef Sausage",
                        Description = "Traditional dry-cured beef sausage seasoned with garlic and aromatic spices.",
                        Price = 165.00m,
                        OldPrice = 195.00m,
                        DiscountPercentage = 15,
                        Unit = "300g",
                        ImageUrl = "kangal_sucuk.png",
                        Stock = 55,
                        IsFavorite = false,
                        NutritionEnergy = "380 kcal",
                        NutritionFat = "33g",
                        NutritionProtein = "18g",
                        NutritionCarbs = "2g"
                    },
                    new Product
                    {
                        ProductId = 35,
                        CategoryId = 6,
                        Brand = "SeaBreeze",
                        Name = "Canned Tuna in Olive Oil",
                        Description = "Premium wild yellowfin tuna chunks packed in pure virgin olive oil.",
                        Price = 78.00m,
                        OldPrice = 95.00m,
                        DiscountPercentage = 18,
                        Unit = "2 x 160g",
                        ImageUrl = "ton_baligi.png",
                        Stock = 60,
                        IsFavorite = false,
                        NutritionEnergy = "198 kcal",
                        NutritionFat = "10g",
                        NutritionProtein = "27g",
                        NutritionCarbs = "0g"
                    },

                    // Cleaning & Household (CategoryId: 7)
                    new Product
                    {
                        ProductId = 36,
                        CategoryId = 7,
                        Brand = "Ariel",
                        Name = "Mountain Spring Detergent",
                        Description = "Deep-cleaning laundry powder with optical brighteners and long-lasting freshness.",
                        Price = 175.00m,
                        OldPrice = 220.00m,
                        DiscountPercentage = 20,
                        Unit = "4 kg Powder",
                        ImageUrl = "ariel.png,camasir_deterjan.png",
                        Stock = 50,
                        IsFavorite = false
                    },
                    new Product
                    {
                        ProductId = 37,
                        CategoryId = 7,
                        Brand = "Fairy",
                        Name = "Dishwashing Liquid",
                        Description = "Ultra-concentrated dish soap that effortlessly dissolves tough grease.",
                        Price = 42.00m,
                        OldPrice = 52.00m,
                        DiscountPercentage = 19,
                        Unit = "650 ml",
                        ImageUrl = "sivi_bulasik_deterjani.png",
                        Stock = 80,
                        IsFavorite = false
                    },
                    new Product
                    {
                        ProductId = 38,
                        CategoryId = 7,
                        Brand = "CleanPro",
                        Name = "All-in-One Dish Tablets",
                        Description = "Advanced enzyme dishwasher tablets for brilliant shine and residue removal.",
                        Price = 125.00m,
                        OldPrice = 160.00m,
                        DiscountPercentage = 22,
                        Unit = "50 Pack",
                        ImageUrl = "bulasik_tableti.png",
                        Stock = 60,
                        IsFavorite = false
                    },
                    new Product
                    {
                        ProductId = 39,
                        CategoryId = 7,
                        Brand = "SoftHome",
                        Name = "Kitchen Paper Towels",
                        Description = "Super-absorbent 2-ply kitchen paper towel rolls with strong fibers.",
                        Price = 58.00m,
                        OldPrice = 70.00m,
                        DiscountPercentage = 17,
                        Unit = "6 Rolls",
                        ImageUrl = "kagit_havlu.png",
                        Stock = 70,
                        IsFavorite = false
                    },
                    new Product
                    {
                        ProductId = 40,
                        CategoryId = 7,
                        Brand = "SoftHome",
                        Name = "Soft 3-Ply Toilet Paper",
                        Description = "Quilted 3-ply extra-soft bathroom tissue designed for maximum comfort.",
                        Price = 89.00m,
                        OldPrice = 110.00m,
                        DiscountPercentage = 19,
                        Unit = "16 Rolls",
                        ImageUrl = "tuvalet_kagidi.png",
                        Stock = 65,
                        IsFavorite = false
                    }
                };

                await _db.InsertAllAsync(products);
            }
        }

        private async Task SeedFiltersAsync()
        {
            var count = await _db.Table<CategoryFilter>().CountAsync();
            if (count == 0)
            {
                var filters = new List<CategoryFilter>
                {
                    new CategoryFilter { Name = "All", IsActive = true },
                    new CategoryFilter { Name = "Popular", IsActive = false },
                    new CategoryFilter { Name = "Discounted", IsActive = false },
                    new CategoryFilter { Name = "Organic", IsActive = false },
                    new CategoryFilter { Name = "New", IsActive = false }
                };
                await _db.InsertAllAsync(filters);
            }
        }

        private async Task SeedCampaignsAsync()
        {
            var count = await _db.Table<Campaign>().CountAsync();
            if (count == 0)
            {
                var campaigns = new List<Campaign>
                {
                    new Campaign
                    {
                        Title = "Super Breakfast Deals",
                        Description = "Up to 30% off on fresh eggs, cheeses, and bakery treats!",
                        ImageUrl = "kahvalti_banner.jpg",
                        BadgeText = "%30 OFF",
                        SecondaryBadgeText = "Morning Special",
                        ProductCountText = "24+ Products",
                        IsActive = true,
                        EndDate = DateTime.Now.AddDays(30)
                    },
                    new Campaign
                    {
                        Title = "Farm Fresh Orchard",
                        Description = "Daily picked fresh fruits and crisp vegetables at great prices!",
                        ImageUrl = "manav_banner.jpg",
                        BadgeText = "FRESH",
                        SecondaryBadgeText = "Direct from Farms",
                        ProductCountText = "35+ Products",
                        IsActive = true,
                        EndDate = DateTime.Now.AddDays(30)
                    },
                    new Campaign
                    {
                        Title = "Snack Mania Weekend",
                        Description = "Buy 2 get 1 free on selected chips, chocolates, and treats!",
                        ImageUrl = "cips_banner.jpg",
                        BadgeText = "BUY 2 GET 1",
                        SecondaryBadgeText = "Weekend Special",
                        ProductCountText = "40+ Products",
                        IsActive = true,
                        EndDate = DateTime.Now.AddDays(30)
                    }
                };
                await _db.InsertAllAsync(campaigns);
            }
        }

        private async Task SeedDiscountsAsync()
        {
            var count = await _db.Table<Discount>().CountAsync();
            if (count == 0)
            {
                var discounts = new List<Discount>
                {
                    new Discount
                    {
                        UserId = 1,
                        Code = "WELCOME20",
                        Title = "$20 Welcome Discount",
                        Description = "$20 off your order over $100",
                        DiscountAmount = 20.00m,
                        BackgroundImageUrl = "kahvalti_banner.jpg",
                        IsActive = true,
                        ExpirationDate = DateTime.Now.AddDays(30)
                    },
                    new Discount
                    {
                        UserId = 1,
                        Code = "QUICK50",
                        Title = "$50 Mega Discount",
                        Description = "$50 instant savings on orders over $250",
                        DiscountAmount = 50.00m,
                        BackgroundImageUrl = "manav_banner.jpg",
                        IsActive = true,
                        ExpirationDate = DateTime.Now.AddDays(15)
                    },
                    new Discount
                    {
                        UserId = 1,
                        Code = "FRESH15",
                        Title = "$15 Fresh Market Coupon",
                        Description = "$15 off any fruits and veggies order",
                        DiscountAmount = 15.00m,
                        BackgroundImageUrl = "cips_banner.jpg",
                        IsActive = true,
                        ExpirationDate = DateTime.Now.AddDays(7)
                    }
                };
                await _db.InsertAllAsync(discounts);
            }
        }

        private async Task SeedAddressesAsync()
        {
            var count = await _db.Table<Address>().CountAsync();
            if (count == 0)
            {
                var addresses = new List<Address>
                {
                    new Address
                    {
                        UserId = 1,
                        Title = "Home",
                        CityAndDistrict = "Manhattan, New York",
                        FullAddress = "742 Evergreen Terrace, Apt 4B",
                        Building = "Maple Heights",
                        Floor = "4",
                        Apartment = "4B",
                        IsActive = true
                    },
                    new Address
                    {
                        UserId = 1,
                        Title = "Office",
                        CityAndDistrict = "Downtown, New York",
                        FullAddress = "100 Broadway St, Floor 12",
                        Building = "Empire Tower",
                        Floor = "12",
                        Apartment = "Suite 1204",
                        IsActive = false
                    }
                };
                await _db.InsertAllAsync(addresses);
            }
        }

        private async Task SeedPaymentMethodsAsync()
        {
            var count = await _db.Table<PaymentMethod>().CountAsync();
            if (count == 0)
            {
                var methods = new List<PaymentMethod>
                {
                    new PaymentMethod
                    {
                        UserId = 1,
                        CardName = "Personal Debit",
                        MaskedNumber = "•••• 4242",
                        ExpiryDate = "12/28",
                        CardType = "Mastercard",
                        IsDefault = true
                    },
                    new PaymentMethod
                    {
                        UserId = 1,
                        CardName = "Corporate Visa",
                        MaskedNumber = "•••• 8899",
                        ExpiryDate = "09/27",
                        CardType = "Visa",
                        IsDefault = false
                    }
                };
                await _db.InsertAllAsync(methods);
            }
        }

        private async Task SeedFavoritesAsync()
        {
            var count = await _db.Table<Favorite>().CountAsync();
            if (count == 0)
            {
                var favorites = new List<Favorite>
                {
                    new Favorite { UserId = 1, ProductId = 101 },
                    new Favorite { UserId = 1, ProductId = 1 },
                    new Favorite { UserId = 1, ProductId = 9 }
                };
                await _db.InsertAllAsync(favorites);
            }
        }

        public async Task<User> GetDefaultUserAsync()
        {
            await InitAsync();
            var user = await _db.Table<User>().FirstOrDefaultAsync();
            if (user == null)
            {
                await SeedDefaultUserAsync();
                user = await _db.Table<User>().FirstOrDefaultAsync();
            }
            return user;
        }

        public async Task<List<CategoryFilter>> GetFiltersAsync()
        {
            await InitAsync();
            return await _db.Table<CategoryFilter>().ToListAsync();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            await InitAsync();
            return await _db.Table<Category>().ToListAsync();
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            await InitAsync();
            return await _db.Table<Product>().ToListAsync();
        }

        public async Task<bool> RegisterUserAsync(User newUser)
        {
            try
            {
                await InitAsync();
                var existingUser = await _db.Table<User>().Where(u => u.Email == newUser.Email).FirstOrDefaultAsync();
                if (existingUser != null)
                    return false;

                var result = await _db.InsertAsync(newUser);
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User> LoginUserAsync(string email, string password)
        {
            await InitAsync();
            return await _db.Table<User>()
                            .Where(u => u.Email == email && u.Password == password)
                            .FirstOrDefaultAsync();
        }

        public async Task AddToCartAsync(int userId, int productId)
        {
            await InitAsync();
            var existingItem = await _db.Table<CartItem>()
                                        .Where(c => c.UserId == userId && c.ProductId == productId)
                                        .FirstOrDefaultAsync();

            if (existingItem != null)
            {
                existingItem.Quantity++;
                await _db.UpdateAsync(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = 1
                };
                await _db.InsertAsync(newItem);
            }
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            await InitAsync();
            var existingItem = await _db.Table<CartItem>()
                                        .Where(c => c.UserId == userId && c.ProductId == productId)
                                        .FirstOrDefaultAsync();

            if (existingItem != null)
            {
                if (existingItem.Quantity > 1)
                {
                    existingItem.Quantity--;
                    await _db.UpdateAsync(existingItem);
                }
                else
                {
                    await _db.DeleteAsync(existingItem);
                }
            }
        }

        public async Task DeleteFromCartAsync(int cartItemId)
        {
            await InitAsync();
            await _db.DeleteAsync<CartItem>(cartItemId);
        }

        public async Task ClearCartAsync(int userId)
        {
            await InitAsync();
            var itemsToDelete = await _db.Table<CartItem>().Where(c => c.UserId == userId).ToListAsync();
            foreach (var item in itemsToDelete)
            {
                await _db.DeleteAsync(item);
            }
        }

        public async Task<List<CartItemDisplayModel>> GetCartItemsAsync(int userId)
        {
            await InitAsync();
            var cartItems = await _db.Table<CartItem>().Where(c => c.UserId == userId).ToListAsync();
            var allProducts = await _db.Table<Product>().ToListAsync();

            var displayItems = new List<CartItemDisplayModel>();

            foreach (var cartItem in cartItems)
            {
                var product = allProducts.FirstOrDefault(p => p.ProductId == cartItem.ProductId);
                if (product != null)
                {
                    displayItems.Add(new CartItemDisplayModel
                    {
                        CartItemId = cartItem.CartItemId,
                        ProductId = product.ProductId,
                        Name = product.Name,
                        Brand = product.Brand,
                        Unit = product.Unit,
                        Image = product.MainImage,
                        Price = product.Price,
                        OldPrice = product.OldPrice,
                        HasDiscount = product.HasDiscount,
                        Quantity = cartItem.Quantity
                    });
                }
            }

            displayItems.Reverse();
            return displayItems;
        }

        public async Task<Discount> GetDiscountByCodeAsync(string code)
        {
            await InitAsync();
            if (string.IsNullOrWhiteSpace(code)) return null;

            string cleanCode = code.Trim().ToUpper();
            return await _db.Table<Discount>()
                            .Where(d => d.Code.ToUpper() == cleanCode)
                            .FirstOrDefaultAsync();
        }

        public async Task<bool> PlaceOrderAsync(int userId, decimal grandTotal)
        {
            await InitAsync();
            var cartItems = await GetCartItemsAsync(userId);
            if (!cartItems.Any()) return false;

            string summary = string.Join(", ", cartItems.Select(x => x.Name).Take(3));
            if (cartItems.Count > 3) summary += " ...";

            var imageList = cartItems.Where(x => !string.IsNullOrEmpty(x.Image)).Select(x => x.Image).ToList();
            string imagesCsv = string.Join(",", imageList);

            var newOrder = new Order
            {
                OrderNumber = "#QD-" + new Random().Next(10000, 99999),
                UserId = userId,
                OrderDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                TotalAmount = grandTotal,
                TotalItemsCount = cartItems.Sum(x => x.Quantity),
                OrderSummary = summary,
                ProductImagesCsv = imagesCsv,
                Status = "Active",
                EstimatedDeliveryTime = "~12 min"
            };

            await _db.InsertAsync(newOrder);

            foreach (var item in cartItems)
            {
                await _db.InsertAsync(new OrderItem
                {
                    OrderId = newOrder.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    Subtotal = item.Subtotal
                });
            }

            await ClearCartAsync(userId);
            return true;
        }

        public async Task<List<Order>> GetOrdersAsync(int userId)
        {
            await InitAsync();
            return await _db.Table<Order>()
                            .Where(o => o.UserId == userId)
                            .OrderByDescending(o => o.OrderDate)
                            .ToListAsync();
        }

        public async Task<List<Address>> GetAddressesAsync(int userId)
        {
            await InitAsync();
            return await _db.Table<Address>().Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task<List<Favorite>> GetFavoritesAsync(int userId)
        {
            await InitAsync();
            return await _db.Table<Favorite>().Where(f => f.UserId == userId).ToListAsync();
        }

        public async Task<List<PaymentMethod>> GetPaymentMethodsAsync(int userId)
        {
            await InitAsync();
            return await _db.Table<PaymentMethod>().Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task<List<Discount>> GetAllDiscountsAsync()
        {
            await InitAsync();
            return await _db.Table<Discount>().ToListAsync();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            await InitAsync();
            return await _db.Table<Product>().ToListAsync();
        }

        public async Task<List<OrderItem>> GetOrderItemsForUserAsync(int userId)
        {
            await InitAsync();
            var orders = await GetOrdersAsync(userId);
            var orderIds = orders.Select(o => o.Id).ToList();

            var allItems = await _db.Table<OrderItem>().ToListAsync();
            return allItems.Where(i => orderIds.Contains(i.OrderId)).ToList();
        }

        public async Task<int> AddAddressAsync(Address address)
        {
            await InitAsync();
            return await _db.InsertAsync(address);
        }

        public async Task<int> UpdateAddressAsync(Address address)
        {
            await InitAsync();
            return await _db.UpdateAsync(address);
        }

        public async Task<int> DeleteAddressAsync(Address address)
        {
            await InitAsync();
            return await _db.DeleteAsync(address);
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            await InitAsync();
            return await _db.Table<Category>().ToListAsync();
        }

        public async Task<int> UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            await InitAsync();
            return await _db.UpdateAsync(paymentMethod);
        }

        public async Task<int> AddDiscountAsync(Discount discount)
        {
            await InitAsync();
            return await _db.InsertAsync(discount);
        }

        public async Task<List<Campaign>> GetActiveCampaignsAsync()
        {
            await InitAsync();
            return await _db.Table<Campaign>().Where(c => c.IsActive).ToListAsync();
        }

        public async Task<List<Discount>> GetUserDiscountsAsync(int userId)
        {
            await InitAsync();
            return await _db.Table<Discount>().Where(d => d.UserId == userId && d.IsActive).ToListAsync();
        }

        public async Task AddCartItemAsync(CartItem item)
        {
            await InitAsync();
            var existingItem = await _db.Table<CartItem>()
                .Where(x => x.UserId == item.UserId && x.ProductId == item.ProductId)
                .FirstOrDefaultAsync();

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
                await _db.UpdateAsync(existingItem);
            }
            else
            {
                await _db.InsertAsync(item);
            }
        }

        public async Task<List<FavoriteProduct>> GetUserFavoritesAsync(int userId)
        {
            await InitAsync();
            var favoriteRecords = await _db.Table<Favorite>().Where(f => f.UserId == userId).ToListAsync();
            var favoriteProducts = new List<FavoriteProduct>();

            foreach (var fav in favoriteRecords)
            {
                var product = await _db.Table<Product>().Where(p => p.ProductId == fav.ProductId).FirstOrDefaultAsync();

                if (product != null)
                {
                    var category = await _db.Table<Category>().Where(c => c.CategoryId == product.CategoryId).FirstOrDefaultAsync();
                    string catName = category != null ? category.Name : "Other";

                    decimal currentPrice = product.Price;
                    decimal? oldPrice = product.OldPrice;
                    string badgeTxt = "Favorite";
                    string badgeClr = "#f97316";
                    string badgeTextClr = "#ffffff";

                    if (product.HasDiscount && product.DiscountPercentage.HasValue)
                    {
                        badgeTxt = $"-{product.DiscountPercentage.Value}%";
                        badgeClr = "#ea580c";
                    }

                    favoriteProducts.Add(new FavoriteProduct
                    {
                        Id = product.ProductId,
                        CategoryId = product.CategoryId, 
                        CategoryName = catName,          
                        Name = product.Name,
                        Price = currentPrice,
                        OldPrice = oldPrice,
                        ImageUrl = product.MainImage,
                        Brand = string.IsNullOrEmpty(product.Brand) ? "QUICKDROP" : product.Brand,
                        BadgeText = badgeTxt,
                        BadgeColor = badgeClr,
                        BadgeTextColor = badgeTextClr
                    });
                }
            }

            return favoriteProducts;
        }

        public async Task<int> RemoveFavoriteAsync(int userId, int productId)
        {
            await InitAsync();
            var favorite = await _db.Table<Favorite>().Where(f => f.UserId == userId && f.ProductId == productId).FirstOrDefaultAsync();

            if (favorite != null)
            {
                return await _db.DeleteAsync(favorite);
            }
            return 0;
        }

        public async Task<int> AddFavoriteAsync(Favorite favorite)
        {
            await InitAsync();
            return await _db.InsertAsync(favorite);
        }

        public async Task ToggleFavoriteAsync(int userId, int productId)
        {
            await InitAsync();
            var existingFav = await _db.Table<Favorite>()
                .Where(f => f.UserId == userId && f.ProductId == productId)
                .FirstOrDefaultAsync();

            if (existingFav != null)
            {
                await RemoveFavoriteAsync(userId, productId);
            }
            else
            {
                await AddFavoriteAsync(new Favorite
                {
                    UserId = userId,
                    ProductId = productId
                });
            }
        }

        public async Task DecreaseCartItemAsync(int userId, int productId)
        {
            await InitAsync();
            var existingItem = await _db.Table<CartItem>()
                .Where(x => x.UserId == userId && x.ProductId == productId)
                .FirstOrDefaultAsync();

            if (existingItem != null)
            {
                if (existingItem.Quantity > 1)
                {
                    existingItem.Quantity -= 1;
                    await _db.UpdateAsync(existingItem);
                }
                else
                {
                    await _db.DeleteAsync(existingItem);
                }
            }
        }

        // ==========================================
        // ADMIN & VENDOR MANAGEMENT METHODS
        // ==========================================
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            await InitAsync();
            return await _db.Table<Order>().OrderByDescending(o => o.Id).ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            await InitAsync();
            var order = await _db.Table<Order>().Where(o => o.Id == orderId).FirstOrDefaultAsync();
            if (order != null)
            {
                order.Status = newStatus;
                await _db.UpdateAsync(order);
                return true;
            }
            return false;
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            await InitAsync();
            var res = await _db.InsertAsync(product);
            return res > 0;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            await InitAsync();
            var res = await _db.UpdateAsync(product);
            return res > 0;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            await InitAsync();
            var prod = await _db.Table<Product>().Where(p => p.ProductId == productId).FirstOrDefaultAsync();
            if (prod != null)
            {
                await _db.DeleteAsync(prod);
                return true;
            }
            return false;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            await InitAsync();
            return await _db.Table<User>().ToListAsync();
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            await InitAsync();
            var user = await _db.Table<User>().Where(u => u.UserId == userId).FirstOrDefaultAsync();
            if (user != null)
            {
                user.Role = newRole;
                await _db.UpdateAsync(user);
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            await InitAsync();
            return await _db.UpdateAsync(user) > 0;
        }

        private async Task SeedReviewsAsync()
        {
            var count = await _db.Table<Review>().CountAsync();
            if (count == 0)
            {
                var reviews = new List<Review>
                {
                    new Review
                    {
                        OrderId = 1,
                        OrderNumber = "ORD-89412",
                        CustomerUserId = 1,
                        CustomerName = "John Doe",
                        RiderRating = 5,
                        RiderComment = "Arrived in under 15 minutes! Very friendly and polite courier.",
                        RiderName = "Alex Rivers",
                        FoodQualityRating = 5,
                        FoodQualityComment = "Strawberries and avocados were super fresh and perfectly ripe!",
                        VendorName = "QuickDrop Fresh",
                        CreatedAt = DateTime.Now.AddDays(-2)
                    },
                    new Review
                    {
                        OrderId = 2,
                        OrderNumber = "ORD-78190",
                        CustomerUserId = 1,
                        CustomerName = "Sarah Jenkins",
                        RiderRating = 4,
                        RiderComment = "Fast delivery, neat thermal bag handling.",
                        RiderName = "Alex Rivers",
                        FoodQualityRating = 3,
                        FoodQualityComment = "Bakery bread was good, but the dairy box was slightly crushed on one side.",
                        VendorName = "QuickDrop Bakery & Dairy",
                        CreatedAt = DateTime.Now.AddDays(-1)
                    },
                    new Review
                    {
                        OrderId = 3,
                        OrderNumber = "ORD-65123",
                        CustomerUserId = 2,
                        CustomerName = "Michael Brown",
                        RiderRating = 5,
                        RiderComment = "Super courteous courier, checked address carefully.",
                        RiderName = "Alex Rivers",
                        FoodQualityRating = 5,
                        FoodQualityComment = "Meat and cold beverages were ice-cold as promised!",
                        VendorName = "QuickDrop Fresh",
                        CreatedAt = DateTime.Now.AddHours(-5)
                    }
                };
                await _db.InsertAllAsync(reviews);
            }
        }

        public async Task<List<Review>> GetAllReviewsAsync()
        {
            await InitAsync();
            return await _db.Table<Review>().OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<Review> GetReviewForOrderAsync(int orderId)
        {
            await InitAsync();
            return await _db.Table<Review>().Where(r => r.OrderId == orderId).FirstOrDefaultAsync();
        }

        public async Task<bool> SaveReviewAsync(Review review)
        {
            await InitAsync();
            var existing = await _db.Table<Review>().Where(r => r.OrderId == review.OrderId).FirstOrDefaultAsync();
            if (existing != null)
            {
                review.Id = existing.Id;
                var res = await _db.UpdateAsync(review);
                return res > 0;
            }
            else
            {
                var res = await _db.InsertAsync(review);
                return res > 0;
            }
        }
    }
}