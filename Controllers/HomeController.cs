using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BTreeDashboard.Web.Models;
using System.Text.Json;

namespace WikiSearchApp.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
[HttpPost]
public async Task<IActionResult> Search(string keyword, int technique, long n_rows)
{
    // 1. Cấu hình đường dẫn
    string pathToSend = "F:\\Dowloads\\random_32B_100M.txt";
    long rowsToLoad = (n_rows <= 0) ? 1000 : n_rows;
    string allLogs = "";
    
    // 2. Thiết lập tiến trình gọi C++ (Chỉ gọi 1 lần duy nhất)
    ProcessStartInfo start = new ProcessStartInfo
    {
        FileName = "./test1.exe",
        Arguments = $"{technique} \"{keyword}\" {rowsToLoad} \"{pathToSend}\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
        StandardOutputEncoding = System.Text.Encoding.UTF8
    };

    using (Process process = Process.Start(start))
    {
        // Đọc toàn bộ kết quả từ C++
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        allLogs = output + "\n" + error;

        // 3. Bóc tách và xử lý JSON
        int jsonStart = output.IndexOf('[');
        int jsonEnd = output.LastIndexOf(']');

        if (jsonStart != -1 && jsonEnd != -1)
        {
            try 
            {
                string jsonString = output.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var results = JsonSerializer.Deserialize<List<BenchmarkResult>>(jsonString);

                if (results != null && results.Count > 0)
                {
                    // Đẩy dữ liệu sang View
                    ViewBag.DetailedResults = results;
                    ViewBag.JsonData = JsonSerializer.Serialize(results);
                    ViewBag.IsMultiple = (technique == 9); // Hiện biểu đồ nếu chọn All Trees
                    
                    // Gán các thông số nhanh cho Cards Dashboard
                    ViewBag.Rows = results[0].rows.ToString("N0");
                    ViewBag.IsSuccess = results.Any(r => r.found);
                    
                    // Tính nhanh trung bình cho tiêu đề (nếu cần)
                    ViewBag.AvgInsert = results.Average(r => r.insert_s).ToString("F6");
                    ViewBag.AvgSearch = results.Average(r => r.search_s).ToString("F9");
                }
            }
            catch (Exception ex)
            {
                allLogs += $"\n[System Error]: Không thể parse JSON. Chi tiết: {ex.Message}";
            }
        }
    }

    ViewBag.Raw = allLogs;
    ViewBag.Technique = technique;
    ViewBag.NRows = n_rows;
    ViewBag.Keyword = keyword;
    return View("Index");
}
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
