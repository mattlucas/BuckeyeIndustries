using Magelia.WebStore.Contracts;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Security;
using Orchard.Services;
using Orchard.Tasks.Scheduling;

namespace Magelia.WebStore.Handlers
{
    public class UserHandler : IContentHandler
    {
        private IWebStoreServices _webStoreServices;
        private IScheduledTaskManager _scheduledTaskManager;
        private IClock _clock;

        public UserHandler(IWebStoreServices webStoreServices, IScheduledTaskManager scheduledTaskManager, IClock clock)
        {
            this._clock = clock;
            this._webStoreServices = webStoreServices;
            this._scheduledTaskManager = scheduledTaskManager;
        }

        public void Activating(ActivatingContentContext context)
        {

        }

        public void Activated(ActivatedContentContext context)
        {

        }

        public void Initializing(InitializingContentContext context)
        {

        }

        public void Creating(CreateContentContext context)
        {

        }

        public void Created(CreateContentContext context)
        {

        }

        public void Loading(LoadContentContext context)
        {

        }

        public void Loaded(LoadContentContext context)
        {

        }

        public void Updating(UpdateContentContext context)
        {

        }

        public void Updated(UpdateContentContext context)
        {
            IUser user = context.ContentManager.Get<IUser>(context.Id);
            if (user != null)
            {
                this._scheduledTaskManager.CreateTask("SychronizeUser", this._clock.UtcNow.AddMinutes(1), context.ContentItem);
            }
        }

        public void Versioning(VersionContentContext context)
        {

        }

        public void Versioned(VersionContentContext context)
        {

        }

        public void Publishing(PublishContentContext context)
        {

        }

        public void Published(PublishContentContext context)
        {

        }

        public void Unpublishing(PublishContentContext context)
        {

        }

        public void Unpublished(PublishContentContext context)
        {

        }

        public void Removing(RemoveContentContext context)
        {

        }

        public void Removed(RemoveContentContext context)
        {
            IUser user = context.ContentManager.Get<IUser>(context.Id);
            if (user != null)
            {
                this._webStoreServices.UsingClient(c => c.CustomerClient.DeleteCustomer(user.UserName));
            }
        }

        public void Indexing(IndexContentContext context)
        {

        }

        public void Indexed(IndexContentContext context)
        {

        }

        public void Importing(ImportContentContext context)
        {

        }

        public void Imported(ImportContentContext context)
        {

        }

        public void Exporting(ExportContentContext context)
        {

        }

        public void Exported(ExportContentContext context)
        {

        }

        public void GetContentItemMetadata(GetContentItemMetadataContext context)
        {

        }

        public void BuildDisplay(BuildDisplayContext context)
        {

        }

        public void BuildEditor(BuildEditorContext context)
        {

        }

        public void UpdateEditor(UpdateEditorContext context)
        {

        }
    }
}