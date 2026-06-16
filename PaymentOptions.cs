using System.ComponentModel.DataAnnotations;

namespace TmsApi;

public class PaymentOptions
{
    // The section keyword in appsettings.json this maps to
    public const string SectionName = "Payments";

    [Required(ErrorMessage = "The GatewayUrl field is required.")]
    [Url(ErrorMessage = "The GatewayUrl must be a valid HTTP/HTTPS URL.")]
    public required string GatewayUrl { get; init; }

    [Range(100, 100000, ErrorMessage = "MaxDepositBirr must be between 100 and 100,000 ETB.")]
    public decimal MaxDepositBirr { get; init; }
}