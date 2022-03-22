using System.Threading.Tasks;
using ApptSched.Models;
using ApptSched.Models.ViewModels;
using ApptSched.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace ApptSched.Controllers
{
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext _db;
        private UserManager<ApplicationUser> _userManger;
        private SignInManager<ApplicationUser> _signInManager;
        private RoleManager<IdentityRole> _roleManager;

        public AccountController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManger = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;

        }

        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid) //server side validation
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = await _userManger.FindByNameAsync(model.Email);
                    HttpContext.Session.SetString("userName",user.Name);
                    //var userName = HttpContext.Session.GetString("userName");
                    return RedirectToAction("Index", "Appt");
                }
                ModelState.AddModelError("","Invalid login attempt"); //generic error for login
            }
            return View(model);
        }
        public async Task<IActionResult> Register()
        {
            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model) //retrieving everything from form via Register page and posting it to database
        {
            if (ModelState.IsValid) //server-side validation
            {
                var user = new ApplicationUser() 
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name
                };

                var result = await _userManger.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManger.AddToRoleAsync(user, model.RoleName);
                    if (!User.IsInRole(Helper.Admin))
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                    }
                    else
                    {
                        //TempData for alerts when new Admin user is created
                        TempData["newAdminSignUp"] = user.Name;
                    }
                    return RedirectToAction("Index", "Appt");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("",error.Description); //login error handler for register functionality
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
