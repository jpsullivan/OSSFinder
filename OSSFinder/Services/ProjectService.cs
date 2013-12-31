using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using OSSFinder.Entities;
using OSSFinder.Services.Interfaces;

namespace OSSFinder.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IEntityRepository<ProjectRegistration> _packageRegistrationRepository;
        private readonly IEntityRepository<Project> _packageRepository;

        public ProjectService(
            IEntityRepository<ProjectRegistration> packageRegistrationRepository,
            IEntityRepository<Project> packageRepository)
        {
            _packageRegistrationRepository = packageRegistrationRepository;
            _packageRepository = packageRepository;
        }

        public ProjectRegistration FindProjectRegistrationById(int projectId) 
        {
            return _packageRegistrationRepository.GetAll()
                .Include(pr => pr.Owners)
                .SingleOrDefault(pr => pr.Id == projectId);
        }

        public Project FindProjectByIdAndVersion(int id, string version, bool allowPrerelease = true) 
        {
            throw new NotImplementedException();
        }

        public IQueryable<Project> GetProjectsForListing(bool includePrerelease) 
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Project> FindProjectsByOwner(User user, bool includeUnlisted) 
        {
            throw new NotImplementedException();
        }

        public Project CreateProject(Project project, User user, bool commitChanges = true) 
        {
            var packageRegistration = CreateOrGetProjectRegistration(user, project);
            packageRegistration.Projects.Add(project);

            if (!commitChanges) return project;

            _packageRegistrationRepository.CommitChanges();

            return project;
        }

        public void DeleteProject(int id, string version, bool commitChanges = true) 
        {
            var package = FindProjectByIdAndVersion(id, version);

            if (package == null)
            {
                throw new EntityException("Project with this ID could not be found", id, version);
            }

            var packageRegistration = package.ProjectRegistration;
            _packageRepository.DeleteOnCommit(package);

            if (packageRegistration.Projects.Count == 0)
            {
                _packageRegistrationRepository.DeleteOnCommit(packageRegistration);
            }

            if (commitChanges)
            {
                // we don't need to call _packageRegistrationRepository.CommitChanges() here because 
                // it shares the same EntityContext as _packageRepository.
                _packageRepository.CommitChanges();
            }
        }

        public void PublishProject(string id, string version, bool commitChanges = true) {
            throw new NotImplementedException();
        }

        public void PublishProject(Project project, bool commitChanges = true) {
            throw new NotImplementedException();
        }

        public void MarkProjectUnlisted(Project project, bool commitChanges = true) {
            throw new NotImplementedException();
        }

        public void MarkProjectListed(Project project, bool commitChanges = true) {
            throw new NotImplementedException();
        }

        public void AddProjectOwner(ProjectRegistration package, User user) {
            throw new NotImplementedException();
        }

        public void RemoveProjectOwner(ProjectRegistration package, User user) {
            throw new NotImplementedException();
        }

        public void SetLicenseReportVisibility(Project project, bool visible, bool commitChanges = true) {
            throw new NotImplementedException();
        }

        private ProjectRegistration CreateOrGetProjectRegistration(User currentUser, Project project)
        {
            var projectRegistration = FindProjectRegistrationById(project.Key);

            if (projectRegistration != null && !projectRegistration.Owners.Contains(currentUser))
            {
                throw new EntityException("Project ID not available", project.Key);
            }

            if (projectRegistration != null) return projectRegistration;

            projectRegistration = new ProjectRegistration
            {
                Id = project.Key
            };

            projectRegistration.Owners.Add(currentUser);

            _packageRegistrationRepository.InsertOnCommit(projectRegistration);

            return projectRegistration;
        }
    }
}