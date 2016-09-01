using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using TodoListWebApp.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoListWebApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private const string TodoListBaseAddress = "https://localhost:44351";

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            AuthenticationResult result = null;
            List<TodoItem> itemList = new List<TodoItem>();

            string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new NaiveSessionCache(userObjectID, HttpContext.Session));
            ClientCredential credential = new ClientCredential(Startup.ClientId, Startup.ClientSecret);
            result = await authContext.AcquireTokenSilentAsync(Startup.TodoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, TodoListBaseAddress + "/v1.0/user");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            string userInfo = response.Content.ReadAsStringAsync().Result;
            dynamic jsonResponse = JsonConvert.DeserializeObject(userInfo);

            return View(jsonResponse);
        }

        public async Task<IActionResult> Groups()
        {
            AuthenticationResult result = null;
            List<TodoItem> itemList = new List<TodoItem>();

            string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new NaiveSessionCache(userObjectID, HttpContext.Session));
            ClientCredential credential = new ClientCredential(Startup.ClientId, Startup.ClientSecret);
            result = await authContext.AcquireTokenSilentAsync(Startup.TodoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, TodoListBaseAddress + "/v1.0/user/groups");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            string userGroups = response.Content.ReadAsStringAsync().Result;
            dynamic jsonResponse = JsonConvert.DeserializeObject(userGroups);

            return View(jsonResponse);
        }

        public async Task<IActionResult> Files()
        {
            AuthenticationResult result = null;
            List<TodoItem> itemList = new List<TodoItem>();

            string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new NaiveSessionCache(userObjectID, HttpContext.Session));
            ClientCredential credential = new ClientCredential(Startup.ClientId, Startup.ClientSecret);
            result = await authContext.AcquireTokenSilentAsync(Startup.TodoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, TodoListBaseAddress + "/v1.0/user/files");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            string files = response.Content.ReadAsStringAsync().Result;
            dynamic jsonResponse = JsonConvert.DeserializeObject(files);

            return View(jsonResponse);
        }

        public async Task<IActionResult> CreateTask()
        {
            AuthenticationResult result = null;
            string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new NaiveSessionCache(userObjectID, HttpContext.Session));
            ClientCredential credential = new ClientCredential(Startup.ClientId, Startup.ClientSecret);
            result = await authContext.AcquireTokenSilentAsync(Startup.TodoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

            Models.Task t = new Models.Task
            {
                 CurrentState = "New",
                 DueDate = DateTime.Now.AddDays(30),
                 TaskId = Guid.NewGuid(),
                 TaskName = "Newly created",
                 UserId = userObjectID
            };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(t), System.Text.Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, TodoListBaseAddress + "/v1.0/user/tasks/create");
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                int x = 0;
            }
            else
            {
                int y = 0;
            }

            return View();
        }

        [HttpPost]
        public ActionResult CreateTask( Models.Task task)
        {
            string taskName = task.TaskName;
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Tasks()
        {
            AuthenticationResult result = null;
            List<TodoItem> itemList = new List<TodoItem>();

            string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new NaiveSessionCache(userObjectID, HttpContext.Session));
            ClientCredential credential = new ClientCredential(Startup.ClientId, Startup.ClientSecret);
            result = await authContext.AcquireTokenSilentAsync(Startup.TodoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, TodoListBaseAddress + "/v1.0/user/tasks");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            string files = response.Content.ReadAsStringAsync().Result;
            dynamic jsonResponse = JsonConvert.DeserializeObject(files);

            return View(jsonResponse);
        }
    }
}
