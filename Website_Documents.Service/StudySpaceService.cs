using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class StudySpaceService : IStudySpaceService
{
    private readonly IUnitOfWork _unitOfWork;

    public StudySpaceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StudySpace> CreateSpaceAsync(long userId, string name, string? description, string spaceType = "public")
    {
        var inviteCode = GenerateInviteCode();
        
        var space = new StudySpace
        {
            Name = name,
            Description = description,
            SpaceType = spaceType,
            InviteCode = inviteCode,
            MaxMembers = 50,
            IsActive = true,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var createdSpace = await _unitOfWork.StudySpaces.CreateAsync(space);

            var member = new StudySpaceMember
            {
                SpaceId = createdSpace.Id,
                UserId = userId,
                Role = "owner",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _unitOfWork.StudySpaceMembers.CreateAsync(member);

            await _unitOfWork.CommitTransactionAsync();
            
            return createdSpace;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new InvalidOperationException($"Failed to create study space: {ex.Message}", ex);
        }
    }

    public async Task<StudySpace?> GetSpaceByIdAsync(long spaceId)
    {
        return await _unitOfWork.StudySpaces.GetByIdAsync(spaceId);
    }

    public async Task<StudySpace?> GetSpaceByInviteCodeAsync(string inviteCode)
    {
        return await _unitOfWork.StudySpaces.GetByInviteCodeAsync(inviteCode);
    }

    public async Task<List<StudySpace>> GetUserSpacesAsync(long userId)
    {
        return await _unitOfWork.StudySpaces.GetUserSpacesAsync(userId);
    }

    public async Task<List<StudySpace>> GetPublicSpacesAsync(int page = 1, int pageSize = 20)
    {
        return await _unitOfWork.StudySpaces.GetPublicSpacesAsync(page, pageSize);
    }

    public async Task<bool> JoinSpaceAsync(long spaceId, long userId, string? inviteCode = null)
    {
        var space = await _unitOfWork.StudySpaces.GetByIdAsync(spaceId);
        if (space == null || space.IsActive != true)
            return false;

        // For private rooms, an invite code is required and must match exactly.
        if (space.SpaceType == "private")
        {
            if (string.IsNullOrEmpty(inviteCode) || space.InviteCode != inviteCode)
                return false;
        }

        var isMember = await _unitOfWork.StudySpaceMembers.IsMemberAsync(spaceId, userId);
        if (isMember)
            return true;

        // Only count active members to enforce the max_members cap correctly.
        var activeMemberCount = space.Members?.Count(m => m.IsActive == true) ?? 0;
        if (activeMemberCount >= space.MaxMembers)
            return false;

        var member = new StudySpaceMember
        {
            SpaceId = spaceId,
            UserId = userId,
            Role = "member",
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };
        await _unitOfWork.StudySpaceMembers.CreateAsync(member);
        return true;
    }

    public async Task<bool> LeaveSpaceAsync(long spaceId, long userId)
    {
        var member = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (member == null)
            return false;

        if (member.Role == "owner")
            return false;

        await _unitOfWork.StudySpaceMembers.DeleteAsync(member.Id);
        return true;
    }

    public async Task<bool> IsMemberAsync(long spaceId, long userId)
    {
        return await _unitOfWork.StudySpaceMembers.IsMemberAsync(spaceId, userId);
    }

    public async Task<List<StudySpaceMember>> GetSpaceMembersAsync(long spaceId)
    {
        return await _unitOfWork.StudySpaceMembers.GetSpaceMembersAsync(spaceId);
    }

    public async Task<bool> UpdateMemberRoleAsync(long spaceId, long userId, long targetUserId, string newRole)
    {
        var requester = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (requester == null || requester.Role != "owner")
            return false;

        var target = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, targetUserId);
        if (target == null)
            return false;

        target.Role = newRole;
        await _unitOfWork.StudySpaceMembers.UpdateAsync(target);
        return true;
    }

    public async Task<bool> RemoveMemberAsync(long spaceId, long userId, long targetUserId)
    {
        var requester = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (requester == null || (requester.Role != "owner" && requester.Role != "admin"))
            return false;

        var target = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, targetUserId);
        if (target == null)
            return false;

        if (target.Role == "owner")
            return false;

        await _unitOfWork.StudySpaceMembers.DeleteAsync(target.Id);
        return true;
    }

    public async Task<string> GenerateInviteCodeAsync(long spaceId, long userId)
    {
        var member = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (member == null || member.Role != "owner")
            return string.Empty;

        var space = await _unitOfWork.StudySpaces.GetByIdAsync(spaceId);
        if (space == null)
            return string.Empty;

        space.InviteCode = GenerateInviteCode();
        await _unitOfWork.StudySpaces.UpdateAsync(space);
        return space.InviteCode;
    }

    public async Task<bool> DeleteSpaceAsync(long spaceId, long userId)
    {
        var member = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (member == null || member.Role != "owner")
            return false;

        await _unitOfWork.StudySpaces.DeleteAsync(spaceId);
        return true;
    }

    public async Task<bool> UpdateSpaceAsync(long spaceId, long userId, string? name, string? description, string? spaceType)
    {
        var member = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (member == null || member.Role != "owner")
            return false;

        var space = await _unitOfWork.StudySpaces.GetByIdAsync(spaceId);
        if (space == null)
            return false;

        if (!string.IsNullOrWhiteSpace(name))
            space.Name = name;
        if (description != null)
            space.Description = description;
        if (!string.IsNullOrWhiteSpace(spaceType))
            space.SpaceType = spaceType;

        await _unitOfWork.StudySpaces.UpdateAsync(space);
        return true;
    }

    private string GenerateInviteCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpper();
    }
}
