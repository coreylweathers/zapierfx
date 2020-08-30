using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using zapierfx.Models;

namespace zapierfx
{
    public static class Functions
    {
        private static HttpClient _httpClient = new HttpClient();
        
        [FunctionName("InboundFx")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route="metrics/{channel}")]
            HttpRequestMessage req, ILogger log, string channel)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            var formContent = await req.Content.ReadAsFormDataAsync();
            
            var dates = formContent["date"].Split(',');
            var entries = BuildMetricsEntriesList(dates, formContent);

            var validEntries = entries.Where(x => x.MinsStreamed > 0).ToList();

            foreach (var entry in validEntries)
            {
                log.LogInformation($"Logging entry: {entry}");
                await SendMetricToZapier(entry, channel);
            }

            return new NoContentResult();

        }

        private static List<MetricsEntry> BuildMetricsEntriesList(string[] dates, NameValueCollection formContent)
        {
            var entries = dates.Select(t => new MetricsEntry {Date = Convert.ToDateTime(t)}).ToList();

            foreach (var entry in entries)
            {
                var dateIndex = Array.IndexOf(formContent["date"].Split(','), entry.Date.ToString("ddd MMM dd yyyy"));

                entry.Chatters = Convert.ToInt32(formContent["chatters"].Split(',')[dateIndex]);
                entry.Followers = Convert.ToInt32(formContent["followers"].Split(',')[dateIndex]);
                entry.AdBreaks = Convert.ToInt32(formContent["ad_breaks"].Split(',')[dateIndex]);
                entry.AvgViewers = Convert.ToSingle(formContent["avg_viewers"].Split(',')[dateIndex]);
                entry.ClipViews = Convert.ToInt32(formContent["clip_views"].Split(',')[dateIndex]);
                entry.ClipsCreated = Convert.ToInt32(formContent["clips_created"].Split(',')[dateIndex]);
                entry.LiveViews = Convert.ToInt32(formContent["total_views"].Split(',')[dateIndex]);
                entry.MaxViewers = Convert.ToInt32(formContent["max_views"].Split(',')[dateIndex]);
                entry.MinsStreamed = Convert.ToInt32(formContent["mins_streamed"].Split(',')[dateIndex]);
                entry.MinsWatched = Convert.ToInt32(formContent["mins_watched"].Split(',')[dateIndex]);
                entry.UniqueViews = Convert.ToInt32(formContent["unique_viewers"].Split(',')[dateIndex]);
                entry.ChatMessages = Convert.ToInt32(formContent["chat_messages"].Split(',')[dateIndex]);
            }

            return entries;
        }

        private static async Task SendMetricToZapier(MetricsEntry entry, string channel)
        {
            entry.Channel = channel;
            var uri = "https://hooks.zapier.com/hooks/catch/3191324/oa68u9r/";
            var settings = new JsonSerializerSettings
            {
                DateFormatString =  "ddd MMM dd yyyy"
            };
            var json = JsonConvert.SerializeObject(entry, settings);

            var zapierResult = await _httpClient.PostAsJsonAsync(uri, json);
            zapierResult.EnsureSuccessStatusCode();
        }
    }
}