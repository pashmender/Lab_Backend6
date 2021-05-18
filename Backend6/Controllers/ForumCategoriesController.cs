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
using Microsoft.AspNetCore.Authorization;

namespace Backend6.Controllers
{
    [Authorize(Roles = ApplicationRoles.Administrators)]
    public class ForumCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ForumCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ForumCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.ForumCategories.ToListAsync());
        }

        // GET: ForumCategories/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumCategory = await _context.ForumCategories
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumCategory == null)
            {
                return NotFound();
            }

            return View(forumCategory);
        }

        // GET: ForumCategories/Create
        public IActionResult Create()
        {
            return View(new ForumCategoriesCreateModel());
        }

        // POST: ForumCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumCategoriesCreateModel category)
        {

            if (ModelState.IsValid)
            {
                ForumCategory forumCategory = new ForumCategory() {
                    Id = Guid.NewGuid(),
                    Name = category.Name
                };

                _context.Add(forumCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Forums");
            }

            return View(category);
        }

        // GET: ForumCategories/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumCategory = await _context.ForumCategories.SingleOrDefaultAsync(m => m.Id == id);
            if (forumCategory == null)
            {
                return NotFound();
            }

            ForumCategoriesEditModel model = new ForumCategoriesEditModel()
            {
                Name = forumCategory.Name
            };
            return View(model);
        }

        // POST: ForumCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ForumCategoriesEditModel category)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var forumCategory = await _context.ForumCategories
               .SingleOrDefaultAsync(m => m.Id == id);
            
            if (forumCategory == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                forumCategory.Name = category.Name;

                _context.Update(forumCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Forums");
            }
            return View(forumCategory);
        }

        // GET: ForumCategories/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumCategory = await _context.ForumCategories
                .SingleOrDefaultAsync(m => m.Id == id);
            
            if (forumCategory == null)
            {
                return NotFound();
            }

            return View(forumCategory);
        }

        // POST: ForumCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var forumCategory = await _context.ForumCategories.SingleOrDefaultAsync(m => m.Id == id);
            _context.ForumCategories.Remove(forumCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Forums");
        }

        private bool ForumCategoryExists(Guid id)
        {
            return _context.ForumCategories.Any(e => e.Id == id);
        }
    }
}
