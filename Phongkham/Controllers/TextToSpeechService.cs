using System;
using System.Net.Http;
using System.Threading.Tasks;

public class TextToSpeechService
{
    private static readonly HttpClient client = new HttpClient();

    public async Task<string> ConvertTextToSpeechAsync(string text)
    {
        // Thay YOUR_API_KEY bằng API Key của bạn
        string apiKey = "xAJjpFBx";
        string url = $"https://code.responsivevoice.org/getvoice.php?t={Uri.EscapeDataString(text)}&lang=vi&key={apiKey}";

        try
        {
            // Gửi yêu cầu HTTP GET đến ResponsiveVoice API
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Lấy dữ liệu phản hồi
            string mp3Url = await response.Content.ReadAsStringAsync();

            // Kiểm tra nếu dữ liệu trả về là data URI
            if (mp3Url.StartsWith("data:audio/mp3;base64,"))
            {
                // Kiểm tra độ dài của data URI để tránh lỗi URI quá dài
                if (mp3Url.Length > 2048) // Điều chỉnh giới hạn độ dài URI tùy theo nhu cầu của bạn
                {
                    throw new Exception("The data URI is too long.");
                }
            }
            else
            {
                // Kiểm tra nếu dữ liệu trả về là một URL hợp lệ
                if (!Uri.IsWellFormedUriString(mp3Url, UriKind.Absolute))
                {
                    throw new Exception("The URL returned by the service is not valid.");
                }
            }

            return mp3Url;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Có lỗi xảy ra: {ex.Message}");
            return null;
        }
    }
}
