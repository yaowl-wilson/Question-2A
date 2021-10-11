using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessagingProducerAPI.Model;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using test_kube_producer.Service;

namespace MessagingProducerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly LoginContext _context;
        private readonly ILogger _logger;
        private readonly IMessageService _messageService;

        public LoginController(
            LoginContext context,
            ILogger<LoginController> logger,
            IMessageService messageService)
        {
            _context = context;
            _logger = logger;
            _messageService = messageService;
        }

        private async Task<String> Authenticate(Login login)
        {
            String token = null;

            HttpClient client = null;
            HttpResponseMessage response = null;

            try
            {
                string apiCallURL = "https://reqres.in/api/login";

                using (client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiCallURL);
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("email", login.email),
                        new KeyValuePair<string, string>("password", login.password)
                    });

                    Debug.WriteLine(content);

                    using (response = (HttpResponseMessage)await client.PostAsync(apiCallURL, content))
                    {
                        var json_results = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(json_results);
                        _logger.LogInformation("json_results: " + json_results);

                        if (!String.IsNullOrEmpty(json_results))
                        {
                            dynamic json = JObject.Parse(json_results);
                            token = json.token;
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurs while authenticate user");
                return null;
            }
            return token;
        }

        [HttpPost]
        [Route("task")]
        public async Task<ActionResult<Login>> PostLogin(Login login)
        {
            Debug.WriteLine("email: " + login.email);
            Debug.WriteLine("password: " + login.password);

            try
            {
                string token = await Authenticate(login);
                Debug.WriteLine("token: " + token);

                if (!String.IsNullOrEmpty(token))
                {
                    _messageService.enqueue(
                        message: token,
                        queue: "TaskQueue",
                        exchange: "",
                        routingKey: "TaskQueue");
                }
                else 
                {
                    return Unauthorized();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurs while authenticate user");
                return NotFound();
            }
            return Ok();
        }

        /*
        [HttpPost]
        public async Task<ActionResult> AuthenticateUser(Login login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return NotFound();
                }
                else
                {
                    return Ok();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurs while authenticate user");
                return NotFound();
            }
        }
        */

        // GET: api/Login
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Login>>> GetTodoItems()
        {
            return await _context.TodoItems.ToListAsync();
        }

        // GET: api/Login/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Login>> GetLogin(int id)
        {
            var login = await _context.TodoItems.FindAsync(id);

            if (login == null)
            {
                return NotFound();
            }

            return login;
        }

        // PUT: api/Login/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogin(int id, Login login)
        {
            if (id != login.userID)
            {
                return BadRequest();
            }

            _context.Entry(login).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoginExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Login
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /*
        [HttpPost]
        public async Task<ActionResult<Login>> PostLogin(Login login)
        {
            _context.TodoItems.Add(login);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLogin", new { id = login.userID }, login);
        }
        */

        // DELETE: api/Login/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogin(int id)
        {
            var login = await _context.TodoItems.FindAsync(id);
            if (login == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(login);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LoginExists(int id)
        {
            return _context.TodoItems.Any(e => e.userID == id);
        }
    }
}
