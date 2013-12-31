using System.Collections.Generic;
using System.Linq;
using OSSFinder.Entities;

namespace OSSFinder.Services.Interfaces
{
    public interface IProjectService
    {
        ProjectRegistration FindProjectRegistrationById(int projectId);
        Project FindProjectByIdAndVersion(int id, string version, bool allowPrerelease = true);
        IQueryable<Project> GetProjectsForListing(bool includePrerelease);
        IEnumerable<Project> FindProjectsByOwner(User user, bool includeUnlisted);

        /// <summary>
        /// Populate the related database tables to create the specified project for the specified user.
        /// </summary>
        /// <remarks>
        /// This method doesn't upload the package binary to the blob storage. The caller must do it after this call.
        /// </remarks>
        /// <param name="project">The project to be created</param>
        /// <param name="user">The user who owns this project</param>
        /// <param name="commitChanges">Specifies whether to commit the changes to database.</param>
        /// <returns>The created project entity.</returns>
        Project CreateProject(Project project, User user, bool commitChanges = true);

        /// <summary>
        /// Delete all related data from database for the specified project id and version.
        /// </summary>
        /// <param name="id">Id of the project to be deleted.</param>
        /// <param name="version">Version of the project to be deleted.</param>
        /// <param name="commitChanges">Specifies whether to commit the changes to database.</param>
        void DeleteProject(int id, string version, bool commitChanges = true);

        void PublishProject(string id, string version, bool commitChanges = true);
        void PublishProject(Project project, bool commitChanges = true);

        void MarkProjectUnlisted(Project project, bool commitChanges = true);
        void MarkProjectListed(Project project, bool commitChanges = true);

//        ProjectOwnerRequest CreateProjectOwnerRequest(ProjectRegistration package, User currentOwner, User newOwner);
//        ConfirmOwnershipResult ConfirmProjectOwner(ProjectRegistration package, User user, string token);
        void AddProjectOwner(ProjectRegistration package, User user);
        void RemoveProjectOwner(ProjectRegistration package, User user);

        void SetLicenseReportVisibility(Project project, bool visible, bool commitChanges = true);
    }
}