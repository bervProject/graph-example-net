using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace GraphOrgExample.Pages
{
    [Authorize]
    [AuthorizeForScopes(Scopes = new[] { "user.read.all", "Group.Read.All", "Tasks.ReadWrite" })]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly GraphServiceClient _graphServiceClient;

        public IndexModel(GraphServiceClient graphServiceClient, ILogger<IndexModel> logger)
        {
            _graphServiceClient = graphServiceClient;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _graphServiceClient.Me.Request().GetAsync();
            ViewData["name"] = user.DisplayName;
            ViewData["upn"] = user.UserPrincipalName;
            try
            {
                _logger.LogInformation($"Try getting {user.DisplayName} photo");
                using (var photoStream = await _graphServiceClient.Me.Photo.Content.Request().GetAsync())
                {
                    byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                    ViewData["photo"] = Convert.ToBase64String(photoByte);
                }
                _logger.LogDebug($"{user.DisplayName} access index");
            }
            catch (Exception ex)
            {
                ViewData["photo"] = null;
                _logger.LogError(ex, "User seems not found");
            }
            try
            {
                var data = await _graphServiceClient.Me.Todo.Lists.Request().GetAsync();
                ViewData["tasks"] = string.Join(',', data.Select(x => $"{x.DisplayName},data:{x.Id}").ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't get tasks count");
            }

            return Page();
        }
    }
}
