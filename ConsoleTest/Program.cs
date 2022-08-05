using System;
using System.Net.Http;
using System.Threading;

var url = args.Length == 1
    ? args[0]
    : "https://graph.microsoft.com/v1.0";

var client = new HttpClient();
client.Timeout = Timeout.InfiniteTimeSpan;
client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
var response = await client.GetAsync(url);

Console.WriteLine(await response.Content.ReadAsStringAsync());
Console.WriteLine("========================");
Console.WriteLine(response.ToString());
