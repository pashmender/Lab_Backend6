using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Backend6.Data;
using Backend6.Models;
using Backend6.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Backend6.Services;
using Microsoft.AspNetCore.Authorization;

namespace Backend6.Controllers
{
    [Authorize(Roles = ApplicationRoles.Administrators)]
    public class ForumsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;

        public ForumsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            _context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        }
        [AllowAnonymous]
        // GET: Forums
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ForumCategories.Include(f => f.Forums);

            return View(await applicationDbContext.ToListAsync());
        }

        [AllowAnonymous]
        // GET: Forums/Details/5
        public async Task<IActionResult> Details(Guid? id, Guid? forumCategoryId)
        {
            if (forumCategoryId == null)
            {
                return this.NotFound();
            }

            var forumCategory = await this._context.ForumCategories
                .SingleOrDefaultAsync(x => x.Id == forumCategoryId);

            if (forumCategory == null)
            {
                return this.NotFound();
            }

            if (id == null)
            {
                return NotFound();
            }

            var forum = await _context.Forums
                .Include(f => f.Category)
                .Include(f => f.ForumTopics)  
                .ThenInclude(x => x.ForumMessages)
                .ThenInclude(x => x.Creator)
                .Include(m => m.ForumTopics)
                .ThenInclude(x => x.Creator)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (forum == null)
            {
                return NotFound();
            }

            this.ViewBag.ForumId = id;

            return View(forum);
        }

        // GET: Forums/Create
        public async Task<IActionResult> Create(Guid? forumCategoryId)
        {
            if (forumCategoryId == null)
            {
                return this.NotFound();
            }

            var forumCategory = await this._context.ForumCategories
                .SingleOrDefaultAsync(x => x.Id == forumCategoryId);

            if (forumCategory == null)
            {
                return this.NotFound();
            }

            this.ViewBag.ForumCategoryId = forumCategoryId;
            
            ViewData["CategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name");
            
            return View(new ForumsCreateModel());
        }

        // POST: Forums/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? forumCategoryId, ForumsCreateModel model)
        {
            if(forumCategoryId == null)
            {
                return this.NotFound();
            }

            var forumCategory = await this._context.ForumCategories
                .SingleOrDefaultAsync(x => x.Id == forumCategoryId);

            if(forumCategory == null)
            {
                return this.NotFound();
            }

            if (ModelState.IsValid)
            {

                Forum forum = new Forum()
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Discription = model.Discription,
                    CategoryId = forumCategory.Id,
                    Created = DateTime.UtcNow
                };

                _context.Add(forum);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewData["CategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name");
            return View(model);
        }

        // GET: Forums/Edit/5
        public async Task<IActionResult> Edit(Guid? id, Guid? forumCategoryId)
        {
            if (forumCategoryId == null)
            {
                return this.NotFound();
            }

            var forumCategory = await this._context.ForumCategories
                .SingleOrDefaultAsync(x => x.Id == forumCategoryId);

            if (forumCategory == null)
            {
                return this.NotFound();
            }

            if (id == null)
            {
                return NotFound();
            }

            var forum = await _context.Forums.SingleOrDefaultAsync(m => m.Id == id);
            if (forum == null)
            {
                return NotFound();
            }

            this.ViewBag.forumCategoryId = forumCategoryId;
            ViewData["CategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name", forum.CategoryId);

            ForumsEditModel model = new ForumsEditModel()
            {
                Discription = forum.Discription,
                Name = forum.Name
            };

            return View(model);
        }

        // POST: Forums/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, Guid? forumCategoryId, ForumsEditModel model)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forum = await this._context.Forums
                .SingleOrDefaultAsync(x => x.Id == id);
            
            if (forum == null)
            {
                return NotFound();
            }


            if (forumCategoryId == null)
            {
                return this.NotFound();
            }

            var forumCategory = await this._context.ForumCategories
                .SingleOrDefaultAsync(x => x.Id == forumCategoryId);

            if (forumCategory == null)
            {
                return this.NotFound();
            }

            if (ModelState.IsValid)
            { 
                forum.Name = model.Name;
                forum.Discription = model.Discription;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name");

            return View(model);
        }

        // GET: Forums/Delete/5
        public async Task<IActionResult> Delete(Guid? id, Guid? forumCategoryId)
        {
            if (forumCategoryId == null)
            {
                return this.NotFound();
            }

            var forumCategory = await this._context.ForumCategories
                .SingleOrDefaultAsync(x => x.Id == forumCategoryId);

            if (forumCategory == null)
            {
                return this.NotFound();
            }

            if (id == null)
            {
                return NotFound();
            }

            var forum = await _context.Forums
                .Include(f => f.Category)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forum == null)
            {
                return NotFound();
            }
            this.ViewBag.forumCategoryId = forumCategoryId;
            return View(forum);
        }

        // POST: Forums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid? forumCategoryId)
        {
            var forum = await _context.Forums.SingleOrDefaultAsync(m => m.Id == id);
            _context.Forums.Remove(forum);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ForumExists(Guid id)
        {
            return _context.Forums.Any(e => e.Id == id);
        }
    }
}
