using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace viaggia_server.Config
{
    // Filtro para suportar multipart/form-data
    public class MultipartFormDataOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var isMultipartFormData = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<ConsumesAttribute>()
                .Any(attr => attr.ContentTypes.Contains("multipart/form-data"));

            if (isMultipartFormData)
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = context.SchemaGenerator
                                    .GenerateSchema(context.MethodInfo.GetParameters()[0].ParameterType, context.SchemaRepository)
                                    .Properties,
                                Required = context.SchemaGenerator
                                    .GenerateSchema(context.MethodInfo.GetParameters()[0].ParameterType, context.SchemaRepository)
                                    .Required
                            },
                            Encoding = new Dictionary<string, OpenApiEncoding>
                        {
                            { "MediaFiles", new OpenApiEncoding { Style = ParameterStyle.Form } },
                            { "RoomTypes", new OpenApiEncoding { Style = ParameterStyle.Form } }
                        }
                        }
                    }
                };
            }
        }
    }
}