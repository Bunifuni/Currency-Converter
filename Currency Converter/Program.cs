using System.Collections;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.RegularExpressions;

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
                float value = ParseInputValue(inputArgs[0]);
                float exchangeFactor = await GetExchangeFactor(inputArgs[1], inputArgs[2], client);
                Console.WriteLine($"{value * exchangeFactor} {inputArgs[2].ToUpper()}");
            }
            else
            {
                Console.WriteLine("WARNING - Please enter a value.");
            }
        }
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

    /**
     * <summary>
     * Parses string to float
     * </summary>
     * <param name="value">input string</param>
     * <returns>float value</returns>
     */
    static float ParseInputValue(string value)
    {
        try
        {
            return float.Parse(value);
        }
        catch (FormatException)
        {
            throw new FormatException("ERROR - Please enter a value in the given format.");
        }
    }

    /**
     * <summary>
     * Returns the exchange factor to multiply with for conversion.
     * </summary>
     * <param name="from">currency to convert</param>
     * <param name="to">result currency</param>
     * <param name="client"></param>
     * <returns></returns>
     */
    static async Task<float> GetExchangeFactor(string from, string to, HttpClient client)
    {
        from = from.ToUpper();
        to = to.ToUpper();
        string currencyPattern = @"^[A-Z][A-Z][A-Z]$";
        if (Regex.IsMatch(from, currencyPattern) && Regex.IsMatch(to, currencyPattern))
        {
            if (from != to)
            {
                Dictionary<string, float> rates = await GetExchangeRates(client);
                try
                {
                    float fromFactor = rates[from];
                    float toFactor = rates[to];
                    return (float)(Math.Pow(fromFactor, -1) * toFactor);
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException("ERROR - The entered 'from' or 'to' currency does not exist.");
                }
            }
            else
            {
                return 1.0F;
            }
        }
        else
        {
            throw new FormatException("ERROR - The entered 'from' or 'to' currency format is wrong.");
        }
    }

    /**
     * <summary>
     * Fetches current exchange rates from API and returns them as map.
     * </summary>
     * <param name="client"></param>
     * <returns>Dictionary<string, float></returns>
     */
    static async Task<Dictionary<string, float>> GetExchangeRates(HttpClient client)
    {
        string jsonString;
        try
        {
            jsonString = await client.GetStringAsync($"http://data.fixer.io/api/latest?access_key={AccessKey}");
        }
        catch (HttpRequestException)
        {
            throw new HttpRequestException("ERROR - Failed to fetch exchange rates from API.");
        }

        // Create dictionary
        Dictionary<string, float> exchangeRates = [];
        JsonDocument json = JsonDocument.Parse(jsonString);
        JsonElement rates = json.RootElement.GetProperty("rates");
        foreach (JsonProperty prop in rates.EnumerateObject())
        {
            exchangeRates.Add(prop.Name, prop.Value.GetSingle());
        }
        return exchangeRates;
    }
}