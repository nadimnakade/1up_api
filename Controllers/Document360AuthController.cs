using Newtonsoft.Json;
using PickupAPi.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

public class AppUser
{
    public bool IsLoggedIn { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<string> Roles { get; set; }
    public Dictionary<string, string> Workspaces { get; set; }
}

namespace PickupAPi.Controllers
{
    [RoutePrefix("api/auth")]
    public class Document360AuthController : ApiController
    {
        [HttpPost]
        [Route("productLogin")]
        public HttpResponseMessage ProductLogin(ProductLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Product))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing required fields.");
            }

            AppUser user = AuthenticateAndAuthorizeUser(request.Email, request.Password, request.Product);

            if (user == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    "Invalid User ID, Password, or you do not have access to this product.");
            }

            try
            {
                string docUrl = GetDocument360UrlForProduct(request.Product);
                if (docUrl == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid product specified.");
                }

                string jwtToken = BuildJwt(user);
                string finalUrl = docUrl + "?token=" + jwtToken;

                return Request.CreateResponse(HttpStatusCode.OK, new { redirectTo = finalUrl });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "An error occurred during login.");
            }
        }

        private AppUser AuthenticateAndAuthorizeUser(string email, string password, string product)
        {
            // --- REPLACE THIS WITH YOUR REAL DATABASE LOGIC ---
            if (email.ToLower() == "user.a@example.com" && password == "password123")
            {
                var roles = new List<string> { "nomadix", "trustedwifi" };

                if (roles.Contains(product.ToLower()))
                {
                    return new AppUser
                    {
                        IsLoggedIn = true,
                        Email = "user.a@example.com",
                        FirstName = "User",
                        LastName = "A",
                        Roles = roles,
                        Workspaces = new Dictionary<string, string>
                        {
                            { "trustedwifi", "edit" },
                            { "nomadix", "read" }
                        }
                    };
                }
            }

            return null;
        }

        private string GetDocument360UrlForProduct(string product)
        {
            switch (product.ToLower())
            {
                case "nomadix":
                    return "https://kms.cloud.global/nomadix/jwt/login";
                case "trustedwifi":
                    return "https://kms.cloud.global/trustedwifi/jwt/login";
                default:
                    return null;
            }
        }

        private string BuildJwt(AppUser user)
        {
            var claims = new Dictionary<string, object>
            {
                { "email", user.Email },
                { "given_name", user.FirstName },
                { "family_name", user.LastName },
                { "roles", user.Roles ?? new List<string>() },
                { "workspaces", user.Workspaces ?? new Dictionary<string, string>() },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds() }
            };

            return JwtHelper.Generate(claims);
        }
    }

    public class ProductLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Product { get; set; }
    }
}
