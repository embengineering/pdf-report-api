using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PdfReport.Api.Infrastructure
{
    public static class ControllerExtensions
    {
        public static Task<string> RenderViewAsync<TModel>(this Controller controller, TModel model, bool partial = false)
        {
            var viewName = controller.ControllerContext.ActionDescriptor.ActionName;

            return RenderViewAsync(controller, viewName, model, partial);
        }

        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName,
            TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;

            controller.ViewData.Model = model;

            await using var writer = new StringWriter();

            var viewEngine = controller.HttpContext.RequestServices
                .GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

            var viewResult = viewEngine?.FindView(controller.ControllerContext, viewName, !partial);

            if (viewResult == null || viewResult.Success == false)
                throw new ArgumentException($"A view with the name {viewName} could not be found");

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);

            return writer.GetStringBuilder().ToString();
        }
    }
}
