using System;
using FitnessCenterApp.ClientService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.ClientService.Infrastructure.Persistence.Repositories;

public class MembershipRepository
{
	public MembershipRepository()
	{
		using var context = new ClientDbContext();
		context.Database.Migrate();
	}

	public IEnumerable<Membership> GetAll()
	{
		using var context = new ClientDbContext();
		return [.. context.Memberships.AsNoTracking()];
	}

	public Membership? GetById(Guid id)
	{
		using var context = new ClientDbContext();
		return context.Memberships.AsNoTracking().FirstOrDefault(m => m.Id.Equals(id));
	}

	public Membership Create(Membership membership)
	{
		using var context = new ClientDbContext();
		context.Memberships.Add(membership);
		context.SaveChanges();
		return membership;
	}

	public Membership? Update(Guid id, Membership updatedMembership)
	{
		using var context = new ClientDbContext();
		var existingMembership = context.Memberships.FirstOrDefault(p => p.Id == id);
		if (existingMembership == null)
		{
			return null;
		}

		existingMembership.ClientName = updatedMembership.ClientName;
		existingMembership.StartDate = updatedMembership.StartDate;
		existingMembership.EndDate = updatedMembership.EndDate;
		existingMembership.Type = updatedMembership.Type;

		context.SaveChanges();
		return existingMembership;
	}

	public bool Delete(Guid id)
	{
		using var context = new ClientDbContext();
		var membershipToRemove = context.Memberships.FirstOrDefault(m => m.Id.Equals(id));
		if (membershipToRemove == null)
		{
			return false;
		}
		context.Memberships.Remove(membershipToRemove);
		context.SaveChanges();
		return true;
	}
}
