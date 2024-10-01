using System.Text.Json;

internal class Program
{
    private const string AccessKey = "b29c4147dc4d701c6979e3ed57a81997";
    private static async Task Main()
    {
        HashSet<string> exitKeywords = ["q", "quit", "exit"];

        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", "Me - learning REST");
        Console.WriteLine("Usage:\t [Value] [From] [To]");

        string inputStr;
        while (true)
        {
            Console.Write("> ");
            inputStr = Console.ReadLine() ?? ""; // ?? null-coalescing operator --> returns left value when it(left value) is not-null, otherwise right
            inputStr = inputStr.ToLower().Trim();

            // exit condition
            if (exitKeywords.Contains(inputStr))
            {
                break;
            }

            // show help
            if (inputStr == "help")
            {
                Console.WriteLine("Possible currencies:");
                await ProcessCurrenciesAsync(client);
                continue;
            }

            // process args
            if (!string.IsNullOrWhiteSpace(inputStr))
            {
                string[] inputArgs = inputStr.Split();
            }
            else
            {
                Console.WriteLine("WARNING - Please enter a value.");
            }
        }

        float fromValue;


    }

    /**
    * <summary>
    * HTTP-request for currencies
    * </summary>
    * <param name="client"></param>
    * <returns></returns>
    */
    static async Task ProcessCurrenciesAsync(HttpClient client)
    {
        var jsonString = await client.GetStringAsync($"http://data.fixer.io/api/symbols?access_key={AccessKey}");
        if (!string.IsNullOrEmpty(jsonString))
            ListCurrencies(jsonString);
    }

    /**
    * <summary>
    * Prints all 'symbols' of the JSON-string aka currencies to terminal
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
            Console.WriteLine($"{prop.Name} - {prop.Value}");
        }
    }

    static float ParseInputValue(string value)
    {
        try
        {
            return float.Parse(value);
        }
        catch (FormatException)
        {
            Console.WriteLine("ERROR - Please enter a value in the given format.");
            throw;
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("WARNING - Please enter a value.");
            throw;
        }
    }
}