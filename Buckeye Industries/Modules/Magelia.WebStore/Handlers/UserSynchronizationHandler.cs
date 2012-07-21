using Magelia.WebStore.Contracts;
using Magelia.WebStore.Services.Contract.Data.Customer;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Tasks.Scheduling;

namespace Magelia.WebStore.Handlers
{
    public class UserSynchronizationHandler : IScheduledTaskHandler
    {
        private IWebStoreServices _webStoreServices;

        public UserSynchronizationHandler(IWebStoreServices webStoreClient)
        {
            this._webStoreServices = webStoreClient;
        }

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == "SychronizeUser")
            {
                IUser user = context.Task.ContentItem.As<IUser>();
                this._webStoreServices.UsingClient(
                    c =>
                    { 
                        Customer customer = c.CustomerClient.GetCustomer(user.UserName, false);
                        if(customer != null)
                        {
                            c.CustomerClient.UpdateCustomer(customer.CustomerId, user.UserName, user.Email, true);
                        }
                    }
                );
            }
        }
    }
}