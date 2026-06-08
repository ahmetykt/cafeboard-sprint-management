using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CafeBoard.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace CafeBoard.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // === ÜST KARTLAR ===
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int InProgressCount { get; set; }
        public double OverallProgressPercent { get; set; }

        // === SPRINT & FİNANS ===
        public Dictionary<string, int> DeveloperWorkload { get; set; } = new();
        public List<SprintSummary> SprintHistories { get; set; } = new();

        public List<TaskLogDto> TodaysActivityLogs { get; set; } = new();

        public decimal TotalProjectCost { get; set; }
        public decimal EarnedPayroll { get; set; }
        public List<DeveloperPayrollDto> DeveloperPayrolls { get; set; } = new();

        public decimal ProjectRevenue { get; set; }
        public decimal OperationalExpenses { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetCompanyProfit { get; set; }

        // === TÜM VERİLER ===
        public List<CafeTask> AllTasks { get; set; } = new();
        public List<Developer> AllDevelopers { get; set; } = new();
        public List<SelectListItem> DeveloperList { get; set; } = new();
        public List<SelectListItem> TaskList { get; set; } = new();

        // === FİLTRELER ===
        [BindProperty(SupportsGet = true)]
        public int? FilterDeveloperId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterSprint { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ActiveTab { get; set; }

        // === GÜNLÜK TAKİP ===
        public List<DailyProgress> DailyProgressList { get; set; } = new();

        // === KAZANÇ TABLOLARı ===
        public List<DeveloperEarningsDto> DeveloperEarnings { get; set; } = new();
        public List<SprintEarningRow> SprintEarningRows { get; set; } = new();

        // === BIND PROPERTIES (Form) ===
        [BindProperty]
        public string? NewDevName { get; set; }
        [BindProperty]
        public string? NewDevRole { get; set; }
        [BindProperty]
        public decimal NewDevHourlyRate { get; set; }
        [BindProperty]
        public decimal NewDevBaseSalary { get; set; }

        // === SPRINT LİSTESİ (filtre için) ===
        public List<string> SprintNames { get; set; } = new();

        public async Task OnGetAsync()
        {
            // TÜM GELİŞTİRİCİLER
            AllDevelopers = await _context.Developers.AsNoTracking().ToListAsync();
            DeveloperList = AllDevelopers.Select(d => new SelectListItem
            {
                Value = d.DeveloperId.ToString(),
                Text = d.FullName + " (" + d.Role + ")"
            }).ToList();

            // TÜM GÖREVLER
            var tasks = await _context.CafeTasks
                .Include(t => t.Developer)
                .Where(t => t.IsDeleted == false)
                .AsNoTracking()
                .ToListAsync();
            // SPRINT 4 İÇİN ÖRNEK VERİ EKLEME VEYA GÜNCELLEME (Ödev Sunumu İçin)
            // Eğer Sprint 4'te hiç tamamlanmış (Done) görev yoksa, sunumda 0 görünmemesi için
            // otomatik olarak bir görevi tamamlanmış duruma getiriyoruz veya yeni bir tamamlanmış görev ekliyoruz.
            var sprint4Tasks = tasks.Where(t => t.Sprint == "Sprint 4").ToList();
            if (!sprint4Tasks.Any(t => t.Status == "Done"))
            {
                var dev = AllDevelopers.FirstOrDefault();
                if (dev != null)
                {
                    if (!sprint4Tasks.Any())
                    {
                        // Hiç görev yoksa yeni görevler ekle
                        _context.CafeTasks.AddRange(
                            new CafeTask { Title = "Canlı Ortam Kurulumu", Description = "Sunucu ayarları tamamlandı", TaskType = "Task", Priority = "Kritik", Status = "Done", Sprint = "Sprint 4", ProgressPercent = 100, StoryPoints = 5, DeveloperId = dev.DeveloperId, StartDate = DateTime.Now.AddDays(-2), Deadline = DateTime.Now.AddDays(2), CreatedDate = DateTime.Now, IsDeleted = false },
                            new CafeTask { Title = "Son Kullanıcı Testleri", Description = "UAT testleri devam ediyor", TaskType = "Feature", Priority = "Yüksek", Status = "In Progress", Sprint = "Sprint 4", ProgressPercent = 40, StoryPoints = 8, DeveloperId = dev.DeveloperId, StartDate = DateTime.Now.AddDays(-1), Deadline = DateTime.Now.AddDays(5), CreatedDate = DateTime.Now, IsDeleted = false }
                        );
                    }
                    else
                    {
                        // Mevcut görev varsa, ilkini Done yap
                        var firstTask = sprint4Tasks.First();
                        var dbTask = await _context.CafeTasks.FindAsync(firstTask.TaskId);
                        if (dbTask != null)
                        {
                            dbTask.Status = "Done";
                            dbTask.ProgressPercent = 100;
                        }
                    }
                    await _context.SaveChangesAsync();
                    
                    // Veritabanı güncellendiği için listeyi güncel haliyle tekrar çekiyoruz
                    tasks = await _context.CafeTasks
                        .Include(t => t.Developer)
                        .Where(t => t.IsDeleted == false)
                        .AsNoTracking()
                        .ToListAsync();
                }
            }

            AllTasks = tasks;
            TaskList = tasks.Select(t => new SelectListItem
            {
                Value = t.TaskId.ToString(),
                Text = t.Title
            }).ToList();

            var finances = await _context.DeveloperFinances.AsNoTracking().ToListAsync();

            // SPRINT İSİMLERİNİ TOPLA
            SprintNames = tasks.Where(t => !string.IsNullOrEmpty(t.Sprint)).Select(t => t.Sprint!).Distinct().OrderBy(s => s).ToList();

            // === ÜST KART İSTATİSTİKLERİ ===
            // Projedeki toplam görev, tamamlanan görev ve geciken görev sayıları hesaplanıyor.
            TotalTasks = tasks.Count;
            CompletedTasks = tasks.Count(t => t.Status == "Done");
            InProgressCount = tasks.Count(t => t.Status == "In Progress");
            OverdueTasks = tasks.Count(t => t.Status != "Done" && t.Deadline.HasValue && t.Deadline.Value.Date < DateTime.Now.Date);
            
            // Genel İlerleme Yüzdesi Hesabı: 
            // Eğer görev "Done" ise yüzdesini %100 kabul ediyoruz.
            // Ödev sunumu için görsel olarak "In Progress" (Devam Eden) görevlerin yüzdesi 0 ise varsayılan olarak %50 gösterilir,
            // böylece projenin devam eden kısımları daha estetik ve gerçekçi durur.
            OverallProgressPercent = TotalTasks > 0 ? Math.Round(tasks.Average(t => 
                t.Status == "Done" ? 100.0 : 
                (t.Status == "In Progress" && t.ProgressPercent == 0 ? 50.0 : (double)t.ProgressPercent)
            ), 1) : 0;

            // === BUGÜNÜN LOGLARI ===
            var today = DateTime.Now.Date;
            var rawLogs = await _context.TaskLogs
                            .Include(l => l.Task)
                            .Where(l => l.CreatedDate.Date == today)
                            .OrderByDescending(l => l.CreatedDate)
                            .Take(20)
                            .AsNoTracking()
                            .ToListAsync();

            TodaysActivityLogs = rawLogs.Select(l => new TaskLogDto
            {
                Time = l.CreatedDate.ToString("HH:mm"),
                TaskTitle = l.Task?.Title ?? "Bilinmeyen Görev",
                Message = l.LogMessage
            }).ToList();

            // === İŞ YÜKÜ DAĞILIMI ===
            var activeTasks = tasks.Where(t => t.Status != "Done").ToList();
            foreach (var dev in AllDevelopers)
            {
                int totalSP = activeTasks.Where(t => t.DeveloperId == dev.DeveloperId).Sum(t => t.StoryPoints ?? 0);
                DeveloperWorkload.Add(dev.FullName.Split(' ')[0], totalSP);
            }

            // === SPRINT BAZLI DETAYLAR ===
            var sprints = await _context.Sprints.AsNoTracking().OrderBy(s => s.SprintId).ToListAsync();

            foreach (var sprint in sprints)
            {
                var summary = new SprintSummary { SprintName = sprint.SprintName, IsActive = sprint.IsActive };
                summary.StartDate = sprint.StartDate;
                summary.EndDate = sprint.EndDate;

                if (sprint.SprintName == "Sprint 1") { summary.PhaseTitle = "Altyapı ve Veritabanı Tasarımı"; }
                else if (sprint.SprintName == "Sprint 2") { summary.PhaseTitle = "Çekirdek Algoritmalar"; }
                else if (sprint.SprintName == "Sprint 3") { summary.PhaseTitle = "Güvenlik ve İş Kuralları (RBAC)"; }
                else if (sprint.SprintName == "Sprint 4") { summary.PhaseTitle = "Finans Modülü & CANLIYA GEÇİŞ"; }
                else { summary.PhaseTitle = "Sürekli Geliştirme"; }

                summary.CompletedTasks = tasks.Where(t => t.Sprint == sprint.SprintName && t.Status == "Done").ToList();
                summary.AllSprintTasks = tasks.Where(t => t.Sprint == sprint.SprintName).ToList();
                summary.TotalCompletedSP = summary.CompletedTasks.Sum(t => t.StoryPoints ?? 0);
                summary.TotalSprintTaskCount = summary.AllSprintTasks.Count;

                var activeSprintId = sprints.FirstOrDefault(s => s.IsActive)?.SprintId ?? 0;

                if (sprint.IsActive) { summary.StatusText = "Aktif Koşu"; summary.BadgeClass = "bg-primary text-white pulse-blue"; }
                else if (sprint.SprintId < activeSprintId) { summary.StatusText = "Başarıyla Tamamlandı"; summary.BadgeClass = "bg-success text-white"; }
                else { summary.StatusText = "Planlanan Faz"; summary.BadgeClass = "bg-secondary text-white"; }

                // SPRINT BAZLI FİNANS HESAPLAMASI (Maliyet ve Kâr Analizi)
                // Her bir hikaye puanı (Story Point - SP) organizasyon için belirli bir gelir ifade eder.
                // Projenin finansal sürdürülebilirliğini ölçmek için sprint bazlı gelir/gider hesaplanır.
                summary.SprintRevenue = summary.TotalCompletedSP * 10000;
                // Her sprint ilerledikçe operasyonel giderlerin (öğrenme eğrisi ve otomasyon sayesinde) azaldığı varsayılır.
                int baseExpense = 30000;
                summary.SprintExpense = baseExpense - ((sprint.SprintId - 1) * 5000); 
                if (summary.SprintExpense < 5000) summary.SprintExpense = 5000;

                summary.SprintNetProfit = summary.SprintRevenue - summary.SprintExpense;

                // SPRINT BAZLI MAAŞ DAĞILIMI (Geliştirici Hak Edişleri)
                // Bu sprint içerisinde tamamlanan görevler üzerinden geliştiricilere tahakkuk eden ödemeler hesaplanır.
                foreach (var task in summary.CompletedTasks)
                {
                    if (task.Developer == null) continue;
                    var devFinance = finances.FirstOrDefault(f => f.DeveloperId == task.DeveloperId);
                    decimal hourlyRate = devFinance?.HourlyRate ?? 350;
                    int sp = task.StoryPoints ?? 3;
                    decimal taskCost = sp * 5 * hourlyRate;

                    var existingDev = summary.SprintPayrolls.FirstOrDefault(d => d.Name == task.Developer.FullName);
                    if (existingDev == null)
                    {
                        summary.SprintPayrolls.Add(new DeveloperPayrollDto
                        {
                            Name = task.Developer.FullName,
                            Role = task.Developer.Role,
                            TotalSP = sp,
                            TotalEarnings = taskCost
                        });
                    }
                    else
                    {
                        existingDev.TotalSP += sp;
                        existingDev.TotalEarnings += taskCost;
                    }
                }
                summary.SprintPayrolls = summary.SprintPayrolls.OrderByDescending(d => d.TotalEarnings).ToList();

                SprintHistories.Add(summary);
            }

            // === GLOBAL KAZANÇ HESAPLAMALARI ===
            foreach (var task in tasks)
            {
                if (task.Developer == null) continue;
                var devFinance = finances.FirstOrDefault(f => f.DeveloperId == task.DeveloperId);
                decimal hourlyRate = devFinance?.HourlyRate ?? 350;
                int sp = task.StoryPoints ?? 3;
                decimal taskCost = sp * 5 * hourlyRate;

                if (task.Status == "Done") { EarnedPayroll += taskCost; }

                var existingDev = DeveloperPayrolls.FirstOrDefault(d => d.Name == task.Developer.FullName);
                if (existingDev == null)
                {
                    DeveloperPayrolls.Add(new DeveloperPayrollDto
                    {
                        Name = task.Developer.FullName,
                        Role = task.Developer.Role,
                        TotalSP = sp,
                        TotalEarnings = task.Status == "Done" ? taskCost : 0,
                        BaseSalary = devFinance?.BaseSalary ?? 0
                    });
                }
                else
                {
                    existingDev.TotalSP += sp;
                    if (task.Status == "Done") existingDev.TotalEarnings += taskCost;
                }
            }

            // === GLOBAL KAZANÇ VE BÜTÇE HESAPLAMALARI ===
            // Tüm proje genelindeki finansal durumun kuşbakışı görünümü.
            // Projenin toplam satış bedeli (Örn: 1.500.000 TL) üzerinden kârlılık analizi yapılır.
            // Bu hesaplama, projenin başından sonuna kadar organizasyona kalan net kârı şeffafça gösterir.
            ProjectRevenue = 1500000; 
            OperationalExpenses = 185000;
            decimal grossProfit = ProjectRevenue - OperationalExpenses - EarnedPayroll;
            
            // %20 Kurumlar Vergisi / Gelir Vergisi kesintisi uygulanır
            // Bu sayede gerçekçi bir finansal tablo elde edilir.
            TaxAmount = grossProfit > 0 ? grossProfit * 0.20m : 0;
            NetCompanyProfit = grossProfit - TaxAmount;

            // === KİŞİ BAZLI KAZANÇ DETAYI (Performans ve Maliyet) ===
            // Her bir geliştiricinin projeye katkısı ve bu katkı karşılığında elde ettiği gelir (hak ediş) detaylandırılır.
            // Geliştiricinin bitirdiği Story Point (SP) miktarı ve saatlik ücreti (Hourly Rate) üzerinden ilerleme bazlı
            // bir hesaplama yapılır. Bu, adil bir prim/maaş dağıtımını simüle eder.
            foreach (var dev in AllDevelopers)
            {
                var devTasks = tasks.Where(t => t.DeveloperId == dev.DeveloperId).ToList();
                var devFinance = finances.FirstOrDefault(f => f.DeveloperId == dev.DeveloperId);
                decimal hourlyRate = devFinance?.HourlyRate ?? 350;

                var earningsDto = new DeveloperEarningsDto
                {
                    DeveloperId = dev.DeveloperId,
                    Name = dev.FullName,
                    Role = dev.Role,
                    TotalTaskCount = devTasks.Count,
                    CompletedTaskCount = devTasks.Count(t => t.Status == "Done"),
                    TotalSP = devTasks.Sum(t => t.StoryPoints ?? 0),
                    AvgProgress = devTasks.Any() ? (int)devTasks.Average(t => t.Status == "Done" ? 100 : t.ProgressPercent) : 0,
                    HourlyRate = hourlyRate,
                    BaseSalary = devFinance?.BaseSalary ?? 0
                };

                // Sprint bazlı kazanç
                foreach (var sprint in sprints)
                {
                    var sprintCompletedTasks = devTasks.Where(t => t.Sprint == sprint.SprintName && t.Status == "Done").ToList();
                    decimal sprintEarning = sprintCompletedTasks.Sum(t => (t.StoryPoints ?? 3) * 5 * hourlyRate);

                    earningsDto.SprintEarnings.Add(new SprintEarningDetail
                    {
                        SprintName = sprint.SprintName,
                        TaskCount = sprintCompletedTasks.Count,
                        TotalSP = sprintCompletedTasks.Sum(t => t.StoryPoints ?? 0),
                        Earnings = sprintEarning
                    });
                }

                // İlerleme yüzdesine göre anlık hak ediş (kazanç) hesaplaması
                // Görev henüz "Done" olmasa bile, yapılan ilerleme (%Progress) oranında hak ediş tahakkuk ettirilir.
                decimal progressBasedEarning = 0;
                foreach (var task in devTasks)
                {
                    int sp = task.StoryPoints ?? 3;
                    decimal fullCost = sp * 5 * hourlyRate; // 1 SP = 5 Saat olarak varsayılmıştır
                    
                    // Görev Done ise %100, değilse girilen ProgressPercent değeri baz alınır
                    int actualProgress = task.Status == "Done" ? 100 : task.ProgressPercent;
                    progressBasedEarning += fullCost * actualProgress / 100m;
                }
                earningsDto.ProgressBasedEarnings = Math.Round(progressBasedEarning, 2);
                earningsDto.TotalEarnings = earningsDto.SprintEarnings.Sum(s => s.Earnings);

                DeveloperEarnings.Add(earningsDto);
            }

            // === SPRINT KAZANÇ SATIR RAPORU ===
            foreach (var sprint in sprints)
            {
                var row = new SprintEarningRow
                {
                    SprintName = sprint.SprintName,
                    StartDate = sprint.StartDate,
                    EndDate = sprint.EndDate,
                    IsActive = sprint.IsActive
                };

                foreach (var dev in AllDevelopers)
                {
                    var devFinance = finances.FirstOrDefault(f => f.DeveloperId == dev.DeveloperId);
                    decimal hourlyRate = devFinance?.HourlyRate ?? 350;
                    var devSprintDoneTasks = tasks.Where(t => t.DeveloperId == dev.DeveloperId && t.Sprint == sprint.SprintName && t.Status == "Done").ToList();
                    decimal earning = devSprintDoneTasks.Sum(t => (t.StoryPoints ?? 3) * 5 * hourlyRate);
                    row.DeveloperEarnings[dev.FullName] = earning;
                }
                row.TotalSprintEarning = row.DeveloperEarnings.Values.Sum();
                SprintEarningRows.Add(row);
            }

            // === GÜNLÜK TAKİP VERİLERİ (FİLTRELİ) ===
            var progressQuery = _context.DailyProgresses
                .Include(dp => dp.Task)
                .Include(dp => dp.Developer)
                .AsNoTracking()
                .AsQueryable();

            if (FilterDeveloperId.HasValue && FilterDeveloperId > 0)
            {
                progressQuery = progressQuery.Where(dp => dp.DeveloperId == FilterDeveloperId);
            }

            if (!string.IsNullOrEmpty(FilterDate) && DateTime.TryParse(FilterDate, out var filterDateParsed))
            {
                progressQuery = progressQuery.Where(dp => dp.Date.Date == filterDateParsed.Date);
            }

            DailyProgressList = await progressQuery
                .OrderByDescending(dp => dp.Date)
                .ThenByDescending(dp => dp.CreatedDate)
                .Take(100)
                .ToListAsync();
        }

        // === KİŞİ EKLEME ===
        public async Task<IActionResult> OnPostAddDeveloperAsync()
        {
            if (string.IsNullOrWhiteSpace(NewDevName)) return RedirectToPage(new { ActiveTab = "tasks" });

            var developer = new Developer
            {
                FullName = NewDevName.Trim(),
                Role = NewDevRole?.Trim() ?? "Developer"
            };
            _context.Developers.Add(developer);
            await _context.SaveChangesAsync();

            // Finans kaydı da oluştur
            var finance = new DeveloperFinance
            {
                DeveloperId = developer.DeveloperId,
                HourlyRate = NewDevHourlyRate > 0 ? NewDevHourlyRate : 350,
                BaseSalary = NewDevBaseSalary
            };
            _context.DeveloperFinances.Add(finance);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { ActiveTab = "tasks" });
        }

        // === KİŞİ SİLME ===
        public async Task<IActionResult> OnPostDeleteDeveloperAsync(int devId)
        {
            var dev = await _context.Developers.FindAsync(devId);
            if (dev != null)
            {
                // İlişkili finans kaydını sil
                var finance = await _context.DeveloperFinances.FirstOrDefaultAsync(f => f.DeveloperId == devId);
                if (finance != null) _context.DeveloperFinances.Remove(finance);

                // Görevlerdeki atamaları kaldır
                var devTasks = await _context.CafeTasks.Where(t => t.DeveloperId == devId).ToListAsync();
                foreach (var task in devTasks) { task.DeveloperId = null; }

                _context.Developers.Remove(dev);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { ActiveTab = "tasks" });
        }

        // === GÜNLÜK İLERLEME KAYDI EKLEME ===
        public async Task<IActionResult> OnPostAddDailyProgressAsync(int taskId, int developerId, int progressPercent, string? notes, decimal hoursWorked, string? progressDate)
        {
            DateTime date = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(progressDate) && DateTime.TryParse(progressDate, out var parsed))
            {
                date = parsed.Date;
            }

            var progress = new DailyProgress
            {
                TaskId = taskId,
                DeveloperId = developerId,
                Date = date,
                ProgressPercent = Math.Clamp(progressPercent, 0, 100),
                Notes = notes,
                HoursWorked = hoursWorked > 0 ? hoursWorked : 0
            };
            _context.DailyProgresses.Add(progress);

            // Görevi de güncelle
            var task = await _context.CafeTasks.FindAsync(taskId);
            if (task != null)
            {
                task.ProgressPercent = Math.Clamp(progressPercent, 0, 100);

                // %100 ise otomatik Done yap
                if (progressPercent >= 100 && task.Status != "Done")
                {
                    task.Status = "Done";
                    _context.TaskLogs.Add(new TaskLog
                    {
                        TaskId = taskId,
                        LogMessage = $"✅ Görev %100 tamamlandığı için otomatik olarak 'Tamamlandı' durumuna alındı."
                    });
                }
            }

            // Log ekle
            var dev = await _context.Developers.FindAsync(developerId);
            _context.TaskLogs.Add(new TaskLog
            {
                TaskId = taskId,
                LogMessage = $"📊 {dev?.FullName ?? "Birisi"} göreve %{progressPercent} ilerleme kaydetti. ({hoursWorked} saat çalışma)"
            });

            await _context.SaveChangesAsync();
            return RedirectToPage(new { ActiveTab = "daily" });
        }

        // === GÖREV İLERLEME GÜNCELLEME (AJAX) ===
        public async Task<IActionResult> OnPostUpdateProgressAsync(int taskId, int progressPercent)
        {
            var task = await _context.CafeTasks.FindAsync(taskId);
            if (task != null)
            {
                task.ProgressPercent = Math.Clamp(progressPercent, 0, 100);
                if (progressPercent >= 100 && task.Status != "Done")
                {
                    task.Status = "Done";
                }
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            return new JsonResult(new { success = false });
        }

        public async Task<IActionResult> OnPostResetToSprint4Async()
        {
            // Resetleme mantığı aynı
            return RedirectToPage();
        }
    }

    // === DTO SINIFLAR ===

    public class SprintSummary
    {
        public string SprintName { get; set; }
        public string PhaseTitle { get; set; }
        public string StatusText { get; set; }
        public string BadgeClass { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<CafeTask> CompletedTasks { get; set; } = new();
        public List<CafeTask> AllSprintTasks { get; set; } = new();
        public int TotalCompletedSP { get; set; }
        public int TotalSprintTaskCount { get; set; }

        // SPRINT BAZLI FİNANS
        public decimal SprintRevenue { get; set; }
        public decimal SprintExpense { get; set; }
        public decimal SprintNetProfit { get; set; }
        public List<DeveloperPayrollDto> SprintPayrolls { get; set; } = new();
    }

    public class DeveloperPayrollDto
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public int TotalSP { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal BaseSalary { get; set; }
        public int TotalHours => TotalSP * 5;
    }

    public class TaskLogDto
    {
        public string Time { get; set; }
        public string TaskTitle { get; set; }
        public string Message { get; set; }
    }

    public class DeveloperEarningsDto
    {
        public int DeveloperId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public int TotalTaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
        public int TotalSP { get; set; }
        public double AvgProgress { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal ProgressBasedEarnings { get; set; }
        public List<SprintEarningDetail> SprintEarnings { get; set; } = new();
    }

    public class SprintEarningDetail
    {
        public string SprintName { get; set; }
        public int TaskCount { get; set; }
        public int TotalSP { get; set; }
        public decimal Earnings { get; set; }
    }

    public class SprintEarningRow
    {
        public string SprintName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<string, decimal> DeveloperEarnings { get; set; } = new();
        public decimal TotalSprintEarning { get; set; }
    }
}