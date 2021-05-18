using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Backend6.Data;
using Backend6.Models;
using Microsoft.AspNetCore.Identity;
using Backend6.Services;
using Microsoft.AspNetCore.Hosting;
using Backend6.Models.ViewModels;
using System.IO;
using System.Net.Http.Headers;

namespace Backend6.Controllers
{
    [Autorize]
    public class ForumMessageAttachmentsController : Controller
    {
        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;
        private readonly IHostingEnvironment hostingEnvironment;

        public ForumMessageAttachmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
            this.hostingEnvironment = hostingEnvironment;
        }   

        // GET: ForumMessageAttachments/Create
        public async Task<IActionResult> Create(Guid? messageId)
        {
            if(messageId == null)
            {
                return NotFound();
            }

            var message = await _context.ForumMessages
                .Include(m => m.ForumTopic)
                .SingleOrDefaultAsync(x => x.Id == messageId);

            if(message == null || !this.userPermissions.CanEditTopicMessage(message))
            {
                return NotFound();
            }

            this.ViewBag.Message = message;

            
            return View(new ForumMessageAttachmentCreateModel());
        }

        // POST: ForumMessageAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? messageId, ForumMessageAttachmentCreateModel model)
        {
            if (messageId == null)
            {
                return NotFound();
            }

            var message = await _context.ForumMessages
                .Include(m => m.ForumTopic)
                .SingleOrDefaultAsync(x => x.Id == messageId);

            if (message == null || !this.userPermissions.CanEditTopicMessage(message))
            {
                return NotFound();
            }

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Trim('"'));
            var fileExt = Path.GetExtension(fileName);

            this.ViewBag.Message = message;

            if(!AllowedExtensions.Contains(fileExt))
            {
                this.ModelState.AddModelError(nameof(model.File), "This file type is prohibited");
            }

            if (ModelState.IsValid)
            {
                var attachment = new ForumMessageAttachment()
                {
                    Id = Guid.NewGuid(),
                    ForumMessageId = message.Id,
                    Created = DateTime.UtcNow,
                    FileName = fileName
                };

                var path = Path.Combine(this.hostingEnvironment.WebRootPath, "attachments", attachment.Id.ToString("N") + fileExt);
                attachment.FilePath = $"/attachments/{attachment.Id:N}{fileExt}";

                using (var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.File.CopyToAsync(fileStream);
                }

                _context.Add(attachment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "ForumTopics", new {id = message.ForumTopicId, forumId = message.ForumTopic.ForumId });
            }
            
            return View(model);
        }

        // GET: ForumMessageAttachments/Delete/5
        public async Task<IActionResult> Delete(Guid? id, Guid? messageId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessageAttachment = await _context.ForumMessageAttachments
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessageAttachment == null)
            {
                return NotFound();
            }

            if (messageId == null)
            {
                return NotFound();
            }

            var message = await _context.ForumMessages
                .Include(m => m.ForumTopic)
                .SingleOrDefaultAsync(x => x.Id == messageId);

            if (message == null || !this.userPermissions.CanEditTopicMessage(message))
            {
                return NotFound();
            }

            this.ViewBag.Message = message;

            return View(forumMessageAttachment);
        }

        // POST: ForumMessageAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid? messageId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessageAttachment = await _context.ForumMessageAttachments
                .SingleOrDefaultAsync(m => m.Id == id);
            
            if (forumMessageAttachment == null)
            {
                return NotFound();
            }

            if (messageId == null)
            {
                return NotFound();
            }

            var message = await _context.ForumMessages
                .Include(m => m.ForumTopic)
                .SingleOrDefaultAsync(x => x.Id == messageId);

            if (message == null || !this.userPermissions.CanEditTopicMessage(message))
            {
                return NotFound();
            }

            var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "attachments", forumMessageAttachment.Id.ToString("N") + Path.GetExtension(forumMessageAttachment.FilePath));
            System.IO.File.Delete(attachmentPath);
            _context.ForumMessageAttachments.Remove(forumMessageAttachment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "ForumTopics", new { id = message.ForumTopicId, forumId = message.ForumTopic.ForumId });
        }

        private bool ForumMessageAttachmentExists(Guid id)
        {
            return _context.ForumMessageAttachments.Any(e => e.Id == id);
        }
    }
}
