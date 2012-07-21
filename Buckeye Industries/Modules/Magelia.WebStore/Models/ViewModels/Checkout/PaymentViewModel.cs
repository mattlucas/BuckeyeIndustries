using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Localization;

namespace Magelia.WebStore.Models.ViewModels.Checkout
{
    public class PaymentViewModel : IValidatableObject
    {
        private Localizer _localizer = NullLocalizer.Instance;

        public String BasketHash { get; set; }
        public Boolean AcceptSalesConditions { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!this.AcceptSalesConditions)
            {
                yield return new ValidationResult(this._localizer("Sales conditions must be accepteds").ToString());
            }
        }
    }
}