using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Layouts;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.XA.Foundation.SitecoreExtensions.Repositories;

namespace Sitecore.Support.XA.Foundation.Grid.Commands
{
  public class ShowGridPropertiesDialog: Sitecore.XA.Foundation.Grid.Commands.ShowGridPropertiesDialog
  {
    protected override void UpdateLayout(NameValueCollection contextParameters, FieldEditorOptions fieldEditorOptions)
    {
      string placeholder = contextParameters["placeHolderKey"];
      string uniqueId = Guid.Parse(contextParameters["renderingUid"]).ToString("B").ToUpperInvariant();
      string fieldName = contextParameters["fieldName"];
      LayoutDefinition layoutDefinition = this.GetLayoutDefinition();
      if (layoutDefinition == null)
      {
        this.ReturnLayout(null, null, null);
      }
      else
      {
        string id = ShortID.Decode(WebUtil.GetFormValue("scDeviceID"));
        DeviceDefinition device = layoutDefinition.GetDevice(id);
        if (device == null)
        {
          this.ReturnLayout(null, null, null);
        }
        else
        {
          RenderingDefinition renderingByUniqueId = device.GetRenderingByUniqueId(uniqueId);
          if (renderingByUniqueId == null)
          {
            this.ReturnLayout(null, null, null);
          }
          else
          {
            if (string.IsNullOrEmpty(renderingByUniqueId.Parameters))
            {
              if (!string.IsNullOrEmpty(renderingByUniqueId.ItemID))
              {
                RenderingItem item = Client.ContentDatabase.GetItem(renderingByUniqueId.ItemID);
                renderingByUniqueId.Parameters = (item != null) ? item.Parameters : string.Empty;
              }
              else
              {
                renderingByUniqueId.Parameters = string.Empty;
              }
            }
            NameValueCollection parameters = WebUtil.ParseUrlParameters(renderingByUniqueId.Parameters);
            foreach (FieldDescriptor descriptor in fieldEditorOptions.Fields)
            {
              Item item = ServiceProviderServiceExtensions.GetService<IContentRepository>(ServiceLocator.ServiceProvider).GetItem(descriptor.FieldID);
              if (fieldName == item.Name)
              {
                this.FillGridParameters(contextParameters, device, parameters, fieldName, descriptor.Value);
              }
              else
              {
                parameters[item.Name] = descriptor.Value;
              }
            }
            renderingByUniqueId.Parameters = new UrlString(parameters).GetUrl();
            string layout = WebEditUtil.ConvertXMLLayoutToJSON(layoutDefinition.ToXml());
            this.ReturnLayout(layout, renderingByUniqueId.UniqueId, placeholder);
          }
        }
      }
    }
  }
}