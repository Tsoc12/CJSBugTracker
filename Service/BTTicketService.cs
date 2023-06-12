using CJSBugTracker.Data;
using CJSBugTracker.Enums;
using CJSBugTracker.Models;
using CJSBugTracker.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace CJSBugTracker.Service
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTTicketService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }
        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ArchiveTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                if (ticket.Project.CompanyId == companyId)
                {
                    ticket.Archived = true;

                   

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets.Where(p => p.Project.CompanyId == companyId && p.Archived == true)
                                                         .ToListAsync();

                return tickets;
            }

            catch (Exception)
            {
                throw;
            }


            
        }

         public async Task<Ticket> GetTicketByIdAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket tickets = await _context.Tickets.Where(t => t.Project.CompanyId == companyId)
                                                               .Include(t => t.Project)
                                                               .Include(t => t.TicketPriority)                 
                                                                .FirstOrDefaultAsync(t=>t.Id == ticketId);

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public async Task<List<TicketPriority>> GetTicketPriorities()
        {
            try
            {
                return await _context.TicketPriorities.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets  = await _context.Tickets.Where(t => t.Project.CompanyId == companyId)
                                                                .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<TicketStatus>> GetTicketStatuses()
        {
            try
            {
                return await _context.TicketStatuses.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<TicketType>>GetTicketTypes()
        {
            try
            {
                return await _context.TicketTypes.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task RestoreTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                if (ticket.Project.CompanyId == companyId)
                {
                    ticket.Archived = false;



                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task UpdateTicketAsync(Ticket ticket, int companyId)
        {
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId)
        {
            try
            {
                BTUser? user = await _context.Users.FindAsync(userId);
                if (user is null) return new List<Ticket>();

                if (await _rolesService.IsUserInRole(user, nameof(BTRoles.Admin)))
                {
                    return await GetTicketsByCompanyIdAsync(user.CompanyId);
                }
                else if (await _rolesService.IsUserInRole(user, nameof(BTRoles.ProjectManager)))
                {
                    return await _context.Tickets
                                         .Include(t => t.TicketType)
                                         .Include(t => t.TicketStatus)
                                         .Include(t => t.TicketPriority)
                                         .Include(t => t.SubmitterUser)
                                         .Include(t => t.DeveloperUser)
                                         .Include(t => t.Project)
                                            .ThenInclude(p => p!.Members)
                                         .Where(t => !t.Archived && t.Project!.Members.Any(m => m.Id == userId))
                                         .ToListAsync();
                }
                else
                {
                    return await _context.Tickets
                                         .Include(t => t.TicketType)
                                         .Include(t => t.TicketStatus)
                                         .Include(t => t.TicketPriority)
                                         .Include(t => t.SubmitterUser)
                                         .Include(t => t.DeveloperUser)
                                         .Include(t => t.Project)
                                            .ThenInclude(p => p!.Members)
                                         .Where(t => !t.Archived && (t.DeveloperUserId == userId || t.SubmitterUserId == userId))
                                         .ToListAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

		public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
		{
			try
			{
				await _context.AddAsync(ticketAttachment);
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
		}

        public Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<Ticket?> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket tickets = await _context.Tickets.AsNoTracking().Where(t => t.Project!.CompanyId == companyId)
                                                               .Include(t => t.TicketTypeId)
                                                               .Include(t => t.Project)
                                                               .Include(t => t.TicketPriority)
                                                               .Include(t => t.Description)
                                                               .Include(t => t.Title)
                                                                .FirstOrDefaultAsync(t => t.Id == ticketId);

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AddTicketCommentAsync(TicketComment comment)
        {
            try
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
