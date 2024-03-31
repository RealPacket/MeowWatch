using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

const string BASE_URL = "https://api.meow.camera";
var client = new HttpClient();
client.DefaultRequestHeaders.UserAgent.Clear();
client.DefaultRequestHeaders.UserAgent.Add(
	new ProductInfoHeaderValue("MeowWatch", "1.0.2-this-barely-gets-updated")
);
client.BaseAddress = new Uri(BASE_URL);
Console.WriteLine("What's the ID of the feeder you want to watch?");
Console.Write("> ");
var feederId = Console.ReadLine();

while (true)
{
	var response = await client.GetAsync($"/catHouse/{feederId}");
	if (response.StatusCode == HttpStatusCode.NotFound)
	{
		Console.WriteLine("Not a valid feeder, exiting in 4 seconds...");
		await Task.Delay(TimeSpan.FromSeconds(4));
		break;
	}
	if (response.StatusCode == HttpStatusCode.TooManyRequests)
	{
		var delay = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(13);
		Console.WriteLine($"Got rate limited... Waiting for {delay}");
		await Task.Delay(delay);
		continue;
	}
	var reset = response.Headers.GetValues("ratelimit-reset").FirstOrDefault();
	var rl_rem = response.Headers.GetValues("ratelimit-remaining").FirstOrDefault();
	var json = await response.Content.ReadFromJsonAsync<JsonElement>();
	#region variables
	var name = json.GetProperty("name").GetString();
	var englishName = "???";
	if (json.TryGetProperty("englishName", out var englishNameProperty))
	{
		englishName = englishNameProperty.GetString();
	}
	var subscribeCount = json.GetProperty("subscribeCount").GetInt32();
	var todayFeedCount = json.GetProperty("todayFeedCount").GetInt32();
	var todayShowCount = json.GetProperty("todayShowCount").GetInt32();
    var catPresent = json.GetProperty("catPresent").GetBoolean();
	var lightsOn = json.GetProperty("lightTurnedOn").GetBoolean();
    var tempC = json.GetProperty("deviceTemperatureCelsius").GetInt32();
	var stockObj = json.GetProperty("stock");
	var kibblePercent = stockObj.GetProperty("kibble").GetString();
	var hasSnacks = json.GetProperty("hasSnacks").GetBoolean();
	var snackPercent = hasSnacks ? stockObj.GetProperty("snack").GetString() : "None";
	var viewersObj = json.GetProperty("viewers");
	var localViewers = viewersObj.GetProperty("local").GetInt32();
	var appViewers = viewersObj.GetProperty("app").GetInt32();
	var totalViewers = localViewers + appViewers;
	#endregion
	Console.Clear();
	Console.WriteLine(new string('█', Console.WindowWidth));
	#region stuff
	Console.WriteLine($"Feeder name: {englishName} ({name})");
    Console.WriteLine($"Subscribe count: {subscribeCount}");
    Console.WriteLine($"Feed count for today: {todayFeedCount}");
	Console.WriteLine($"Visit count for today: {todayShowCount}");
    Console.WriteLine($"Cat present?: {catPresent}");
    Console.WriteLine($"Lights on?: {lightsOn}");
    Console.WriteLine($"Views:");
	Console.WriteLine($"    {appViewers} (JieMao)");
	Console.WriteLine($"    {localViewers} (meow.camera)");
	Console.WriteLine($"{totalViewers} (total viewers)");
	Console.WriteLine($"Temperature: {tempC}°C");
	Console.WriteLine("Stock(%):");
	Console.WriteLine($"Kibble/Food: {kibblePercent}");
	Console.WriteLine($"Snacks: {snackPercent}");
	#endregion
	Console.WriteLine(new string('█', Console.WindowWidth));
    #region Rate Limit garbage
    if (!double.TryParse(reset, out var wait))
	{
		Console.Error.WriteLine(
			$"Failed to parse ratelimit-reset header. Content: {reset}"
		);
		continue;
	}
	if (!double.TryParse(rl_rem, out var remaining))
	{
		Console.Error.WriteLine($"Failed to parse ratelimit-remaining header: {rl_rem}");
	}
	Console.WriteLine($"Remaining requests before waiting: {remaining}");
	if (remaining < 2)
	{
		var span = TimeSpan.FromSeconds(wait);
		Console.WriteLine($"Waiting for {span} to avoid rate limits");
		await Task.Delay(span);
	}
	#endregion
}