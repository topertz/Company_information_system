using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

[ApiController]
[Route("[controller]/[action]")]
public class ProductController : Controller
{
    [HttpGet]
    public IActionResult GetAllProducts()
    {
        List<Product> products = new List<Product>();
        string sql = "SELECT * FROM Product ORDER BY ProductID";

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    products.Add(new Product
                    {
                        ProductID = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        ProductPrice = reader.GetInt32(2),
                    });
                }
            }
        }
        return Json(products);
    }

    [HttpPost]
    public IActionResult CreateProduct([FromForm] string productName, [FromForm] int productPrice, [FromForm] string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            return BadRequest("ImageUrl cannot be null or empty.");
        }

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            string sql = "INSERT INTO Product (ProductName, ProductPrice, ImageUrl) VALUES (@ProductName, @ProductPrice, @ImageUrl)";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@ProductName", productName);
                cmd.Parameters.AddWithValue("@ProductPrice", productPrice);
                cmd.Parameters.AddWithValue("@ImageUrl", imageUrl);
                cmd.ExecuteNonQuery();
            }
        }

        return Ok("Product created successfully.");
    }

    [HttpPost]
    public IActionResult UpdateProduct([FromForm] int productID, [FromForm] string? productName, [FromForm] int? productPrice)
    {
        using var connection = DatabaseConnector.CreateNewConnection();
        string sql = @"
    UPDATE Product
    SET 
        ProductName = COALESCE(NULLIF(@ProductName, ''), ProductName),
        ProductPrice = COALESCE(@ProductPrice, ProductPrice)
    WHERE ProductID = @ProductID";

        using var cmd = new SQLiteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ProductID", productID);
        cmd.Parameters.AddWithValue("@ProductName", productName ?? "");
        cmd.Parameters.AddWithValue("@ProductPrice", (object?)productPrice ?? DBNull.Value);

        if (cmd.ExecuteNonQuery() == 0)
            return NotFound("Product not found.");

        return Ok("Product updated.");
    }

    [HttpPost]
    public IActionResult DeleteProduct([FromForm] int productID)
    {
        using var connection = DatabaseConnector.CreateNewConnection();
        string sql = "DELETE FROM Product WHERE ProductID = @ProductID";
        using var cmd = new SQLiteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ProductID", productID);

        if (cmd.ExecuteNonQuery() == 0)
            return NotFound("Product not found.");

        return Ok("Product deleted.");
    }

    [HttpPost]
    public IActionResult SeedProducts()
    {
        try
        {
            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                string sql = @"
            INSERT INTO Product (ProductID, ProductName, ProductPrice, ImageUrl) VALUES
                (1, 'Gerbera', 50, '/images/gerbera.png'),
                (2, 'Liliom', 100, '/images/liliom.png'),
                (3, 'Rózsa', 150, '/images/rozsa.png'),
                (4, 'Tulipán', 200, '/images/tulipan.png'),
                (5, 'Kaktusz', 250, '/images/kaktusz.png'),
                (6, 'Pipacs', 300, '/images/pipacs.png');";


                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        return Ok($"{ rows} products seeded.");
                    }
                    else
                    {
                        return BadRequest("No products were seeded.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return StatusCode(500, $"An error occurred while seeding the products: {ex.Message}");
        }
    }
}