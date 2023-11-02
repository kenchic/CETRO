using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Cetro.App.Areas.Login
{
    [AllowAnonymous]
    public class Login : PageModel
    {
        public async Task<IActionResult> OnGetAsync(string empresa, string usuario, string clave, string retornoUrl)
        {
            try
            {
                // Clear the existing external cookie
                await HttpContext
                    .SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch { }
            var autenticacionDTO = new AutenticacionDTO
            {
                Empresa = empresa,
                Usuario = usuario,
                Clave = clave,
                Token = string.Empty,
                Terminal = string.Empty,
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.MinValue
            };

            var resultado = ClienteApi.PostRecurso(Configuracion.UrlApiDefender(), "api/Autenticar", autenticacionDTO);
            if (!string.IsNullOrEmpty(resultado))
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UsuarioDTO>>(resultado);
                if (apiResponse.Success)
                {
                    UsuarioDTO usuarioDTO = apiResponse.Data;
                    retornoUrl = Url.Content($"~/{retornoUrl}");

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuarioDTO.Nombre),
                        new Claim(ClaimTypes.Locality, empresa),
                        new Claim(ClaimTypes.Email, usuarioDTO.Correo),
                        new Claim(ClaimTypes.NameIdentifier, usuario)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        RedirectUri = this.Request.Host.Value
                    };
                    try
                    {
                        await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                        return LocalRedirect(retornoUrl);
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                    }
                }
            }

            return NotFound();
        }
    }
}