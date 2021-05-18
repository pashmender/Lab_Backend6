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

namespace Backend6.Controllers
{
    [Autorize]
    public class ForumMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;


        public ForumMessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            this._context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        } 

        // GET: ForumMessages/Create
        public async Task<IActionResult> Create(Guid? topicId)
        {
            if(topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this._context.ForumTopics
                .SingleOrDefaultAsync(x => x.Id == topicId);

            if(topic == null)
            {
                return this.NotFound();
            }

            this.ViewBag.Topic = topic;

            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Name");
            ViewData["ForumTopicId"] = new SelectList(_context.ForumTopics, "Id", "CreatorId");
            
            return View(new ForumMessageCreateModel());
        }

        // POST: ForumMessages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? topicId, ForumMessageCreateModel forumMessage)
        {
            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this._context.ForumTopics
                .SingleOrDefaultAsync(x => x.Id == topicId);

            if (topic == null)
            {
                return this.NotFound();
            }

            var user = await this.userManager.GetUserAsync(this.HttpContext.User);

            if (ModelState.IsValid)
            {
                var message = new ForumMessage()
                {
                    Created = DateTime.UtcNow,
                    Creator = user,
                    Modified = DateTime.UtcNow,
                    Text = forumMessage.Text,
                    ForumTopicId = topic.Id
                };

                _context.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "ForumTopics", new { id = topic.Id, forumId = topic.ForumId});
            }
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ForumTopicId"] = new SelectList(_context.ForumTopics, "Id", "CreatorId");
            return View(forumMessage);
        }

        // GET: ForumMessages/Edit/5
        public async Task<IActionResult> Edit(Guid? id, Guid? topicId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessage = await _context.ForumMessages.SingleOrDefaultAsync(m => m.Id == id);
            
            if (forumMessage == null)
            {
                return NotFound();
            }

            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this._context.ForumTopics
                .SingleOrDefaultAsync(x => x.Id == topicId);

            if (topic == null)
            {
                return this.NotFound();
            }

            this.ViewBag.Topic = topic;
            this.ViewBag.MessageId = id;

            var message = new ForumMessageEditModel
            {
                Text = forumMessage.Text
            };

            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Name", forumMessage.CreatorId);
            ViewData["ForumTopicId"] = new SelectList(_context.ForumTopics, "Id", "CreatorId", forumMessage.ForumTopicId);
            return View(message);
        }

        // POST: ForumMessages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid messageId,Guid? topicId, ForumMessageEditModel forumMessage)
        {
            if (messageId == null)
            {
                return NotFound();
            }

            var message = await this._context.ForumMessages
                .SingleOrDefaultAsync(x => x.Id == messageId);
            
            if(message == null)
            {
                return NotFound();
            }

            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this._context.ForumTopics
                .SingleOrDefaultAsync(x => x.Id == topicId);

            if (topic == null)
            {
                return this.NotFound();
            }

            if (ModelState.IsValid)
            {
                message.Text = forumMessage.Text;
                message.Modified = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "ForumTopics", new { id = topic.Id, forumId = topic.ForumId});
            }
            
            return View(forumMessage);
        }

        // GET: ForumMessages/Delete/5
        public async Task<IActionResult> Delete(Guid? id, Guid? topicId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessage = await _context.ForumMessages
                .Include(f => f.Creator)
                .Include(f => f.ForumTopic)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessage == null)
            {
                return NotFound();
            }

            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this._context.ForumTopics
                .SingleOrDefaultAsync(x => x.Id == topicId);

            if (topic == null)
            {
                return this.NotFound();
            }

            this.ViewBag.Topic = topic;
            this.ViewBag.MessageId = id;
            return View(forumMessage);
        }

        // POST: ForumMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid? topicId)
        {
            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this._context.ForumTopics
                .SingleOrDefaultAsync(x => x.Id == topicId);

            if (topic == null)
            {
                return this.NotFound();
            }

            var forumMessage = await _context.ForumMessages.SingleOrDefaultAsync(m => m.Id == id);
            _context.ForumMessages.Remove(forumMessage);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "ForumTopics", new { id = topic.Id, forumId = topic.ForumId });
        }

        private bool ForumMessageExists(Guid id)
        {
            return _context.ForumMessages.Any(e => e.Id == id);
        }
    }
}
