using AspireCRM.Web.Models;
using System.Net.Http.Json;
using AspireCRM.Domain.CategoryRules;
using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Leads;
using AspireCRM.Domain.Marketing;
using AspireCRM.Domain.Sales;
using AspireCRM.Domain.Payments;
using AspireCRM.Domain.Products;
using AspireCRM.Domain.Relationships;

namespace AspireCRM.Web;

public class CrmApiClient(HttpClient http)
{
    public async Task<List<T>> GetListAsync<T>(string endpoint) =>
        await http.GetFromJsonAsync<List<T>>(endpoint) ?? [];

    public async Task<T?> GetByIdAsync<T>(string endpoint, long id) =>
        await http.GetFromJsonAsync<T>($"{endpoint}/{id}");

    public async Task<T> CreateAsync<T>(string endpoint, T entity)
    {
        var response = await http.PostAsJsonAsync(endpoint, entity);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public async Task UpdateAsync<T>(string endpoint, long id, T entity) =>
        await http.PutAsJsonAsync($"{endpoint}/{id}", entity);

    public async Task DeleteAsync(string endpoint, long id) =>
        await http.DeleteAsync($"{endpoint}/{id}");

    public Task<List<Lead>> GetLeadsAsync() => GetListAsync<Lead>("/api/leads");
    public Task<Lead?> GetLeadAsync(long id) => GetByIdAsync<Lead>("/api/leads", id);
    public Task<Lead> CreateLeadAsync(Lead lead) => CreateAsync("/api/leads", lead);
    public Task UpdateLeadAsync(long id, Lead lead) => UpdateAsync("/api/leads", id, lead);
    public Task DeleteLeadAsync(long id) => DeleteAsync("/api/leads", id);

    public Task<List<Contractor>> GetContractorsAsync() => GetListAsync<Contractor>("/api/contractors");
    public Task<List<Contractor>> GetContractorsListAsync() => GetListAsync<Contractor>("/api/contractors/list");
    public Task<Contractor?> GetContractorAsync(long id) => GetByIdAsync<Contractor>("/api/contractors", id);
    public Task<Contractor> CreateContractorAsync(Contractor c) => CreateAsync("/api/contractors", c);
    public async Task<Contractor> CreateContractorWithTypeAsync(CreateContractorRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/contractors", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Contractor>())!;
    }
    public Task UpdateContractorAsync(long id, Contractor c) => UpdateAsync("/api/contractors", id, c);
    public async Task UpdateContractorWithDetailsAsync(long id, UpdateContractorRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/contractors/{id}", request);
        response.EnsureSuccessStatusCode();
    }
    public Task DeleteContractorAsync(long id) => DeleteAsync("/api/contractors", id);

    public async Task<Contractor?> GetContractorDetailAsync(long id) =>
        await http.GetFromJsonAsync<Contractor>($"/api/contractors/{id}/details");

    public async Task<Email> AddContractorEmailAsync(long contractorId, string emailAddress, string? description = null)
    {
        var response = await http.PostAsJsonAsync($"/api/contractors/{contractorId}/emails", new { emailAddress, description });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Email>())!;
    }

    public async Task RemoveContractorEmailAsync(long contractorId, long emailId) =>
        await http.DeleteAsync($"/api/contractors/{contractorId}/emails/{emailId}");

    public async Task<Phone> AddContractorPhoneAsync(long contractorId, string phoneNumber, string? description = null)
    {
        var response = await http.PostAsJsonAsync($"/api/contractors/{contractorId}/phones", new { phoneNumber, description });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Phone>())!;
    }

    public async Task RemoveContractorPhoneAsync(long contractorId, long phoneId) =>
        await http.DeleteAsync($"/api/contractors/{contractorId}/phones/{phoneId}");

    public async Task<Address?> GetLegalAddressAsync(long contractorId) =>
        await http.GetFromJsonAsync<Address>($"/api/contractors/{contractorId}/addresses/legal");

    public async Task UpdateLegalAddressAsync(long contractorId, AddressDto address)
    {
        var response = await http.PutAsJsonAsync($"/api/contractors/{contractorId}/addresses/legal", address);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Address?> GetPostalAddressAsync(long contractorId) =>
        await http.GetFromJsonAsync<Address>($"/api/contractors/{contractorId}/addresses/postal");

    public async Task UpdatePostalAddressAsync(long contractorId, AddressDto address)
    {
        var response = await http.PutAsJsonAsync($"/api/contractors/{contractorId}/addresses/postal", address);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<BankAccount>> GetBankAccountsAsync(long contractorId) =>
        await http.GetFromJsonAsync<List<BankAccount>>($"/api/contractors/{contractorId}/bank-accounts") ?? [];

    public async Task<BankAccount> CreateBankAccountAsync(long contractorId, BankAccount account)
    {
        var response = await http.PostAsJsonAsync($"/api/contractors/{contractorId}/bank-accounts", account);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<BankAccount>())!;
    }

    public async Task UpdateBankAccountAsync(long contractorId, long accountId, BankAccount account) =>
        await http.PutAsJsonAsync($"/api/contractors/{contractorId}/bank-accounts/{accountId}", account);

    public async Task DeleteBankAccountAsync(long contractorId, long accountId) =>
        await http.DeleteAsync($"/api/contractors/{contractorId}/bank-accounts/{accountId}");

    public async Task<List<PaymentCard>> GetPaymentCardsAsync(long contractorId) =>
        await http.GetFromJsonAsync<List<PaymentCard>>($"/api/contractors/{contractorId}/payment-cards") ?? [];

    public async Task<PaymentCard> CreatePaymentCardAsync(long contractorId, PaymentCard card)
    {
        var response = await http.PostAsJsonAsync($"/api/contractors/{contractorId}/payment-cards", card);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PaymentCard>())!;
    }

    public async Task UpdatePaymentCardAsync(long contractorId, long cardId, PaymentCard card) =>
        await http.PutAsJsonAsync($"/api/contractors/{contractorId}/payment-cards/{cardId}", card);

    public async Task DeletePaymentCardAsync(long contractorId, long cardId) =>
        await http.DeleteAsync($"/api/contractors/{contractorId}/payment-cards/{cardId}");

    public Task<List<Contact>> GetContactsAsync(long? contractorId = null)
    {
        var url = contractorId.HasValue ? $"/api/contacts?contractorId={contractorId}" : "/api/contacts";
        return GetListAsync<Contact>(url);
    }

    public Task<Contact?> GetContactAsync(long id) => GetByIdAsync<Contact>("/api/contacts", id);
    public Task<Contact> CreateContactAsync(Contact c) => CreateAsync("/api/contacts", c);
    public Task UpdateContactAsync(long id, Contact c) => UpdateAsync("/api/contacts", id, c);
    public Task DeleteContactAsync(long id) => DeleteAsync("/api/contacts", id);

    public Task<List<Sale>> GetSalesAsync() => GetListAsync<Sale>("/api/sales");
    public Task<Sale?> GetSaleAsync(long id) => GetByIdAsync<Sale>("/api/sales", id);
    public Task<Sale> CreateSaleAsync(Sale s) => CreateAsync("/api/sales", s);
    public Task UpdateSaleAsync(long id, Sale s) => UpdateAsync("/api/sales", id, s);
    public Task DeleteSaleAsync(long id) => DeleteAsync("/api/sales", id);

    public async Task<SaleSummary?> GetSaleSummaryAsync() =>
        await http.GetFromJsonAsync<SaleSummary>("/api/sales/summary");

    public Task<List<Inpayment>> GetInpaymentsAsync(long? saleId = null)
    {
        var url = saleId.HasValue ? $"/api/inpayments?saleId={saleId}" : "/api/inpayments";
        return GetListAsync<Inpayment>(url);
    }

    public Task<List<InpaymentListItem>> GetInpaymentListAsync(long? saleId = null)
    {
        var url = saleId.HasValue ? $"/api/inpayments/list?saleId={saleId}" : "/api/inpayments/list";
        return GetListAsync<InpaymentListItem>(url);
    }

    public Task<Inpayment?> GetInpaymentAsync(long id) => GetByIdAsync<Inpayment>("/api/inpayments", id);
    public Task<Inpayment> CreateInpaymentAsync(Inpayment i) => CreateAsync("/api/inpayments", i);
    public Task UpdateInpaymentAsync(long id, Inpayment i) => UpdateAsync("/api/inpayments", id, i);
    public Task DeleteInpaymentAsync(long id) => DeleteAsync("/api/inpayments", id);

    public async Task ChangeInpaymentStatusAsync(long id, string status, string? comment = null)
    {
        var response = await http.PostAsJsonAsync($"/api/inpayments/{id}/change-status", new { status, comment });
        response.EnsureSuccessStatusCode();
    }

    public async Task<InpaymentSummary?> GetInpaymentSummaryAsync() =>
        await http.GetFromJsonAsync<InpaymentSummary>("/api/inpayments/summary");

    public Task<List<Product>> GetProductsAsync() => GetListAsync<Product>("/api/products");
    public Task<Product?> GetProductAsync(long id) => GetByIdAsync<Product>("/api/products", id);
    public async Task<List<ProductTreeNode>> GetProductTreeAsync() =>
        await http.GetFromJsonAsync<List<ProductTreeNode>>("/api/products/tree") ?? [];
    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/products", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Product>())!;
    }
    public async Task UpdateProductAsync(long id, UpdateProductRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/products/{id}", request);
        response.EnsureSuccessStatusCode();
    }
    public async Task MoveProductAsync(long id, long? parentId)
    {
        var response = await http.PutAsJsonAsync($"/api/products/{id}/move", new MoveProductRequest { ParentId = parentId });
        response.EnsureSuccessStatusCode();
    }
    public Task DeleteProductAsync(long id) => DeleteAsync("/api/products", id);

    public Task<List<SaleProduct>> GetSaleProductsAsync(long saleId) =>
        GetListAsync<SaleProduct>($"/api/sales/{saleId}/products");
    public Task<SaleProduct> CreateSaleProductAsync(long saleId, SaleProduct sp) =>
        CreateAsync($"/api/sales/{saleId}/products", sp);
    public Task UpdateSaleProductAsync(long saleId, long productId, SaleProduct sp) =>
        UpdateAsync($"/api/sales/{saleId}/products", productId, sp);
    public Task DeleteSaleProductAsync(long saleId, long productId) =>
        DeleteAsync($"/api/sales/{saleId}/products", productId);

    public async Task<Sale> ChangeSaleStageAsync(long id, long stageId, string? comment = null)
    {
        var response = await http.PostAsJsonAsync($"/api/sales/{id}/change-stage", new { stageId, comment });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Sale>())!;
    }

    public async Task<Sale> ChangeSaleStatusAsync(long id, string status, string? comment = null)
    {
        var response = await http.PostAsJsonAsync($"/api/sales/{id}/change-status", new { status, comment });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Sale>())!;
    }

    public Task<List<Category>> GetCategoriesAsync() => GetListAsync<Category>("/api/categories");
    public Task<Category?> GetCategoryAsync(long id) => GetByIdAsync<Category>("/api/categories", id);
    public Task<Category> CreateCategoryAsync(Category c) => CreateAsync("/api/categories", c);
    public Task UpdateCategoryAsync(long id, Category c) => UpdateAsync("/api/categories", id, c);
    public Task DeleteCategoryAsync(long id) => DeleteAsync("/api/categories", id);

    public Task<List<CategoryRule>> GetCategoryRulesAsync() => GetListAsync<CategoryRule>("/api/category-rules");
    public Task<CategoryRule?> GetCategoryRuleAsync(long id) => GetByIdAsync<CategoryRule>("/api/category-rules", id);
    public async Task<CategoryRule> CreateCategoryRuleAsync(CreateCategoryRuleRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/category-rules", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryRule>())!;
    }
    public async Task UpdateCategoryRuleAsync(long id, UpdateCategoryRuleRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/category-rules/{id}", request);
        response.EnsureSuccessStatusCode();
    }
    public Task DeleteCategoryRuleAsync(long id) => DeleteAsync("/api/category-rules", id);

    public Task<List<MarketingActivity>> GetMarketingActivitiesAsync() => GetListAsync<MarketingActivity>("/api/marketing-activities");
    public Task<MarketingActivity?> GetMarketingActivityAsync(long id) => GetByIdAsync<MarketingActivity>("/api/marketing-activities", id);
    public async Task<MarketingActivity> CreateMarketingActivityAsync(CreateMarketingActivityRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/marketing-activities", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MarketingActivity>())!;
    }
    public async Task UpdateMarketingActivityAsync(long id, UpdateMarketingActivityRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/marketing-activities/{id}", request);
        response.EnsureSuccessStatusCode();
    }
    public Task DeleteMarketingActivityAsync(long id) => DeleteAsync("/api/marketing-activities", id);
    public async Task<MarketingActivityStats?> GetMarketingActivityStatsAsync(long id) =>
        await http.GetFromJsonAsync<MarketingActivityStats>($"/api/marketing-activities/{id}/stats");

    public Task<List<SalesPlan>> GetSalesPlansAsync() => GetListAsync<SalesPlan>("/api/sales-plans");
    public Task<SalesPlan?> GetSalesPlanAsync(long id) => GetByIdAsync<SalesPlan>("/api/sales-plans", id);
    public Task<List<SalesPlan>> GetSalesPlansByYearAsync(int year) => GetListAsync<SalesPlan>($"/api/sales-plans/by-year/{year}");
    public async Task<SalesPlan> CreateSalesPlanAsync(Models.CreateSalesPlanRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/sales-plans", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SalesPlan>())!;
    }
    public async Task UpdateSalesPlanAsync(long id, Models.UpdateSalesPlanRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/sales-plans/{id}", request);
        response.EnsureSuccessStatusCode();
    }
    public Task DeleteSalesPlanAsync(long id) => DeleteAsync("/api/sales-plans", id);

    public async Task<RelationshipListResponse?> GetRelationshipsAsync(int page = 1, int pageSize = 20,
        string? type = null, long? leadId = null, long? contractorId = null, long? saleId = null,
        DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = $"/api/relationships?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(type)) query += $"&type={type}";
        if (leadId.HasValue) query += $"&leadId={leadId}";
        if (contractorId.HasValue) query += $"&contractorId={contractorId}";
        if (saleId.HasValue) query += $"&saleId={saleId}";
        if (startDate.HasValue) query += $"&startDate={startDate.Value:yyyy-MM-dd}";
        if (endDate.HasValue) query += $"&endDate={endDate.Value:yyyy-MM-dd}";
        return await http.GetFromJsonAsync<RelationshipListResponse>(query);
    }

    public Task<Relationship?> GetRelationshipAsync(long id) =>
        http.GetFromJsonAsync<Relationship>($"/api/relationships/{id}");

    public async Task<Relationship> CreateRelationshipAsync(CreateRelationshipRequest r)
    {
        var response = await http.PostAsJsonAsync("/api/relationships", r);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Relationship>())!;
    }

    public async Task UpdateRelationshipAsync(long id, UpdateRelationshipRequest r)
    {
        var response = await http.PutAsJsonAsync($"/api/relationships/{id}", r);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteRelationshipAsync(long id)
    {
        var response = await http.DeleteAsync($"/api/relationships/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task CompleteRelationshipAsync(long id)
    {
        var response = await http.PostAsync($"/api/relationships/{id}/complete", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<RelationshipUser>> GetRelationshipUsersAsync(long relationshipId) =>
        await http.GetFromJsonAsync<List<RelationshipUser>>($"/api/relationships/{relationshipId}/users") ?? [];

    public async Task<RelationshipUser> AddRelationshipUserAsync(long relationshipId, long userId, RelationshipUserStatus status)
    {
        var response = await http.PostAsJsonAsync($"/api/relationships/{relationshipId}/users", new { userId, status });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RelationshipUser>())!;
    }

    public async Task RemoveRelationshipUserAsync(long relationshipId, long userId)
    {
        var response = await http.DeleteAsync($"/api/relationships/{relationshipId}/users/{userId}");
        response.EnsureSuccessStatusCode();
    }

    public Task<List<T>> GetLookupAsync<T>(string name) => GetListAsync<T>($"/api/lookups/{name}");

    public Task<List<SaleFunnel>> GetSaleFunnelsAsync() => GetListAsync<SaleFunnel>("/api/sale-funnels");
    public Task<SaleFunnel?> GetSaleFunnelAsync(long id) => GetByIdAsync<SaleFunnel>("/api/sale-funnels", id);
    public Task<SaleFunnel> CreateSaleFunnelAsync(SaleFunnel e) => CreateAsync("/api/sale-funnels", e);
    public Task UpdateSaleFunnelAsync(long id, SaleFunnel e) => UpdateAsync("/api/sale-funnels", id, e);
    public Task DeleteSaleFunnelAsync(long id) => DeleteAsync("/api/sale-funnels", id);

    public Task<List<SaleStage>> GetSaleStagesAsync() => GetListAsync<SaleStage>("/api/sale-stages");
    public Task<SaleStage?> GetSaleStageAsync(long id) => GetByIdAsync<SaleStage>("/api/sale-stages", id);
    public Task<SaleStage> CreateSaleStageAsync(SaleStage e) => CreateAsync("/api/sale-stages", e);
    public Task UpdateSaleStageAsync(long id, SaleStage e) => UpdateAsync("/api/sale-stages", id, e);
    public Task DeleteSaleStageAsync(long id) => DeleteAsync("/api/sale-stages", id);
    public async Task ReorderSaleStagesAsync(List<SaleStage> items)
    {
        var response = await http.PutAsJsonAsync("/api/sale-stages/reorder", items);
        response.EnsureSuccessStatusCode();
    }

    public Task<List<SaleType>> GetSaleTypesAsync() => GetListAsync<SaleType>("/api/sale-types");
    public Task<SaleType?> GetSaleTypeAsync(long id) => GetByIdAsync<SaleType>("/api/sale-types", id);
    public Task<SaleType> CreateSaleTypeAsync(SaleType e) => CreateAsync("/api/sale-types", e);
    public Task UpdateSaleTypeAsync(long id, SaleType e) => UpdateAsync("/api/sale-types", id, e);
    public Task DeleteSaleTypeAsync(long id) => DeleteAsync("/api/sale-types", id);

    public async Task BeginLeadAsync(long id) =>
        await http.PostAsync($"/api/leads/{id}/begin", null);

    public async Task FailLeadAsync(long id, string? comment = null) =>
        await http.PostAsJsonAsync($"/api/leads/{id}/fail", new { comment });

    public async Task ConversationNotStartAsync(long id) =>
        await http.PostAsync($"/api/leads/{id}/conversation-not-start", null);

    public async Task ActivateLeadsAsync(long[] ids) =>
        await http.PostAsJsonAsync("/api/leads/batch/activate", new { ids });

    public async Task<List<DuplicateCandidate>> CheckDuplicatesAsync(string name) =>
        await http.GetFromJsonAsync<List<DuplicateCandidate>>($"/api/leads/check-duplicates?name={Uri.EscapeDataString(name)}") ?? [];

    public async Task<List<DuplicateCandidate>> GetDuplicatesAsync(long id) =>
        await http.GetFromJsonAsync<List<DuplicateCandidate>>($"/api/leads/{id}/duplicates") ?? [];

    public async Task MarkDuplicateAsync(long id, long targetLeadId, string? comment = null) =>
        await http.PostAsJsonAsync($"/api/leads/{id}/mark-duplicate", new { targetLeadId, comment });

    public async Task UnmarkDuplicateAsync(long id) =>
        await http.PostAsync($"/api/leads/{id}/unmark-duplicate", null);

    public async Task<LeadConversionPreview?> GetConversionPreviewAsync(long id) =>
        await http.GetFromJsonAsync<LeadConversionPreview>($"/api/leads/{id}/convert/preview");

    public async Task<LeadConversionResult?> ConvertLeadAsync(long id, LeadConversionRequest req) =>
        await http.PostAsJsonAsync<LeadConversionRequest>($"/api/leads/{id}/convert", req).Result.Content.ReadFromJsonAsync<LeadConversionResult>();

    public async Task BatchAssignAsync(long[] ids, long responsibleId) =>
        await http.PostAsJsonAsync("/api/leads/batch/assign", new { ids, responsibleId });

    public async Task BatchChangeSourceAsync(long[] ids, long? sourceId) =>
        await http.PostAsJsonAsync("/api/leads/batch/change-source", new { ids, sourceId });

    public async Task<List<UserDto>> GetUsersAsync() =>
        await http.GetFromJsonAsync<List<UserDto>>("/api/auth/users") ?? [];

    public async Task<List<LeadSource>> GetLeadSourcesAsync() =>
        await GetLookupAsync<LeadSource>("lead-sources");
}