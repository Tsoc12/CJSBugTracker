using CJSBugTracker.Data;
using CJSBugTracker.Enums;
using CJSBugTracker.Models;
using CJSBugTracker.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace CJSBugTracker.Service
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        

        public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        public async Task<bool> AddMemberToProjectAsync(BTUser member, int projectId, int companyId)
        {
            try
            {
                // get the project for this company
                Project? project = await _context.Projects
                                                    .Include(p => p.Members)
                                                    .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                // get the user for this company
                BTUser? developer = await _context.Users.FirstOrDefaultAsync(u => u.Id == member.Id && u.CompanyId == companyId);

                if (project is not null && developer is not null)
                {
                    // make sure the user is actually a PM
                    if (!await _rolesService.IsUserInRole(developer, nameof(BTRoles.Developer))) return false;

                    // remove any potentially existing PM
                    await RemoveProjectManagerAsync(projectId, companyId);

                    // assign the new PM
                    project.Members.Add(developer);
                    // save our changes
                    await _context.SaveChangesAsync();

                    // success!
                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName, int companyId)
        {
            try
            {
                List<BTUser> members = new List<BTUser>();

                Project? project = await _context.Projects
                                                    .Include(p => p.Members)
                                                    .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRole(member ,roleName))
                        {
                            members.Add(member);
                        }
                    }
                }

                return members;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Project>> GetUnassignedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {

                //get all company projects
                List<Project> projects = await _context.Projects.Where(p=>p.CompanyId == companyId).ToListAsync();

                //check all that are unassigned 
                             
                //Make empty list of projects where we can add the unassigned projects

                List<Project> unassignedProjects = new List<Project>();

                // go through each of the project and check if there unassigned 
                foreach (Project project in projects)
                {
                    //go through each member in project

                    bool isUnassigned = true;

                    foreach(BTUser member in project.Members)
                    {
                        // if unassigned is to project
                        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
                           {
                            isUnassigned = false;
                           }
                    }
                    if (isUnassigned == true)
                    {
                        unassignedProjects.Add(project);
                    }
                }
                //return list of projects 
                return unassignedProjects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId, int companyId)
        {
            throw new NotImplementedException();
        }

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception) 
            {
                throw;
            }
        }

        public async Task ArchiveProjectAsync(Project project, int companyId)
        {
            try
            {
              if (project.CompanyId == companyId)
                {
                    project.Archived = true;

                    foreach (Ticket ticket in project.Tickets)
                    { 
                        
                       ticket.ArchivedByProject = !ticket.Archived;

                        ticket.Archived = true;
                    }

                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                                .ToListAsync();

                return projects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority)
        {
            try
            {
                List<Project> projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == false)
                                                                .Include(p => p.Tickets)
                                                                .Include(p => p.ProjectPriority)
                                                                .Include(p => p.Members)
                                                                .Where(p => string.Equals(priority, p.ProjectPriority!.Name))
                                                                .ToListAsync();

                return projects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<Project>> GetAllUserProjectsAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            List<Project> projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true)
                                                            .ToListAsync();

            return projects;
        }

        public async Task<Project?>GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                return await _context.Projects
                                     .Include(p=>p.Company)
                                     .Include(p => p.ProjectPriority)
                                     .Include(p => p.Members)
                                     .Include(p => p.Tickets)
                                             .ThenInclude(p=> p.DeveloperUser)
                                     .Include(p => p.Tickets)
                                              .ThenInclude(p=>p.SubmitterUser)
                                     .FirstOrDefaultAsync(p=>p.Id == projectId && p.CompanyId == companyId);  
                                            
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                

                return await _context.ProjectPriorities.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RestoreProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId == companyId)
                {
                    project.Archived = false;

                    foreach (Ticket ticket in project.Tickets)
                    {

                        ticket.ArchivedByProject = !ticket.Archived;

                        ticket.Archived = false;
                    }

                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task UpdateProjectAsync(Project project, int companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId, int companyId)
        {
            try
            {
                // get the project for this company
                Project? project = await _context.Projects
                                                    .Include(p => p.Members)
                                                    .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                // get the user for this company
                BTUser? projectManager = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

                if (project is not null && projectManager is not null)
                {
                    // make sure the user is actually a PM
                    if (!await _rolesService.IsUserInRole(projectManager, nameof(BTRoles.ProjectManager))) return false;

                    // remove any potentially existing PM
                    await RemoveProjectManagerAsync(projectId, companyId);

                    // assign the new PM
                    project.Members.Add(projectManager);
                    // save our changes
                    await _context.SaveChangesAsync();

                    // success!
                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser?> GetProjectManagerAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                                    .AsNoTracking()
                                                    .Include(p => p.Members)
                                                    .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
                        {
                            return member;
                        }
                    }
                }

                return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                                    .Include(p => p.Members)
                                                    .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
                        {
                            project.Members.Remove(member);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
