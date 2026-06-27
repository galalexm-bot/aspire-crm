using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.CategoryRules;

namespace AspireCRM.ApiService.Endpoints;

public static class CategoryRuleEndpoints
{
    public static void MapCategoryRuleEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/category-rules");

        api.MapGet("/", async (IRepository<CategoryRule> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/{id:long}", async (long id, IRepository<CategoryRule> repo) =>
        {
            var rule = await repo.GetByIdAsync(id);
            return rule is null ? Results.NotFound() : Results.Ok(rule);
        });

        api.MapPost("/", async (CreateCategoryRuleRequest request, IRepository<CategoryRule> repo) =>
        {
            var rule = new CategoryRule
            {
                Name = request.Name.Trim(),
                TargetEntity = request.TargetEntity,
                ConditionField = request.ConditionField,
                Operator = request.Operator,
                ConditionValue = request.ConditionValue.Trim(),
                CategoryId = request.CategoryId,
                SortOrder = request.SortOrder,
                StopOnMatch = request.StopOnMatch,
                IsEnabled = request.IsEnabled
            };
            var created = await repo.AddAsync(rule);
            return Results.Created($"/api/category-rules/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, UpdateCategoryRuleRequest request, IRepository<CategoryRule> repo) =>
        {
            var rule = await repo.GetByIdAsync(id);
            if (rule is null) return Results.NotFound();

            rule.Name = request.Name.Trim();
            rule.TargetEntity = request.TargetEntity;
            rule.ConditionField = request.ConditionField;
            rule.Operator = request.Operator;
            rule.ConditionValue = request.ConditionValue.Trim();
            rule.CategoryId = request.CategoryId;
            rule.SortOrder = request.SortOrder;
            rule.StopOnMatch = request.StopOnMatch;
            rule.IsEnabled = request.IsEnabled;
            await repo.UpdateAsync(rule);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<CategoryRule> repo) =>
        {
            var rule = await repo.GetByIdAsync(id);
            if (rule is null) return Results.NotFound();
            await repo.DeleteAsync(rule);
            return Results.NoContent();
        });
    }
}

public record CreateCategoryRuleRequest(
    string Name,
    RuleTargetEntity TargetEntity,
    string ConditionField,
    RuleOperator Operator,
    string ConditionValue,
    long CategoryId,
    int SortOrder,
    bool StopOnMatch,
    bool IsEnabled
);

public record UpdateCategoryRuleRequest(
    string Name,
    RuleTargetEntity TargetEntity,
    string ConditionField,
    RuleOperator Operator,
    string ConditionValue,
    long CategoryId,
    int SortOrder,
    bool StopOnMatch,
    bool IsEnabled
);