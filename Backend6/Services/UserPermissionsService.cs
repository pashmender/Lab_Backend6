﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend6.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Backend6.Services
{
    public class UserPermissionsService : IUserPermissionsService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<ApplicationUser> userManager;

        public UserPermissionsService(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        private HttpContext HttpContext => this.httpContextAccessor.HttpContext;

        public Boolean CanEditPost(Post post)
        {
            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            if (this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return true;
            }

            return this.userManager.GetUserId(this.httpContextAccessor.HttpContext.User) == post.CreatorId;
        }

        public Boolean CanEditPostComment(PostComment postComment)
        {
            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            if (this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return true;
            }

            return this.userManager.GetUserId(this.httpContextAccessor.HttpContext.User) == postComment.CreatorId;
        }

        public Boolean CanEditTopic(ForumTopic topic)
        {
            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            if (this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return true;
            }

            return this.userManager.GetUserId(this.httpContextAccessor.HttpContext.User) == topic.CreatorId;
        }

        public Boolean CanEditTopicMessage(ForumMessage message)
        {
            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            if (this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return true;
            }

            return this.userManager.GetUserId(this.httpContextAccessor.HttpContext.User) == message.CreatorId;
        }

        //public Boolean CanEditMessageAttachment(ForumMessageAttachment attachment)
        //{
        //    if (!this.HttpContext.User.Identity.IsAuthenticated)
        //    {
        //        return false;
        //    }
        //
        //    if (this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
        //    {
        //        return true;
        //    }
        //
        //    attachment
        //
        //    return this.userManager.GetUserId(this.httpContextAccessor.HttpContext.User) == attachment.CreatorId;
        //}
    }
}
