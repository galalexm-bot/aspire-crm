using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.CategoryRules;

public class CategoryRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public RuleTargetEntity TargetEntity { get; set; }
    public string ConditionField { get; set; } = string.Empty;
    public RuleOperator Operator { get; set; }
    public string ConditionValue { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public Category? Category { get; set; }
    public int SortOrder { get; set; }
    public bool StopOnMatch { get; set; }
    public bool IsEnabled { get; set; } = true;
}