﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viaggia.Swagger
{
    public class FormFileSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(IFormFile))
            {
                schema.Type = "string";
                schema.Format = "binary";
            }
        }
    }
}
