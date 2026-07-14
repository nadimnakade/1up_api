using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens; // <-- Primary namespace for this version
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

// This is a simple class to represent your user.
// In your real app, this would be your User model from your database or session.
public class AppUser
{
    public bool IsLoggedIn { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

namespace PickupAPi.Controllers
{

    [RoutePrefix("api/auth")]
    public class Document360AuthController : ApiController
    {
        // --- (Keep your existing 'GetAuthenticatedUser' and 'RedirectToDocs' methods) ---

        // --- NEW METHOD FOR YOUR LOGIN PAGE ---
        [HttpPost]
        [Route("productLogin")]
        public HttpResponseMessage ProductLogin(ProductLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Product))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing required fields.");
            }

            // 1. Authenticate user AND check if they are authorized for the product
            AppUser user = AuthenticateAndAuthorizeUser(request.Email, request.Password, request.Product);

            if (user == null)
            {
                // Use a generic message for security
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid User ID, Password, or you do not have access to this product.");
            }

            try
            {
                // 2. User is valid, get the Document360 URL for the product they asked for
                string docUrl = GetDocument360UrlForProduct(request.Product);
                if (docUrl == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid product specified.");
                }

                // 3. Generate a JWT for this user (with ALL their roles)
                string jwtToken = GenerateJwtToken(user);

                // 4. Construct the final URL and send it back to the JavaScript
                string finalUrl = $"{docUrl}?token={jwtToken}";

                return Request.CreateResponse(HttpStatusCode.OK, new { redirectTo = finalUrl });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred during login.");
            }
        }

        // --- NEW HELPER METHODS ---

        /// <summary>
        /// Your new function to check credentials AND product access.
        /// </summary>
        private AppUser AuthenticateAndAuthorizeUser(string email, string password, string product)
        {
            // --- REPLACE THIS WITH YOUR REAL DATABASE LOGIC ---
            // 1. Check if email and password are valid
            // Example: var dbUser = YourDb.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == Hash(password));

            // For this example, we'll hardcode a user
            if (email.ToLower() == "user.a@example.com" && password == "password123")
            {
                // This user has access to both
                var roles = new List<string> { "nomadix", "trustedwifi" };

                // 2. Check if they are authorized for the *requested* product
                if (roles.Contains(product.ToLower()))
                {
                    return new AppUser
                    {
                        IsLoggedIn = true,
                        Email = "user.a@example.com",
                        FirstName = "User",
                        LastName = "A",
                        Roles = roles
                    };
                }
            }

            // If login fails or they don't have access to the product, return null.
            return null;
        }

        /// <summary>
        /// Gets the correct JWT login URL for the specified product.
        /// </summary>
        private string GetDocument360UrlForProduct(string product)
        {
            switch (product.ToLower())
            {
                case "nomadix":
                    return "https://kms.cloud.global/nomadix/jwt/login";
                case "trustedwifi":
                    return "https://kms.cloud.global/trustedwifi/jwt/login";
                default:
                    return null; // Invalid product
            }
        }

        /// <summary>
        /// Centralized function to generate a JWT token.
        /// </summary>
        private string GenerateJwtToken(AppUser user)
        {
            const string DOCUMENT360_JWT_SECRET = "PASTE_YOUR_DOCUMENT360_JWT_SECRET_HERE";

            var secretBytes = System.Text.Encoding.UTF8.GetBytes(DOCUMENT360_JWT_SECRET);
            var securityKey = new InMemorySymmetricSecurityKey(secretBytes);

            var claims = new List<Claim>
            {
                new Claim("email", user.Email),
                new Claim("given_name", user.FirstName),
                new Claim("family_name", user.LastName)
            };

            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim("roles", role));
                }
            }

            var credentials = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256Signature,
                SecurityAlgorithms.Sha256Digest);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }

    public class AppUser
    {
        public bool IsLoggedIn { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> GroupIDs { get; set; } // Renamed from Roles
    }
    public class ProductLoginRequest
    {
        public string Email
        {
            get; set;
        }
        public string Password
        {
            get; set;
        }
        public string Product
        {
            get; set;
        } // This will be "nomadix" or "trustedwifi"
    }
}
