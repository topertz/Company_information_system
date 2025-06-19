using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

[ApiController]
[Route("[controller]/[action]")]
public class OrderController : Controller
{
    [HttpGet]
    public IActionResult GetAllOrders([FromQuery] string userRole, [FromQuery] int? userID)
    {
        List<Order> orders = new List<Order>();
        string sql = @"
        SELECT o.OrderID, o.CustomerName, o.CustomerEmail, o.CustomerAddress, o.HouseNumber, p.ProductName, o.Price, o.Amount, o.Status, o.OrderDate
        FROM ""Order"" o
        LEFT JOIN Product p ON o.ProductID = p.ProductID";

        if (userRole == "customer" && userID.HasValue)
        {
            sql += " WHERE o.UserID = @UserID";
        }

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                if (userRole == "customer" && userID.HasValue)
                {
                    cmd.Parameters.AddWithValue("@UserID", userID.Value);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new Order
                        {
                            OrderID = reader.GetInt32(0),
                            CustomerName = reader.GetString(1),
                            CustomerEmail = reader.GetString(2),
                            CustomerAddress = reader.GetString(3),
                            HouseNumber = reader.GetInt32(4),
                            ProductName = reader.IsDBNull(5) ? "Ismeretlen termék" : reader.GetString(5),
                            Price = reader.GetInt32(6),
                            Amount = reader.GetInt32(7),
                            Status = reader.GetString(8),
                            OrderDate = reader.GetDateTime(9)
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
            string sql = @"SELECT OrderID, CustomerName, CustomerEmail, CustomerAddress, HouseNumber, ProductID, Price, Amount, Status, OrderDate 
                   FROM ""Order"" WHERE OrderID = @OrderID";

            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@OrderID", orderID);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var order = new
                {
                    OrderID = reader.GetInt32(0),
                    CustomerName = reader.GetString(1),
                    CustomerEmail = reader.GetString(2),
                    CustomerAddress = reader.GetString(3),
                    HouseNumber = reader.GetInt32(4),
                    ProductID = reader.GetInt32(5),
                    Price = reader.GetInt32(6),
                    Amount = reader.GetInt32(7),
                    Status = reader.GetString(8),
                    OrderDate = reader.GetDateTime(9).ToString("yyyy-MM-dd")
                };
                return Json(order);
            }
        }
        return NotFound("Rendelés nem található.");
    }

    [HttpPost]
    public IActionResult CreateOrder([FromForm] int userID, [FromForm] string customerName, [FromForm] string customerEmail, [FromForm] string customerAddress, [FromForm] int houseNumber, [FromForm] int productID, [FromForm] int price, [FromForm] int amount, [FromForm] string status, [FromForm] string orderDate)
    {
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            string sql = @"INSERT INTO ""Order"" (UserID, CustomerName, CustomerEmail, CustomerAddress, HouseNumber, ProductID, Price, Amount, Status, OrderDate)
                   VALUES (@UserID, @CustomerName, @CustomerEmail, @CustomerAddress, @HouseNumber, @ProductID, @Price, @Amount, @Status, @OrderDate)";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters.AddWithValue("@CustomerName", customerName);
                cmd.Parameters.AddWithValue("@CustomerEmail", customerEmail);
                cmd.Parameters.AddWithValue("@CustomerAddress", customerAddress);
                cmd.Parameters.AddWithValue("@HouseNumber", houseNumber);
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
    public IActionResult UpdateOrder([FromForm] int orderID, [FromForm] string customerName, [FromForm] string customerAddress, [FromForm] int houseNumber, [FromForm] string customerEmail, [FromForm] int productID, [FromForm] int price, [FromForm] int amount, [FromForm] string status, [FromForm] string orderDate)
    {
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            string sql = @"
        UPDATE ""Order""
        SET CustomerName = @CustomerName,
            CustomerEmail = @CustomerEmail,
            CustomerAddress = @CustomerAddress,
            HouseNumber = @HouseNumber,
            ProductID = @ProductID,
            Price = @Price,
            Amount = @Amount,
            Status = @Status,
            OrderDate = @OrderDate
        WHERE OrderID = @OrderID";

            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@CustomerName", customerName);
            cmd.Parameters.AddWithValue("@CustomerEmail", customerEmail);
            cmd.Parameters.AddWithValue("@CustomerAddress", customerAddress);
            cmd.Parameters.AddWithValue("@HouseNumber", houseNumber);
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
            string sql = @"DELETE FROM ""Order"" WHERE OrderID = @OrderID";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@OrderID", orderID);

            if (cmd.ExecuteNonQuery() == 0)
                return NotFound("Order not found.");
        }
        return Ok("Order deleted.");
    }
}