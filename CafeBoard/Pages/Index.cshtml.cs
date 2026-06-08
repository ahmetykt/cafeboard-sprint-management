using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CafeBoard.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace CafeBoard.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CafeTask> TodoTasks { get; set; } = new();
        public List<CafeTask> InProgressTasks { get; set; } = new();
        public List<CafeTask> DoneTasks { get; set; } = new();
        public List<CafeTask> BacklogTasks { get; set; } = new();

        // YENİ: Aktif sprinti arayüze taşımak için
        public Sprint ActiveSprint { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        [BindProperty]
        public CafeTask NewTask { get; set; }

        public List<SelectListItem> DeveloperList { get; set; }

        public async Task OnGetAsync()
        {
            // Aktif sprinti bul, yoksa çökmesin diye varsayılan oluştur
            ActiveSprint = await _context.Sprints.FirstOrDefaultAsync(s => s.IsActive)
                           ?? new Sprint { SprintName = "Sprint 1", IsActive = true };

            var query = _context.CafeTasks
                .Include(t => t.Developer)
                .Where(t => t.IsDeleted == false)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(t => t.Title.Contains(SearchQuery) || t.Description.Contains(SearchQuery));
            }

            var allTasks = await query.ToListAsync();

            BacklogTasks = allTasks.Where(t => t.Status == "Backlog").ToList(); // Yeni satır
            TodoTasks = allTasks.Where(t => t.Status == "To-Do").ToList();
            InProgressTasks = allTasks.Where(t => t.Status == "In Progress").ToList();
            DoneTasks = allTasks.Where(t => t.Status == "Done").ToList();

            var developers = await _context.Developers.AsNoTracking().ToListAsync();
            DeveloperList = developers.Select(d => new SelectListItem
            {
                Value = d.DeveloperId.ToString(),
                Text = d.FullName + " (" + d.Role + ")"
            }).ToList();
        }

        // === GÖREV OLUŞTURMA ===
        // Proje yöneticisinin (veya ekip üyesinin) sisteme yeni bir iş paketi girmesini sağlar.
        public async Task<IActionResult> OnPostCreateTaskAsync()
        {
            if (NewTask == null) return RedirectToPage();

            // Eğer bir bitiş tarihi (deadline) girilmemişse, varsayılan olarak 7 gün sonrası atanır.
            if (NewTask.Deadline == DateTime.MinValue || !NewTask.Deadline.HasValue)
                NewTask.Deadline = DateTime.Now.AddDays(7);

            if (NewTask.StoryPoints == null || NewTask.StoryPoints == 0)
                NewTask.StoryPoints = 3;

            // ÇÖZÜM: Hardcoded "Sprint 4" yerine veritabanındaki Aktif Sprinti buluyoruz
            var currentSprint = await _context.Sprints.FirstOrDefaultAsync(s => s.IsActive);
            string activeSprintName = currentSprint?.SprintName ?? "Sprint 1";

            // Görev yeni oluşturulduğu için otomatik olarak "Yapılacaklar (To-Do)" durumuna alınır.
            // Ayrıca başlangıç ilerleme yüzdesi (ProgressPercent) 0 olarak set edilir.
            NewTask.Status = "To-Do";
            NewTask.CreatedDate = DateTime.Now;
            NewTask.IsDeleted = false;
            NewTask.Sprint = activeSprintName;
            NewTask.ProgressPercent = 0;

            // Başlangıç tarihi girilmediyse bugünü ata
            if (!NewTask.StartDate.HasValue)
                NewTask.StartDate = DateTime.Now.Date;

            _context.CafeTasks.Add(NewTask);
            await _context.SaveChangesAsync();

            var log = new TaskLog
            {
                TaskId = NewTask.TaskId,
                LogMessage = $"Görev '{NewTask.Title}' başlığıyla {activeSprintName} koşusunda oluşturuldu."
            };
            _context.TaskLogs.Add(log);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        // DEVRİM: SPRINT BİTİRME, FİNANS HESAPLAMA VE DEVRETME MOTORU
        // Agile (Çevik) metodolojinin temel yapı taşı olan iterasyon (sprint) tamamlama algoritmasıdır.
        // Bu fonksiyon çalıştırıldığında eski sprintin finansal kapanışı yapılır ve kalan işler bir sonraki sprinte devredilir.
        public async Task<IActionResult> OnPostCompleteSprintAsync()
        {
            var oldSprint = await _context.Sprints.FirstOrDefaultAsync(s => s.IsActive);
            if (oldSprint == null) return RedirectToPage();

            // --- 1. KISIM: FİNANSAL DONDURMA VE BİLANÇO HESAPLAMASI ---

            // O sprintte TAMAMLANMIŞ olan (Done) işler filtrelenir.
            var completedTasks = await _context.CafeTasks
                .Where(t => t.Sprint == oldSprint.SprintName && t.Status == "Done" && t.IsDeleted == false)
                .ToListAsync();

            int doneCount = completedTasks.Count;
            int totalSpDone = completedTasks.Sum(t => t.StoryPoints ?? 0);

            // Finansal Bilanço Formülü: 
            // - Her 1 SP (Story Point), şirkete belirli bir TL ciro olarak yansır (Örn: 1 SP = 10.000 ₺).
            // - Haftalık sabit bir operasyonel gider (sunucu, ofis vs.) düşülerek Net Kâr hesaplanır.
            decimal sprintRevenue = totalSpDone * 10000;
            decimal sprintExpense = 25000;
            decimal netProfit = sprintRevenue - sprintExpense;

            // Sprint kapanış verileri, geçmişe dönük raporlama (Dashboard) için kalıcı olarak veritabanına dondurulur.
            var financialSummary = new SprintFinancialSummary
            {
                SprintName = oldSprint.SprintName,
                CompletedTaskCount = doneCount,
                TotalRevenue = sprintRevenue,
                TotalExpense = sprintExpense,
                NetProfit = netProfit,
                ClosedDate = DateTime.Now
            };
            _context.SprintFinancialSummaries.Add(financialSummary);

            // -------------------------------------------------------------

            // 1. Eski sprinti pasif yap
            oldSprint.IsActive = false;

            // 2. İsmini analiz edip sayıyı artır ("Sprint 4" -> "Sprint 5")
            int sprintNum = 1;
            if (oldSprint.SprintName.StartsWith("Sprint "))
            {
                int.TryParse(oldSprint.SprintName.Replace("Sprint ", ""), out sprintNum);
            }
            string newSprintName = $"Sprint {sprintNum + 1}";

            // 3. Yeni sprinti oluştur
            var newSprint = new Sprint
            {
                SprintName = newSprintName,
                IsActive = true,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7)
            };
            _context.Sprints.Add(newSprint);

            // 4. BİTMEMİŞ işleri bul, yeni sprinte geçir ve Log at!
            var incompleteTasks = await _context.CafeTasks
                .Where(t => t.Sprint == oldSprint.SprintName && t.Status != "Done" && t.IsDeleted == false)
                .ToListAsync();

            foreach (var task in incompleteTasks)
            {
                task.Sprint = newSprintName;
                _context.TaskLogs.Add(new TaskLog
                {
                    TaskId = task.TaskId,
                    LogMessage = $"🚨 Görev tamamlanamadığı için {oldSprint.SprintName} koşusundan {newSprintName} koşusuna devredildi!"
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteTaskAsync(int taskId)
        {
            var task = await _context.CafeTasks.FindAsync(taskId);
            if (task != null)
            {
                task.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // === KANBAN SÜRÜKLE-BIRAK (Drag & Drop) İŞLEMİ ===
        // Kullanıcı arayüzünde (UI) bir görev farklı bir sütuna taşındığında, bu AJAX çağrısı ile veritabanında güncellenir.
        public async Task<IActionResult> OnPostUpdateStatusAsync(int taskId, string newStatus)
        {
            var task = await _context.CafeTasks.FindAsync(taskId);
            if (task != null)
            {
                string eskiDurum = task.Status;

                // İş Kuralı (Business Rule): "Tamamlandı" (Done) statüsündeki bir görev geri alınamaz.
                if (eskiDurum == "Done")
                {
                    return new JsonResult(new { success = false, message = "Tamamlanan iş geri alınamaz!" });
                }

                task.Status = newStatus;
                
                // Eğer görev direkt "Done" sütununa sürüklenirse, ilerleme yüzdesi otomatik olarak %100 yapılır.
                if (newStatus == "Done")
                {
                    task.ProgressPercent = 100;
                }
                await _context.SaveChangesAsync();

                if (eskiDurum != newStatus)
                {
                    var log = new TaskLog
                    {
                        TaskId = taskId,
                        LogMessage = $"Görev durumu '{eskiDurum}' sütunundan '{newStatus}' sütununa sürüklendi."
                    };
                    _context.TaskLogs.Add(log);
                    await _context.SaveChangesAsync();
                }
                return new JsonResult(new { success = true });
            }
            return new JsonResult(new { success = false });
        }

        public async Task<IActionResult> OnPostAddCommentAsync(int taskId, int developerId, string commentText)
        {
            if (!string.IsNullOrWhiteSpace(commentText))
            {
                var comment = new TaskComment
                {
                    TaskId = taskId,
                    DeveloperId = developerId,
                    CommentText = commentText
                };
                _context.TaskComments.Add(comment);

                var dev = await _context.Developers.FindAsync(developerId);
                var devName = dev?.FullName ?? "Birisi";
                _context.TaskLogs.Add(new TaskLog { TaskId = taskId, LogMessage = $"{devName} göreve yeni bir yorum bıraktı." });

                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            return new JsonResult(new { success = false });
        }

        public async Task<IActionResult> OnGetTaskFullDetailsAsync(int taskId)
        {
            var logs = await _context.TaskLogs
                .Where(l => l.TaskId == taskId)
                .OrderByDescending(l => l.CreatedDate)
                .Select(l => new { msg = l.LogMessage, date = l.CreatedDate.ToString("dd.MM HH:mm") })
                .ToListAsync();

            var comments = await _context.TaskComments
                .Include(c => c.Developer)
                .Where(c => c.TaskId == taskId)
                .OrderBy(c => c.CreatedDate)
                .Select(c => new {
                    author = c.Developer.FullName,
                    role = c.Developer.Role,
                    text = c.CommentText,
                    date = c.CreatedDate.ToString("dd.MM.yyyy HH:mm")
                })
                .ToListAsync();

            return new JsonResult(new { logs = logs, comments = comments });
        }
    }
}