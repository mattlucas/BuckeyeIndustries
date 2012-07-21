using System;
using System.Linq;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.User;
using Magelia.WebStore.Services.Contract.Data.Store;

namespace Magelia.WebStore.Services
{
    public class UserOrdersServices : IUserOrdersServices
    {
        private const String UserOrdersUserModelsStateCategory = "userorders";

        private IUserModelsStateServices _userModelsStateServices;
        private IWebStoreServices _webStoreClientServices;

        private OrdersViewModel.OrdersState GetState(UserOrdersPart part)
        {
            return this._userModelsStateServices.GetFromUserContext<OrdersViewModel.OrdersState>(UserOrdersServices.UserOrdersUserModelsStateCategory, part.Id);
        }

        private void UpdateState(UserOrdersPart part, OrdersViewModel viewModel)
        {
            if ((part.EnablePaging && !viewModel.State.Page.HasValue) || (!part.EnablePaging) || viewModel.State.Page < 1)
            {
                viewModel.State.Page = 1;
            }
            if (part.EnableSorting)
            {
                if (!viewModel.State.SortDirection.HasValue)
                {
                    viewModel.State.SortDirection = OrderSortDirection.Descending;
                }
                if (!viewModel.State.SortExpression.HasValue)
                {
                    viewModel.State.SortExpression = OrderSortExpression.OrderNumber;
                }
            }
            else
            {
                viewModel.State.SortDirection = OrderSortDirection.Descending;
                viewModel.State.SortExpression = OrderSortExpression.OrderNumber;
            }
        }

        private void LoadOrders(UserOrdersPart part, OrdersViewModel viewModel)
        {
            Exception exeption = this._webStoreClientServices.UsingClient(
                c =>
                {
                    OrderList orderList = c.StoreClient.GetOrders(viewModel.State.SortExpression.Value, viewModel.State.SortDirection.Value, part.EnablePaging && part.PageSize.HasValue ? part.PageSize.Value : Int32.MaxValue, viewModel.State.Page.Value - 1, false, false);
                    viewModel.State.PageCount = part.PageSize.HasValue ? (Int32)Math.Ceiling((Decimal)orderList.Count / (Decimal)part.PageSize) : 1;
                    viewModel.AddRange(orderList.Orders);
                }
            );
            if (exeption == null && !viewModel.Any() && viewModel.State.Page > 1)
            {
                viewModel.State.Page = 1;
                this.LoadOrders(part, viewModel);
            }
        }

        public UserOrdersServices(IWebStoreServices webStoreClientServices, IUserModelsStateServices userModelsStateServices)
        {
            this._webStoreClientServices = webStoreClientServices;
            this._userModelsStateServices = userModelsStateServices;
        }

        public OrdersViewModel GetModel(UserOrdersPart part)
        {
            OrdersViewModel viewModel = new OrdersViewModel();
            viewModel.State = this.GetState(part);
            this.UpdateState(part, viewModel);
            this.LoadOrders(part, viewModel);
            return viewModel;
        }


        public void UpdateState(UserOrdersPart part, Nullable<OrderSortDirection> sortDirection, Nullable<OrderSortExpression> sortExpression, Nullable<Int32> page)
        {
            OrdersViewModel.OrdersState state = this.GetState(part);
            if (sortDirection.HasValue)
            {
                state.SortDirection = sortDirection;
            }
            if (sortExpression.HasValue)
            {
                state.SortExpression = sortExpression;
            }
            if (page.HasValue)
            {
                state.Page = page;
            }
        }
    }
}