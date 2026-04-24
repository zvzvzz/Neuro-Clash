using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace FlashcardBattle
{
    public class PdfImporter
    {
        private readonly string apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? string.Empty;
        private static readonly HttpClient client = new HttpClient();

        public string ExtractText(string filePath)
        {
            string text = "";
            using (PdfDocument document = PdfDocument.Open(filePath))
            {
                foreach (var page in document.GetPages())
                {
                    text += page.Text;
                }
            }
            return text;
        }

        public async Task<string> SendToGroq(string pdfText)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Missing GROQ_API_KEY environment variable.");

            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a flashcard extractor for exam reviewers. Your goal is NOT to extract everything. Your goal is to extract ONLY the MOST IMPORTANT concepts a student must study for exams.\r\n-----------------------------------\r\nSELECTION RULE (VERY IMPORTANT):\r\n-----------------------------------\r\nOnly include terms that are:\r\n- central definitions\r\n- key theories\r\n- major organizations\r\n- important authors or thinkers\r\n- core concepts repeatedly emphasized\r\nDO NOT include:\r\n- basic/common words\r\n- minor supporting ideas\r\n- obvious terms students already know\r\n- duplicate or similar concepts\r\nLimit output to the MOST IMPORTANT ~15-20 flashcards maximum.\r\n-----------------------------------\r\nCONTENT RULES:\r\n-----------------------------------\r\n- Extract only from lesson/explanatory sections\r\n- Ignore quizzes, activities, and references\r\n- Combine broken lines into full definitions\r\n- Do NOT invent or expand beyond the text\r\n-----------------------------------\r\nACRONYM RULE:\r\n-----------------------------------\r\nAlways format as:\r\nFull Term (ACRONYM): definition\r\n-----------------------------------\r\nOUTPUT FORMAT:\r\n-----------------------------------\r\nkeyword:definition\r\n- one per line\r\n- no numbering\r\n- no extra text"
                    },
                    new
                    {
                        role = "user",
                        content = "Here is the text:\n" + pdfText
                    }
                }
            };

            string json = JsonSerializer.Serialize(requestBody);
            var response = await client.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            return responseJson;
        }

        public List<Flashcard> ParseFlashcards(string groqResponse)
        {
            List<Flashcard> flashcards = new List<Flashcard>();

            using JsonDocument doc = JsonDocument.Parse(groqResponse);

            if (doc.RootElement.TryGetProperty("error", out JsonElement error))
            {
                string errorMsg = error.GetProperty("message").GetString() ?? "Unknown API error";
                throw new Exception("Groq API error: " + errorMsg);
            }

            string content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            string[] lines = content.Split('\n');

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                int colonIndex = trimmed.IndexOf(':');
                if (colonIndex == -1) continue;

                string keyword = trimmed.Substring(0, colonIndex).Trim();
                string definition = trimmed.Substring(colonIndex + 1).Trim();

                if (!string.IsNullOrEmpty(keyword) && !string.IsNullOrEmpty(definition))
                {
                    flashcards.Add(new Flashcard(keyword, definition));
                }
            }

            return flashcards;
        }
    }
}