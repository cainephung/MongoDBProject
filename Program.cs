using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        Console.Write("Enter Instructor's User ID: ");
        string userId = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Enter Instructor's Password: ");
        string password = Console.ReadLine()?.Trim() ?? string.Empty;

        // Construct the MongoDB connection string using user-provided credentials
        string connectionString = $"mongodb+srv://{userId}:{password}@a03-movies.sok38.mongodb.net/?retryWrites=true&w=majority&appName=A03-Movies";

        MongoClient client;
        try
        {
            client = new MongoClient(connectionString);
            var database = client.GetDatabase("A03-Movies");

            // ✅ Check if connection works
            database.ListCollectionNames();  // This will throw an exception if credentials are invalid
            Console.WriteLine("\n✅ Connected to MongoDB successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n❌ Failed to connect to MongoDB. Check your credentials.");
            Console.WriteLine($"Error: {ex.Message}");
            return;  // Exit program if authentication fails
        }

        var fullMoviesCollection = client.GetDatabase("A03-Movies").GetCollection<BsonDocument>("fullmovies");

        while (true)
        {
            Console.WriteLine("\n=== MongoDB Movie Database ===");
            Console.WriteLine("1. List all movies with cast members");
            Console.WriteLine("2. Search movies by cast member");
            Console.WriteLine("3. Search movies by keyword in overview");
            Console.WriteLine("4. Exit");
            Console.Write("Choose an option: ");

            string choice = Console.ReadLine()?.Trim() ?? "0";

            switch (choice)
            {
                case "1":
                    ListAllMovies(fullMoviesCollection);
                    break;
                case "2":
                    SearchByCastMember(fullMoviesCollection);
                    break;
                case "3":
                    SearchByKeyword(fullMoviesCollection);
                    break;
                case "4":
                    Console.WriteLine("Exiting...");
                    return;
                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
        }
    }

    // ✅ List all movies with their cast members
    static void ListAllMovies(IMongoCollection<BsonDocument> fullMoviesCollection)
    {
        var movies = fullMoviesCollection.Find(new BsonDocument()).ToList();

        if (movies.Count == 0)
        {
            Console.WriteLine("\nNo movies found in the database!");
            return;
        }

        Console.WriteLine("\n=== Movie List ===");
        foreach (var movie in movies)
        {
            string title = movie.GetValue("title", "Unknown Title").ToString();
            Console.WriteLine($"🎬 Movie: {title}");

            if (movie.Contains("cast") && movie["cast"].IsBsonArray)
            {
                Console.WriteLine("👥 Cast:");
                foreach (var cast in movie["cast"].AsBsonArray)
                {
                    string castName = cast.AsBsonDocument.GetValue("name", "Unknown").ToString();
                    Console.WriteLine($"   🎭 {castName}");
                }
            }
            else
            {
                Console.WriteLine("👥 No cast available.");
            }
            Console.WriteLine("----------------------------");
        }
    }

    // ✅ Search movies by cast member
    static void SearchByCastMember(IMongoCollection<BsonDocument> fullMoviesCollection)
    {
        Console.Write("\nEnter cast member name: ");
        string castName = Console.ReadLine()?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(castName))
        {
            Console.WriteLine("Invalid input. Try again.");
            return;
        }

        var filter = Builders<BsonDocument>.Filter.ElemMatch("cast", Builders<BsonDocument>.Filter.Eq("name", castName));
        var movies = fullMoviesCollection.Find(filter).ToList();

        if (movies.Count == 0)
        {
            Console.WriteLine($"\n❌ No movies found featuring {castName}.");
            return;
        }

        Console.WriteLine($"\n🎭 Movies featuring {castName}:");
        foreach (var movie in movies)
        {
            string title = movie.GetValue("title", "Unknown Title").ToString();
            Console.WriteLine($"- {title}");
        }
    }

    // ✅ Search movies by keyword in the "overview" field
    static void SearchByKeyword(IMongoCollection<BsonDocument> fullMoviesCollection)
    {
        Console.Write("\nEnter keyword: ");
        string keyword = Console.ReadLine()?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(keyword))
        {
            Console.WriteLine("Invalid input. Try again.");
            return;
        }

        var filter = Builders<BsonDocument>.Filter.Regex("overview", new BsonRegularExpression(keyword, "i"));
        var movies = fullMoviesCollection.Find(filter).ToList();

        if (movies.Count == 0)
        {
            Console.WriteLine($"\n❌ No movies found matching keyword '{keyword}'.");
            return;
        }

        Console.WriteLine($"\n🔍 Movies matching keyword '{keyword}':");
        foreach (var movie in movies)
        {
            string title = movie.GetValue("title", "Unknown Title").ToString();
            string overview = movie.GetValue("overview", "No overview available.").ToString();
            Console.WriteLine($"   🎬 {title}: {overview}");
        }
    }
}
