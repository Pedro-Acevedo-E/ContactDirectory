using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactDirectory.Data;
using Login.Models;

namespace ContactDirectory.Controllers
{
    public class UserController : Controller
    {
        private readonly UserContext _context;

        public UserController(UserContext context)
        {
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            if(UserIsLoggedIn()) {
                return View(await _context.User.ToListAsync());
            }

            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Id,Email,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                var usr = await _context.User
                    .FirstOrDefaultAsync(m => 
                        m.Email == user.Email && m.Password == user.Password
                    );

                if (usr == null) {
                    ViewData["LoginError"] = "Invalid Credentials";
                    return View();
                } else {
                    HttpContext.Session.SetString("_UserSession", usr.Id.ToString());
                    return RedirectToAction("Details", new {id = usr.Id});
                }
            }
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("_UserSession");
            return RedirectToAction(nameof(Login));
        }

        // GET: User/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if(UserIsLoggedIn()) {
                if (id == null) {
                    return NotFound();
                }

                var user = await _context.User
                    .Include(u => u.Contacts) 
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (user == null) {
                    return NotFound();
                }

                return View(user);
            }

            return RedirectToAction(nameof(Login));
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();

                var usr = await _context.User
                    .FirstOrDefaultAsync(m => m.Email == user.Email);

                for (int i = 0; Request.Form.ContainsKey($"NewContacts.{i}.Name"); i++) {
                    Contact contact = new Contact();
                    contact.Name = Request.Form[$"NewContacts.{i}.Name"];
                    contact.LastName = Request.Form[$"NewContacts.{i}.LastName"];
                    contact.PhoneNumber = Request.Form[$"NewContacts.{i}.PhoneNumber"];
                    contact.UserId = usr.Id;
                    contact.CreatedAt = DateTime.Now;
                     _context.Add(contact);
                    await _context.SaveChangesAsync();
                }
                
                return RedirectToAction(nameof(Login));
            }
            return View(user);
        }

        // GET: User/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if(UserIsLoggedIn()) {
                if (id == null) {
                    return NotFound();
                }

                var user = await _context.User
                    .Include(u => u.Contacts) 
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (user == null) {
                    return NotFound();
                }
                
                return View(user);
            }
            
            return RedirectToAction(nameof(Login));
        }

        public IActionResult CreateContact(int? id)
        {
            if(UserIsLoggedIn()) {
                if (id == null) {
                    return NotFound();
                }

                ViewData["CurrentUser"] = id;
                return View();
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> CreateContact([Bind("Name,LastName,PhoneNumber,UserId")] Contact contact)
        {
            if(ModelState.IsValid) {
                if(UserIsLoggedIn()) {
                    contact.CreatedAt = DateTime.Now;
                    _context.Add(contact);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction("Edit", new {id = contact.UserId});
                }

                return RedirectToAction(nameof(Login));
            }
            @ViewData["CurrentUser"] = contact.UserId;
            return View(contact);
        }

        //GET
        public async Task<IActionResult> EditContact(int? id) {
            if(UserIsLoggedIn()) {
                if (id == null) {
                    return NotFound();
                }

                var contact = await _context.Contact
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (contact == null) {
                    return NotFound();
                }
                
                return View(contact);
            }

            return RedirectToAction(nameof(Login));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditContact(int id, [Bind("Id,Name,LastName,PhoneNumber,UserId,CreatedAt")] Contact contact) {
            if(UserIsLoggedIn()) {
                if (id != contact.Id) {
                    return NotFound();
                }

                try {
                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!ContactExists(contact.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }

                return RedirectToAction("Edit", new {id = contact.UserId});
            }

            return RedirectToAction(nameof(Login));
        }

        // GET
        public async Task<IActionResult> DeleteContact(int? id)
        {
            if(UserIsLoggedIn()) {
                if (id == null) {
                    return NotFound();
                }

                var contact = await _context.Contact
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (contact == null) {
                    return NotFound();
                }

                return View(contact);
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContact(int id) {
            if(UserIsLoggedIn()) {
                var contact = await _context.Contact
                    .FindAsync(id);
                
                if (contact != null) {
                    _context.Contact.Remove(contact);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Edit", new {id = contact.UserId});
            }

            return RedirectToAction(nameof(Login));
        }

        private bool UserExists(int id) {
            return _context.User.Any(e => e.Id == id);
        }

        private bool ContactExists(int id) {
            return _context.Contact.Any(e => e.Id == id);
        }

        private bool UserIsLoggedIn() {
            return (HttpContext.Session.GetString("_UserSession") != null);
        }
    }
}
