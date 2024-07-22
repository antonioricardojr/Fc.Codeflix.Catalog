using System.Text.Json;
using FC.Codeflix.Catalog.Api.Extensions.String;

namespace FC.Codeflix.Catalog.Api.Configurations.Policies;

public class JsonSnakeCasePolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) 
        => name.ToSnakeCase();
}