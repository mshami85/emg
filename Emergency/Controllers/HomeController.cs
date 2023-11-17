using ChartJSCore.Helpers;
using ChartJSCore.Models;
using Emergency.Data;
using Emergency.Dtos;
using Emergency.Models;
using Emergency.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emergency.Controllers
{
    [Authorize(Roles = UserRoles.WEB_USER)]
    public class HomeController : Controller
    {
        DBContext _context;
        IConnectedUserService _userService;

        public HomeController(DBContext context, IConnectedUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                //إحصائية رسائل الإدارة
                var values = new MessageStatistics
                {
                    TotalCount = await _context.MessagesDelivery.CountAsync(),
                    DeliveredCount = await _context.MessagesDelivery.Where(d => d.DeliverTime != null).CountAsync(),
                };
                ViewBag.AdminMessageStatistics = values;
                ViewBag.AdminMessageStatisticsChart = GenerateAdminMessageChart(values.TotalCount, values.DeliveredCount);

                //إحصائية رسائل الحالات
                var values2 = await _context.MobileMessages.GroupBy(m => 1)
                                                            .Select(g => new StatusStatistics
                                                            {
                                                                TotalCount = g.Count(),
                                                                OkCount = g.Count(m => m.Status == MobileStatus.IAM_OK),
                                                                HelpCount = g.Count(m => m.Status == MobileStatus.NEED_HELP),
                                                                EmergencyCount = g.Count(m => m.Status == MobileStatus.EMERGENCY)
                                                            })
                                                            .SingleOrDefaultAsync() ?? new StatusStatistics();
                ViewBag.StatusStatistics = values2;
                ViewBag.StatusStatisticsChart = GenerateStatusStatisticsChart(values2.OkCount, values2.HelpCount, values2.EmergencyCount);

                //إحصائية رسائل الحالات حسب الشهر
                var values3 = await _context.MobileMessages.GroupBy(m => m.SendTime.Date)
                                                           .Select(g => new
                                                           {
                                                               Date = g.Key,
                                                               TotalCount = g.Count(),
                                                               OkCount = g.Count(m => m.Status == MobileStatus.IAM_OK),
                                                               HelpCount = g.Count(m => m.Status == MobileStatus.NEED_HELP),
                                                               EmergencyCount = g.Count(m => m.Status == MobileStatus.EMERGENCY)
                                                           })
                                                           .ToListAsync();

                ViewBag.StatusByMonthStatisticsChart = GenerateStatusByMonthStatisticsChart(values3.Select(v => (v.Date, v.OkCount, v.HelpCount, v.EmergencyCount)));

                //إحصائية رسائل الشات حسب المستخدم
                //var values4 = await _context.ChatMessages.Select(cm => new { Sender = cm.Sender, Count = _context.ChatMessages.Count(c => c.Sender == cm.Sender) })
                //                                         .Distinct()
                //                                         .ToListAsync();

                //ViewBag.ChatBySenderStatistics = values4;

                //إحصائية رسائل الشات حسب الشهر
                //ViewBag.ChatByMonthStatistics = await _context.ChatMessages.GroupBy(m => new { Month = m.SendTime.Month, Year = m.SendTime.Year })
                //                                                           .Select(m => new { Month = m.Key.Month, Year = m.Key.Year, Count = m.Count() })
                //                                                           .ToDictionaryAsync(m => (m.Year, m.Month), m => m.Count);
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel(ex));
            }
        }

        #region Chart Generators
        private static Chart GenerateAdminMessageChart(int total, int delivered)
        {
            var options = new Options
            {
                Scales = new Dictionary<string, Scale>()
            };
            options.Scales.Add("y", new Scale
            {
                Ticks = new Tick { StepSize = 1 }
            });
            var chart = new Chart
            {
                Type = Enums.ChartType.Bar,
                Options = options,
                Data = new ChartJSCore.Models.Data
                {
                    Labels = new List<string> { "الكل", "تم التوصيل", "انتظار" },
                    Datasets = new List<Dataset>
                    {
                        new BarDataset
                        {
                            Label="",
                            Data = new List<double?>{total, delivered, total-delivered},
                            Type = Enums.ChartType.Bar,
                            BackgroundColor =new List<ChartColor>{
                                ChartColor.FromRgba(54, 162, 235, 0.2),
                                ChartColor.FromRgba(255, 205, 86, 0.2),
                                ChartColor.FromRgba(255, 99, 132, 0.2),
                            },
                            BorderColor = new List<ChartColor>
                            {
                                ChartColor.FromRgb(54, 162, 235),
                                ChartColor.FromRgb(255, 205, 86),
                                ChartColor.FromRgb(255, 99, 132)
                            },
                            BorderWidth = new List<int>{1,1,1},
                        }
                    }
                }
            };
            return chart;
        }
        private static Chart GenerateStatusStatisticsChart(int ok, int help, int emergency)
        {
            var chart = new Chart
            {
                Type = Enums.ChartType.Pie,
                Data = new ChartJSCore.Models.Data
                {
                    Labels = new List<string> { "أنا بخير", "أحتاج مساعدة", "حالة طوارئ" },
                    Datasets = new List<Dataset>
                    {
                        new PieDataset
                        {
                            Data = new List<double?>{ok,help,emergency},
                            Type = Enums.ChartType.Pie,
                            BackgroundColor = new List<ChartColor>
                            {
                                ChartColor.FromHexString("#198754"),
                                ChartColor.FromHexString("#ffc107"),
                                ChartColor.FromHexString("#dc3545")
                            },
                        }
                    }
                }
            };
            return chart;
        }
        private static Chart GenerateStatusByMonthStatisticsChart(IEnumerable<(DateTime date, int ok, int help, int emergency)> data)
        {
            var opt = new Options
            {
                Scales = new Dictionary<string, Scale>()
            };
            opt.Scales.Add("y", new Scale
            {
                Ticks = new Tick { StepSize = 1 }
            });
            var chart = new Chart
            {
                Options = opt,
                Type = Enums.ChartType.Line,
                Data = new ChartJSCore.Models.Data
                {
                    Labels = data.Select(d => d.date.ToShortDateString()).ToList(),
                    Datasets = new List<Dataset>
                    {
                        new LineDataset
                        {
                            Data = data.Select(d=>(double?) d.ok).ToList(),
                            Type = Enums.ChartType.Line,
                            Label = "أنا بخير",
                            BorderColor = new List<ChartColor>
                            {
                                ChartColor.FromHexString("#198754"),
                            },
                            //BackgroundColor = new List<ChartColor>
                            //{
                            //    ChartColor.FromHexString("#198754"),
                            //},
                            Tension = 0.2,
                        },
                        new LineDataset
                        {
                            Data = data.Select(d=>(double?)d.help).ToList(),
                            Type = Enums.ChartType.Line,
                            Label ="أحتاج مساعدة",
                            BorderColor = new List<ChartColor>
                            {
                                ChartColor.FromHexString("#ffc107"),
                            },
                            //BackgroundColor = new List<ChartColor>
                            //{
                            //    ChartColor.FromHexString("#ffc107"),
                            //},
                            Tension = 0.2,
                        },
                        new LineDataset
                        {
                            Data = data.Select(d=>(double?)d.emergency).ToList(),
                            Type = Enums.ChartType.Line,
                            Label = "حالة طوارئ",
                            BorderColor = new List<ChartColor>
                            {
                                ChartColor.FromHexString("#dc3545"),
                            },
                            //BackgroundColor = new List<ChartColor>
                            //{
                            //    ChartColor.FromHexString("#dc3545"),
                            //},
                            Tension = 0.2,
                        }
                    }
                }
            };
            return chart;
        }
        #endregion

    }
}