using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace codecrafters_http_server
{
    public class RouteHandler
    {
        public static Dictionary<string, Dictionary<string, Func<HttpContext, string>>> Routes { get; private set; } = new();

        public static void RegisterRoute(string Route, string Action, Func<HttpContext, string> Handler)
        {
            var RouteExists = Routes.TryGetValue(Route, out Dictionary<string, Func<HttpContext, string>>? Value);

            if (RouteExists && Value!.ContainsKey(Action)) throw new Exception("Endpoint with same name and action already registered");

            if (RouteExists)
            {
                Routes[Route].Add(Action, Handler);
            }
            else
            {
                Routes.Add(Route, new Dictionary<string, Func<HttpContext, string>>() { { Action, Handler } }) ;
            }
        }

        public static string RouteRequest(HttpContext ctx)
        {
            var RouteExists = Routes.TryGetValue(ctx.Path, out Dictionary<string, Func<HttpContext, string>>? Value);

            if (!RouteExists) throw new Exception("Endpoint not found");

            return Routes[ctx.Path][ctx.Action](ctx);
        }
    }
}
