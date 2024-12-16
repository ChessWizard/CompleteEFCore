# 📄 Problem

Bu slice, **Northwind** veritabanındaki ürünlerin, kullanıcıların belirttiği çeşitli koşullara göre dinamik bir biçimde filtrelenmesini sağlamaktadır.

---

## 📋 İstenenler

- **Dinamik Filtreleme**:
    - Ürün ismi
    - Tedarikçi
    - Ürün kategorisi
    - Minimum ürün fiyatı
    - Maksimum ürün fiyatı

- **Predicate Builder Kullanımı**:
    - Koşulların yalnızca gerektiği durumda sorgulara dahil edilmesi.
    - Mantıksal `AND` ve `OR` mantıklarıyla sorgu oluşturulması. Varsayılan olarak `AND`.

- **Performans Odaklı Sorgular**:
    - Veritabanı üzerinden dinamik filtrelemeye uygun şekilde optimize edilmiş sorgular.
