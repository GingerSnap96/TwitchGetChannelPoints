using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program
{
    static string redirectUri = "http://localhost";
    static string accessToken = "";

    static async Task Main(string[] args)
    {
        // Read configuration from JSON file
        var config = ReadConfigFromJson();
        if (config == null)
        {
            Console.WriteLine("Failed to read configuration from JSON file.");
            return;
        }

        // Use config values
        var clientId = config.ClientId;
        var clientSecret = config.ClientSecret;
        var authtoken = config.AuthToken;

        // Perform OAuth flow if access token is not available
        if (string.IsNullOrEmpty(accessToken))
        {
            accessToken = await PerformOAuthFlowAsync(clientId, clientSecret);
        }

        if (!string.IsNullOrEmpty(accessToken))
        {
            Console.Write("Enter the Username you want to get channel points for: ");
            var username = Console.ReadLine();
            var userId = await GetUserIdAsync(username, clientId);

            if (!string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("Fetching followed channels for user: " + username);
                var followedChannels = await GetAllFollowedChannelsAsync(userId, clientId);
                var dateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // Get current date-time for file name
                var csvFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", $"{username}_ChannelPoints_{dateTimeNow}.csv");

                var csvRows = new List<string>(); // To store CSV rows

                Console.WriteLine("Fetching channel points for " + followedChannels.Count.ToString() + " followed streams... Please wait.");
                Console.WriteLine("Once finished the following file will be produced: " + csvFilePath);

                foreach (var followedChannel in followedChannels)
                {
                    var channelName = followedChannel.ToName;
                    // Capture the console output using a StringWriter
                    using (var consoleOutput = new StringWriter())
                    {
                        Console.SetOut(consoleOutput);
                        await GetChannelPointsForChannel(authtoken, channelName.ToString());
                        var channelPointsOutput = consoleOutput.ToString().Trim(); // Get the captured output

                        // Create CSV row data
                        var csvRow = $"{channelPointsOutput}";
                        csvRows.Add(csvRow); // Add to the list of CSV rows
                    }
                }

                // Create and write the CSV file
                File.WriteAllLines(csvFilePath, csvRows);
            }
        }
    }

    // Read configuration values from JSON file
    static Config ReadConfigFromJson()
    {
        var configFilePath = "config.json";
        try
        {
            var jsonConfig = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<Config>(jsonConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading configuration from JSON file: {ex.Message}");
            return null;
        }
    }

    // Save configuration values to JSON file
    static void SaveConfigToJson(Config config)
    {
        var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        try
        {
            var jsonConfig = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFilePath, jsonConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration to JSON file: {ex.Message}");
        }
    }

    static async Task<string> PerformOAuthFlowAsync(string clientId, string clientSecret)
    {
        // Step 1: Redirect user to Twitch for authorization
        string authorizationUrl = $"https://id.twitch.tv/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope=user:read:follows+channel:read:redemptions";
        Console.WriteLine($"Please visit the following URL and grant permission: {authorizationUrl}");
        Console.WriteLine("After granting permission, you'll be redirected to your redirect URL.");

        // Step 2: User is redirected back to the redirect URL with a code query parameter
        Console.Write("Enter the code query parameter from the redirected URL: ");
        string authorizationCode = Console.ReadLine();

        // Step 3: Exchange authorization code for access token
        using (var httpClient = new HttpClient())
        {
            var tokenRequestData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            };

            var tokenRequest = new FormUrlEncodedContent(tokenRequestData);

            var tokenResponse = await httpClient.PostAsync("https://id.twitch.tv/oauth2/token", tokenRequest);
            if (tokenResponse.IsSuccessStatusCode)
            {
                string jsonResponse = await tokenResponse.Content.ReadAsStringAsync();
                OAuthTokenResponse tokenData = JsonConvert.DeserializeObject<OAuthTokenResponse>(jsonResponse);
                return tokenData.AccessToken;
            }
            else
            {
                Console.WriteLine($"Failed to exchange authorization code for access token: {tokenResponse.ReasonPhrase}");
                return null;
            }
        }
    }

    // Retrieve user ID based on the provided username
    static async Task<string> GetUserIdAsync(string username, string clientId)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await httpClient.GetAsync($"https://api.twitch.tv/helix/users?login={username}");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var userData = JObject.Parse(jsonResponse)["data"] as JArray;
                if (userData != null && userData.Count > 0)
                {
                    return userData[0]["id"].ToString();
                }
                else
                {
                    Console.WriteLine($"User with login '{username}' not found.");
                    return null;
                }
            }
            else
            {
                Console.WriteLine($"Failed to fetch user info: {response.ReasonPhrase}");
                return null;
            }
        }
    }

    // Retrieve all followed channels for a given user ID
    static async Task<List<FollowedChannel>> GetAllFollowedChannelsAsync(string userId, string clientId)
    {
        var allFollowedChannels = new List<FollowedChannel>();
        string cursor = null;

        do
        {
            var (channels, newCursor) = await GetFollowedChannelsAsync(userId, cursor, clientId);
            if (channels != null)
            {
                allFollowedChannels.AddRange(channels);
            }
            cursor = newCursor;

        } while (!string.IsNullOrEmpty(cursor));

        return allFollowedChannels;
    }

    // Retrieve followed channels using pagination
    static async Task<(List<FollowedChannel> channels, string newCursor)> GetFollowedChannelsAsync(string userId, string cursor, string clientId)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var requestUrl = $"https://api.twitch.tv/helix/users/follows?from_id={userId}";
            if (!string.IsNullOrEmpty(cursor))
            {
                requestUrl += $"&after={cursor}";
            }

            var response = await httpClient.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var channels = ParseFollowedChannels(jsonResponse);
                var newCursor = GetNewCursor(jsonResponse);
                return (channels, newCursor);
            }
            else
            {
                Console.WriteLine($"Failed to fetch followed channels: {response.ReasonPhrase}");
                return (null, null);
            }
        }
    }

    // Parse followed channels from JSON response
    static List<FollowedChannel> ParseFollowedChannels(string json)
    {
        var jsonObject = JObject.Parse(json);
        var dataArray = jsonObject["data"] as JArray;

        var followedChannels = new List<FollowedChannel>();
        foreach (var item in dataArray)
        {
            var channel = new FollowedChannel
            {
                ToId = item["to_id"].ToString(),
                ToLogin = item["to_login"].ToString(),
                ToName = item["to_name"].ToString(),
                FollowedAt = DateTime.Parse(item["followed_at"].ToString())
            };
            followedChannels.Add(channel);
        }

        return followedChannels;
    }

    // Get the new cursor for pagination
    static string GetNewCursor(string json)
    {
        var jsonObject = JObject.Parse(json);
        var pagination = jsonObject["pagination"];
        if (pagination != null)
        {
            return pagination["cursor"]?.ToString();
        }
        return null;
    }

    // Retrieve and print channel points data for a channel
    static async Task GetChannelPointsForChannel(string oauthToken, string channelName)
    {
        // Define headers for the HTTP request
        var headers = new Dictionary<string, string>
        {
            { "Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko" },
            { "Authorization", $"OAuth {oauthToken}" }
        };

        // Fetch channel points data using a custom function
        var channelPointsData = await FetchData("https://gql.twitch.tv/gql", GetChannelPointsQuery(channelName), headers);
        if (channelPointsData != null)
        {
            var community = channelPointsData["data"]?["community"] as JObject;
            if (community != null)
            {
                var channelData = community["channel"] as JObject;
                if (channelData != null)
                {
                    var selfData = channelData["self"] as JObject;
                    if (selfData != null)
                    {
                        var communityPoints = selfData["communityPoints"];
                        if (communityPoints != null)
                        {
                            // Extract display name and points balance
                            var displayName = community["displayName"];
                            var points = communityPoints["balance"];
                            Console.WriteLine($"{displayName},{points}");
                        }
                        else
                        {
                            Console.WriteLine(channelName + ",Unable to fetch channel points: Community points data is missing in the response.");
                        }
                    }
                    else
                    {
                        Console.WriteLine(channelName + ",Unable to fetch channel points: Self data is missing in the response.");
                    }
                }
                else
                {
                    Console.WriteLine(channelName + ",Unable to fetch channel points: Channel data is missing in the response.");
                }
            }
            else
            {
                Console.WriteLine(channelName + ",Unable to fetch channel points: Community data is missing in the response.");
            }
        }
    }

    // Fetch data from a specified URL using HTTP POST
    static async Task<dynamic> FetchData(string url, string query, Dictionary<string, string> headers)
    {
        using (var httpClient = new HttpClient())
        {
            // Set custom headers for the HTTP request
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            // Create and send the HTTP POST request
            var content = new StringContent(query, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content);

            // Handle the response and parse JSON data
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject(jsonResponse);
            }
            else
            {
                Console.WriteLine($"Error fetching data: {response.ReasonPhrase}");
                return null;
            }
        }
    }

    // Generate the GraphQL query to retrieve channel points data
    static string GetChannelPointsQuery(string channel)
    {
        return $@"{{
            ""operationName"": ""ChannelPointsContext"",
            ""variables"": {{
                ""channelLogin"": ""{channel}"",
                ""includeGoalTypes"": [
                    ""CREATOR"",
                    ""BOOST""
                ]
            }},
            ""extensions"": {{
                ""persistedQuery"": {{
                    ""version"": 1,
                    ""sha256Hash"": ""1530a003a7d374b0380b79db0be0534f30ff46e61cffa2bc0e2468a909fbc024""
                }}
            }}
        }}";
    }

}

// Configuration class to hold your JSON data
class Config
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string AuthToken { get; set; }
}

class FollowedChannel
{
    // Followed channel properties
    public string ToId { get; set; }
    public string ToLogin { get; set; }
    public string ToName { get; set; }
    public DateTime FollowedAt { get; set; }
}

class OAuthTokenResponse
{
    // OAuth token response properties
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
}

