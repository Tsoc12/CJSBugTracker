using System;
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
using Microsoft.AspNetCore.Identity;
using CJSBugTracker.Service.Interface;

namespace CJSBugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;

        public ProjectsController(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IBTFileService fileService,
                                  IBTProjectService projectService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            BTUser? user = await _userManager.GetUserAsync(User);

            var applicationDbContext = _context.Projects 
                                                .Where(p=> p.CompanyId == user.CompanyId)
                                               .Include(p => p.Company)
                                               .Include(p => p.ProjectPriority);


            return View(await applicationDbContext.ToListAsync());
        }


        public async Task<IActionResult> ArchivedProjects()
        {
            BTUser? user = await _userManager.GetUserAsync(User);

            var applicationDbContext = _context.Projects.Where(p => p.CompanyId == user.CompanyId)
                                                        .Include(p => p.Company);                    
            return View(await applicationDbContext.ToListAsync());

        }








        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
           

            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }
            
            BTUser? user = await _userManager.GetUserAsync(User);

            var project = await _context.Projects
                .Where(p => p.CompanyId == user.CompanyId)
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [HttpGet]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public IActionResult Create()
        {

          

            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]

        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFormFile")] Project project)
        {
            ModelState.Remove("CompanyId");

            if (ModelState.IsValid)
            {
                project.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                BTUser? user = await _userManager.GetUserAsync(User);





                project.CompanyId = user!.CompanyId;

                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                if (User.IsInRole(nameof(BTRoles.ProjectManager)))
                {
                    project.Members.Add(user);
                }


                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        [HttpGet]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFormFile,ImageFileData,ImageFileType")] Project project)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }




            ModelState.Remove("CompanyId");

            if (ModelState.IsValid)
            {


                project.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                BTUser? user = await _userManager.GetUserAsync(User);
                project.CompanyId = user!.CompanyId;

                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                //if(project.Archived == true)
                //{
                //    //archive all tickets inside project by project 
                //    project
                //}
                //else
                //{
                //   //unachieve all tickets in project 
                //}
                

                if(User.IsInRole(nameof(BTRoles.ProjectManager)))
                {
                    project.Members.Add(user);
                }


                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            //var project = await _projectService.ArchiveProjectAsync(project);

            var project = await _context.Projects
               .Include(p => p.Company)
               .Include(p => p.ProjectPriority)
               .FirstOrDefaultAsync(m => m.Id == id);


            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                project.Archived = true;
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
          return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
