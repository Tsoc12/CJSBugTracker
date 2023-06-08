﻿using CJSBugTracker.Data;
using CJSBugTracker.Models;
using CJSBugTracker.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace CJSBugTracker.Service
{
    public class BTCompanyService : IBTCompanyService
    {
        private readonly ApplicationDbContext _context;

        public BTCompanyService(ApplicationDbContext context)
        { 
            _context = context;
        }
        public async Task<Company?> GetCompanyInfoAsync(int companyId)
        {
            try
            {
                Company? company = await _context.Companies
                                                  .Include(c => c.Members)
                                                  .Include(c => c.Projects)
                                                      .ThenInclude(p => p.Tickets)
                                                  .Include(c => c.Invites)
                                                  .FirstOrDefaultAsync(c => c.Id == companyId);
                return company;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BTUser>> GetCompanyMemberAsync(int companyId)
        {
            try
            {
               List<BTUser> user = await _context.Users.Where(u=>u.CompanyId == companyId).ToListAsync();
                return user;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
