using CJSBugTracker.Data;
using CJSBugTracker.Models;
using CJSBugTracker.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace CJSBugTracker.Service
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddHistoryAsync(Ticket? oldTicket, Ticket newTicket, string userId)
        {
            try
            {
                if (oldTicket == null)
                {
                    // create a history item "new ticket created"
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = string.Empty,
                        OldValue = string.Empty,
                        NewValue = string.Empty,
                        Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                        UserId = userId,
                        Description = "New Ticket Creadted"
                    };
                    _context.Add(history);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // check each property  and make a ticket history item for anything 

                    if(!string.Equals(oldTicket.Title, newTicket.Title))
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = nameof(Ticket.Title),
                            OldValue = oldTicket.Title,
                            NewValue = newTicket.Title,
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = $"Ticket Title was changed to {newTicket.Title}"
                        };
                        _context.Add(history);
                    }
                    if (!string.Equals(oldTicket.Title, newTicket.Description))
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = nameof(Ticket.Description),
                            OldValue = oldTicket.Description,
                            NewValue = newTicket.Description,
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = $"Ticket description was changed to {newTicket.Description}"
                        };
                        _context.Add(history);
                    }
                    if (oldTicket.Archived != newTicket.Archived)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = nameof(Ticket.Archived),
                            OldValue = oldTicket.Archived.ToString(),
                            NewValue = newTicket.Archived.ToString(),
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = newTicket.Archived == true ? $"Ticket archived" : "Ticket restored"
                        };
                        _context.Add(history);
                    }
                    if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = nameof(Ticket.TicketType),
                            OldValue = oldTicket.TicketType!.Name,
                            NewValue = newTicket.TicketType!.Name,
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = $"Ticket Title was changed to {newTicket.TicketType!.Name}"
                        };
                        _context.Add(history);
                    }
                    if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = nameof(Ticket.TicketStatus),
                            OldValue = oldTicket.TicketStatus!.Name,
                            NewValue = newTicket.TicketStatus!.Name,
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = $"Ticket status was changed to {newTicket.TicketStatus!.Name}"
                        };
                        _context.Add(history);
                    }
                    if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = nameof(Ticket.TicketPriority),
                            OldValue = oldTicket.TicketPriority!.Name,
                            NewValue = newTicket.TicketPriority!.Name,
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = $"Ticket prioritiy was changed to {newTicket.TicketPriority!.Name}"
                        };
                        _context.Add(history);
                    }
                    if (!string.Equals(oldTicket.DeveloperUserId, newTicket.DeveloperUserId))
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Developer",
                            OldValue = oldTicket.DeveloperUser!.FullName ?? "Unassigned",
                            NewValue = newTicket.DeveloperUser!.FullName ?? "Unassigned",
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = $"Ticket developer was changed to {newTicket.DeveloperUser!.FullName ?? "Unassigned"}"
                        };
                        _context.Add(history);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task AddHistoryAsync(int ticketId, string model, string userId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    return;
                }
                string description = model.ToLower().Replace("ticket", "");
                description = $"New {description} addeddto ticket: {ticket.Title}";

                TicketHistory history = new() 
                {
                    TicketId = ticket.Id,
                    PropertyName = model,
                    OldValue = string.Empty,
                    NewValue = string.Empty,
                    Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                    UserId = userId,
                    Description = description
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketHistory>> GetCompanyTicketHistoriesAsync(int companyId)
        {
            try
            {
                Company? company = await _context.Companies.Include(c => c.Projects)
                                                                .ThenInclude(t => t.Tickets)
                                                                     .ThenInclude(h => h.History)
                                                                           .ThenInclude(u => u.User)
                                                            .FirstOrDefaultAsync();


                if(company is not null)
                {
                    return company.Projects.SelectMany(p=>p.Tickets)
                                           .SelectMany(p=>p.History)
                                           .ToList();

                    
                }
                else
                {
                    return new List<TicketHistory>();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketHistory>> GetProjectTicketHistoriesAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Tickets).ThenInclude(t => t.History).ThenInclude(h => h.User).FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project == null) return new List<TicketHistory>();

                List<TicketHistory> history = project.Tickets.SelectMany(t => t.History).ToList();

                return history;
            }
            catch (Exception)
            {
                throw; 
            }
        }
    }
}
