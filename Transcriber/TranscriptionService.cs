using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Transcriber
{
    internal class TranscriptionService
    {
        public static async Task<string> GetTranscriptionAsync(string url)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token.RevAiToken}");
            JObject json = JObject.Parse(@"{
        source_config: {
          url: '" + url + @"',
        },
        transcriber: 'machine',
        skip_diarization: false,
        skip_punctuation: false,
        skip_postprocessing: false,
        remove_disfluencies: false,
        filter_profanity: false,
        speaker_channel_count: 1,
        delete_after_seconds: 2592000,
        custom_vocabulary_id: null,
        language: 'da'
      }");
            var postData = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            var request = await client.PostAsync("https://api.rev.ai/speechtotext/v1/jobs", postData);
            var response = await request.Content.ReadAsStringAsync();

            JObject jsonResponse = JObject.Parse(response);
            string id = (string)jsonResponse["id"];

            string status;
            do
            {
                // Vent i 5 sekunder
                await Task.Delay(5000);
                client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token.RevAiToken}");
                request = await client.GetAsync("https://api.rev.ai/speechtotext/v1/jobs/" + id);
                response = await request.Content.ReadAsStringAsync();

                jsonResponse = JObject.Parse(response);
                status = (string)jsonResponse["status"];

                if (status == "failed")
                {
                    string failureDetail = (string)jsonResponse["failure_detail"];
                    throw new Exception("Transcription failed.\n" + failureDetail);
                }
            } while (status == "in_progress");

            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token.RevAiToken}");
            client.DefaultRequestHeaders.Add("Accept", "application/x-subrip");
            request = await client.GetAsync("https://api.rev.ai/speechtotext/v1/jobs/" + id + "/captions");
            response = await request.Content.ReadAsStringAsync();

            return response;
        }
    }
}

