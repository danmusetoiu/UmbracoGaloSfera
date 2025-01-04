using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Core.Security;


public class UmbLoginController : Controller
{
    private readonly IMemberSignInManager _memberSignInManager;

    public UmbLoginController(IMemberSignInManager memberSignInManager)
    {
        _memberSignInManager = memberSignInManager;
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _memberSignInManager.SignOutAsync();
        return Redirect("/");
    }
}
