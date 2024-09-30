using System.Text.Json;

using HttpClient client = new();
client.DefaultRequestHeaders.Add("User-Agent", "Me - learning REST");

// Start
await ProcessCurrenciesAsync(client);
Console.WriteLine("Type in the amount in Euro(€) - e.g. 12,34");
Console.Write("> ");
string? inputStr = Console.ReadLine();
float fromValue;

static async Task ProcessCurrenciesAsync(HttpClient client)
{
    var jsonString = await client.GetStringAsync("http://data.fixer.io/api/symbols?access_key=b29c4147dc4d701c6979e3ed57a81997");
    if (!string.IsNullOrEmpty(jsonString))
        ListCurrencies(jsonString);
}

/**
 * <summary>
 * Lists all 'symbols' of the JSON-string
 * </summary>
 * <param name="jsonString"></param>
 */
static void ListCurrencies(string jsonString)
{
    JsonDocument json = JsonDocument.Parse(jsonString);
    JsonElement root = json.RootElement;
    JsonElement symbols = root.GetProperty("symbols");
    foreach (JsonProperty prop in symbols.EnumerateObject())
    {
        Console.WriteLine($"{prop.Name}\t - {prop.Value}");
    }
}

try
{
    if (!string.IsNullOrWhiteSpace(inputStr))
    {
        fromValue = float.Parse(inputStr);
        Console.WriteLine($"Your Value is {fromValue}");
    }
    else
    {
        Console.WriteLine("WARNING - Please enter a value.");
    }
}
catch (FormatException)
{
    Console.WriteLine("ERROR - Please enter a value in the given format.");
}