using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using System.Globalization;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    private async Task SendEmail(MimeMessage email)
    {
        try
        {
            var smtpUser = _config["Smtp:User"];
            var smtpPass = _config["Smtp:Pass"];
            var fromEmail = _config["Smtp:From"];
            var host = _config["Smtp:Host"];
            var portString = _config["Smtp:Port"];

            // === VALIDATION ===
            if (string.IsNullOrEmpty(smtpUser))
                throw new Exception("SMTP user missing");

            if (string.IsNullOrEmpty(smtpPass))
                throw new Exception("SMTP password missing");

            if (string.IsNullOrEmpty(host))
                throw new Exception("SMTP host missing");

            if (!int.TryParse(portString, out var port))
                throw new Exception("SMTP port invalid");

            // === FROM ===
            email.From.Clear();
            email.From.Add(MailboxAddress.Parse(fromEmail ?? smtpUser));

            using var smtp = new SmtpClient();

            // === CONNECT ===
            await smtp.ConnectAsync(
                host,
                port,
                MailKit.Security.SecureSocketOptions.StartTls);

            // === AUTH ===
            await smtp.AuthenticateAsync(smtpUser, smtpPass);

            // === SEND ===
            await smtp.SendAsync(email);

            // === DISCONNECT ===
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("EMAIL ERROR: " + ex.ToString());
            throw;
        }
    }

    public async Task SendBookingConfirmation(
    string toEmail,
    string name,
    string service,
    DateTime date)
    {
        var email = new MimeMessage();

        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = $"✨ Booking Confirmed — {service}";

        email.Body = new TextPart("html")
        {
            Text = $@"
    <div style='font-family: ""Playfair Display"", serif; background:#f5f2ed; padding:40px;'>

    <div style='max-width:600px; margin:auto; background:#ffffff; border-radius:16px; padding:40px; border:1px solid #e8dcc8;'>

        <!-- HEADER -->
        <div style='text-align:center; margin-bottom:30px;'>
            <h1 style='margin:0; font-size:28px; color:#c59d5f;'>Anna Rakhno</h1>
            <div style='letter-spacing:2px; font-size:12px; color:#999; margin-top:8px;'>
                LUXURY MAKEUP ATELIER
            </div>
        </div>

        <hr style='border:none; border-top:1px solid #eee; margin:30px 0;'>

        <!-- CONTENT -->
        <p style='font-size:16px;'>Hello, <b>{name}</b> ✨</p>

        <p style='font-size:15px; color:#444;'>
            Your appointment has been successfully reserved. We look forward to welcoming you.
        </p>

        <!-- CARD -->
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
            Please arrive on time for your appointment. If you need changes, reply to this email or contact us directly.
        </p>

        <!-- BUTTON -->
        <div style='text-align:center; margin:30px 0;'>
            <a href='https://instagram.com/a.podlenskih.mua'
               style='display:inline-block; padding:12px 24px; background:#c59d5f; color:#fff; border-radius:20px; text-decoration:none; font-size:14px;'>
               Instagram
            </a>
        </div>

        <hr style='border:none; border-top:1px solid #eee; margin:30px 0;'>

        <!-- FOOTER -->
        <div style='text-align:center; font-size:12px; color:#888;'>
            Anna Rakhno Beauty Studio<br>
            Private appointments only • Katowice
        </div>

    </div>

</div>
"
        };

        await SendEmail(email);
    }

    public async Task SendAdminNotification(
        string clientName,
        string clientEmail,
        string clientPhone,
        string service,
        DateTime date)
    {
        var email = new MimeMessage();
        var adminEmail = _config["Smtp:Admin"];

        if (string.IsNullOrEmpty(adminEmail))
            throw new Exception("Admin email missing");

        email.To.Add(MailboxAddress.Parse(adminEmail));
     
        email.Subject = $"🔔 {service} | {clientName}";

        email.Body = new TextPart("html")
        {
            Text = $@"
            <div style='font-family: Arial, sans-serif; font-size:14px; color:#333;'>

                <h3>New booking</h3>

                <p><b>Name:</b> {clientName}</p>
                <p><b>Email:</b> {clientEmail}</p>
                <p><b>Phone:</b> <a href='tel:{clientPhone}'>{clientPhone}</a></p>

                <hr />

                <p><b>Service:</b> {service}</p>
                <p><b>Date:</b> {date.ToString("dd MMM yyyy HH:mm", new CultureInfo("en-GB"))}</p>

            </div>
            "
        };
        await SendEmail(email);
    }
}