using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Menyusun bahan cetak etiket obat untuk satu resep.
/// </summary>
/// <remarks>
/// <para>
/// Layanan ini hanya membaca dan tidak menyimpan apa pun. Mencetak etiket tidak mengubah
/// keadaan resep maupun stok; ia hanya menyalin apa yang sudah tercatat ke atas kertas.
/// </para>
/// <para>
/// Isi etiket diambil dari snapshot yang tersimpan pada resep, bukan dibaca ulang dari master
/// obat. Nama dan kekuatan obat di master dapat berubah kemudian, sedangkan etiket harus
/// mencerminkan obat sebagaimana diresepkan saat itu.
/// </para>
/// </remarks>
public sealed class PrescriptionLabelService
{
    private readonly ApplicationDbContext _dbContext;

    public PrescriptionLabelService(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<PrescriptionLabelResponse?> GetAsync(Guid prescriptionId,
        CancellationToken cancellationToken = default)
    {
        var header = await _dbContext.PhmPrescriptions.AsNoTracking()
            .Where(x => x.Id == prescriptionId && !x.IsDelete)
            .Select(x => new
            {
                x.Id,
                x.PrescriptionNumber,
                x.PrescriptionDateTime,
                x.PatientId,
                x.DoctorId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (header == null) return null;

        var patient = await _dbContext.MstPatients.AsNoTracking()
            .Where(x => x.Id == header.PatientId)
            .Select(x => new { x.FullName, x.MedicalRecordNumber, x.BirthDate })
            .FirstOrDefaultAsync(cancellationToken);

        var doctorName = await _dbContext.MstDoctors.AsNoTracking()
            .Where(x => x.Id == header.DoctorId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        // Identitas penerbit diambil dari lokasi rumah sakit yang tercatat pertama. Bila
        // kelak satu sistem melayani beberapa lokasi, penentuannya harus mengikuti tempat
        // resep dilayani — itu keputusan yang belum ditetapkan.
        var site = await _dbContext.MstHospitalSites.AsNoTracking()
            .Where(x => !x.IsDelete)
            .OrderBy(x => x.SiteName)
            .Select(x => new { x.SiteName, x.Address, x.PhoneNumber })
            .FirstOrDefaultAsync(cancellationToken);

        var items = await _dbContext.TrxPrescriptionItems.AsNoTracking()
            .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete)
            .OrderBy(x => x.SortOrder)
            .Select(x => new PrescriptionLabelItemResponse
            {
                Id = x.Id,
                LineNumber = x.SortOrder,
                DrugName = x.DrugNameSnapshot,
                GenericName = x.GenericNameSnapshot,
                Strength = x.StrengthSnapshot,
                DrugForm = x.DrugFormSnapshot,
                Route = x.RouteSnapshot,
                Quantity = x.Quantity,
                DispenseUnit = x.DispenseUnitSymbolSnapshot ?? x.DispenseUnitNameSnapshot,
                Signa = x.Signa,
                FrequencyText = x.FrequencyText,
                AdministrationInstruction = x.AdministrationInstruction,
                IsNarcotic = x.IsNarcoticSnapshot,
                IsPsychotropic = x.IsPsychotropicSnapshot,
                IsHighAlert = x.IsHighAlertSnapshot,
                IsAntibiotic = x.IsAntibioticSnapshot
            })
            .ToListAsync(cancellationToken);

        return new PrescriptionLabelResponse
        {
            PrescriptionId = header.Id,
            PrescriptionNumber = header.PrescriptionNumber,
            PrescriptionDateTime = header.PrescriptionDateTime,
            PatientName = patient?.FullName ?? string.Empty,
            MedicalRecordNumber = patient?.MedicalRecordNumber ?? string.Empty,
            BirthDate = patient?.BirthDate == null
                ? null
                : DateOnly.FromDateTime(patient.BirthDate.Value),
            DoctorName = doctorName ?? string.Empty,
            SiteName = site?.SiteName ?? string.Empty,
            SiteAddress = site?.Address,
            SitePhoneNumber = site?.PhoneNumber,
            Items = items
        };
    }
}
