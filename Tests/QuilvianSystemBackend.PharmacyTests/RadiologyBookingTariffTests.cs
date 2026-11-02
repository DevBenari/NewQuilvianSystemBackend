using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Pilihan pemeriksaan Booking Radiologi yang digerakkan tarif, penilaian tarif per kelas pasien, dan identitas butir
/// tagihan Radiologi → Billing.
///
/// Kasus lapangan: master tarif Radiology memuat 360 pemeriksaan × 19 varian tarif (6.840 baris, satu kode per varian,
/// mis. RADIOLOGI_5509-0 s.d. RADIOLOGI_5509-151). Daftar "Nama Pemeriksaan" harus satu baris per pemeriksaan, harga
/// mengikuti kelas pasien, dan dua tarif sama spesifik dengan harga berbeda tidak pernah dipilih diam-diam.
/// </summary>
public class RadiologyBookingTariffTests : IDisposable
{
    private readonly TestDatabase _database = TestDatabase.Create();
    private readonly ApplicationDbContext _dbContext;
    private readonly RadOrderService _service;

    private readonly Guid _classI = Guid.NewGuid();
    private readonly Guid _classVip = Guid.NewGuid();
    private readonly Guid _encounterId = Guid.NewGuid();
    private readonly Guid _radiologyCategoryId = Guid.NewGuid();
    private readonly Guid _laboratoryCategoryId = Guid.NewGuid();

    public RadiologyBookingTariffTests()
    {
        _dbContext = _database.CreateContext();

        // Jalur baca hanya memakai ApplicationDbContext; dependensi lain tidak disentuh.
        _service = new RadOrderService(_dbContext, null!, null!, null!, null!, null!);

        _dbContext.Set<MstPatientClass>().AddRange(
            new MstPatientClass { Id = _classI, PatientClassCode = "KLS1", PatientClassName = "Kelas I", SortOrder = 2 },
            new MstPatientClass { Id = _classVip, PatientClassCode = "VIP", PatientClassName = "VIP", SortOrder = 1 },
            new MstPatientClass { Id = Guid.NewGuid(), PatientClassCode = "NOMAP", PatientClassName = "Tanpa Pemetaan Tarif", IsForTariffMapping = false });

        _dbContext.Set<MstTariffCategory>().AddRange(
            new MstTariffCategory { Id = _radiologyCategoryId, TariffCategoryCode = "RAD", TariffCategoryName = "Radiology" },
            new MstTariffCategory { Id = _laboratoryCategoryId, TariffCategoryCode = "LAB", TariffCategoryName = "Laboratory", IsLaboratory = true });

        _dbContext.RegPatientEncounters.Add(new RegPatientEncounter
        {
            Id = _encounterId,
            EncounterNumber = "ENC-RAD-1",
            PatientId = Guid.NewGuid(),
            EncounterStatus = EncounterStatus.Billing,
            EncounterDate = DateTime.UtcNow,
            PatientClassId = _classI,
            IsActive = true,
        });

        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }

    private MstProcedure SeedProcedure(string code, string name, bool radiology = true, bool active = true)
    {
        var procedure = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = code,
            ProcedureName = name,
            IsRadiology = radiology,
            IsActive = active,
        };
        _dbContext.MstProcedures.Add(procedure);
        _dbContext.SaveChanges();
        return procedure;
    }

    private MstTariff SeedTariff(
        MstProcedure? procedure,
        string code,
        decimal price,
        Guid? classId,
        string? name = null,
        DateTime? start = null,
        DateTime? end = null,
        bool active = true,
        Guid? categoryId = null)
    {
        var tariff = new MstTariff
        {
            Id = Guid.NewGuid(),
            TariffCode = code,
            TariffName = name ?? procedure?.ProcedureName ?? code,
            TariffCategoryId = categoryId ?? _radiologyCategoryId,
            ProcedureId = procedure?.Id,
            PatientClassId = classId,
            NormalPrice = price,
            EffectiveStartDate = start ?? DateTime.UtcNow.AddYears(-1),
            EffectiveEndDate = end ?? new DateTime(2099, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActive = active,
        };
        _dbContext.Set<MstTariff>().Add(tariff);
        _dbContext.SaveChanges();
        return tariff;
    }

    private static MstTariff Tariff(string code, decimal price, Guid? classId = null, Guid? clinicId = null) => new()
    {
        Id = Guid.NewGuid(),
        TariffCode = code,
        TariffName = "ABDOMEN 2 POSISI",
        PatientClassId = classId,
        ClinicId = clinicId,
        NormalPrice = price,
    };

    private async Task<RadBookingProcedureOptionResponse[]> OptionsAsync(Guid? classId = null, string? search = null)
    {
        var page = await _service.GetBookingProcedureOptionsAsync(new RadBookingProcedureOptionQuery
        {
            EncounterId = _encounterId,
            PatientClassId = classId,
            Search = search,
        });

        return page.Items.ToArray();
    }

    // ------------------------------------------------------------------------------------- selektor (murni)

    [Fact]
    public void Selector_class_specific_tariff_beats_general_and_other_classes_are_ignored()
    {
        var candidates = new[]
        {
            Tariff("RADIOLOGI_5509-0", 666000m, _classI),
            Tariff("RADIOLOGI_5509-1", 933000m, _classVip),
            Tariff("RADIOLOGI_5509-UMUM", 500000m, null),
        };

        var selection = MstTariffSelector.Select(candidates, _classI, null);

        Assert.Equal(MstTariffSelectionStatus.Resolved, selection.Status);
        Assert.Equal(666000m, selection.Chosen!.NormalPrice);
        Assert.Single(selection.Tied);
    }

    [Fact]
    public void Selector_falls_back_to_general_tariff_only_when_it_exists_and_never_to_another_class()
    {
        var onlyOtherClass = new[] { Tariff("A-1", 933000m, _classVip) };
        Assert.Equal(MstTariffSelectionStatus.NotFound, MstTariffSelector.Select(onlyOtherClass, _classI, null).Status);

        var withGeneral = new[] { Tariff("A-1", 933000m, _classVip), Tariff("A-UMUM", 500000m, null) };
        var selection = MstTariffSelector.Select(withGeneral, _classI, null);
        Assert.Equal(MstTariffSelectionStatus.Resolved, selection.Status);
        Assert.Equal(500000m, selection.Chosen!.NormalPrice);
    }

    [Fact]
    public void Selector_without_a_patient_class_uses_general_tariffs_only()
    {
        var candidates = new[] { Tariff("A-1", 666000m, _classI), Tariff("A-UMUM", 500000m, null) };

        var selection = MstTariffSelector.Select(candidates, null, null);

        Assert.Equal(MstTariffSelectionStatus.Resolved, selection.Status);
        Assert.Equal(500000m, selection.Chosen!.NormalPrice);
        Assert.Equal(MstTariffSelectionStatus.NotFound, MstTariffSelector.Select(new[] { Tariff("A-1", 1m, _classI) }, null, null).Status);
    }

    [Fact]
    public void Selector_two_equally_specific_tariffs_with_different_prices_are_ambiguous_and_nothing_is_chosen()
    {
        var candidates = new[]
        {
            Tariff("A-1", 666000m, _classI),
            Tariff("A-2", 733000m, _classI),
        };

        var selection = MstTariffSelector.Select(candidates, _classI, null);

        Assert.Equal(MstTariffSelectionStatus.Ambiguous, selection.Status);
        Assert.Null(selection.Chosen);
        Assert.Equal(new[] { "A-1", "A-2" }, selection.Tied.Select(t => t.TariffCode).ToArray());
    }

    [Fact]
    public void Selector_duplicate_tariffs_with_the_same_price_are_safe_and_pick_the_lowest_code()
    {
        var candidates = new[] { Tariff("A-2", 666000m, _classI), Tariff("A-1", 666000m, _classI) };

        var selection = MstTariffSelector.Select(candidates, _classI, null);

        Assert.Equal(MstTariffSelectionStatus.Resolved, selection.Status);
        Assert.Equal("A-1", selection.Chosen!.TariffCode);
    }

    [Fact]
    public void Selector_clinic_specific_tariff_outranks_class_specific_and_is_ignored_for_other_clinics()
    {
        var clinic = Guid.NewGuid();
        var candidates = new[]
        {
            Tariff("A-CLASS", 666000m, _classI),
            Tariff("A-CLINIC", 700000m, null, clinic),
            Tariff("A-OTHER-CLINIC", 1m, null, Guid.NewGuid()),
        };

        Assert.Equal("A-CLINIC", MstTariffSelector.Select(candidates, _classI, clinic).Chosen!.TariffCode);
        Assert.Equal("A-CLASS", MstTariffSelector.Select(candidates, _classI, null).Chosen!.TariffCode);
    }

    [Fact]
    public void Selector_never_picks_cheapest_or_most_expensive_as_a_fallback()
    {
        var candidates = new[] { Tariff("A-1", 100m, _classI), Tariff("A-2", 900m, _classI) };

        var selection = MstTariffSelector.Select(candidates, _classI, null);

        Assert.Null(selection.Chosen);
    }

    // ------------------------------------------------------------------------------ identitas butir tagihan

    [Fact]
    public void Charge_identity_outpatient_studies_always_use_the_study()
    {
        var order = Guid.NewGuid();
        var study = Guid.NewGuid();

        Assert.Equal(study, RadStudyService.ChooseChargeSourceItemId(order, study, false, false));
        Assert.Equal(study, RadStudyService.ChooseChargeSourceItemId(order, study, false, true));
    }

    [Fact]
    public void Charge_identity_inpatient_first_accepted_study_reuses_the_order_charge_so_there_is_one_invoice_item()
    {
        var order = Guid.NewGuid();
        var study = Guid.NewGuid();

        Assert.Equal(order, RadStudyService.ChooseChargeSourceItemId(order, study, true, false));
    }

    [Fact]
    public void Charge_identity_inpatient_later_study_is_a_separate_charge_after_the_order_charge_is_carried()
    {
        var order = Guid.NewGuid();
        var repeat = Guid.NewGuid();

        Assert.Equal(repeat, RadStudyService.ChooseChargeSourceItemId(order, repeat, true, true));
    }

    // ------------------------------------------------------------------------------------- pesan alasan

    [Fact]
    public void Problem_messages_name_the_class_and_never_exist_for_a_resolved_tariff()
    {
        Assert.Null(RadOrderService.DescribeTariffProblem(RadTariffStatuses.Resolved, "Kelas I"));
        Assert.Contains("Kelas I", RadOrderService.DescribeTariffProblem(RadTariffStatuses.NotFound, "Kelas I"));
        Assert.Contains("ProcedureId", RadOrderService.DescribeTariffProblem(RadTariffStatuses.ProcedureNotMapped, null));
        Assert.Contains("IsRadiology", RadOrderService.DescribeTariffProblem(RadTariffStatuses.ProcedureNotRadiology, null));
        Assert.Contains("belum tercatat", RadOrderService.DescribeTariffProblem(RadTariffStatuses.NotFound, null));
        Assert.Contains("No. RM", RadOrderService.DescribeTariffProblem(RadTariffStatuses.EncounterRequired, null));
        Assert.Contains("A-1, A-2", RadOrderService.DescribeTariffProblem(
            RadTariffStatuses.Ambiguous, "Kelas I", new[] { Tariff("A-1", 1m, null), Tariff("A-2", 2m, null) }));
    }

    // -------------------------------------------------------------------------- pilihan pemeriksaan (DB)

    [Fact]
    public async Task Options_list_one_row_per_procedure_with_the_price_of_the_encounter_class_despite_19_class_variants()
    {
        var abdomen = SeedProcedure("5509", "ABDOMEN 2 POSISI");
        SeedTariff(abdomen, "RADIOLOGI_5509-0", 666000m, _classI);
        SeedTariff(abdomen, "RADIOLOGI_5509-1", 933000m, _classVip);
        for (var i = 0; i < 17; i++)
        {
            var classId = Guid.NewGuid();
            _dbContext.Set<MstPatientClass>().Add(new MstPatientClass { Id = classId, PatientClassCode = $"X{i}", PatientClassName = $"Kelas X{i}" });
            _dbContext.SaveChanges();
            SeedTariff(abdomen, $"RADIOLOGI_5509-X{i}", 700000m + i, classId);
        }

        var options = await OptionsAsync();

        var option = Assert.Single(options);
        Assert.Equal(abdomen.Id, option.Id);
        Assert.Equal("ABDOMEN 2 POSISI", option.DisplayName);
        Assert.Equal(RadTariffStatuses.Resolved, option.TariffStatus);
        Assert.True(option.IsSelectable);
        Assert.Equal(666000m, option.TariffAmount);
        Assert.Equal(_classI, option.TariffPatientClassId);
        Assert.Equal("Kelas I", option.TariffPatientClassName);
        Assert.Equal("RADIOLOGI_5509-0", option.TariffCode);
    }

    [Fact]
    public async Task Options_price_follows_the_preview_class_without_changing_the_encounter_class()
    {
        var abdomen = SeedProcedure("5509", "ABDOMEN 2 POSISI");
        SeedTariff(abdomen, "RADIOLOGI_5509-0", 666000m, _classI);
        SeedTariff(abdomen, "RADIOLOGI_5509-1", 933000m, _classVip);

        var vip = Assert.Single(await OptionsAsync(_classVip));
        var kelasI = Assert.Single(await OptionsAsync());

        Assert.Equal(933000m, vip.TariffAmount);
        Assert.Equal(666000m, kelasI.TariffAmount);
        Assert.Equal(_classI, _dbContext.RegPatientEncounters.Single(e => e.Id == _encounterId).PatientClassId);
    }

    [Fact]
    public async Task Options_inactive_non_radiology_and_expired_or_inactive_tariffs_are_not_selectable()
    {
        var active = SeedProcedure("R1", "FOTO AKTIF");
        SeedTariff(active, "R1-0", 100m, _classI);

        var inactive = SeedProcedure("R2", "FOTO NONAKTIF", active: false);
        SeedTariff(inactive, "R2-0", 100m, _classI);

        var notRadiology = SeedProcedure("L1", "DARAH LENGKAP", radiology: false);
        SeedTariff(notRadiology, "L1-0", 100m, _classI, categoryId: _laboratoryCategoryId);

        var expired = SeedProcedure("R3", "FOTO KEDALUWARSA");
        SeedTariff(expired, "R3-0", 100m, _classI, end: DateTime.UtcNow.AddDays(-1));

        var inactiveTariff = SeedProcedure("R4", "FOTO TARIF NONAKTIF");
        SeedTariff(inactiveTariff, "R4-0", 100m, _classI, active: false);

        var options = await OptionsAsync();
        var byName = options.ToDictionary(o => o.ProcedureName);

        Assert.True(byName["FOTO AKTIF"].IsSelectable);
        Assert.False(byName.ContainsKey("FOTO NONAKTIF"));
        Assert.False(byName.ContainsKey("DARAH LENGKAP"));
        Assert.False(byName["FOTO KEDALUWARSA"].IsSelectable);
        Assert.Equal(RadTariffStatuses.NotFound, byName["FOTO KEDALUWARSA"].TariffStatus);
        Assert.Contains("Kelas I", byName["FOTO KEDALUWARSA"].UnavailableReason);
        Assert.False(byName["FOTO TARIF NONAKTIF"].IsSelectable);

        // Yang dapat dipilih tampil lebih dulu.
        Assert.Equal("FOTO AKTIF", options[0].ProcedureName);
    }

    [Fact]
    public async Task Options_procedure_without_any_tariff_is_listed_with_a_clear_reason_and_cannot_be_selected()
    {
        SeedProcedure("R9", "FOTO TANPA TARIF");

        var option = Assert.Single(await OptionsAsync());

        Assert.False(option.IsSelectable);
        Assert.Null(option.TariffAmount);
        Assert.Equal(RadTariffStatuses.NotFound, option.TariffStatus);
        Assert.Contains("Tarif belum tersedia", option.UnavailableReason);
    }

    [Fact]
    public async Task Options_ambiguous_tariffs_are_reported_as_a_conflict_without_a_price()
    {
        var procedure = SeedProcedure("R5", "FOTO KONFLIK");
        SeedTariff(procedure, "R5-A", 100m, _classI);
        SeedTariff(procedure, "R5-B", 200m, _classI);

        var option = Assert.Single(await OptionsAsync());

        Assert.Equal(RadTariffStatuses.Ambiguous, option.TariffStatus);
        Assert.False(option.IsSelectable);
        Assert.Null(option.TariffAmount);
        Assert.Contains("R5-A, R5-B", option.UnavailableReason);
    }

    [Fact]
    public async Task Options_search_matches_procedure_name_code_and_tariff_name_or_code()
    {
        var abdomen = SeedProcedure("5509", "ABDOMEN 2 POSISI");
        SeedTariff(abdomen, "RADIOLOGI_5509-0", 666000m, _classI, name: "ABDOMEN 2 POSISI (BNO)");
        SeedTariff(SeedProcedure("5510", "THORAX PA"), "RADIOLOGI_5510-0", 400000m, _classI);

        Assert.Single(await OptionsAsync(search: "abdomen"));
        Assert.Single(await OptionsAsync(search: "5509"));
        Assert.Single(await OptionsAsync(search: "bno"));
        Assert.Single(await OptionsAsync(search: "RADIOLOGI_5510"));
        Assert.Empty(await OptionsAsync(search: "tidak-ada"));
    }

    [Fact]
    public async Task Options_display_name_follows_the_tariff_name_when_the_tariff_is_valid()
    {
        var procedure = SeedProcedure("5509", "Abdomen 2 Posisi (master tindakan)");
        SeedTariff(procedure, "RADIOLOGI_5509-0", 666000m, _classI, name: "  ABDOMEN 2 POSISI  ");

        var option = Assert.Single(await OptionsAsync());

        Assert.Equal("ABDOMEN 2 POSISI", option.DisplayName);
        Assert.Equal("Abdomen 2 Posisi (master tindakan)", option.ProcedureName);
    }

    [Fact]
    public async Task Options_without_an_encounter_require_a_patient_first()
    {
        var procedure = SeedProcedure("5509", "ABDOMEN 2 POSISI");
        SeedTariff(procedure, "RADIOLOGI_5509-0", 666000m, _classI);

        var page = await _service.GetBookingProcedureOptionsAsync(new RadBookingProcedureOptionQuery());

        var option = Assert.Single(page.Items);
        Assert.Equal(RadTariffStatuses.EncounterRequired, option.TariffStatus);
        Assert.False(option.IsSelectable);
        Assert.Null(option.TariffAmount);
    }

    [Fact]
    public async Task Options_unmapped_radiology_tariff_names_are_listed_once_per_name_with_the_reason()
    {
        // 19 varian tarif untuk satu nama, seluruhnya tanpa ProcedureId: satu baris nonaktif, bukan 19.
        for (var i = 0; i < 19; i++)
        {
            SeedTariff(null, $"RADIOLOGI_5509-{i}", 666000m + i, null, name: "ABDOMEN 2 POSISI");
        }

        var option = Assert.Single(await OptionsAsync());

        Assert.Equal("ABDOMEN 2 POSISI", option.DisplayName);
        Assert.Equal(RadTariffStatuses.ProcedureNotMapped, option.TariffStatus);
        Assert.False(option.IsSelectable);
        Assert.Null(option.TariffAmount);
        Assert.Contains("belum dipetakan", option.UnavailableReason);
        Assert.Contains("19 varian tarif", option.UnavailableReason);
        Assert.NotEqual(Guid.Empty, option.Id);
    }

    [Fact]
    public async Task Options_search_matches_unmapped_tariff_names_and_codes()
    {
        SeedTariff(null, "RADIOLOGI_5509-0", 666000m, null, name: "ABDOMEN 2 POSISI");
        SeedTariff(null, "RADIOLOGI_5510-0", 400000m, null, name: "THORAX PA");

        var byName = Assert.Single(await OptionsAsync(search: "abdomen"));
        var byCode = Assert.Single(await OptionsAsync(search: "5510"));

        Assert.Equal("ABDOMEN 2 POSISI", byName.DisplayName);
        Assert.Equal("THORAX PA", byCode.DisplayName);
        Assert.Empty(await OptionsAsync(search: "tidak-ada"));
    }

    [Fact]
    public async Task Options_procedure_with_a_radiology_tariff_but_not_flagged_radiology_is_listed_but_not_selectable()
    {
        var procedure = SeedProcedure("5509", "ABDOMEN 2 POSISI", radiology: false);
        SeedTariff(procedure, "RADIOLOGI_5509-0", 666000m, _classI);

        var option = Assert.Single(await OptionsAsync());

        Assert.Equal(RadTariffStatuses.ProcedureNotRadiology, option.TariffStatus);
        Assert.False(option.IsSelectable);
        Assert.Contains("IsRadiology", option.UnavailableReason);
    }

    [Fact]
    public async Task Options_mapped_flagged_procedures_come_first_then_unmapped_names_and_paging_covers_both()
    {
        var ready = SeedProcedure("R1", "FOTO SIAP");
        SeedTariff(ready, "R1-0", 100m, _classI);
        SeedTariff(null, "U-0", 1m, null, name: "NAMA TARIF TANPA PEMETAAN");

        var page1 = await _service.GetBookingProcedureOptionsAsync(new RadBookingProcedureOptionQuery
        {
            EncounterId = _encounterId,
            PageNumber = 1,
            PageSize = 1,
        });
        var page2 = await _service.GetBookingProcedureOptionsAsync(new RadBookingProcedureOptionQuery
        {
            EncounterId = _encounterId,
            PageNumber = 2,
            PageSize = 1,
        });

        Assert.Equal(2, page1.TotalData);
        Assert.Equal("FOTO SIAP", Assert.Single(page1.Items).DisplayName);
        Assert.Equal("NAMA TARIF TANPA PEMETAAN", Assert.Single(page2.Items).DisplayName);
    }

    [Fact]
    public async Task Options_non_radiology_tariffs_never_appear_as_unmapped_names()
    {
        SeedTariff(null, "LAB-0", 50m, null, name: "DARAH LENGKAP", categoryId: _laboratoryCategoryId);

        Assert.Empty(await OptionsAsync());
    }

    // ------------------------------------------------------------------------------------------ kelas pasien

    [Fact]
    public async Task Class_options_come_from_the_patient_class_master_and_mark_the_encounter_class_as_default()
    {
        var classes = await _service.GetBookingClassOptionsAsync(_encounterId);

        Assert.Equal(new[] { "VIP", "KLS1" }, classes.Select(c => c.PatientClassCode).ToArray());
        Assert.Equal("KLS1", Assert.Single(classes, c => c.IsEncounterClass).PatientClassCode);
        Assert.DoesNotContain(classes, c => c.PatientClassCode == "NOMAP");
    }

    // ------------------------------------------------------------------------------------------------ harga

    [Fact]
    public async Task Price_returns_the_resolved_tariff_and_flags_a_preview_class_that_differs_from_the_encounter()
    {
        var procedure = SeedProcedure("5509", "ABDOMEN 2 POSISI");
        SeedTariff(procedure, "RADIOLOGI_5509-0", 666000m, _classI);
        SeedTariff(procedure, "RADIOLOGI_5509-1", 933000m, _classVip);

        var encounterPrice = (await _service.GetBookingPriceAsync(_encounterId, procedure.Id)).Value!;
        var previewPrice = (await _service.GetBookingPriceAsync(_encounterId, procedure.Id, default, _classVip)).Value!;

        Assert.Equal(RadTariffStatuses.Resolved, encounterPrice.TariffStatus);
        Assert.Equal(666000m, encounterPrice.TariffAmount);
        Assert.False(encounterPrice.IsPreviewClass);

        Assert.Equal(933000m, previewPrice.TariffAmount);
        Assert.True(previewPrice.IsPreviewClass);
        Assert.Equal(_classI, previewPrice.EncounterPatientClassId);
        Assert.Equal(_classVip, previewPrice.PriceBasisPatientClassId);
    }

    [Fact]
    public async Task Price_is_missing_not_zero_when_there_is_no_valid_tariff_or_the_tariff_is_ambiguous()
    {
        var none = SeedProcedure("R9", "FOTO TANPA TARIF");
        var conflict = SeedProcedure("R5", "FOTO KONFLIK");
        SeedTariff(conflict, "R5-A", 100m, _classI);
        SeedTariff(conflict, "R5-B", 200m, _classI);

        var noneResult = (await _service.GetBookingPriceAsync(_encounterId, none.Id)).Value!;
        var conflictResult = (await _service.GetBookingPriceAsync(_encounterId, conflict.Id)).Value!;

        Assert.Equal(RadTariffStatuses.NotFound, noneResult.TariffStatus);
        Assert.Null(noneResult.TariffAmount);
        Assert.Equal(RadTariffStatuses.Ambiguous, conflictResult.TariffStatus);
        Assert.Null(conflictResult.TariffAmount);
        Assert.NotNull(conflictResult.TariffMessage);
    }
}
