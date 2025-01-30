using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UenodaiAPI.Data;
using UenodaiCommon.Models;


namespace UenodaiAPI.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetEvents()
        {
            var events = _context.Events.ToList();
            return Ok(events);
        }

        [HttpGet("suggested")]
        public IActionResult GetFeaturedEvents(int flex)
        {
            var param = new SqlParameter("@FLEX", flex);

            var events = _context.Events
                .FromSqlRaw("EXEC SP_T_EVENT_SELECT_FLEX @FLEX", param) 
                .ToList();

            // APIから返すレスポンスを変換
            var featuredEvents = events.Select(e => new
            {
                Image = e.IMAGE == null ? "default.jpg" : e.IMAGE,
                Title = e.TITLE,         
                Detail = e.DETAIL  
            }).ToList();

            return Ok(featuredEvents);
        }

        [HttpPost]
        public IActionResult AddEvent(Event newEvent)
        {
            var param = new SqlParameter[]
            {
                new SqlParameter("@TITLE", newEvent.TITLE),
                new SqlParameter("@IMAGE", newEvent.IMAGE),
                new SqlParameter("@PLACE", newEvent.PLACE),
                new SqlParameter("@DETAIL", newEvent.DETAIL),
                new SqlParameter("@BEGINDATETIME", newEvent.BEGINDATETIME),
                new SqlParameter("@ENDDATETIME", newEvent.ENDDATETIME),
                new SqlParameter("@UPDATERID", newEvent.UPDATERID)
            };

            var result = _context.Database.ExecuteSqlRaw(
                "EXEC [dbo].[SP_T_EVENT_INSERT] @TITLE, @IMAGE, @PLACE, @DETAIL, @BEGINDATETIME, @ENDDATETIME, @UPDATERID",
                param);

            // 実行が成功した場合、新しく追加されたイベント情報を返す
            return Ok(newEvent);
        }
    }
}
