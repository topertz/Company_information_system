using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

[ApiController]
[Route("[controller]/[action]")]
public class FinanceController : Controller
{
    [HttpGet]
    public IActionResult GetAllFinance()
    {
        List<Finance> financeList = new List<Finance>();
        string sql = "SELECT * FROM Finance";

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    financeList.Add(new Finance
                    {
                        FinanceID = reader.GetInt32(0),
                        Year = reader.GetString(1),
                        Income = reader.GetInt32(2),
                        Expense = reader.GetInt32(3)
                    });
                }
            }
        }

        return Json(financeList);
    }

    [HttpPost]
    public IActionResult CreateFinance([FromForm] string year, [FromForm] int income, [FromForm] int expense)
    {
        using var connection = DatabaseConnector.CreateNewConnection();
        string sql = "INSERT INTO Finance (Year, Income, Expense) VALUES (@Year, @Income, @Expense)";
        using var cmd = new SQLiteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Year", year);
        cmd.Parameters.AddWithValue("@Income", income);
        cmd.Parameters.AddWithValue("@Expense", expense);
        cmd.ExecuteNonQuery();

        return Ok("Finance record created successfully.");
    }

    [HttpPost]
    public IActionResult UpdateFinance([FromForm] int financeID, [FromForm] string? year, [FromForm] int? income, [FromForm] int? expense)
    {
        using var connection = DatabaseConnector.CreateNewConnection();
        string sql = @"
        UPDATE Finance 
        SET 
            Year = COALESCE(NULLIF(@Year, ''), Year),
            Income = COALESCE(@Income, Income),
            Expense = COALESCE(@Expense, Expense)
        WHERE FinanceID = @FinanceID";

        using var cmd = new SQLiteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@FinanceID", financeID);
        cmd.Parameters.AddWithValue("@Year", year ?? "");
        cmd.Parameters.AddWithValue("@Income", (object?)income ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Expense", (object?)expense ?? DBNull.Value);

        if (cmd.ExecuteNonQuery() == 0)
            return NotFound("Finance record not found.");

        return Ok("Finance record updated.");
    }

    [HttpPost]
    public IActionResult DeleteFinance([FromForm] int financeID)
    {
        using var connection = DatabaseConnector.CreateNewConnection();
        string sql = "DELETE FROM Finance WHERE FinanceID = @FinanceID";
        using var cmd = new SQLiteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@FinanceID", financeID);

        if (cmd.ExecuteNonQuery() == 0)
            return NotFound("Finance record not found.");

        return Ok("Finance record deleted.");
    }

    [HttpPost]
    public IActionResult GenerateMonthlyFinanceFromOrders()
    {
        using var connection = DatabaseConnector.CreateNewConnection();

        string sql = @"
    SELECT strftime('%Y', OrderDate) AS Year,
           SUM(Price * Amount) AS TotalIncome
    FROM ""Order""
    WHERE Status = 'Rendelés zárolva'
    GROUP BY Year
    ORDER BY Year";

        var yearlyData = new List<(string Year, int Income)>();

        using var cmd = new SQLiteCommand(sql, connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            string year = reader.GetString(0);
            int income = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader["TotalIncome"]);
            yearlyData.Add((year, income));
        }

        const int InitialCapital = 5000;

        foreach (var item in yearlyData)
        {
            int expense = InitialCapital - item.Income;
            if (expense < 0) expense = 0;

            string updateSql = @"
        UPDATE Finance SET Income = @Income, Expense = @Expense WHERE Year = @Year";

            using var updateCmd = new SQLiteCommand(updateSql, connection);
            updateCmd.Parameters.AddWithValue("@Income", item.Income);
            updateCmd.Parameters.AddWithValue("@Expense", expense);
            updateCmd.Parameters.AddWithValue("@Year", item.Year);

            int rowsAffected = updateCmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                string insertSql = @"
            INSERT INTO Finance (Year, Income, Expense) VALUES (@Year, @Income, @Expense)";

                using var insertCmd = new SQLiteCommand(insertSql, connection);
                insertCmd.Parameters.AddWithValue("@Year", item.Year);
                insertCmd.Parameters.AddWithValue("@Income", item.Income);
                insertCmd.Parameters.AddWithValue("@Expense", expense);
                insertCmd.ExecuteNonQuery();
            }
        }

        return Ok("Pénzügyi adatok sikeresen frissítve a rendelések alapján.");
    }

    [HttpGet]
    public IActionResult GetProductSalesSummary()
    {
        List<ProductSaleSummary> summaries = new List<ProductSaleSummary>();
        string sql = @"
        SELECT p.ProductName, 
               SUM(o.Amount) AS TotalAmount, 
               SUM(o.Price * o.Amount) AS TotalRevenue
        FROM ""Order"" o
        INNER JOIN Product p ON o.ProductID = p.ProductID
        GROUP BY p.ProductName
        ORDER BY TotalRevenue DESC";

        using var connection = DatabaseConnector.CreateNewConnection();
        using var cmd = new SQLiteCommand(sql, connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            summaries.Add(new ProductSaleSummary
            {
                ProductName = reader.GetString(0),
                TotalAmount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                TotalRevenue = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
            });
        }

        return Json(summaries);
    }
}