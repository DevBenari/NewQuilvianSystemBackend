using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Seeders
{
    public class DefaultWorkScheduleSeeder
    {
        private const string DefaultScheduleCode = "SCH-RSMMC-DEFAULT";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;

            if (!seedEnabled)
            {
                Console.WriteLine("[Seeder] SeedDefaultData disabled.");
                return;
            }

            await EnsureDefaultWorkScheduleAsync(dbContext);
        }

        private static async Task EnsureDefaultWorkScheduleAsync(ApplicationDbContext dbContext)
        {
            var existingSchedule = await dbContext.MstWorkSchedules
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.ScheduleCode == DefaultScheduleCode);

            if (existingSchedule == null)
            {
                var schedule = new MstWorkSchedule
                {
                    Id = Guid.NewGuid(),

                    ScheduleCode = DefaultScheduleCode,
                    ScheduleName = "Jadwal Default RSMMC",

                    UserId = null,
                    UserType = null,
                    DepartmentId = null,
                    PositionId = null,

                    WorkStartTime = new TimeOnly(8, 0, 0),
                    WorkEndTime = new TimeOnly(17, 0, 0),

                    CheckInToleranceMinutes = 15,
                    CheckOutToleranceMinutes = 0,

                    EffectiveStartDate = null,
                    EffectiveEndDate = null,

                    IsDefault = true,
                    IsActive = true,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = Guid.Empty,
                    UpdateBy = Guid.Empty,
                    DeleteBy = Guid.Empty,
                    CancelBy = Guid.Empty,
                    IsDelete = false,
                    IsCancel = false
                };

                dbContext.MstWorkSchedules.Add(schedule);

                await dbContext.SaveChangesAsync();

                Console.WriteLine("[Seeder] Default work schedule created.");

                return;
            }

            var needUpdate = false;

            if (existingSchedule.IsDelete)
            {
                existingSchedule.IsDelete = false;
                existingSchedule.DeleteDateTime = null;
                existingSchedule.DeleteBy = Guid.Empty;
                needUpdate = true;
            }

            if (existingSchedule.IsCancel)
            {
                existingSchedule.IsCancel = false;
                existingSchedule.CancelDateTime = null;
                existingSchedule.CancelBy = Guid.Empty;
                needUpdate = true;
            }

            if (!existingSchedule.IsActive)
            {
                existingSchedule.IsActive = true;
                needUpdate = true;
            }

            if (!existingSchedule.IsDefault)
            {
                existingSchedule.IsDefault = true;
                needUpdate = true;
            }

            if (string.IsNullOrWhiteSpace(existingSchedule.ScheduleName))
            {
                existingSchedule.ScheduleName = "Jadwal Default RSMMC";
                needUpdate = true;
            }

            if (existingSchedule.WorkStartTime == default)
            {
                existingSchedule.WorkStartTime = new TimeOnly(8, 0, 0);
                needUpdate = true;
            }

            if (existingSchedule.WorkEndTime == default)
            {
                existingSchedule.WorkEndTime = new TimeOnly(17, 0, 0);
                needUpdate = true;
            }

            if (existingSchedule.CheckInToleranceMinutes < 0)
            {
                existingSchedule.CheckInToleranceMinutes = 15;
                needUpdate = true;
            }

            if (existingSchedule.CheckOutToleranceMinutes < 0)
            {
                existingSchedule.CheckOutToleranceMinutes = 0;
                needUpdate = true;
            }

            if (needUpdate)
            {
                existingSchedule.UpdateDateTime = DateTime.UtcNow;
                existingSchedule.UpdateBy = Guid.Empty;

                await dbContext.SaveChangesAsync();

                Console.WriteLine("[Seeder] Default work schedule updated.");
            }
            else
            {
                Console.WriteLine("[Seeder] Default work schedule already exists.");
            }
        }
    }
}
