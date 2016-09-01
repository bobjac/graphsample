using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TodoListService.Models;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using TodoListService.Utils;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using TodoListService.DAL;
//using Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoListService.Controllers
{
   // [Authorize]
    [Route("v1.0/user")]
    public class UserController : Controller
    {
        public IUserTaskRepository UserTaskRepository { get; set; }

        public UserController(IUserTaskRepository userTasks)
        {
            UserTaskRepository = userTasks;
        }

        // GET: v1.0/user
        [HttpGet]
        public Models.UserInfo Get()
        {
            string graphResourceID = "https://graph.windows.net";

            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = "2013-11-08";
            graphSettings.GraphDomainName = "graph.windows.net";

            string clientId = "492ef35b-83ef-4663-9904-00eb0d461512";
            string appKey = "JL4Vg4PIbSAt1ABUrwhoYbFmzhuJKgDsD5HCrCuq5Hk=";

            string signedInUserID = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string tenantID = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            string userObjectID = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            try
            {
                Guid ClientRequestId = Guid.NewGuid();
                ActiveDirectoryClient graphClient = new ActiveDirectoryClient(
                    new Uri(graphResourceID + '/' + tenantID),
                    () => getTokenForGraph(tenantID, signedInUserID, userObjectID, clientId, appKey, graphResourceID));

                var users = graphClient.Users.Where(u => u.ObjectId.Equals(userObjectID)).ExecuteAsync().Result;

                var user = users.CurrentPage.FirstOrDefault();
                if (user != null)
                {
                    var objects = user.GetMemberObjectsAsync(true).Result;
                    var memberships = user.GetMemberGroupsAsync(true).Result;
                }
            }
            catch(Exception ex)
            {
                string status = "Log Error";
            }

            return new Models.UserInfo
            {
                DisplayName = (User.FindFirst(ClaimTypes.NameIdentifier))?.Value,
                GivenName = (User.FindFirst(ClaimTypes.GivenName))?.Value,
                Surname = (User.FindFirst(ClaimTypes.Surname))?.Value,
                MobilePhone = (User.FindFirst(ClaimTypes.MobilePhone))?.Value,
                EmailAddress = (User.FindFirst(ClaimTypes.Email))?.Value,
                Gender = (User.FindFirst(ClaimTypes.Gender))?.Value
            };
        }
        private async Task<string> getTokenForGraph(string tenantID, string signedInUserID, string userObjectID, string clientId, string appKey, string graphResourceID)
        {

            AuthenticationContext authContext = new AuthenticationContext(string.Format("https://login.microsoftonline.com/{0}", tenantID), new NaiveSessionCache(userObjectID, HttpContext.Session));
            ClientCredential credential = new ClientCredential(clientId, appKey);
            // AuthenticationResult result = await authContext.AcquireTokenSilentAsync(graphResourceID, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
            AuthenticationResult result = await authContext.AcquireTokenAsync(graphResourceID, credential);
            #region old token acquisition code
            // get a token for the Graph without triggering any user interaction (from the cache, via multi-resource refresh token, etc)
            //ClientCredential clientcred = new ClientCredential(clientId, appKey);
            // initialize AuthenticationContext with the token cache of the currently signed in user, as kept in the app's EF DB
            //AuthenticationContext authContext = new AuthenticationContext(string.Format("https://login.microsoftonline.com/{0}", tenantID), new EFADALTokenCache(signedInUserID));
            //AuthenticationContext authContext = new AuthenticationContext(string.Format("https://login.microsoftonline.com/{0}", tenantID));
            //AuthenticationResult result = await authContext.AcquireTokenSilentAsync(graphResourceID, clientcred, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
            #endregion

            return result.AccessToken;
        }

        [HttpPost]
        public string PostFile()
        {
            return "URI OF BLOB";
        }

        [HttpGet("tasks")]
        public IEnumerable<UserTask> GetUserTasks()
        {
            string signedInUserID = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            //UserTask[] tasks = new UserTask[]
            //    {
            //        new Controllers.UserTask { CurrentState = "New", DueDate = DateTime.Now.AddDays(5), TaskId = Guid.NewGuid(), TaskName = "Task1", UserId = signedInUserID },
            //        new UserTask { CurrentState = "InProcess", DueDate = DateTime.Now.AddDays(7), TaskId = Guid.NewGuid(), TaskName = "Task1", UserId = signedInUserID }
            //    };

            IEnumerable<UserTask> tasks = UserTaskRepository.GetForUser(signedInUserID);
            return tasks;
        }

        [HttpPost("tasks/create")]
        public void CreateTask([FromBody] UserTask task)
        {
            string signedInUserID = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            string taskName = task.TaskName;

            UserTaskRepository.AddForUser(signedInUserID, task);
            int x = 0;
        }


        //https://localhost:44351/v1.0/user/files
        [HttpGet("files")]
        public Models.UserFiles GetFiles()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=bobjacsilkroad2;AccountKey=7JSPQio+N6pSYGU2JKVcfqNbVMrmRVBSMPY14h36Vvb4IjG8Tvzw1vx1RkzYxOCjhubvFIx5volG/TJjfiGA3A==";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("bobjacsilkroad");
            bool created = container.CreateIfNotExistsAsync().Result;

            BlobContinuationToken token = new BlobContinuationToken();
            var blobs = container.ListBlobsSegmentedAsync(token).Result;

            List<string> files = new List<string>();
            foreach (IListBlobItem item in blobs.Results)
            {
                files.Add(item.Uri.ToString());
            }

            return new UserFiles { Files = files};
        }

        [HttpGet("groups")]
        public IEnumerable<string> GetGroups()
        {
            string graphResourceID = "https://graph.windows.net";

            GraphSettings graphSettings = new GraphSettings();
            graphSettings.ApiVersion = "2013-11-08";
            graphSettings.GraphDomainName = "graph.windows.net";

            string clientId = "492ef35b-83ef-4663-9904-00eb0d461512";
            string appKey = "JL4Vg4PIbSAt1ABUrwhoYbFmzhuJKgDsD5HCrCuq5Hk=";

            string signedInUserID = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string tenantID = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            string userObjectID = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            try
            {
                Guid ClientRequestId = Guid.NewGuid();
                ActiveDirectoryClient graphClient = new ActiveDirectoryClient(
                    new Uri(graphResourceID + '/' + tenantID),
                    () => getTokenForGraph(tenantID, signedInUserID, userObjectID, clientId, appKey, graphResourceID));

                var users = graphClient.Users.Where(u => u.ObjectId.Equals(userObjectID)).ExecuteAsync().Result;

                var user = users.CurrentPage.FirstOrDefault();
                if (user != null)
                {
                    var objects = user.GetMemberObjectsAsync(true).Result;
                    var memberships = user.GetMemberGroupsAsync(true).Result;
                }

                var groups = graphClient.Groups.ExecuteAsync().Result;
            }
            catch (Exception ex)
            {
                string status = "Log Error";
            }

            return new string[] { "TestGroup1", "TestGroup2" };
        }

        // GET api/values/5
      //  [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
