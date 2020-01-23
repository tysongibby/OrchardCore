using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.ViewModels;

namespace OrchardCore.Lists.Settings
{
    public class ListPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IStringLocalizer S;

        public ListPartSettingsDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<ListPartSettingsDisplayDriver> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(ListPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            return Initialize<ListPartSettingsViewModel>("ListPartSettings_Edit", model =>
            {
                model.ListPartSettings = contentTypePartDefinition.GetSettings<ListPartSettings>();
                model.PageSize = model.ListPartSettings.PageSize;
                model.ContainedContentTypes = model.ListPartSettings.ContainedContentTypes;
                model.ContentTypes = new NameValueCollection();

                foreach (var contentTypeDefinition in _contentDefinitionManager.ListTypeDefinitions())
                {
                    model.ContentTypes.Add(contentTypeDefinition.Name, contentTypeDefinition.DisplayName);
                }
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(ListPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            var model = new ListPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ContainedContentTypes, m => m.PageSize);

            if (model.ContainedContentTypes == null || model.ContainedContentTypes.Length == 0)
            {
                context.Updater.ModelState.AddModelError(nameof(model.ContainedContentTypes), S["At least one content type must be selected."]);
            }
            else
            {
                context.Builder.WithSettings(new ListPartSettings
                {
                    PageSize = model.PageSize,
                    ContainedContentTypes = model.ContainedContentTypes
                });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
