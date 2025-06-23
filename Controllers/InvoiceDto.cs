public class InvoiceDto
{
    public string CustomerName { get; set; } = string.Empty;
    public List<InvoiceItemDto> Items { get; set; } = new();
}