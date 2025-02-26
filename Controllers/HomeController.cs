using FirebaseAdmin.Auth;
using LoginSystem.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using Microsoft.Extensions.Primitives;

namespace LoginSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            try
            {
                string userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId) && User.Identity.IsAuthenticated)
                {
                    userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    HttpContext.Session.SetString("UserId", userId); // Restore session
                }

                List<Trip> Trips = new List<Trip>();
                string sql = @"
                SELECT trip_id, trip_name, country, start_date, end_date
                FROM Trips 
                WHERE user_id = @userId
                ORDER BY start_date";


                string constr = _configuration.GetConnectionString("DefaultConnection");

                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        con.Open();
                        using (MySqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                Trips.Add(new Trip
                                {
                                    trip_id = sdr["trip_id"].ToString(),
                                    trip_name = sdr["trip_name"].ToString(),
                                    trip_country = sdr["country"].ToString(),
                                    trip_start = Convert.ToDateTime(sdr["start_date"]).Date,
                                    trip_end = Convert.ToDateTime(sdr["end_date"]).Date

                                });
                            }
                        }
                        con.Close();
                    }
                }
                return View(Trips);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message} {ex.StackTrace}");
            }
        }

        public IActionResult Destinations(string tripId)
        {
            try
            {
                List<Destination> Destinations = new List<Destination>();

                string sql = @"
        SELECT d.destination_id, d.state_province, d.city, d.arrival_date, d.departure_date,
               a.activity_id, a.title, a.description, a.day, a.place, a.start_time, a.end_time
        FROM Destinations d
        LEFT JOIN Activities a ON d.destination_id = a.destination_id
        WHERE d.trip_id = @tripId
        ORDER BY d.arrival_date, a.day, a.start_time";

                string constr = _configuration.GetConnectionString("DefaultConnection");

                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@tripId", tripId);
                        con.Open();
                        using (MySqlDataReader sdr = cmd.ExecuteReader())
                        {
                            Dictionary<string, Destination> destinationMap = new Dictionary<string, Destination>();

                            while (sdr.Read())
                            {
                                string destId = sdr["destination_id"].ToString();

                                if (!destinationMap.ContainsKey(destId))
                                {
                                    destinationMap[destId] = new Destination
                                    {
                                        destination_id = destId,
                                        dest_state = sdr["state_province"].ToString(),
                                        dest_city = sdr["city"].ToString(),
                                        dest_arr = Convert.ToDateTime(sdr["arrival_date"]),
                                        dest_dept = Convert.ToDateTime(sdr["departure_date"]),
                                        Activities = new List<ActivityModel>()
                                    };
                                }

                                if (!sdr.IsDBNull(sdr.GetOrdinal("activity_id")))
                                {
                                    destinationMap[destId].Activities.Add(new ActivityModel
                                    {
                                        activity_id = sdr["activity_id"].ToString(),
                                        activity_title = sdr["title"].ToString(),
                                        activity_description = sdr["description"].ToString(),
                                        activity_day = Convert.ToInt32(sdr["day"]),
                                        activity_place = sdr["place"].ToString(),
                                        activity_start = DateTime.Today.Add((TimeSpan)sdr["start_time"]),
                                        activity_end = DateTime.Today.Add((TimeSpan)sdr["end_time"])

                                    });
                                }
                            }
                            Destinations = destinationMap.Values.ToList();
                        }
                        con.Close();
                    }
                }
                return View(Destinations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message} {ex.StackTrace}");
            }
        }


        public IActionResult Activities(string destinationId)
        {
            try
            {

                List<ActivityModel> Activities = new List<ActivityModel>();

                string sql = @"
                    SELECT activity_id, title, description, day, place, start_time, end_time
                    FROM Activities
                    WHERE destination_id = @destinationId
                    ORDER BY day, start_time";

                string constr = _configuration.GetConnectionString("DefaultConnection");

                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        con.Open();
                        using (MySqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                Activities.Add(new ActivityModel
                                {
                                    activity_id = sdr["activity_id"].ToString(),
                                    activity_title = sdr["title"].ToString(),
                                    activity_description = sdr["description"].ToString(),
                                    activity_day = Convert.ToInt32(sdr["day"]),
                                    activity_place = sdr["place"].ToString(),
                                    activity_start = DateTime.Today.Add((TimeSpan)sdr["start_time"]),
                                    activity_end = DateTime.Today.Add((TimeSpan)sdr["end_time"]),

                                });
                            }
                        }
                        con.Close();
                    }
                }
                return Json(Activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message} {ex.StackTrace}");
            }
        }

        public IActionResult SignUp()
        {
            return View();
        }


        public IActionResult SetTrip()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/Test")]
        public IActionResult Test()
        {
            Console.WriteLine("✅ Test endpoint was hit!");
            return Ok("Test successful");
        }

        [HttpPost]
        [Route("Home/StoreTripInfoToDatabase")]
        public IActionResult StoreTripInfoToDatabase([FromForm] IFormCollection form)
        {
            string userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                if (User.Identity.IsAuthenticated)
                {
                    userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    HttpContext.Session.SetString("UserId", userId); // Restore session
                }
                else
                {
                    return Unauthorized("User is not authenticated. Please log in.");
                }
            }

            string constr = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                string tripId = Guid.NewGuid().ToString(); // Generate a unique trip ID
                string tripName = form["trip_name"];
                string tripCountry = form["trip_country"];
                DateTime.TryParse(form["trip_start"], out DateTime startDate);
                DateTime.TryParse(form["trip_end"], out DateTime endDate);

                List<(string destinationId, string city, string stateProvince, DateTime arrival, DateTime departure)> destinations = new();
                List<(string activityId, string destinationId, string title, string description, int day, string place, TimeSpan start, TimeSpan end)> activities = new();

                int destinationCount = 1;
                while (!StringValues.IsNullOrEmpty(form[$"destination_city_{destinationCount}"]))
                {
                    string destinationId = Guid.NewGuid().ToString();
                    string city = form[$"destination_city_{destinationCount}"].ToString() ?? string.Empty;
                    string stateProvince = form[$"destination_stateprovince_{destinationCount}"].ToString() ?? string.Empty;

                    DateTime.TryParse(form[$"destination_arrival_date_{destinationCount}"], out DateTime arrival);
                    DateTime.TryParse(form[$"destination_departure_date_{destinationCount}"], out DateTime departure);

                    destinations.Add((destinationId, city, stateProvince, arrival, departure));

                    int activityCount = 1;
                    while (!StringValues.IsNullOrEmpty(form[$"activity_{destinationCount}_{activityCount}_day"]))
                    {
                        string activityId = Guid.NewGuid().ToString();
                        string title = form[$"activity_title_{destinationCount}_{activityCount}"].ToString() ?? "Untitled Activity";
                        string description = form[$"activity_description_{destinationCount}_{activityCount}"].ToString() ?? "No description";
                        string place = form[$"activity_place_{destinationCount}_{activityCount}"].ToString() ?? "Unknown location";
                        int day = int.Parse(form[$"activity_{destinationCount}_{activityCount}_day"].ToString().Replace("Day ", ""));
                        TimeSpan.TryParse(form[$"activity_start_time_{destinationCount}_{activityCount}"], out TimeSpan start);
                        TimeSpan.TryParse(form[$"activity_end_time_{destinationCount}_{activityCount}"], out TimeSpan end);

                        activities.Add((activityId, destinationId, title, description, day, place, start, end));

                        activityCount++;
                    }

                    destinationCount++;
                }
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    con.Open();

                    string tripQuery = "INSERT INTO trips (trip_id, user_id, trip_name, start_date, end_date, country, created_at) VALUES (@TripID, @UserID, @TripName, @StartDate, @EndDate, @Country, NOW())";
                    using (MySqlCommand cmd = new MySqlCommand(tripQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@TripID", tripId);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@TripName", tripName);
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@EndDate", endDate);
                        cmd.Parameters.AddWithValue("@Country", tripCountry);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (var destination in destinations)
                    {
                        string destinationQuery = "INSERT INTO destinations (destination_id, trip_id, state_province, city, arrival_date, departure_date, created_at) VALUES (@DestinationID, @TripID, @StateProvince, @City, @ArrivalDate, @DepartureDate, NOW())";
                        using (MySqlCommand cmd = new MySqlCommand(destinationQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@DestinationID", destination.destinationId);
                            cmd.Parameters.AddWithValue("@TripID", tripId);
                            cmd.Parameters.AddWithValue("@StateProvince", destination.stateProvince);
                            cmd.Parameters.AddWithValue("@City", destination.city);
                            cmd.Parameters.AddWithValue("@ArrivalDate", destination.arrival);
                            cmd.Parameters.AddWithValue("@DepartureDate", destination.departure);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    foreach (var activity in activities)
                    {
                        string activityQuery = "INSERT INTO activities (activity_id, trip_id, destination_id, title, description, day, place, start_time, end_time, created_at) VALUES (@ActivityID, @TripID, @DestinationID, @Title, @Description, @Day, @Place, @StartTime, @EndTime, NOW())";
                        using (MySqlCommand cmd = new MySqlCommand(activityQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@ActivityID", activity.activityId);
                            cmd.Parameters.AddWithValue("@TripID", tripId);
                            cmd.Parameters.AddWithValue("@DestinationID", activity.destinationId);
                            cmd.Parameters.AddWithValue("@Title", activity.title);
                            cmd.Parameters.AddWithValue("@Description", activity.description);
                            cmd.Parameters.AddWithValue("@Day", activity.day);
                            cmd.Parameters.AddWithValue("@Place", activity.place);
                            cmd.Parameters.AddWithValue("@StartTime", activity.start);
                            cmd.Parameters.AddWithValue("@EndTime", activity.end);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message} {ex.StackTrace}");
            }
            return RedirectToAction("Index", new { success = true });
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
    }

    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                string idToken = request.IdToken;

                if (string.IsNullOrEmpty(idToken))
                {
                    return Json(new { success = false, message = "Log ID token must not be null or empty." });
                }

                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid ?? string.Empty;
                string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"]?.ToString() ?? string.Empty : string.Empty;

                // Store user_id in session
                HttpContext.Session.SetString("UserId", uid);

                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, uid ?? string.Empty),
                new Claim(ClaimTypes.Email, email ?? string.Empty),
            };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Route("Account/StoreUserInDatabase")]
        public async Task<IActionResult> StoreUserInDatabase([FromBody] UserRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Email))
            {
                return Json(new { success = false, message = "User ID and Email must not be empty." });
            }

            string constr = _configuration.GetConnectionString("DefaultConnection");
            using (MySqlConnection conn = new MySqlConnection(constr))
            {
                await conn.OpenAsync();

                string checkQuery = "SELECT COUNT(*) FROM Users WHERE user_id = @UserId";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@UserId", request.UserId);
                    int count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                    if (count == 0)
                    {
                        string insertQuery = "INSERT INTO Users (user_id, email, created_at) VALUES (@UserId, @Email, @CreatedAt)";
                        using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@UserId", request.UserId);
                            insertCmd.Parameters.AddWithValue("@Email", request.Email);
                            insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            return Json(new { success = true });
        }

    }

}

// MOVE THE MODELS NAMESPACE OUTSIDE THE CONTROLLERS NAMESPACE
namespace LoginSystem.Models
{
    public class LoginRequest
    {
        public required string IdToken { get; set; }
    }
}

namespace LoginSystem.Models
{
    public class UserRequest
    {
        public required string UserId { get; set; }
        public required string Email { get; set; }
    }
}


