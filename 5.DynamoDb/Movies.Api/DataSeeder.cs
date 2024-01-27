using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace Movies.Api;

public class DataSeeder
{
    public async Task ImportDataAsync()
    {
        var dynamoDb = new AmazonDynamoDBClient();
        var lines = await File.ReadAllLinesAsync("./movies.csv");
        for (int rIdx = 1; rIdx < lines.Length; rIdx++) // rIdx = 0 is header (Skip it)
        {
            var line = lines[rIdx];
            var commaSplit = line.Split(',');
            var title = commaSplit[0];
            var year = int.Parse(commaSplit[1]);
            var ageRestriction = int.Parse(commaSplit[2]);
            var rottenTomatoes = int.Parse(commaSplit[3]);

            var movie = new MovieYearTitle
            {
                Id = Guid.NewGuid(),
                Title = title,
                AgeRestriction = ageRestriction,
                ReleaseYear = year,
                RottenTomatoesPercentage = rottenTomatoes
            };
            
            var movieAsJson = JsonSerializer.Serialize(movie);
            var itemAsDocument = Document.FromJson(movieAsJson);
            var itemAsAttributes = itemAsDocument.ToAttributeMap();

            var createItemRequest = new PutItemRequest
            {
                TableName = "movies",
                Item = itemAsAttributes
            };

            var response = await dynamoDb.PutItemAsync(createItemRequest);
            await Task.Delay(300);
        }
    }
}
