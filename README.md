# StokDepo – Web Tabanlı Stok Yönetim Sistemi

StokDepo, küçük ve orta ölçekli işletmeler için geliştirilmiş, web tabanlı bir stok yönetim uygulamasıdır.  
Proje ASP.NET Core MVC mimarisi kullanılarak geliştirilmiş ve Docker ile Render platformunda canlıya alınmıştır.

---

## Canlı Demo
 https://stokdepo-nh60.onrender.com/

## GitHub Repository
 https://github.com/furkandgl/StokDepo

---

## Özellikler

- Ürün ekleme, düzenleme ve silme
- Otomatik stok hareketi takibi
- Dashboard üzerinden genel stok durumu
- Kullanıcı girişi ve rol bazlı yetkilendirme
- Admin paneli (kullanıcı ve yetki yönetimi)
- Dark / Light tema desteği
- Mobil uyumlu (responsive) tasarım

---

## Kullanıcı Rolleri

### Admin
- Ürün ekleme / düzenleme / silme
- Kullanıcıları görüntüleme
- Kullanıcılara admin yetkisi verme / alma
- Tüm stok hareketlerini görüntüleme

### User
- Ürünleri görüntüleme
- Stok hareketlerini inceleme
- Dashboard üzerinden stok özetini görme

---

## Kullanılan Teknolojiler

- **Backend:** ASP.NET Core MVC
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity
- **Database:** SQLite
- **Frontend:** Razor Pages, Bootstrap 5
- **Icons:** Bootstrap Icons
- **Deployment:** Docker + Render
- **Version Control:** Git & GitHub

---

## Yerelde Çalıştırma

Bu projeyi kendi bilgisayarınızda (localhost) çalıştırmak için aşağıdaki adımları izleyebilirsiniz.

### Gereksinimler
- .NET SDK 8.0 veya üzeri
- Git
- (Opsiyonel) Visual Studio 2022 veya VS Code


### Kurulum Adımları

1. Projeyi GitHub’dan klonlayın:
```bash
git clone https://github.com/furkandgl/StokDepo.git

2. Proje klasörüne girin:

cd StokDepo


3. Gerekli NuGet paketlerini yükleyin:

dotnet restore


4. Veritabanını oluşturun:

dotnet ef database update


5. Uygulamayı çalıştırın:

dotnet run


6. Tarayıcıdan uygulamaya erişin:

https://localhost:xxxx
veya
http://localhost:xxxx


### Varsayılan Admin Kullanıcısı

- Uygulama ilk çalıştırıldığında, appsettings.json dosyasında tanımlı bilgilerle otomatik olarak bir Admin kullanıcı oluşturulur.

- Bu kullanıcı ile giriş yaparak:

- Ürün ekleme

- Kullanıcı yönetimi

- Admin yetkileri işlemleri yapılabilir.

---

## Demo Hesap Bilgileri

Değerlendirme ve test amacıyla sistemde varsayılan bir admin kullanıcı
otomatik olarak oluşturulmaktadır.

- **Admin Kullanıcı**
  - Email: `admin@stokdepo.com`
  - Şifre: `Admin123!`

Bu kullanıcı bilgileri `appsettings.json` dosyasında tanımlıdır ve
uygulama ilk çalıştırıldığında otomatik olarak sisteme eklenir.

---

## Veritabanı Yapısı

### Product
- Id
- Name
- Category
- Price
- Quantity
- Description
- CreatedDate

### StockMovement
- Id
- ProductId
- QuantityChange
- CreatedAt
- PerformedBy
- Note

---

## Bilinen Kısıtlar

Bu proje Render’ın **ücretsiz planı** üzerinde çalışmaktadır.  
Ücretsiz planda **kalıcı disk desteği bulunmadığından**, SQLite verileri servis yeniden başlatıldığında sıfırlanabilmektedir.

Bu durum kod kaynaklı değil, **hosting ortamının kısıtından** kaynaklanmaktadır.

---

## Geliştirici

**Furkan Dağal**  
Web Programlama Dersi – Final Projesi – Yozgat Bozok Üniversitesi 
