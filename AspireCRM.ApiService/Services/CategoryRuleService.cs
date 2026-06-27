using AspireCRM.DataLayer;
using AspireCRM.Domain.CategoryRules;
using AspireCRM.Domain.Leads;
using AspireCRM.Domain.Contractors;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Services;

public class CategoryRuleService
{
    private readonly AspireCRMDbContext _db;

    public CategoryRuleService(AspireCRMDbContext db)
    {
        _db = db;
    }

    public async Task ApplyLeadRules(Lead lead)
    {
        var rules = await _db.CategoryRules
            .Where(r => r.TargetEntity == RuleTargetEntity.Lead && r.IsEnabled)
            .OrderBy(r => r.SortOrder)
            .Include(r => r.Category)
            .ToListAsync();

        foreach (var rule in rules)
        {
            if (EvaluateCondition(lead, rule))
            {
                if (rule.Category is not null && !lead.Categories.Any(c => c.Id == rule.CategoryId))
                {
                    lead.Categories.Add(rule.Category);
                }
                if (rule.StopOnMatch) break;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task ApplyContractorRules(Contractor contractor)
    {
        var rules = await _db.CategoryRules
            .Where(r => r.TargetEntity == RuleTargetEntity.Contractor && r.IsEnabled)
            .OrderBy(r => r.SortOrder)
            .Include(r => r.Category)
            .ToListAsync();

        foreach (var rule in rules)
        {
            if (EvaluateCondition(contractor, rule))
            {
                if (rule.Category is not null && !contractor.Categories.Any(c => c.Id == rule.CategoryId))
                {
                    contractor.Categories.Add(rule.Category);
                }
                if (rule.StopOnMatch) break;
            }
        }

        await _db.SaveChangesAsync();
    }

    private static bool EvaluateCondition(object entity, CategoryRule rule)
    {
        var value = GetFieldValue(entity, rule.ConditionField);
        if (value is null) return false;

        var strValue = value.ToString() ?? string.Empty;
        var condition = rule.ConditionValue;

        return rule.Operator switch
        {
            RuleOperator.Contains => strValue.Contains(condition, StringComparison.OrdinalIgnoreCase),
            RuleOperator.Equals => strValue.Equals(condition, StringComparison.OrdinalIgnoreCase),
            RuleOperator.StartsWith => strValue.StartsWith(condition, StringComparison.OrdinalIgnoreCase),
            RuleOperator.EndsWith => strValue.EndsWith(condition, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static object? GetFieldValue(object entity, string fieldName)
    {
        var prop = entity.GetType().GetProperty(fieldName);
        if (prop is not null)
            return prop.GetValue(entity);

        foreach (var baseType in GetBaseTypes(entity.GetType()))
        {
            prop = baseType.GetProperty(fieldName);
            if (prop is not null)
                return prop.GetValue(entity);
        }

        return null;
    }

    private static IEnumerable<Type> GetBaseTypes(Type type)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            yield return current;
            current = current.BaseType;
        }
    }
}