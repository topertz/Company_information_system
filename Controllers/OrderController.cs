using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

[ApiController]
[Route("[controller]/[action]")]
public class OrderController : Controller
{
    [HttpGet]
    public IActionResult GetAllOrders()
    {
        List<Order> orders = new List<Order>();
        string sql = @"
        SELECT o.OrderID, o.CustomerName, p.ProductName, o.Price, o.Amount, o.Status, o.OrderDate
        FROM Orders o
        LEFT JOIN Products p ON o.ProductID = p.ProductID";

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new Order
                        {
                            OrderID = reader.GetInt32(0),
                            CustomerName = reader.GetString(1),
                            ProductName = reader.IsDBNull(2) ? "Ismeretlen termék" : reader.GetString(2),
                            Price = reader.GetInt32(3),
                            Amount = reader.GetInt32(4),
                            Status = reader.GetString(5),
                            OrderDate = reader.GetDateTime(6)
                        });
                    }
                }
            }
        }

        return Json(orders);
    }

    [HttpGet]
    public IActionResult GetOrderById([FromQuery] int orderID)
    {
        using (var connection = DatabaseConnector.CreateNewConnection()) 
        {
            string sql = @"SELECT OrderID, CustomerName, ProductID, Price, Amount, Status, OrderDate 
                   FROM Orders WHERE OrderID = @OrderID";

            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@OrderID", orderID);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var order = new
                {
                    OrderID = reader.GetInt32(0),
                    CustomerName = reader.GetString(1),
                    ProductID = reader.GetInt32(2),
                    Price = reader.GetInt32(3),
                    Amount = reader.GetInt32(4),
                    Status = reader.GetString(5),
                    OrderDate = reader.GetDateTime(6).ToString("yyyy-MM-dd")
                };
                return Json(order);
            }
        }
        return NotFound("Rendelés nem található.");
    }

    [HttpPost]
    public IActionResult CreateOrder([FromForm] string customerName, [FromForm] int productID, [FromForm] int price, [FromForm] int amount, [FromForm] string status, [FromForm] string orderDate)
    {
        using (var connection = DatabaseConnector.CreateNewConnection()) 
        {
            string sql = @"INSERT INTO Orders (CustomerName, ProductID, Price, Amount, Status, OrderDate)
                   VALUES (@CustomerName, @ProductID, @Price, @Amount, @Status, @OrderDate)";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@CustomerName", customerName);
                cmd.Parameters.AddWithValue("@ProductID", productID);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@OrderDate", orderDate);
                cmd.ExecuteNonQuery();
            }     
        }
        return Ok("Order created.");
    }

    [HttpPost]
    public IActionResult UpdateOrder([FromForm] int orderID, [FromForm] string customerName, [FromForm] int productID, [FromForm] int price, [FromForm] int amount, [FromForm] string status, [FromForm] string orderDate)
    {
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            string sql = @"
        UPDATE Orders
        SET CustomerName = @CustomerName,
            ProductID = @ProductID,
            Price = @Price,
            Amount = @Amount,
            Status = @Status,
            OrderDate = @OrderDate
        WHERE OrderID = @OrderID";

            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@CustomerName", customerName);
            cmd.Parameters.AddWithValue("@ProductID", productID);
            cmd.Parameters.AddWithValue("@Price", price);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@OrderDate", orderDate);
            cmd.Parameters.AddWithValue("@OrderID", orderID);

            if (cmd.ExecuteNonQuery() == 0)
                return NotFound("Order not found.");
        }
        return Ok("Order updated.");
    }

    [HttpPost]
    public IActionResult DeleteOrder([FromForm] int orderID)
    {
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            string sql = "DELETE FROM Orders WHERE OrderID = @OrderID";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@OrderID", orderID);

            if (cmd.ExecuteNonQuery() == 0)
                return NotFound("Order not found.");
        }
        return Ok("Order deleted.");
    }
}