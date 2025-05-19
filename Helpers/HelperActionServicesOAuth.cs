using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ZuvoPetApiAWS.Helpers
{
    public class HelperActionServicesOAuth
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }
        public HelperActionServicesOAuth(string issuer, string audience, string secretKey)
        {
            this.Issuer = issuer;
            this.Audience = audience;
            this.SecretKey = secretKey;
        }
        // Necesitamos un método para generar el token
        // dicho token se basa en nuestro secret key
        public SymmetricSecurityKey GetKeyToken()
        {
            // Convertimos el scret key a bytes
            byte[] data = Encoding.UTF8.GetBytes(this.SecretKey);
            // Devolvemos la key generada a partir de los bytes
            return new SymmetricSecurityKey(data);
        }
        // Esta clase la hemos creado también para quitar código
        // del program
        public Action<JwtBearerOptions> GetJwtBearerOptions()
        {
            Action<JwtBearerOptions> options =
                new Action<JwtBearerOptions>(options =>
                {
                    // Indicamos que debemos validar para el token
                    options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = this.Issuer,
                        ValidAudience = this.Audience,
                        IssuerSigningKey = this.GetKeyToken()
                    };
                });
            return options;
        }
        // Toda seguridad siempre está basada en un schema
        public Action<AuthenticationOptions> GetAuthenticateSchema()
        {
            Action<AuthenticationOptions> options =
                new Action<AuthenticationOptions>(options =>
                {
                    options.DefaultScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                });
            return options;
        }
    }
}
