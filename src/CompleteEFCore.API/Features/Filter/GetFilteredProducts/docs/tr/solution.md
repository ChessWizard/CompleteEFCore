# 💡 Çözüm

### A) Request Modelimizin Belirlenmesi

- Ürün üzerindeki alanların filtrelenebilmesi için “AND” ve “OR” mantıklarını da içerebilecek bir biçimde “LogicalOperator” isminde enum oluşturularak hangi tipte alan filtrelenecekse ona uygun olabilecek generic biçimde “FilterField<>” isminde bir base model oluşturulur.
- Hiçbir mantıksal operatör belirtilmemesi durumuna karşı “AND” mantığı işletilmektedir.

```csharp
public record FilterField<T>(
    T Value,
    LogicalOperator Operator = LogicalOperator.And);
```

- Problem’de belirtilen filtrelenmek üzere işletilecek alanlar için bu base modeli “ProductFilterDto” ismindeki request’te kullanacağımız modelimize ekleyelim.
- Birden fazla aynı alan için filtre olabileceğinden dolayı liste halinde filtrelerimizi alıyoruz.

```csharp
public record ProductFilterDto(List<FilterField<string>>? ProductNameFilters,
                               List<FilterField<int>>? SupplierFilters,
                               List<FilterField<int>>? CategoryFilters,
                               List<FilterField<RangeFilterDto>>? UnitFilters);

public record RangeFilterDto(float MinValue, float MaxValue);
```

### B) Response Modelimizin Belirlenmesi

```csharp
public record NorthwindProductDto(
    int ProductId,
    string ProductName,
    int? SupplierId,
    int? CategoryId,
    string QuantityPerUnit,
    decimal? UnitPrice,
    short? UnitsInStock,
    short? UnitsOnOrder,
    short? ReorderLevel,
    bool Discontinued
);
```

### C) Business Kurallarımızın İşletilmesi

C.1) `GetFilteredProductsQuery`

```csharp
public record GetFilteredProductsQuery(ProductFilterDto ProductFilterDto) : IQuery<Result<List<NorthwindProductDto>>>;
```

C.2) `GetFilteredProductsQueryHandler`

- Filtreleme işlerimizin yapıldı QueryHandler sınıfının tüm hali aşağıdaki gibidir.

```csharp
using System.Linq.Expressions;
using System.Net;
using CompleteEFCore.API.Domain.Entities.Northwind;
using CompleteEFCore.API.Features.Common.Models.Response;
using CompleteEFCore.API.Features.Filter.Common.Enums;
using CompleteEFCore.BuildingBlocks.CQRS.Query;
using CompleteEFCore.BuildingBlocks.Result;
using LinqKit;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CompleteEFCore.API.Features.Filter.GetFilteredProducts;

public record GetFilteredProductsQuery(ProductFilterDto ProductFilterDto) : IQuery<Result<List<NorthwindProductDto>>>;

public class GetFilteredProductsQueryHandler(NorthwindContext context) : IQueryHandler<GetFilteredProductsQuery, Result<List<NorthwindProductDto>>>
{
    public async Task<Result<List<NorthwindProductDto>>> Handle(GetFilteredProductsQuery request, CancellationToken cancellationToken)
    {
        var filters = CreateProductFilters(request.ProductFilterDto);
        var products = await context.Products
                                                .AsExpandable()
                                                .Where(filters)
                                                .ToListAsync(cancellationToken);
        
        var result = products.Adapt<List<NorthwindProductDto>>();
        return Result<List<NorthwindProductDto>>.Success(result, (int)HttpStatusCode.OK);
    }

    private static ExpressionStarter<Product> CreateProductFilters(ProductFilterDto productFilterDto)
    {
        var productPredicate = PredicateBuilder.New<Product>(true);

        if (productFilterDto.CategoryFilters is not null)
        {
            foreach (var categoryFilter in productFilterDto.CategoryFilters)
            {
                ApplyCondition(productPredicate, 
                    product => product.CategoryId.GetValueOrDefault() == categoryFilter.Value,
                    categoryFilter.Operator);
            } 
        }
        
        if(productFilterDto.SupplierFilters is not null)
        {
            foreach (var supplierFilter in productFilterDto.SupplierFilters)
            {
                ApplyCondition(productPredicate,
                    product => product.SupplierId.GetValueOrDefault() == supplierFilter.Value,
                    supplierFilter.Operator);
            }
        }

        if (productFilterDto.ProductNameFilters is not null)
        {
            foreach (var productNameFilter in productFilterDto.ProductNameFilters)
            {
                ApplyCondition(productPredicate,
                    product => product.ProductName.ToLower()
                        .Contains(productNameFilter.Value.ToLower()),
                    productNameFilter.Operator);
            }
        }

        if (productFilterDto.UnitFilters is not null)
        {
            foreach (var unitFilter in productFilterDto.UnitFilters)
            {
                var minValue = unitFilter.Value.MinValue;
                var maxValue = unitFilter.Value.MaxValue;
                
                ApplyCondition(productPredicate,
                    product => product.UnitPrice.GetValueOrDefault() >= minValue &&
                                       product.UnitPrice.GetValueOrDefault() <= maxValue,
                    unitFilter.Operator);
            }
        }

        return productPredicate;
    }

    private static void ApplyCondition(
        ExpressionStarter<Product> predicate,
        Expression<Func<Product, bool>> condition,
        LogicalOperator logicalOperator)
    {
        switch (logicalOperator)
        {
            case LogicalOperator.And:
                predicate.And(condition);
                break;
            case LogicalOperator.Or:
                predicate.Or(condition);
                break;
            default:
                throw new ArgumentException("Invalid logical operator. Use 'AND' or 'OR'.");
        }
    }
}
```

C.2.1) Handle Metodu:

```csharp
public async Task<Result<List<NorthwindProductDto>>> Handle(GetFilteredProductsQuery request, CancellationToken cancellationToken)
{
        var filters = CreateProductFilters(request.ProductFilterDto);
        var products = await context.Products
                                    .AsExpandable()
                                    .Where(filters)
                                    .ToListAsync(cancellationToken);
        
        var result = products.Adapt<List<NorthwindProductDto>>();
        return Result<List<NorthwindProductDto>>.Success(result, (int)HttpStatusCode.OK);
}
```

- Kullanıcıdan gelen ürün filtreleri alınarak LinqKit kütüphanesiyle uyumlu olacak biçimde “ExpressionStarter” türünde filtrelerimizin işlenmiş halini alıyoruz.
- LinqKit kütüphanesi AND, OR ve daha karmaşık biçimde oluşturulabilecek pek çok dinamik filtre için altyapı sağlayarak daha genişletilmiş ve profesyonel filtreler oluşturabilmemizi sağlayan bir kütüphanedir.
- Bu kütüphaneden gelen ExpressionStarter nesnesi üzerinden tüm filtreyi oluşturduktan sonra “Products” tablosuna bu oluşan filtremizle istek atıyoruz. İsteği atmadan önce “AsExpandable()” ile sorgumuzu EF Core tarafından başarılı bir biçimde filtrelerimizin algılanması için işaretlememiz gerekir.


**AsExpandable()**

- LinqKit'e ait bir metottur.
- `ExpressionStarter` ile oluşturulan dinamik sorguların EF Core tarafından işlenebilmesini sağlar.
- Olmazsa, EF Core dinamik sorguları anlayamaz ve hata verir.

- Bu işaretlemenin ardından “Where()” metodu içerisine filtrelerimizi koyup “ToListAsync()” ile tüm bu koşullara uyan ürünleri çektirmiş oluruz.
- Son olarak Mapster kütüphanesini kullanarak ilgili response modele map’leme işlemi gerçekleştirmemizin ardından result’ımızı dönerek işlemimizi tamamlamış oluruz.

### Daha derinlere inecek olursak;

C.2.2) `CreateProductFilters`

```csharp
private static ExpressionStarter<Product> CreateProductFilters(ProductFilterDto productFilterDto)
    {
        var productPredicate = PredicateBuilder.New<Product>(true);

        if (productFilterDto.CategoryFilters is not null)
        {
            foreach (var categoryFilter in productFilterDto.CategoryFilters)
            {
                ApplyCondition(productPredicate, 
                    product => product.CategoryId.GetValueOrDefault() == categoryFilter.Value,
                    categoryFilter.Operator);
            } 
        }
        
        if(productFilterDto.SupplierFilters is not null)
        {
            foreach (var supplierFilter in productFilterDto.SupplierFilters)
            {
                ApplyCondition(productPredicate,
                    product => product.SupplierId.GetValueOrDefault() == supplierFilter.Value,
                    supplierFilter.Operator);
            }
        }

        if (productFilterDto.ProductNameFilters is not null)
        {
            foreach (var productNameFilter in productFilterDto.ProductNameFilters)
            {
                ApplyCondition(productPredicate,
                    product => product.ProductName.ToLower()
                        .Contains(productNameFilter.Value.ToLower()),
                    productNameFilter.Operator);
            }
        }

        if (productFilterDto.UnitFilters is not null)
        {
            foreach (var unitFilter in productFilterDto.UnitFilters)
            {
                var minValue = unitFilter.Value.MinValue;
                var maxValue = unitFilter.Value.MaxValue;
                
                ApplyCondition(productPredicate,
                    product => product.UnitPrice.GetValueOrDefault() >= minValue &&
                                       product.UnitPrice.GetValueOrDefault() <= maxValue,
                    unitFilter.Operator);
            }
        }

        return productPredicate;
    }
```

- Kullanıcının isterlerine göre ürünlerin filtreleneceği bölüm burasıdır.
- Hangi filtre tipi boş değilse ona uygun biçimde filtrelemeler yapılır.
- Filtreler farklı koşullara uygun olabilmesi için liste halinde birden fazla almıştık. Bundan kaynaklı foreach ile dönerek her birinin işletilmesini sağlıyoruz.
- Generic olarak hepsiyle uyumlu biçimde yazılan “ApplyCondition()” metodunda işlemlerimiz yaptırılır.

C.2.3) `ApplyCondition`

```csharp
private static void ApplyCondition(
        ExpressionStarter<Product> predicate,
        Expression<Func<Product, bool>> condition,
        LogicalOperator logicalOperator)
    {
        switch (logicalOperator)
        {
            case LogicalOperator.And:
                predicate.And(condition);
                break;
            case LogicalOperator.Or:
                predicate.Or(condition);
                break;
            default:
                throw new ArgumentException("Invalid logical operator. Use 'AND' or 'OR'.");
        }
```

- Bu metot içerisine aldığı parametreler bazında;
    - **ExpressionStarter<Product> predicate**: Güncel filtrenin durumunun korunması için referans geçiliyor ve onun üzerinde işlemlere devam ediliyor.
    - **Expression<Func<Product, bool>> condition**: Kullanıcının isterleri doğrultusunda istenilen filtreleme.
    - **LogicalOperator logicalOperator**: AND veya OR’dan hangisinin mantıksal olarak sorguya dahil edileceğinin ayırt edilebilmesi içindir.
- Güncel filtreler “predicate” nesnemize eklenerek koşulları “chain” etmiş oluruz.

## Örnek

- Tedarikçi id'si 18, 19 veya 20 olan ve ürün fiyatı en az 10 en fazla 25 olan ürünleri listeleyiniz.

### Request

```json
{
  "supplierFilters": [
    {
      "value": 18,
      "operator": 2
    },
    {
      "value": 19,
      "operator": 2
    },
    {
      "value": 20,
      "operator": 2
    }
  ],
  "unitFilters": [
    {
      "value": {"MinValue":10, "MaxValue": 25},
      "operator": 1
    }
  ]
}
```

### Response

```json

{
  "data": [
    {
      "productId": 39,
      "productName": "Chartreuse verte",
      "supplierId": 18,
      "categoryId": 1,
      "quantityPerUnit": "750 cc per bottle",
      "unitPrice": 18,
      "unitsInStock": 69,
      "unitsOnOrder": 0,
      "reorderLevel": 5,
      "discontinued": false
    },
    {
      "productId": 40,
      "productName": "Boston Crab Meat",
      "supplierId": 19,
      "categoryId": 8,
      "quantityPerUnit": "24 - 4 oz tins",
      "unitPrice": 18.4,
      "unitsInStock": 123,
      "unitsOnOrder": 0,
      "reorderLevel": 30,
      "discontinued": false
    },
    {
      "productId": 42,
      "productName": "Singaporean Hokkien Fried Mee",
      "supplierId": 20,
      "categoryId": 5,
      "quantityPerUnit": "32 - 1 kg pkgs.",
      "unitPrice": 14,
      "unitsInStock": 26,
      "unitsOnOrder": 0,
      "reorderLevel": 0,
      "discontinued": true
    },
    {
      "productId": 44,
      "productName": "Gula Malacca",
      "supplierId": 20,
      "categoryId": 2,
      "quantityPerUnit": "20 - 2 kg bags",
      "unitPrice": 19.45,
      "unitsInStock": 27,
      "unitsOnOrder": 0,
      "reorderLevel": 15,
      "discontinued": false
    }
  ],
  "message": null,
  "httpStatusCode": 200,
  "errorDto": null
}

```


