﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CJSBugTracker.Data;
using CJSBugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using CJSBugTracker.Enums;
using CJSBugTracker.Service.Interface;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using CJSBugTracker.Extensions;

namespace CJSBugTracker.Controllers
{
    [Authorize(Roles = nameof(BTRoles.Admin))]
    public class InvitesController : Controller
    {
        private readonly IBTInviteService _inviteService;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyService _companyService;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<BTUser> _userManager;
        private readonly IDataProtector _protector;
        private readonly string _protectorPurpose;

        public InvitesController(IBTInviteService inviteService,
                                 IBTProjectService projectService,
                                 IBTCompanyService companyService,
                                 UserManager<BTUser> userManager,
                                 IEmailSender emailSender,
                                 IDataProtectionProvider protectionProvider)
        {
            _inviteService = inviteService;
            _projectService = projectService;
            _companyService = companyService;
            _userManager = userManager;
            _emailSender = emailSender;

            _protectorPurpose = "TsocBT4056!?";
            _protector = protectionProvider.CreateProtector(_protectorPurpose);
        }

        // GET: Invites


        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {
            List<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());

            ViewData["ProjectId"] = new SelectList(companyProjects, "Id", "Name");

            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,InviteeEmail,InviteeFirstName,InviteeLastName,Message")] Invite invite)
        {
            int companyId = User.Identity!.GetCompanyId();

            ModelState.Remove(nameof(Invite.InvitorId));

            if (ModelState.IsValid)
            {
                try
                {
                    Guid guid = Guid.NewGuid();

                    invite.CompanyToken = guid;
                    invite.CompanyId = companyId;
                    invite.InviteDate = DateTime.UtcNow;
                    invite.InvitorId = _userManager.GetUserId(User);
                    invite.IsValid = true;

                    await _inviteService.AddNewInviteAsync(invite);

                    //encrypting our top secret invite information 
                    string token = _protector.Protect(guid.ToString());
                    string email = _protector.Protect(invite.InviteeEmail);
                    string company = _protector.Protect(companyId.ToString());

                    string? callbackUrl = Url.Action("ProccessInvite", "Invites", new { token, email, company }, Request.Scheme);

                    string body = $@"<h4>You've been invited to jin the bug tracker!</h4><br/>
                                        {invite.Message}<br /><br />
                                         <a href=""{callbackUrl}"">Click here</a> to join our team.";

                    string subject = "You've been invited to join the Bug Tracker";

                    await _emailSender.SendEmailAsync(invite.InviteeEmail!, subject, body);

                    return RedirectToAction("Index", "Home", new { swalMessage = "Invite Sent" });
                }
                catch (Exception)
                {

                    throw;
                }
            }

            List<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());

            ViewData["ProjectId"] = new SelectList(companyProjects, "Id", "Name");

            return View(invite);
        }


        [AllowAnonymous]
        public async Task<IActionResult> ProccessInvite(string? token, string? email, string? company)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(company))
            {
                return NotFound();
            }

            Guid companyToken = Guid.Parse(_protector.Unprotect(token));
            string inviteeEmail = _protector.Unprotect(email);
            int companyId = int.Parse(_protector.Unprotect(company));

            try
            {
                Invite? invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

                if (invite == null)
                {
                    return NotFound();
                }
                return View(invite);

            }

            catch (Exception)
            {

                throw;
            }

        }


    }
}

