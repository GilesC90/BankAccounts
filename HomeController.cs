using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BankAccounts.Models;
using System.Linq;
using System;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;
        public HomeController(MyContext context)
        {
            _context = context;
        }
        public User UserInDB()
        {
            return _context.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("UserId"));
        }
        [HttpGet("")]

        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpPost("create")]
        public IActionResult Register(User user)
        {
            if(ModelState.IsValid)
            {
                if(_context.Users.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");
                
                    // You may consider returning to the View at this point
                    return View("Index");
                }
                // Initializing a PasswordHasher object, providing our User class as its type
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                //Save your user object to the database
                _context.Add(user);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("UserId", (int)user.UserId);

                return Redirect("Home");
            }
            else
            {
                return View("Index");
            }
        }
        [HttpGet("welcomeback")]
        public IActionResult WelcomeBack()
        {
            return View("Login");
        }

        [HttpPost("Login")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                // If no user exists with provided email
                if(userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");

                    return View("Login");
                }
                else
                {
                    // Initialize hasher object
                    var hasher = new PasswordHasher<LoginUser>();
                
                    // verify provided password against hash stored in db
                    var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                
                    // result can be compared to 0 for failure
                    if(result == 0)
                    {
                        // handle failure (this should be similar to how "existing email" is handled)
                        ModelState.AddModelError("Password", "Invalid Email/Password");

                        return View("Login");
                    }
                }
                HttpContext.Session.SetInt32("UserId", (int)userInDb.UserId);
                return Redirect("Home");
            }
            return View("Login");
        }

        [HttpGet("Home")]
        public IActionResult Home()
        {
            User userInDB = UserInDB();
            if(HttpContext.Session.GetInt32("UserId") == null)
                {
                    return RedirectToAction("logout");
                }
            User toRender = _context.Users.Include(transact => transact.Transactions).FirstOrDefault(user => user.UserId == (int)HttpContext.Session.GetInt32("UserId"));
            var bal = _context.Users
                .OrderByDescending(transact => transact.CreatedAt)
                .Where(transact => transact.UserId == toRender.UserId);
            return View("Home", toRender);
        }
        [HttpPost("total")]
        public IActionResult Total(Balance fromForm)
        {
            if(HttpContext.Session.GetInt32("UserId") == null)  
                {
                    return RedirectToAction("logout");
                }
            fromForm.UserId = (int)HttpContext.Session.GetInt32("UserId");
            _context.Add(fromForm);
            _context.SaveChanges();
            return RedirectToAction("Home");

        }

        [HttpGet("logout")]
        public RedirectToActionResult LogOut()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }
    }
}