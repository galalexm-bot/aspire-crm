using System.Net.Http.Json;
using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Leads;
using AspireCRM.Domain.Payments;
using AspireCRM.Domain.Products;
using AspireCRM.Domain.Relationships;
using AspireCRM.Domain.Sales;

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
    public Task<Contractor?> GetContractorAsync(long id) => GetByIdAsync<Contractor>("/api/contractors", id);
    public Task<Contractor> CreateContractorAsync(Contractor c) => CreateAsync("/api/contractors", c);
    public Task UpdateContractorAsync(long id, Contractor c) => UpdateAsync("/api/contractors", id, c);
    public Task DeleteContractorAsync(long id) => DeleteAsync("/api/contractors", id);

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

    public Task<List<Inpayment>> GetInpaymentsAsync(long? saleId = null)
    {
        var url = saleId.HasValue ? $"/api/inpayments?saleId={saleId}" : "/api/inpayments";
        return GetListAsync<Inpayment>(url);
    }

    public Task<Inpayment?> GetInpaymentAsync(long id) => GetByIdAsync<Inpayment>("/api/inpayments", id);
    public Task<Inpayment> CreateInpaymentAsync(Inpayment i) => CreateAsync("/api/inpayments", i);
    public Task UpdateInpaymentAsync(long id, Inpayment i) => UpdateAsync("/api/inpayments", id, i);
    public Task DeleteInpaymentAsync(long id) => DeleteAsync("/api/inpayments", id);

    public Task<List<Product>> GetProductsAsync() => GetListAsync<Product>("/api/products");
    public Task<Product?> GetProductAsync(long id) => GetByIdAsync<Product>("/api/products", id);
    public Task<Product> CreateProductAsync(Product p) => CreateAsync("/api/products", p);
    public Task UpdateProductAsync(long id, Product p) => UpdateAsync("/api/products", id, p);
    public Task DeleteProductAsync(long id) => DeleteAsync("/api/products", id);

    public Task<List<Category>> GetCategoriesAsync() => GetListAsync<Category>("/api/categories");
    public Task<Category?> GetCategoryAsync(long id) => GetByIdAsync<Category>("/api/categories", id);
    public Task<Category> CreateCategoryAsync(Category c) => CreateAsync("/api/categories", c);
    public Task UpdateCategoryAsync(long id, Category c) => UpdateAsync("/api/categories", id, c);
    public Task DeleteCategoryAsync(long id) => DeleteAsync("/api/categories", id);

    public Task<List<Relationship>> GetRelationshipsAsync(long? leadId = null)
    {
        var url = leadId.HasValue ? $"/api/relationships?leadId={leadId}" : "/api/relationships";
        return GetListAsync<Relationship>(url);
    }

    public Task<Relationship?> GetRelationshipAsync(long id) => GetByIdAsync<Relationship>("/api/relationships", id);
    public Task<Relationship> CreateRelationshipAsync(Relationship r) => CreateAsync("/api/relationships", r);
    public Task UpdateRelationshipAsync(long id, Relationship r) => UpdateAsync("/api/relationships", id, r);
    public Task DeleteRelationshipAsync(long id) => DeleteAsync("/api/relationships", id);

    public Task<List<T>> GetLookupAsync<T>(string name) => GetListAsync<T>($"/api/lookups/{name}");
}