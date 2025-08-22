using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using RestSharp;
using Revue_Crafters.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Revue_Crafters
{
    [TestFixture]
    public class RevueCraftersTests
    {
        private RestClient client;
        private string accessToken;

        private static string lastRevueId;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            accessToken = GlobalConstants.AuthenticateUser("user@example.com", "string");
        }

        [TearDown]
        public void Dispose()
        {
            client.Dispose();
        }

        [Test, Order(1)]
        public void AddRevue_Test()
        {
            var addRevueRequest = new RestRequest("Revue/Create", Method.Post);
            addRevueRequest.AddHeader("Authorization", $"Bearer {accessToken}");
            var newRevue = new RevueDto
            {
                Title = "DTO Test Revue",                     
                Description = "This is a test created with DTO", 
                Url = ""                                      
            };

            addRevueRequest.AddJsonBody(newRevue);

            var addRevueResponse = client.Execute(addRevueRequest);
            addRevueRequest.AddHeader("Authorization", $"Bearer {accessToken}");
            var addRevueResponseMessage = JObject.Parse(addRevueResponse.Content)["msg"]?.ToString();

            Assert.That(addRevueResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(addRevueResponseMessage, Is.EqualTo("Successfully created!"));
        }

        [Test, Order(2)]
        public void GetAllRevues_Test()
        {
            var getRequest = new RestRequest("Revue/All", Method.Get);
            getRequest.AddHeader("Authorization", $"Bearer {accessToken}");
            var getResponse = client.Execute(getRequest);

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getResponse.Content, Is.Not.Null.Or.Empty);

            var revues = JArray.Parse(getResponse.Content);
            Assert.IsNotEmpty(revues, "The revues array should not be empty");

            var lastRevue = revues.Last();
            lastRevueId = lastRevue["id"]?.ToString();
            Assert.IsNotNull(lastRevueId, "Last revue ID should not be null");

            TestContext.WriteLine($"Last created Revue ID: {lastRevueId}");
        }

        [Test, Order(3)]
        public void Edit_Test()
        {
            var editRequest = new RestRequest("Revue/Edit", Method.Put);
            editRequest.AddHeader("Authorization", $"Bearer {accessToken}");

            editRequest.AddQueryParameter("revueId", lastRevueId);
            Assert.IsNotNull(lastRevueId, "Last revue ID should not be null");

            var updatedRevue = new RevueDto
            {
                Title = "Updated Test Revue",
                Description = "Updated Description",
                Url = ""
            };

            editRequest.AddJsonBody(updatedRevue);

            var editResponse = client.Execute(editRequest);
            var editResponseMessage = JObject.Parse(editResponse.Content)["msg"]?.ToString();

            Assert.That(editResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponseMessage, Is.EqualTo("Edited successfully"));
        }

        [Test, Order(4)]
        public void Delet_Test()
        {
            var deleteRequest = new RestRequest("Revue/Delete", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {accessToken}");
            deleteRequest.AddQueryParameter("revueId", lastRevueId);
            Assert.IsNotNull(lastRevueId, "Last revue ID should not be null");

            var deleteResponse = client.Execute(deleteRequest);
            var deleteResponseMessage = JObject.Parse(deleteResponse.Content)["msg"]?.ToString();

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteResponseMessage, Is.EqualTo("The revue is deleted!"));
        }

        [Test, Order(5)]

        public void CreateRevueWithoutRequiredFields_Test()
        {
            var addRequest = new RestRequest("Revue/Create", Method.Post);
            addRequest.AddHeader("Authorization", $"Bearer {accessToken}");

            var incompleteRevue = new
            {
                Url = "https://example.com/image.jpg",
                Description = "string"
            };
            
            addRequest.AddJsonBody(incompleteRevue);

            var addResponse = client.Execute(addRequest);
            Assert.That(addResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void DeleteNonExistingRevue_Test()
        {
            var deleteRequest = new RestRequest("Revue/Delete", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {accessToken}");

            var fakeRevueId = "00000000-0000-0000-0000-000000000000";
            deleteRequest.AddQueryParameter("revueId", fakeRevueId);

            var deleteResponse = client.Execute(deleteRequest);
            var deleteResponseMessage = JObject.Parse(deleteResponse.Content)["msg"]?.ToString();

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deleteResponseMessage, Is.EqualTo("There is no such revue!"));

        }

    }
}
