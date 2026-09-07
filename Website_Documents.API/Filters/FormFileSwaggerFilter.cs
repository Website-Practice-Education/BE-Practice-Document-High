using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Website_Documents.API.Filters;

public class FormFileSwaggerFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var parameters = context.ApiDescription.ActionDescriptor.Parameters;
        foreach (var parameter in parameters)
        {
            if (parameter.ParameterType == typeof(IFormFile))
            {
                var formFileParameter = operation.Parameters.FirstOrDefault(p => p.Name == parameter.Name);
                if (formFileParameter != null)
                {
                    operation.Parameters.Remove(formFileParameter);
                }

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    [parameter.Name] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary",
                                        Description = "File to upload"
                                    }
                                },
                                Required = new HashSet<string> { parameter.Name }
                            }
                        }
                    }
                };
            }
        }
    }
}
