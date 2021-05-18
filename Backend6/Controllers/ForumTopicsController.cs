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
    [Authorize]
    public class ForumTopicsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;

        public ForumTopicsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            this._context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        }

        // GET: ForumTopics
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ForumTopics.Include(f => f.Creator).Include(f => f.Forum);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ForumTopics/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id, Guid? forumId)
        {
            if (forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this._context.Forums
                .SingleOrDefaultAsync(x => x.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await _context.ForumTopics
                .Include(f => f.Creator)
                .Include(f => f.Forum)
                .Include(x => x.ForumMessages)
                .ThenInclude(y => y.Creator)
                .Include(x => x.ForumMessages)
                .ThenInclude(y => y.Attachments)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (forumTopic == null)
            {
                return NotFound();
            }

            this.ViewBag.Forum = forum;

            return View(forumTopic);
        }

        // GET: ForumTopics/Create
        [Authorize]
        public async Task<IActionResult> Create(Guid? forumId)
        {
            if(forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this._context.Forums
                .SingleOrDefaultAsync(x => x.Id == forumId);

            if(forum == null)
            {
                return this.NotFound();
            }

            this.ViewBag.Forum = forum;

            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["ForumId"] = new SelectList(_context.Forums, "Id", "Name");

            return View(new ForumTopicCreateModel());
        }

        // POST: ForumTopics/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? forumId, ForumTopicCreateModel forumTopic)
        {
            if (forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this._context.Forums
                .SingleOrDefaultAsync(x => x.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            var user = await this.userManager.GetUserAsync(this.HttpContext.User);

            if (ModelState.IsValid)
            {
                ForumTopic topic = new ForumTopic()
                {
                    Id = Guid.NewGuid(),
                    Name = forumTopic.Name,
                    Created = DateTime.UtcNow,
                    CreatorId = user.Id,
                    ForumId = forum.Id
                };

                _context.Add(topic);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details","Forums", new { id = forumId, forumCategoryid = forum.CategoryId});
            }
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["ForumId"] = new SelectList(_context.Forums, "Id", "Name");
            return View(forumTopic);
        }

        // GET: ForumTopics/Edit/5
        public async Task<IActionResult> Edit(Guid? id, Guid? forumId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await _context.ForumTopics.SingleOrDefaultAsync(m => m.Id == id);
            
            if (forumTopic == null || !this.userPermissions.CanEditTopic(forumTopic))
            {
                return NotFound();
            }

            if (forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this._context.Forums
                .SingleOrDefaultAsync(x => x.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            this.ViewBag.Forum = forum;
            this.ViewBag.TopicId = id;

            ForumTopicEditModel model = new ForumTopicEditModel()
            {
                Name = forumTopic.Name
            };
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Name", forumTopic.CreatorId);
            ViewData["ForumId"] = new SelectList(_context.Forums, "Id", "Name", forumTopic.ForumId);
            return View(model);
        }

        // POST: ForumTopics/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Guid? forumId, ForumTopicEditModel topic)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await _context.ForumTopics.SingleOrDefaultAsync(m => m.Id == id);
            
            if (forumTopic == null)
            {
                return NotFound();
            }

            if (forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this._context.Forums
                .SingleOrDefaultAsync(x => x.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            if (ModelState.IsValid)
            {
                forumTopic.Name = topic.Name;
                
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = id, forumId = forum.Id });
            }
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", forumTopic.CreatorId);
            ViewData["ForumId"] = new SelectList(_context.Forums, "Id", "Name", forumTopic.ForumId);
            return View(topic);
        }

        // GET: ForumTopics/Delete/5
        public async Task<IActionResult> Delete(Guid? id, Guid? forumId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await _context.ForumTopics
                .Include(f => f.Creator)
                .Include(f => f.Forum)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumTopic == null)
            {
                return NotFound();
            }

            if (forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this._context.Forums
                .SingleOrDefaultAsync(x => x.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            this.ViewBag.Forum = forum;
            return View(forumTopic);
        }

        // POST: ForumTopics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid? forumId)
        {
            if (forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this._context.Forums
                .SingleOrDefaultAsync(x => x.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }


            var forumTopic = await _context.ForumTopics.SingleOrDefaultAsync(m => m.Id == id);
            _context.ForumTopics.Remove(forumTopic);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Forums", new { id = forumId, forumCategoryid = forum.CategoryId });
        }
    

        private bool ForumTopicExists(Guid id)
        {
            return _context.ForumTopics.Any(e => e.Id == id);
        }
    }
}
