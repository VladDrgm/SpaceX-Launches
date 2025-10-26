using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;

namespace SpaceXLaunchDataService.Common.Filters;

/// <summary>
/// Swagger schema filter to add enum descriptions and examples
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumValues = new List<string>();
            var descriptions = new List<string>();

            foreach (var enumValue in Enum.GetValues(context.Type))
            {
                var enumMember = context.Type.GetMember(enumValue.ToString()!).FirstOrDefault();
                var descriptionAttribute = enumMember?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>().FirstOrDefault();

                var enumString = enumValue.ToString()!;
                enumValues.Add(enumString);

                if (descriptionAttribute != null)
                {
                    descriptions.Add($"{enumString}: {descriptionAttribute.Description}");
                }
                else
                {
                    descriptions.Add(enumString);
                }

                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumString));
            }

            schema.Description = string.Join(", ", descriptions);
            schema.Example = new Microsoft.OpenApi.Any.OpenApiString(enumValues.First());
        }
    }
}