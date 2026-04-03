using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;

namespace SmartParkingFinder.Controllers
{
    public class AdminController : Controller
    {
        string connStr = "Server=SmartParkingDB.mssql.somee.com,1433;Database=SmartParkingDB;User Id=amar05_SQLLogin_1;Password=zf2vmy8o1g;TrustServerCertificate=True;";

        public IActionResult Test()
        {
            return Content("App running");
        }

        // 🔹 GET: Login page
        public IActionResult Login()
        {
            return View();
        }

        // 🔹 POST: Login check
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM Admins WHERE Username=@u AND Password=@p";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", username.Trim());
                cmd.Parameters.AddWithValue("@p", password.Trim());

                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    // ✅ LOGIN SUCCESS
                    HttpContext.Session.SetString("Admin", username);
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    // ❌ LOGIN FAIL
                    ViewBag.Error = "Invalid username or password";
                    return View();
                }
            }
        }

        // 🔹 Dashboard
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM Users", conn);
                int users = (int)cmd1.ExecuteScalar();

                SqlCommand cmd2 = new SqlCommand("SELECT COUNT(*) FROM Bookings", conn);
                int bookings = (int)cmd2.ExecuteScalar();

                SqlCommand cmd3 = new SqlCommand("SELECT COUNT(*) FROM ParkingSlots WHERE IsBooked = 1", conn);
                int available = (int)cmd3.ExecuteScalar();

                ViewBag.Users = users;
                ViewBag.Bookings = bookings;
                ViewBag.Slots = available;
            }

            return View();
        }
        public IActionResult AllBookings(string search)
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login");

            List<string> list = new List<string>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = "SELECT Username, Location, Slot, BookingDate FROM BookingData";

                if (!string.IsNullOrEmpty(search))
                {
                    query += " WHERE Username LIKE @s OR SlotNumber LIKE @s";
                }

                SqlCommand cmd = new SqlCommand(query, conn);

                if (!string.IsNullOrEmpty(search))
                {
                    cmd.Parameters.AddWithValue("@s", "%" + search + "%");
                }

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string row = reader["Username"] + "|" +
             reader["SlotNumber"] + "|" +
             reader["BookingTime"];

                    list.Add(row);
                }
            }

            return View(list);
        }
        public IActionResult CancelBooking(string user, string location, string slot)
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // booking delete
                string deleteQuery = "DELETE FROM BookingData WHERE Username=@u AND Location=@l AND Slot=@s";

                SqlCommand cmd = new SqlCommand(deleteQuery, conn);
                cmd.Parameters.AddWithValue("@u", user);
                cmd.Parameters.AddWithValue("@l", location);
                cmd.Parameters.AddWithValue("@s", slot);

                int rows = cmd.ExecuteNonQuery();

                // slot wapas available karo
                if (rows > 0)
                {
                    string updateQuery = "UPDATE ParkingSlots SET IsBooked=0 WHERE SlotName=@s AND Location=@l";

                    SqlCommand cmd2 = new SqlCommand(updateQuery, conn);
                    cmd2.Parameters.AddWithValue("@s", slot);
                    cmd2.Parameters.AddWithValue("@l", location);

                    cmd2.ExecuteNonQuery();
                }
            }

            return RedirectToAction("AllBookings");
        }

        // 🔹 Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Admin");
            return RedirectToAction("Login");
        }
    }
}