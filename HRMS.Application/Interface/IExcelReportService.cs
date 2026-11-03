using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.Interface;

public interface IExcelReportService
{
    Task<MemoryStream> ExportEmployeesToExcelAsync();
    Task<MemoryStream> ExportAttendanceToExcelAsync();
}