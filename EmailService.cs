using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Globalization;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    public EmailService(IConfiguration config, HttpClient http)
    {
        _config = config;
        _http = http;
    }

    private async Task SendEmail(string toEmail, string toName, string subject, string html)
    {
        try
        {
            var apiKey = _config["Brevo:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Brevo API key missing");

            var body = new
            {
                sender = new
                {
                    name = "Anna Rakhno",
                    email = "artvorozhbyt@gmail.com"
                },
                to = new[]
                {
                    new { email = toEmail, name = toName }
                },
                subject = subject,
                htmlContent = html
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.brevo.com/v3/smtp/email"
            );

            request.Headers.Add("api-key", apiKey);

            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("BREVO ERROR: " + responseText);
            }
            else
            {
                Console.WriteLine("EMAIL SUCCESS: " + toEmail);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("EMAIL ERROR: " + ex.ToString());
        }
    }

    public async Task SendBookingConfirmation(
        string toEmail,
        string name,
        string service,
        DateTime date)
    {
        var html = $@"
<div style='font-family: ""Playfair Display"", serif; background:#f5f2ed; padding:40px;'>

    <div style='max-width:600px; margin:auto; background:#ffffff; border-radius:16px; padding:40px; border:1px solid #e8dcc8;'>

        <div style='text-align:center; margin-bottom:30px;'>
            <h1 style='margin:0; font-size:28px; color:#c59d5f;'>Anna Rakhno</h1>
            <div style='letter-spacing:2px; font-size:12px; color:#999; margin-top:8px;'>
                LUXURY MAKEUP ATELIER
            </div>
        </div>

        <hr style='border:none; border-top:1px solid #eee; margin:30px 0;'>

        <p style='font-size:16px;'>Hello, <b>{name}</b> ✨</p>

        <p style='font-size:15px; color:#444;'>
            Your appointment has been successfully reserved. We look forward to welcoming you.
        </p>

        <div style='background:#f3ede4; border-radius:12px; padding:20px; margin:25px 0;'>

            <p style='margin:8px 0;'><b>Service:</b> {service}</p>

            <p style='margin:8px 0;'>
                <b>Date:</b> {date.ToString("dd MMM yyyy HH:mm", new CultureInfo("en-GB"))}
            </p>

            <p style='margin:8px 0;'>
                <b>Location:</b><br>
                Adama Mickiewicza 18<br>
                Katowice
            </p>

        </div>

        <p style='font-size:13px; color:#666;'>
            Please arrive on time for your appointment.
        </p>

        <div style='text-align:center; margin:30px 0;'>
            <a href='https://instagram.com/a.podlenskih.mua'
               style='display:inline-block; padding:12px 24px; background:#c59d5f; color:#fff; border-radius:20px; text-decoration:none; font-size:14px;'>
               Instagram
            </a>
        </div>

        <hr style='border:none; border-top:1px solid #eee; margin:30px 0;'>

        <div style='text-align:center; font-size:12px; color:#888;'>
            Anna Rakhno Beauty Studio<br>
            Private appointments only • Katowice
        </div>

    </div>

</div>";

        await SendEmail(toEmail, name, $"✨ Booking Confirmed — {service}", html);
    }

    public async Task SendAdminNotification(
        string clientName,
        string clientEmail,
        string clientPhone,
        string service,
        DateTime date)
    {
        var adminEmail = "artvorozhbyt@gmail.com";

        var html = $@"
<div style='font-family: Arial, sans-serif; font-size:14px; color:#333;'>

    <h3>New booking</h3>

    <p><b>Name:</b> {clientName}</p>
    <p><b>Email:</b> {clientEmail}</p>
    <p><b>Phone:</b> <a href='tel:{clientPhone}'>{clientPhone}</a></p>

    <hr />

    <p><b>Service:</b> {service}</p>
    <p><b>Date:</b> {date.ToString("dd MMM yyyy HH:mm", new CultureInfo("en-GB"))}</p>

</div>";

        await SendEmail(adminEmail, "Admin", $"🔔 {service} | {clientName}", html);
    }
}