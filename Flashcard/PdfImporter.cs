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

        private const string SystemPrompt = """
You are a flashcard extractor for exam reviewers.

Your goal is NOT to extract everything.
Your goal is to extract ONLY the MOST IMPORTANT concepts a student must study for exams.

SELECTION RULE (VERY IMPORTANT):
- Include only:
  - central definitions
  - key theories
  - major organizations
  - important authors or thinkers
  - core concepts repeatedly emphasized
- Do NOT include:
  - basic/common words
  - minor supporting ideas
  - obvious terms students already know
  - duplicate or similar concepts

Limit output to the MOST IMPORTANT ~15-20 flashcards maximum.

CONTENT RULES:
- Extract only from lesson/explanatory sections
- Ignore quizzes, activities, and references
- Combine broken lines into full definitions
- Do NOT invent or expand beyond the text

ACRONYM RULE:
Always format as:
Full Term (ACRONYM): definition

OUTPUT FORMAT:
keyword:definition
- one per line
- no numbering
- no extra text
""";

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

        public async Task<(bool Ok, string Response, string ErrorMessage)> SendToGroq(string pdfText)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return (false, "", "API key is missing. Please configure GROQ_API_KEY.");
            }

            try
            {
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

                var requestBody = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "system", content = SystemPrompt },
                        new { role = "user", content = "Here is the text:\n" + pdfText }
                    }
                };

                string json = JsonSerializer.Serialize(requestBody);
                var response = await client.PostAsync(
                    "https://api.groq.com/openai/v1/chat/completions",
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    return (false, "", "Could not generate flashcards right now. Please try again.");
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                return (true, responseJson, "");
            }
            catch
            {
                return (false, "", "Network error while importing. Check your internet and try again.");
            }
        }

        public List<Flashcard> ParseFlashcards(string groqResponse)
        {
            List<Flashcard> flashcards = new List<Flashcard>();

            try
            {
                using JsonDocument doc = JsonDocument.Parse(groqResponse);

                if (doc.RootElement.TryGetProperty("error", out _))
                {
                    return flashcards;
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
            }
            catch
            {
                return new List<Flashcard>();
            }

            return flashcards;
        }
    }
}