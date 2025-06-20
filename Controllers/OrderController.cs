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
        SELECT o.OrderID, o.CustomerName, o.CustomerEmail, o.CustomerAddress, o.HouseNumber, o.PhoneNumber, p.ProductName, o.Price, o.Amount, o.Status, o.OrderDate
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
                            PhoneNumber = reader.GetString(5),
                            ProductName = reader.IsDBNull(6) ? "Ismeretlen termék" : reader.GetString(6),
                            Price = reader.GetInt32(7),
                            Amount = reader.GetInt32(8),
                            Status = reader.GetString(9),
                            OrderDate = reader.GetDateTime(10)
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
            string sql = @"SELECT OrderID, CustomerName, CustomerEmail, CustomerAddress, HouseNumber, PhoneNumber, ProductID, Price, Amount, Status, OrderDate 
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
                    PhoneNumber = reader.GetString(5),
                    ProductID = reader.GetInt32(6),
                    Price = reader.GetInt32(7),
                    Amount = reader.GetInt32(8),
                    Status = reader.GetString(9),
                    OrderDate = reader.GetDateTime(10).ToString("yyyy-MM-dd")
                };
                return Json(order);
            }
        }
        return NotFound("Rendelés nem található.");
    }

    [HttpPost]
    public IActionResult CreateOrder([FromForm] int userID,
                                 [FromForm] string customerName,
                                 [FromForm] string customerEmail,
                                 [FromForm] string customerAddress,
                                 [FromForm] int houseNumber,
                                 [FromForm] string phoneNumber,
                                 [FromForm] int productID,
                                 [FromForm] int price,
                                 [FromForm] int amount,
                                 [FromForm] string status,
                                 [FromForm] string orderDate)
    {
        string productName = "Ismeretlen termék";

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            string productSql = @"SELECT ProductName FROM Product WHERE ProductID = @ProductID";
            using (var cmd = new SQLiteCommand(productSql, connection))
            {
                cmd.Parameters.AddWithValue("@ProductID", productID);
                var productResult = cmd.ExecuteScalar();

                if (productResult != null)
                {
                    productName = productResult.ToString() ?? "Ismeretlen termék";
                }
            }
        }

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            string sql = @"
            INSERT INTO ""Order"" 
            (UserID, CustomerName, CustomerEmail, CustomerAddress, HouseNumber, PhoneNumber, ProductID, ProductName, Price, Amount, Status, OrderDate)
            VALUES (@UserID, @CustomerName, @CustomerEmail, @CustomerAddress, @HouseNumber, @PhoneNumber, @ProductID, @ProductName, @Price, @Amount, @Status, @OrderDate)";

            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters.AddWithValue("@CustomerName", customerName);
                cmd.Parameters.AddWithValue("@CustomerEmail", customerEmail);
                cmd.Parameters.AddWithValue("@CustomerAddress", customerAddress);
                cmd.Parameters.AddWithValue("@HouseNumber", houseNumber);
                cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                cmd.Parameters.AddWithValue("@ProductID", productID);
                cmd.Parameters.AddWithValue("@ProductName", productName);
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
    public IActionResult UpdateOrder([FromForm] int orderID, [FromForm] string customerName, [FromForm] string customerAddress, [FromForm] int houseNumber, [FromForm] string phoneNumber, [FromForm] string customerEmail, [FromForm] int productID, [FromForm] int price, [FromForm] int amount, [FromForm] string status, [FromForm] string orderDate)
    {
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            string sql = @"
        UPDATE ""Order""
        SET CustomerName = @CustomerName,
            CustomerEmail = @CustomerEmail,
            CustomerAddress = @CustomerAddress,
            HouseNumber = @HouseNumber,
            PhoneNumber = @PhoneNumber,
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
            cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
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