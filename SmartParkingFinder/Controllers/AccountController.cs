using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace SmartParkingFinder.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            string connStr = "Server=SmartParkingDB.mssql.somee.com;Database=SmartParkingDB;User Id=amar05_SQLLogin_1;Password=zf2vmy8o1g;TrustServerCertificate=True;";

            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();

            string query = "SELECT Username, Password FROM Users";

            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string dbUser = reader["Username"].ToString();
                string dbPass = reader["Password"].ToString();

                if (username == dbUser && password == dbPass)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return Content("Login Failed");
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password)
        {
            string connStr = "Server=.\\SQLEXPRESS;Database=SmartParkingDB;Trusted_Connection=True;TrustServerCertificate=True;";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = "INSERT INTO Users (Username,Email,Password) VALUES (@username,@email,@password)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}