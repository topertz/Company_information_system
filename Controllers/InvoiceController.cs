using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

[ApiController]
[Route("[controller]/[action]")]
public class InvoiceController : Controller
{
    [HttpPost]
    public IActionResult SaveInvoice([FromBody] InvoiceDto invoice)
    {
        using var connection = DatabaseConnector.CreateNewConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            string insertInvoiceSql = "INSERT INTO Invoice (CustomerName, CreatedAt) VALUES (@CustomerName, @CreatedAt)";
            using var insertInvoiceCmd = new SQLiteCommand(insertInvoiceSql, connection);
            insertInvoiceCmd.Parameters.AddWithValue("@CustomerName", invoice.CustomerName);
            var createdAt = DateTime.UtcNow;
            insertInvoiceCmd.Parameters.AddWithValue("@CreatedAt", createdAt.ToString("yyyy-MM-dd HH:mm:ss"));
            insertInvoiceCmd.ExecuteNonQuery();

            long invoiceId = connection.LastInsertRowId;

            var savedItems = new List<InvoiceItemDto>();

            foreach (var item in invoice.Items)
            {
                string? productName = "";
                using (var getProductCmd = new SQLiteCommand("SELECT ProductName FROM Product WHERE ProductID = @ProductID", connection))
                {
                    getProductCmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                    var result = getProductCmd.ExecuteScalar();
                    productName = result != null ? result.ToString() : "Ismeretlen termék";
                }

                string insertItemSql = @"
                INSERT INTO InvoiceItem (InvoiceID, ProductID, Quantity, UnitPrice)
                VALUES (@InvoiceID, @ProductID, @Quantity, @UnitPrice)";
                using var insertItemCmd = new SQLiteCommand(insertItemSql, connection);
                insertItemCmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                insertItemCmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                insertItemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                insertItemCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                insertItemCmd.ExecuteNonQuery();

                savedItems.Add(new InvoiceItemDto
                {
                    ProductID = item.ProductID,
                    ProductName = productName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            transaction.Commit();

            var savedInvoice = new
            {
                InvoiceID = invoiceId,
                CustomerName = invoice.CustomerName,
                CreatedAt = createdAt.ToString("yyyy-MM-dd HH:mm:ss"),
                Items = savedItems
            };

            return Ok(savedInvoice);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return BadRequest("Hiba történt: " + ex.Message);
        }
    }
}