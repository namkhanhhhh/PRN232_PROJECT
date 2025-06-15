using Net.payOS;
using Net.payOS.Types;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


public class PayOSService
{
    private readonly PayOS _payOS;
    private readonly string _checksumKey;

    public PayOSService(PayOS payOS, IConfiguration configuration)
    {
        _payOS = payOS;
        _checksumKey = configuration["Environment:PAYOS_CHECKSUM_KEY"] ?? throw new Exception("Missing PAYOS_CHECKSUM_KEY");
    }

    public bool VerifyWebhookSignature(WebhookPayload payload, string signature)
    {
        var json = JsonSerializer.Serialize(payload);
        var keyBytes = Encoding.UTF8.GetBytes(_checksumKey);
        var dataBytes = Encoding.UTF8.GetBytes(json);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        var calculatedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return calculatedSignature == signature.ToLower();
    }

    public async Task<CreatePaymentResult?> CreatePaymentAsync(int userId, decimal amount, string cancelUrl, string successUrl)
    {
        long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        List<ItemData> items = new()
        {
            new ItemData("Nap tiền vào ví SJOB", 1, (int)amount)
        };

        PaymentData paymentData = new(
            orderCode,
            (int)amount,
            $"NAPTIENSJOB+{userId}",
            items,
            cancelUrl,
            successUrl
        );


        try
        {
            return await _payOS.createPaymentLink(paymentData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PayOS Error: {ex.Message}");
            return null;
        }
    }

    private DateTime GetVietnamTime()
    {
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
    }

}




public class WebhookPayload
{
    public string OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
    public string UserId { get; set; }
    public string Description { get; set; }
}

