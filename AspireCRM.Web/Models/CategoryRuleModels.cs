using AspireCRM.Domain.CategoryRules;

namespace AspireCRM.Web.Models;

public class CategoryRuleListItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string ConditionField { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string ConditionValue { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int SortOrder { get; set; }
    public bool StopOnMatch { get; set; }
    public bool IsEnabled { get; set; }
}

public class CreateCategoryRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public RuleTargetEntity TargetEntity { get; set; }
    public string ConditionField { get; set; } = string.Empty;
    public RuleOperator Operator { get; set; }
    public string ConditionValue { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public int SortOrder { get; set; }
    public bool StopOnMatch { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class UpdateCategoryRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public RuleTargetEntity TargetEntity { get; set; }
    public string ConditionField { get; set; } = string.Empty;
    public RuleOperator Operator { get; set; }
    public string ConditionValue { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public int SortOrder { get; set; }
    public bool StopOnMatch { get; set; }
    public bool IsEnabled { get; set; } = true;
}