using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParkingFinder.Models;
using Microsoft.Data.SqlClient;

namespace SmartParkingFinder.Controllers
{
    public class ParkingController : Controller
    {
        string connStr = "workstation id=SmartParkingDB.mssql.somee.com;packet size=4096;user id=yourUser;password=zf2vmy8o1g;data source=SmartParkingDB.mssql.somee.com;persist security info=False;initial catalog=SmartParkingDB";

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("User") != null;
        }
        public IActionResult ShowParking()
        {
            if (HttpContext.Session.GetString("User") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        // PARKING PAGE
        public IActionResult FindParking()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            List<BookingData> list = new List<BookingData>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = "SELECT SlotName, Location, IsBooked FROM ParkingSlots";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new BookingData
                    {
                        SlotName = reader["SlotName"].ToString(),
                        Location = reader["Location"].ToString(),
                        IsBooked = Convert.ToBoolean(reader["IsBooked"])
                    });
                }
            }

            return View(list);
        }

        // BOOK SLOT
        public IActionResult Book(string location, string slot)
        {
            string user = HttpContext.Session.GetString("User");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // slot booked
                string update = "UPDATE ParkingSlots SET IsBooked = 1 WHERE SlotName=@slot AND Location=@lOC";
                SqlCommand cmd = new SqlCommand(update, conn);
                cmd.Parameters.AddWithValue("@slot", slot);
                cmd.Parameters.AddWithValue("@loc", location);
                cmd.ExecuteNonQuery();

                // booking history save
                string insert = "INSERT INTO BookingData (Username, Location, Slot, BookingDate)VALUES(@user, @loc, @slot, @date)";

                SqlCommand cmd2 = new SqlCommand(insert, conn);
                cmd2.Parameters.AddWithValue("@user", user);
                cmd2.Parameters.AddWithValue("@loc", location);
                cmd2.Parameters.AddWithValue("@slot", slot);
                cmd2.Parameters.AddWithValue("@date", DateTime.Now);

                cmd2.ExecuteNonQuery();
            }

            TempData["msg"] = "Slot Booked Successfully";
            TempData["type"] = "success";

            return RedirectToAction("FindParking");
        }

        // CANCEL SLOT
        public IActionResult Cancel(string location, string slot)
        {
            string user = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Account");
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // check owner
                string check = "SELECT COUNT(*) FROM BookingData WHERE Username=@user AND Location=@loc AND Slot=@slot";

                SqlCommand checkCmd = new SqlCommand(check, conn);
                checkCmd.Parameters.AddWithValue("@user", user);
                checkCmd.Parameters.AddWithValue("@loc", location);
                checkCmd.Parameters.AddWithValue("@slot", slot);

                int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (exists == 0)
                {
                    TempData["msg"] = "You can only cancel your own booking!";
                    TempData["type"] = "error";
                    return RedirectToAction("FindParking");
                }

                // slot free
                string update = "UPDATE ParkingSlots SET IsBooked = 0 WHERE Location=@loc AND SlotName=@slot";

                SqlCommand cmd = new SqlCommand(update, conn);
                cmd.Parameters.AddWithValue("@loc", location);
                cmd.Parameters.AddWithValue("@slot", slot);
                cmd.ExecuteNonQuery();

                // delete booking
                string delete = "DELETE FROM BookingData WHERE Username=@user AND Location=@loc AND Slot=@slot";

                SqlCommand del = new SqlCommand(delete, conn);
                del.Parameters.AddWithValue("@user", user);
                del.Parameters.AddWithValue("@loc", location);
                del.Parameters.AddWithValue("@slot", slot);
                del.ExecuteNonQuery();
            }

            TempData["msg"] = "Booking Cancelled";
            TempData["type"] = "success";

            return RedirectToAction("FindParking");
        }

        public IActionResult ClearHistory()
        {
            string user = HttpContext.Session.GetString("User");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = "DELETE FROM BookingData WHERE Username=@user";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@user", user);

                cmd.ExecuteNonQuery();
            }

            TempData["msg"] = "History Cleared!";
            TempData["type"] = "success";

            return RedirectToAction("History");
        }

        // HISTORY
        public IActionResult History()
        {
            string user = HttpContext.Session.GetString("User");

            List<string> history = new List<string>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = "SELECT Location, Slot, BookingDate FROM BookingData WHERE Username=@user ORDER BY BookingDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@user", user);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string location = reader["Location"].ToString();
                    string slot = reader["Slot"].ToString();
                    string date = reader["BookingDate"].ToString();

                    history.Add(location + " | " + slot + " | " + date);
                }
            }

            return View(history);
        }
    }
}