using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Users.Models;

namespace Magelia.WebStore.Models.ViewModels.User
{
    public class AddressViewModel : IValidatableObject
    {
        private IEnumerable<SelectListItem> _countries;
        private IEnumerable<SelectListItem> _regions;

        public Nullable<Guid> AddressId { get; set; }

        public IEnumerable<SelectListItem> Countries
        {
            get
            {
                if (this._countries == null)
                {
                    this._countries = Enumerable.Empty<SelectListItem>();
                }
                return this._countries;
            }
            set
            {
                this._countries = value;
            }
        }

        public IEnumerable<SelectListItem> Regions
        {
            get
            {
                if (this._regions == null)
                {
                    this._regions = Enumerable.Empty<SelectListItem>();
                }
                return this._regions;
            }
            set
            {
                this._regions = value;
            }
        }

        public Boolean PromptShippingAddressIsDifferent { get; set; }
        public Boolean ShippingAddressIsDifferent { get; set; }
        public Boolean DisplayNexButton { get; set; }
        public Boolean Named { get; set; }
        public Boolean PromptEmail { get; set; }

        [StringLength(50)]
        public String Name { get; set; }
        [Required]
        [StringLength(50)]
        public String FirstName { get; set; }
        [StringLength(50)]
        public String MiddleName { get; set; }
        [Required]
        [StringLength(50)]
        public String LastName { get; set; }
        [StringLength(50)]
        public String Company { get; set; }
        [Required]
        [StringLength(50)]
        public String Line1 { get; set; }
        [StringLength(50)]
        public String Line2 { get; set; }
        [StringLength(50)]
        public String Line3 { get; set; }
        [StringLength(50)]
        public String Floor { get; set; }
        [StringLength(50)]
        public String DigiCode { get; set; }
        [Required]
        [StringLength(50)]
        public String City { get; set; }
        [Required]
        [StringLength(20)]
        public String ZipCode { get; set; }
        [Required]
        public Nullable<Int32> CountryId { get; set; }
        public Nullable<Guid> RegionId { get; set; }
        [StringLength(50)]
        public String PhoneNumber { get; set; }
        [StringLength(50)]
        public String MobileNumber { get; set; }
        [StringLength(50)]
        public String FaxNumber { get; set; }
        [StringLength(50)]
        public String Email { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            Localizer localizer = NullLocalizer.Instance;
            Boolean hasEmail = !String.IsNullOrEmpty(this.Email);
            if (this.Named && String.IsNullOrEmpty(this.Name))
            {
                yield return new ValidationResult(localizer("Name is required").ToString());
            }
            if (this.PromptEmail && !hasEmail)
            {
                yield return new ValidationResult(localizer("Email is required").ToString());
            }
            if (this.PromptEmail && hasEmail && !Regex.IsMatch(this.Email ?? "", UserPart.EmailPattern, RegexOptions.IgnoreCase))
            {
                yield return new ValidationResult(localizer("Invalid email address").ToString());
            }
        }
    }
}