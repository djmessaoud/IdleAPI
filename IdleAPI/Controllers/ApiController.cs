using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
using IdleAPI.Data;
using IdleAPI.Models;
using Microsoft.EntityFrameworkCore;
using IdleAPI.Services;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
namespace IdleAPI.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IdleContext _context;
        private readonly ConfigHandler _handler;
        private readonly ILogger _logger;
        public ApiController(IdleContext context, ConfigHandler handler, ILogger<ApiController> logger)
        {
            _context = context;
            _handler = handler;
            _logger = logger;
        }
        //Auth user with access token 
        [HttpGet("auth")]
        public async Task<IActionResult> AuthenticateUser([FromQuery] long user_id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.User_id == user_id);
                //If no user found, let's sign him up
                if (user == null)
                {
                    // Return not found user 
                    // return NotFound("User not found");
                    //Or create new user 
                    var token = Guid.NewGuid().ToString();
                    user = new User
                    {
                        User_id = user_id,
                        Balance = 0,
                        Access_token = token,
                        role = "user"
                    };
                   await _context.Users.AddAsync(user);
                    //Create a new IDLE session for the new user
                    var session = new Session
                    {
                        User_Id = user.User_id,
                        isStarted = false,
                        isComplete = false,
                        value = decimal.Parse(await _handler.GetConfig("reward_value")),
                        progress = 0
                    };
                  await  _context.Sessions.AddAsync(session);
                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        access_token = token
                    });
                }
                else
                {
                    var token = Guid.NewGuid().ToString();
                    user.Access_token = token;
                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        access_token = token
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in AuthenticateUser");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error happened");
            }
        }

        // GET api/me
        [HttpGet("me")]
        public async Task<IActionResult> GetSessionInfo([FromQuery] string access_token)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Access_token == access_token);
                //If no user found with the token provided, we return Unauthorized 
                if (user == null)
                    return Unauthorized("Invalid token");
                //User found and authorized, we look for an active session and calculate progress if found
                else
                {

                    var session = await _context.Sessions.FirstOrDefaultAsync(s => s.User_Id == user.User_id);
                    //If an active session is found, we calculate the progress
                    if (session.isStarted && !session.isComplete)
                    {
                        //Calculate progress
                        var time = (DateTime.UtcNow - session.start_time).TotalSeconds;
                        var progress = (float)(time / _handler.session_time);
                        if (progress > 1)
                            progress = 1;
                        session.progress = progress; // Update progress in the session
                                                     // If progress is 1, we update the session as competed and save changes in db
                        if (progress == 1)
                            session.isComplete = true;
                        await _context.SaveChangesAsync();
                    }
                    // Return the balance and session info
                    return Ok(new
                    {
                        balance = user.Balance,
                        idle = new
                        {
                            isStarted = session.isStarted,
                            isComplete = session.isComplete,
                            value = session.value,
                            progress = session.progress
                        }
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetSessionInfo");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error happened");
            }
        }

        //Better use PUT :))
        // POST api/v1/start
        [HttpPost("start")]
        public async Task<IActionResult> StartSession([FromQuery] string access_token)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Access_token == access_token);
                if (user == null)
                    return Unauthorized("Invalid token");
                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.User_Id == user.User_id);
                if (session.isStarted)
                    return BadRequest("Session already started");
                session.isStarted = true;
                session.start_time = DateTime.UtcNow;
                session.value = decimal.Parse(await _handler.GetConfig("reward_value"));
                session.progress = 0;
                await _context.SaveChangesAsync();
                return Ok(
                    new
                    {
                        isStarted = session.isStarted,
                        isComplete = session.isComplete,
                        value = session.value,
                        progress = session.progress
                    });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in StartSession");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error happened");
            }
        }


        [HttpPost("claim")]
        public async Task<IActionResult> ClaimReward([FromQuery] string access_token)
        {
            try
            {
                //Gettingu user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Access_token == access_token);
                if (user == null)
                    return Unauthorized("Invalid token");
                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.User_Id == user.User_id);
                //Checking if the session is started and completed and if not, we return a bad request
                if (!session.isStarted)
                    return BadRequest("Session not started yet");
                if (!session.isComplete)
                    return BadRequest("Session not finished yet");
                //If the session is started and completed, we update the user balance and reset the session
                user.Balance += session.value; // We can switch for _handler.reward_value if we want to reward the updated value
                //Resetting the session
                session.isStarted = false;
                session.isComplete = false;
                session.progress = 0;
                //Adding the change of balance to the history table
                BalanceLog log = new BalanceLog
                {
                    User_Id = user.Id,
                    value = session.value,
                    change_time = DateTime.UtcNow
                };
               await _context.BalanceHistory.AddAsync(log);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    balance = user.Balance,
                    rewards = new
                    {
                        type = "coin",
                        value = session.value 
                    }
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ClaimReward");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error happened");
            }
        }
        //Function to inject a new/modify config value(s) to the database
        [HttpPut("config")]
        public async Task<IActionResult> SetConfig([FromBody] ConfigRequest RequestBody)
        {
            try
            {
                //Dictionary to store the changed values to return them in the response
                Dictionary<string,string> changedValues = new Dictionary<string, string>();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Access_token == RequestBody.access_token);
                // We check if the user doing the request exists and he's an admin
                if (user == null || user.role !="admin")
                    return Unauthorized();
                //We loop through the config values and update them in the database
                foreach (var item in RequestBody.Config)
                  if ( await  _handler.SetConfig(item.Key, item.Value) == "1") // If the config value is updated successfully 
                        changedValues.Add(item.Key, item.Value); // Add the changed value to the response
                return Ok(changedValues); 
            }
            catch (Exception e) 
            {
                _logger.LogError(e, "Error in SetConfig");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error happened");
            }
        }
    }
    //Config data Put request model 
    public class ConfigRequest
    {
        public string access_token { get; set; }
        public Dictionary<string, string> Config { get; set; }
    }
}



