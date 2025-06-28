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
        using var connection = DatabaseConnector.CreateNewConnection();

        string getOrderSql = @"
        SELECT strftime('%Y', OrderDate) AS Year, Price * Amount AS Income
        FROM ""Order""
        WHERE OrderID = @OrderID";

        string orderYear = "";
        int orderIncome = 0;

        using (var cmd = new SQLiteCommand(getOrderSql, connection))
        {
            cmd.Parameters.AddWithValue("@OrderID", orderID);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                orderYear = reader.GetString(0);
                orderIncome = reader.GetInt32(1);
            }
            else
            {
                return NotFound("Order not found.");
            }
        }

        string deleteOrderSql = @"DELETE FROM ""Order"" WHERE OrderID = @OrderID";
        using (var cmd = new SQLiteCommand(deleteOrderSql, connection))
        {
            cmd.Parameters.AddWithValue("@OrderID", orderID);
            cmd.ExecuteNonQuery();
        }

        string getTotalIncomeSql = @"
        SELECT IFNULL(SUM(Price * Amount), 0) AS TotalIncome
        FROM ""Order""
        WHERE strftime('%Y', OrderDate) = @Year
          AND Status = 'Rendelés zárolva'";

        int totalIncome = 0;
        using (var cmd = new SQLiteCommand(getTotalIncomeSql, connection))
        {
            cmd.Parameters.AddWithValue("@Year", orderYear);
            totalIncome = Convert.ToInt32(cmd.ExecuteScalar());
        }

        const int InitialCapital = 5000;
        int updatedExpense = Math.Max(InitialCapital - totalIncome, 0);

        string updateFinanceSql = @"
        UPDATE Finance
        SET Income = @Income, Expense = @Expense
        WHERE Year = @Year";

        using (var cmd = new SQLiteCommand(updateFinanceSql, connection))
        {
            cmd.Parameters.AddWithValue("@Income", totalIncome);
            cmd.Parameters.AddWithValue("@Expense", updatedExpense);
            cmd.Parameters.AddWithValue("@Year", orderYear);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)       // && totalIncome > 0
            {
                string insertFinanceSql = @"
                INSERT INTO Finance (Year, Income, Expense)
                VALUES (@Year, @Income, @Expense)";
                using var insertCmd = new SQLiteCommand(insertFinanceSql, connection);
                insertCmd.Parameters.AddWithValue("@Year", orderYear);
                insertCmd.Parameters.AddWithValue("@Income", totalIncome);
                insertCmd.Parameters.AddWithValue("@Expense", updatedExpense);
                insertCmd.ExecuteNonQuery();
            }
        }

        return Ok("Order deleted and Finance record updated.");
    }

    [HttpPost]
    public IActionResult DeleteAllUnlockedOrders()
    {
        using var connection = DatabaseConnector.CreateNewConnection();

        var getYearsSql = @"
        SELECT DISTINCT strftime('%Y', OrderDate) AS Year
        FROM ""Order""
        WHERE Status != 'Rendelés zárolva'";

        var affectedYears = new List<string>();
        using (var cmd = new SQLiteCommand(getYearsSql, connection))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                affectedYears.Add(reader.GetString(0));
            }
        }

        var deleteSql = @"DELETE FROM ""Order"" WHERE Status != 'Rendelés zárolva'";
        using (var cmd = new SQLiteCommand(deleteSql, connection))
        {
            cmd.ExecuteNonQuery();
        }

        foreach (var year in affectedYears)
        {
            var getIncomeSql = @"
            SELECT IFNULL(SUM(Price * Amount), 0)
            FROM ""Order""
            WHERE strftime('%Y', OrderDate) = @Year
              AND Status = 'Rendelés zárolva'";

            int income = 0;
            using (var cmd = new SQLiteCommand(getIncomeSql, connection))
            {
                cmd.Parameters.AddWithValue("@Year", year);
                income = Convert.ToInt32(cmd.ExecuteScalar());
            }

            const int InitialCapital = 5000;
            int expense = Math.Max(InitialCapital - income, 0);

            var updateFinanceSql = @"
            UPDATE Finance
            SET Income = @Income, Expense = @Expense
            WHERE Year = @Year";

            using (var cmd = new SQLiteCommand(updateFinanceSql, connection))
            {
                cmd.Parameters.AddWithValue("@Income", income);
                cmd.Parameters.AddWithValue("@Expense", expense);
                cmd.Parameters.AddWithValue("@Year", year);
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)  // && income > 0
                {
                    var insertFinanceSql = @"
                    INSERT INTO Finance (Year, Income, Expense)
                    VALUES (@Year, @Income, @Expense)";
                    using var insertCmd = new SQLiteCommand(insertFinanceSql, connection);
                    insertCmd.Parameters.AddWithValue("@Year", year);
                    insertCmd.Parameters.AddWithValue("@Income", income);
                    insertCmd.Parameters.AddWithValue("@Expense", expense);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        return Ok("Összes nem zárolt rendelés törölve.");
    }

    [HttpPost]
    public IActionResult LockOrdersForCustomer([FromForm] int userID)
    {
        using var connection = DatabaseConnector.CreateNewConnection();

        string updateSql = @"
        UPDATE ""Order""
        SET Status = 'Rendelés zárolva'
        WHERE UserID = @UserID AND Status != 'Rendelés zárolva'";

        using var cmd = new SQLiteCommand(updateSql, connection);
        cmd.Parameters.AddWithValue("@UserID", userID);

        int affectedRows = cmd.ExecuteNonQuery();

        if (affectedRows > 0)
        {
            return Ok($"A(z) {affectedRows} rendelés zárolva lett.");
        }
        else
        {
            return Ok("Nem volt zárolható rendelés, vagy minden rendelés már zárolva van.");
        }
    }
}