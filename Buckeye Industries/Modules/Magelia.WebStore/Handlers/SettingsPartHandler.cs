using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;

namespace Magelia.WebStore.Handlers
{
    public class SettingsPartHandler : ContentHandler
    {
        private Localizer _localizer;

        public SettingsPartHandler(IRepository<SettingsPartRecord> repository)
        {
            this._localizer = NullLocalizer.Instance;
            this.Filters.Add(StorageFilter.For(repository));
            this.Filters.Add(new ActivatingFilter<SettingsPart>("Site"));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType.Equals("Site"))
            {
                base.GetItemMetadata(context);
                context.Metadata.EditorGroupInfo.Add(new GroupInfo(this._localizer("Magelia WebStore")) { Id = "WebStoreSettings" });
            }
        }
    }
}