using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using TaskyAPI.Models;

namespace TaskyAPI.Middleware
{
    public class AuthTokenParseFilter : ActionFilterAttribute
    {
        public AuthTokenParseFilter()
        {

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var Request = context.HttpContext.Request;
            if (Request.Headers.Keys.Contains("Authorization"))
            {
                StringValues values;

                if (Request.Headers.TryGetValue("Authorization", out values))
                {
                    var jwt = values.ToString();

                    if (jwt.Contains("Bearer"))
                    {
                        jwt = jwt.Replace("Bearer", "").Trim();
                    }

                    var handler = new JwtSecurityTokenHandler();

                    JwtSecurityToken token = handler.ReadJwtToken(jwt);

                    var iter = 0;
                    foreach (var cvalues in token.Payload.Keys)
                    {
                        if (cvalues == "id")
                        {
                            int value = Int32.Parse(token.Payload.Values.ElementAt(iter).ToString());
                            context.HttpContext.Items.Add("user_id", value);
                        }
                        iter++;
                    }
                }
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {

            base.OnActionExecuted(context);
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {

            base.OnResultExecuting(context);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {

            base.OnResultExecuted(context);
        }
    }
}