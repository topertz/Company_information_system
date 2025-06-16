public class Order
{
    public int OrderID { get; set; }
    public string? CustomerName { get; set; }
    public string? ProductName { get; set; }
    public int ProductID { get; set; }
    public int Price { get; set; }
    public int Amount { get; set; }
    public string? Status { get; set; }
    public DateTime OrderDate { get; set; }
}