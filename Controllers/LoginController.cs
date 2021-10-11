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
    }
}
