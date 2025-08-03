using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace viaggia_server.Config
{
    public class MultipartFormDataOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var isMultipartFormData = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<ConsumesAttribute>()
                .Any(attr => attr.ContentTypes.Contains("multipart/form-data"));

            if (!isMultipartFormData)
                return;

            // Get the parameter that is bound from form data (usually marked with [FromForm])
            var formParameter = context.MethodInfo.GetParameters()
                .FirstOrDefault(p => p.GetCustomAttributes(true).OfType<FromFormAttribute>().Any());

            if (formParameter == null)
                return;

            var schema = context.SchemaGenerator.GenerateSchema(formParameter.ParameterType, context.SchemaRepository);

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = schema,
                        Encoding = schema.Properties.ToDictionary(
                            prop => prop.Key,
                            prop => new OpenApiEncoding
                            {
                                Style = ParameterStyle.Form,
                                ContentType = prop.Value.Type == "string" && prop.Value.Format == "binary" ? "application/octet-stream" : null
                            })
                    }
                }
            };
        }
    }
}