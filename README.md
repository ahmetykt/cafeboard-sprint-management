# CafeBoard - Çevik Proje Yönetimi ve Finansal Analiz Paneli

CafeBoard, **CafeSuiteERP** ekosisteminin yazılım geliştirme süreçlerini, ekip eforlarını, haftalık sprint (koşu) maliyetlerini ve saniye saniye gerçekleştirilen tüm sistem hareketlerini izlemek, analiz etmek ve raporlamak üzere tasarlanmış kapsamlı bir **Yönetici ve Performans İzleme Paneli (Dashboard)** projesidir.

Proje, hazır yönetim araçlarına bağımlı kalmaksızın, bir yazılım ekibinin ihtiyaç duyabileceği **Çevik (Agile) Metodoloji** gereksinimlerini kurumsal finans analizleriyle harmanlar.

---

## Kullanılan Teknolojiler ve Mimari

* **Framework:** .NET Core / C# Razor Pages Mimarisi
* **Veritabanı:** Microsoft SQL Server (MSSQL)
* **ORM / Veri Erişimi:** Entity Framework Core (EF Core)
* **Arayüz & Tasarım:** Bootstrap 5, Bootstrap Icons, Custom CSS3
* **Grafik & Görselleştirme:** Chart.js (Dinamik JS Grafik Motoru)

---

## Öne Çıkan Gelişmiş Özellikler

### 1. Dinamik İş Yükü ve Ekip Dengesi Analizi
* **Chart.js** entegrasyonu ile aktif sprintte hangi geliştiricinin üzerinde kaç **SP (Story Point)** iş yükü olduğu anlık olarak grafik üzerinde görselleştirilir. Bu sayede ekip içi görev dağılımı adaleti tek bakışta denetlenebilir.

### 2. Sprint (Hafta) Bazlı Detaylı Finansal Karneler
* Yazılım dünyasındaki sabit gider mantığı yıkılarak, **dinamik bir bütçe motoru** kurulmuştur.
* Her sprint akordeonuna tıklandığında; o hafta tamamlanan görevlerin toplam zorluk derecesine göre üretilen **Haftalık Ciro**, o hafta görev teslim eden personellerin taban maaşlarının toplamından oluşan **Haftalık Dinamik Gider** ve neticede elde edilen **Haftalık Net Kâr** anlık hesaplanır.

### 3. Geliştirici Efor ve Bordro Analizi
* **1 SP = 5 Saatlik İş Gücü** formülü kullanılarak, ekibin projeye harcadığı toplam efor ilişkisel olarak hesaplanır.
* Personellerin durum tablosundaki işleri "Done" (Tamamlandı) sütununa çekildikçe hak edişleri bordroya yansıtılır; bitmeyen veya devam eden işlerin maliyetleri hakedişe yansıtılmaz.

### 4. Saniye Saniye Hareket Dökümü (Task Logging Mimarisi)
* Veritabanı düzeyinde bağlanan `TaskLogs` yapısı sayesinde, ana panoda bir görev oluşturulduğunda, silindiğinde veya durum sütunları arasında sürüklendiğinde sistem otomatik olarak zaman damgalı log üretir.

### 5. Agile Proje Gantt Şeması (Zaman Çizelgesi)
* Tüm geçmiş ve aktif görevlerin hangi sprint fazında (Altyapı, Çekirdek Algoritmalar, Güvenlik, Finans vb.) yer aldığı, sorumlu geliştiricisiyle birlikte interaktif bir yol haritası (Roadmap) şeklinde listelenir.

---

## Veritabanı İlişkisel Veri Modeli (MSSQL)

Sistem, EF Core Code-First mimarisiyle tasarlanmış olup şu çekirdek tablolar üzerinde tam ilişkisel (Foreign Key) mimariyle çalışmaktadır:
* `CafeTasks`: Projedeki tüm görevleri, durumları (Status), öncelikleri ve SP puanlarını tutar.
* `Developers`: Ekip üyelerinin ad, soyad ve uzmanlık rollerini saklar.
* `DeveloperFinances`: Personellerin saatlik ücretlerini ve taban maaşlarını tutarak bordroyu besler.
* `Sprints`: Koşuların aktiflik durumunu yönetir.
* `SprintFinancialSummaries`: Kapatılan haftaların finansal verilerini mühürleyerek saklar.
* `TaskLogs`: Sistemdeki tüm drag-drop ve durum güncellemelerini kayıt altına alır.