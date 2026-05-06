// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;

namespace TVBookingMVC.Areas.Identity.Pages.Account
{
    [Authorize(Roles = RoleNames.Admin)]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public List<int> AvailableRooms { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "Room number")]
            [Range(0, 999, ErrorMessage = "Room number must be between 0 and 999")]
            public int RoomNumber { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
                [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = _signInManager != null 
                ? (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList() 
                : new List<AuthenticationScheme>();
            AvailableRooms = await GetAvailableRoomsAsync();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = _signInManager != null 
                ? (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList() 
                : new List<AuthenticationScheme>();
            
            _logger.LogInformation("Register attempt: Email={Email}, Room={Room}", Input.Email, Input.RoomNumber);
            
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Duplicate email found: {Email}", Input.Email);
                    ModelState.AddModelError("Input.Email", "This email is already registered.");
                    AvailableRooms = await GetAvailableRoomsAsync();
                    return Page();
                }

                var existingRoom = await _context.Users.FirstOrDefaultAsync(u => u.RoomNumber == Input.RoomNumber);
                if (existingRoom != null)
                {
                    _logger.LogWarning("Duplicate room registration attempt: Room={Room}, ExistingUser={Email}", Input.RoomNumber, existingRoom.Email);
                    ModelState.AddModelError("Input.RoomNumber", "This room number is already registered to another guest.");
                    AvailableRooms = await GetAvailableRoomsAsync();
                    return Page();
                }

                var user = CreateUser();

                user.RoomNumber = Input.RoomNumber;
                Input.Password = "Password1!";

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                
                try
                {
                    var result = await _userManager.CreateAsync(user, Input.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created: Email={Email}, Room={Room}", Input.Email, Input.RoomNumber);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            TempData["Message"] = $"Registration successful! Guest account created for room {Input.RoomNumber}.";
                            Input = new InputModel();
                            AvailableRooms = await GetAvailableRoomsAsync();
                            return Page();
                        }
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database constraint violation during registration");
                    if (ex.InnerException?.Message.Contains("UNIQUE constraint") == true || 
                        ex.Message.Contains("RoomNumber"))
                    {
                        ModelState.AddModelError("Input.RoomNumber", "This room number is already registered to another guest.");
                    }
                    else if (ex.InnerException?.Message.Contains("UNIQUE constraint") == true ||
                            ex.Message.Contains("Email") || ex.Message.Contains("UserName"))
                    {
                        ModelState.AddModelError("Input.Email", "This email is already registered.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Registration failed. The user or room may already exist.");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            AvailableRooms = await GetAvailableRoomsAsync();
            return Page();
        }

        private static ApplicationUser CreateUser()
        {
            return Activator.CreateInstance<ApplicationUser>();
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        private async Task<List<int>> GetAvailableRoomsAsync()
        {
            var assignedRooms = await _context.Users.Select(u => u.RoomNumber).ToListAsync();
            var assignedSet = assignedRooms.ToHashSet();
            return Enumerable.Range(1, 999).Where(r => !assignedSet.Contains(r)).ToList();
        }
    }
}
