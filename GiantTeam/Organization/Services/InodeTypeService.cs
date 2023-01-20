using GiantTeam.Organization.Etc.Models;
using GiantTeam.UserData.Services;

namespace GiantTeam.Organization.Services
{
    public class InodeTypeService
    {
        private static readonly Dictionary<Guid, IReadOnlyDictionary<string, InodeType>> _inodeTypesDictionaryCache = new();
        private readonly UserDataServiceFactory userDataServiceFactory;

        public InodeTypeService(
            UserDataServiceFactory userDataServiceFactory)
        {
            this.userDataServiceFactory = userDataServiceFactory;
        }

        public async ValueTask<IReadOnlyDictionary<string, InodeType>> FetchInodeTypesDictionaryAsync(Guid organizationId)
        {
            if (!_inodeTypesDictionaryCache.TryGetValue(organizationId, out var result))
            {
                var userDataService = userDataServiceFactory.NewDataService(organizationId);

                var inodeTypesConstraints = await userDataService
                    .ListAsync<Etc.Data.InodeTypeConstraintRecord>();

                var allowedChildGroups = inodeTypesConstraints
                    .GroupBy(o => o.ParentInodeTypeId)
                    .ToDictionary(g => g.Key, g => g.Select(o => o.InodeTypeId).OrderBy(o => o).ToList());

                var allowedParentGroups = inodeTypesConstraints
                    .GroupBy(o => o.InodeTypeId)
                    .ToDictionary(g => g.Key, g => g.Select(o => o.ParentInodeTypeId).OrderBy(o => o).ToList());

                result =
                _inodeTypesDictionaryCache[organizationId] = inodeTypesConstraints
                    .GroupBy(o => o.InodeTypeId)
                    .Select(g => new InodeType()
                    {
                        InodeTypeId = g.Key,
                        AllowedChildNodeTypeIds = allowedChildGroups.TryGetValue(g.Key, out var childIds) ? childIds : new List<string>(),
                        AllowedParentNodeTypeIds = allowedParentGroups.TryGetValue(g.Key, out var parentIds) ? parentIds : new List<string>(),
                    })
                    .ToDictionary(o => o.InodeTypeId);
            }
            return result;
        }

        public async ValueTask<InodeType> FetchInodeTypeAsync(Guid organizationId, string inodeTypeId)
        {
            var inodeTypes = await FetchInodeTypesDictionaryAsync(organizationId);
            var result = inodeTypes.TryGetValue(inodeTypeId, out var value) ?
                value :
                throw new ArgumentException($"The value of the {nameof(inodeTypeId)} argument ({inodeTypeId}) was not found.");
            return result;
        }

        public async ValueTask<bool> CanBeInAsyc(Guid organizationId, string inodeTypeId, string parentInodeTypeId)
        {
            return (await FetchInodeTypeAsync(organizationId, inodeTypeId)).AllowedParentNodeTypeIds.Contains(parentInodeTypeId);
        }

        public async ValueTask<bool> CanContainAsync(Guid organizationId, string inodeTypeId, string childInodeTypeId)
        {
            return (await FetchInodeTypeAsync(organizationId, inodeTypeId)).AllowedChildNodeTypeIds.Contains(childInodeTypeId);
        }
    }
}
