using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Database")]
public class DatabaseController :  Controller {

      private readonly AppDbContext _context;

      public DatabaseController(AppDbContext context)
      {
            _context = context;
      }

      public ActionResult Index() {
            return View();
      }
      [HttpGet]
      public ActionResult DeleteDb() {
            return View();
      }
      [TempData]
      public string Message {get; set;} = string.Empty;
      [HttpPost]
      public async Task<ActionResult> DeleteDbAsync() {
            var success = await _context.Database.EnsureDeletedAsync();
            Message = success ? "Successfully delete database" : "Something went wrong";
            return RedirectToAction(nameof(Index));
      }
       [HttpPost]
       public async Task<ActionResult> Migration() {
            await _context.Database.MigrateAsync();
            Message = "Update database successfully!";
            return RedirectToAction(nameof(Index));
      }
}