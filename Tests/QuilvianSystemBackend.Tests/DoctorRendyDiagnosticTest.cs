using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace QuilvianSystemBackend.Tests;

public class DoctorRendyDiagnosticTest
{
    private readonly ITestOutputHelper _output;

    public DoctorRendyDiagnosticTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DiagnoseDoctorRendy()
    {
        var connectionString = "Host=160.22.250.77;Port=5432;Username=Quilvian_2026@;Password=Quilvian_2026!@#;Database=QuilvianNewDevHamzah;";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        using var db = new ApplicationDbContext(options);

        // Find users with "Rendy" in display name or username
        var users = await db.Users
            .Where(u => EF.Functions.ILike(u.DisplayName, "%Rendy%") || EF.Functions.ILike(u.UserName, "%Rendy%"))
            .ToListAsync();

        _output.WriteLine($"Found {users.Count} users matching 'Rendy':");
        foreach (var u in users)
        {
            _output.WriteLine($"User: Id={u.Id}, UserName={u.UserName}, DisplayName={u.DisplayName}, Email={u.Email}, UserType={u.UserType}, DoctorId={u.DoctorId}, EmployeeId={u.EmployeeId}, PrimaryDept={u.PrimaryDepartmentId}, PrimaryPos={u.PrimaryPositionId}, IsActive={u.IsActive}");

            // User Organizations
            var orgs = await db.ApplicationUserOrganizations
                .Where(o => o.UserId == u.Id)
                .ToListAsync();
            _output.WriteLine($"  Organizations ({orgs.Count}):");
            foreach (var o in orgs)
            {
                _output.WriteLine($"    Org: Id={o.Id}, DeptId={o.DepartmentId}, PosId={o.PositionId}, IsActive={o.IsActive}, IsDelete={o.IsDelete}");
                
                // Check policies for this Dept/Pos
                var policies = await db.SysAccessPolicies
                    .Include(p => p.ControllerAccess)
                    .Include(p => p.ActionAccess)
                    .Where(p => p.DepartmentId == o.DepartmentId && p.PositionId == o.PositionId && !p.IsDelete)
                    .ToListAsync();
                
                _output.WriteLine($"    Policies for Dept={o.DepartmentId}, Pos={o.PositionId}: Total={policies.Count}");
                var inpatientPolicies = policies.Where(p => p.ControllerAccess?.ControllerName?.Contains("Inpatient") == true).ToList();
                _output.WriteLine($"    Inpatient policies ({inpatientPolicies.Count}):");
                foreach (var p in inpatientPolicies)
                {
                    _output.WriteLine($"      Controller={p.ControllerAccess?.ControllerName}, Action={p.ActionAccess?.ActionName}, Allowed={p.IsAllowed}, Active={p.IsActive}");
                }
            }

            // Also check doctor record if doctorId is set
            if (u.DoctorId.HasValue)
            {
                var doc = await db.MstDoctors.FirstOrDefaultAsync(d => d.Id == u.DoctorId.Value);
                _output.WriteLine($"  MstDoctor: Found={doc != null}, Name={doc?.FullName}");
            }
        }

        // Also let's check SysControllerAccess for InpatientCensus
        var censusController = await db.SysControllerAccesses
            .Include(c => c.Actions)
            .FirstOrDefaultAsync(c => c.ControllerName == "InpatientCensus");
        _output.WriteLine($"InpatientCensus Controller: Found={censusController != null}, Id={censusController?.Id}");
        if (censusController != null)
        {
            foreach (var a in censusController.Actions)
            {
                _output.WriteLine($"  Action: Id={a.Id}, Name={a.ActionName}, Display={a.DisplayName}, Type={a.AccessType}, IsActive={a.IsActive}, VisibleInRoleAccess={a.VisibleInRoleAccess}, IsSystemOnly={a.IsSystemOnly}");
            }
        }
    }
}
