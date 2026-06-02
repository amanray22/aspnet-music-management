using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_Music.CustomControllers;
using MVC_Music.Data;
using MVC_Music.Models;
using MVC_Music.Utilities;
using MVC_Music.ViewModels;
using OfficeOpenXml;

namespace MVC_Music.Controllers
{
    [Authorize(Roles = "Staff,Supervisor,Admin")]
    public class SongPerformanceController : ElephantController
    {
        private readonly MusicContext _context;

        public SongPerformanceController(MusicContext context)
        {
            _context = context;
        }

        // GET: SongPerformance
        public async Task<IActionResult> Index(int? SongID, int? MusicianID, int? InstrumentID, int? page, int? pageSizeID, string SearchString, string actionButton, string sortDirection = "asc", string sortField = "Musician")
        {
            //Get the URL with the last filter, sort and page parameters
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Song");

            if (!SongID.HasValue)
            {
                //Go back to the proper return URL for the Song controller
                return SafeRedirectToReturnUrl(ViewData["returnURL"]?.ToString(), "Song");
            }

            PopulateDropDownLists();

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Musician", "Instrument", "Fee Paid" };

            var performances = from p in _context.Performances
                                   .Include(p => p.Song)
                                   .Include(p => p.Musician)
                                   .Include(p => p.Instrument)
                               where p.SongID == SongID.GetValueOrDefault()
                               select p;

            if (MusicianID.HasValue)
            {
                performances = performances.Where(p => p.MusicianID == MusicianID);
                numberFilters++;
            }
            if (InstrumentID.HasValue)
            {
                performances = performances.Where(p => p.InstrumentID == InstrumentID);
                numberFilters++;
            }
            if (!String.IsNullOrWhiteSpace(SearchString))
            {
                performances = performances.Where(p => p.Comments.ToUpper()
                    .Contains(SearchString.ToUpper()));
                numberFilters++;
            }

            //Give feedback about the state of the filters
            if (numberFilters != 0)
            {
                ViewData["Filtering"] = " btn-danger";
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                //ViewData["ShowFilter"] = " show";
            }
            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton))
            {
                page = 1;

                if (sortOptions.Contains(actionButton))
                {
                    if (actionButton == sortField)
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;
                }
            }
            //Now we know which field and direction to sort by.
            if (sortField == "Musician")
            {
                if (sortDirection == "asc")
                {
                    performances = performances
                        .OrderBy(p => p.Musician.LastName)
                        .ThenBy(p => p.Musician.FirstName);
                }
                else
                {
                    performances = performances
                        .OrderByDescending(p => p.Musician.LastName)
                        .ThenByDescending(p => p.Musician.FirstName);
                }
            }
            else if (sortField == "Instrument")
            {
                if (sortDirection == "asc")
                {
                    performances = performances
                        .OrderBy(p => p.Instrument.Name);
                }
                else
                {
                    performances = performances
                        .OrderByDescending(p => p.Instrument.Name);
                }
            }
            else // Fee Paid
            {
                if (sortDirection == "asc")
                {
                    performances = performances.OrderBy(p => p.FeePaid);
                }
                else
                {
                    performances = performances.OrderByDescending(p => p.FeePaid);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Now get the MASTER record, the Song
            Song? song = await _context.Songs
                .Include(s => s.Album)
                .Include(s => s.Genre)
                .Where(s => s.ID == SongID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.Song = song;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Performance>
                .CreateAsync(performances.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: SongPerformance/Add
        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public IActionResult Add(int? SongID, string SongTitle)
        {

            if (!SongID.HasValue)
            {
                return SafeRedirectToReturnUrl(ViewData["returnURL"]?.ToString(), "Song");
            }
            var song = _context.Songs
                .AsNoTracking()
                .FirstOrDefault(s => s.ID == SongID.Value);

            ViewData["SongTitle"] = SongTitle;

            Performance s = new()
            {
                SongID = SongID.GetValueOrDefault(),
                FeePaid = 50.00
            };

            PopulateDropDownLists();
            return View(s);
        }

        // POST: SongPerformance/Add
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public async Task<IActionResult> Add([Bind("ID,Comments, SongID, FeePaid,MusicianID,InstrumentID")] Performance performance, string SongTitle)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(performance);
                    await _context.SaveChangesAsync();
                    return SafeRedirectToReturnUrl(ViewData["returnURL"]?.ToString(), "Song");
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to add performance. Try again, and if the problem " +
                    "persists see your system administrator.");
            }

            PopulateDropDownLists(performance);
            ViewData["SongTitle"] = SongTitle;
            return View(performance);
        }

        // GET: SongPerformance/Update/5
        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var performance = await _context.Performances
               .Include(a => a.Musician)
               .Include(a => a.Instrument)
               .Include(a => a.Song)
               .AsNoTracking()
               .FirstOrDefaultAsync(m => m.ID == id);
            if (performance == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(performance);
            return View(performance);
        }

        // POST: SongPerformance/Update/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var performanceToUpdate = await _context.Performances
               .Include(a => a.Musician)
               .Include(a => a.Instrument)
               .Include(a => a.Song)
               .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (performanceToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Performance>(performanceToUpdate, "",
                p => p.Comments, p => p.MusicianID, p => p.FeePaid, p => p.InstrumentID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return SafeRedirectToReturnUrl(ViewData["returnURL"]?.ToString(), "Song");
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!PerformanceExists(performanceToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                        "persists see your system administrator.");
                }
            }
            PopulateDropDownLists(performanceToUpdate);
            return View(performanceToUpdate);
        }

        // GET: SongPerformance/Remove/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var performance = await _context.Performances
                .Include(p => p.Instrument)
                .Include(p => p.Musician)
                .Include(p => p.Song)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (performance == null)
            {
                return NotFound();
            }

            return View(performance);
        }

        // POST: SongPerformance/Remove/5
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveConfirmed(int id)
        {
            var performance = await _context.Performances
               .Include(a => a.Musician)
               .Include(a => a.Instrument)
               .Include(a => a.Song)
               .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                if (performance == null)
                {
                    return NotFound();
                }

                _context.Performances.Remove(performance);
                await _context.SaveChangesAsync();
                return SafeRedirectToReturnUrl(ViewData["returnURL"]?.ToString(), "Song");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                    "persists see your system administrator.");
            }

            return View(performance);
        }

        private SelectList InstrumentSelectList(int? selectedId)
        {
            var dQuery = from d in _context.Instruments
                         orderby d.Name
                         select d;

            return new SelectList(dQuery, "ID", "Name", selectedId);
        }

        private SelectList MusicianSelectList(int? selectedId)
        {
            var dQuery = from d in _context.Musicians
                         orderby d.LastName, d.FirstName
                         select d;

            return new SelectList(dQuery, "ID", "Summary", selectedId);
        }

        private void PopulateDropDownLists(Performance? performance = null)
        {
            ViewData["InstrumentID"] = InstrumentSelectList(performance?.InstrumentID);
            ViewData["MusicianID"] = MusicianSelectList(performance?.MusicianID);
        }

        private bool PerformanceExists(int id)
        {
            return _context.Performances.Any(e => e.ID == id);
        }




        [Authorize(Roles = "Staff,Supervisor,Admin")]
        #region Extra steps for Excel a files and reporting 

        public async Task<IActionResult> PerformanceSummary(int? page, int? pageSizeID)
        {
            var sumQ = GetPerfromanceSummaries();

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "PerformanceSummary"); //Remember for this View
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<PerformanceSummaryVM>.CreateAsync(sumQ.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }


        private IQueryable<PerformanceSummaryVM> GetPerfromanceSummaries()
        {
            return _context.Musicians
                .GroupJoin(
                        _context.Performances,
                        m => m.ID,
                        p => p.MusicianID,
                        (musician, performances) => new { musician, performances }
                        )
                .Select(grp => new PerformanceSummaryVM
                {
                    ID = grp.musician.ID,
                    First_Name = grp.musician.FirstName,
                    Middle_Name = grp.musician.MiddleName,
                    Last_Name = grp.musician.LastName,
                    Average_FeePaid = grp.performances.Any() ? (double?)grp.performances.Average(a => a.FeePaid) : null,
                    Highest_FeePaid = grp.performances.Any() ? (double?)grp.performances.Max(a => a.FeePaid) : null,
                    Lowest_FeePaid = grp.performances.Any() ? (double?)grp.performances.Min(a => a.FeePaid) : null,
                    Total_Performances = grp.performances.Count(),
                    Total_Songs = grp.performances.Select(a => a.SongID).Distinct().Count()
                })
                .OrderBy(a => a.Last_Name).ThenBy(a => a.First_Name);

        }


        //Code to Export Summary 
        public IActionResult PerformanceSummaryExport()
        {
            var sumQ = GetPerfromanceSummaries().ToList();

            int numRows = sumQ.Count();
            int totalPerformances = sumQ.Sum(a => a.Total_Performances ?? 0);

            if (numRows > 0) //We have data
            {
                //Create a new spreadsheet from scratch.
                using (ExcelPackage excel = new ExcelPackage())
                {
                    var workSheet = excel.Workbook.Worksheets.Add("Performance Summary");

                    // Report Title
                    workSheet.Cells[1, 1].Value = "Performance Summary Report";
                    workSheet.Cells[1, 1].Style.Font.Bold = true;
                    workSheet.Cells[1, 1].Style.Font.Size = 16;

                    // Date/Time generated
                    workSheet.Cells[2, 1].Value = "Generated:";
                    workSheet.Cells[2, 2].Value = DateTime.Now.ToString("g");

                    // Data start position row 4
                    workSheet.Cells[4, 1].LoadFromCollection(sumQ, true);

                    int dataEndRow = 4 + numRows;

                    // Summary Rows
                    workSheet.Cells[dataEndRow + 2, 1].Value = "Total Performances:";
                    workSheet.Cells[dataEndRow + 2, 2].Value = totalPerformances;

                    workSheet.Cells[dataEndRow + 3, 1].Value = "Total Musicians:";
                    workSheet.Cells[dataEndRow + 3, 2].Value = numRows;

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();

                    //Ok, time to download the Excel

                    try
                    {
                        Byte[] theData = excel.GetAsByteArray();
                        string filename = "PerformanceSummary.xlsx";
                        string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        return File(theData, mimeType, filename);
                    }
                    catch (Exception)
                    {
                        return BadRequest("Could not build and download the file.");
                    }
                }
            }
            return NotFound("No data to Show");

        }

        #endregion

    }
}
