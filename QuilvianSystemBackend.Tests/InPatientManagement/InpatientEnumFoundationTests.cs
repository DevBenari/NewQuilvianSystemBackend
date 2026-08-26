using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Menjaga bentuk enum modul Rawat Inap terhadap erd/data-dictionary.md bagian 15.
/// Angka enum tersimpan sebagai int di database, sehingga mengubah nilainya diam-diam
/// akan menggeser arti baris yang sudah tersimpan.
/// </summary>
public sealed class InpatientEnumFoundationTests
{
    [Fact]
    public void InpEpisodeStatus_MemuatTepatLimaNilai()
    {
        var nilai = Enum.GetNames<InpEpisodeStatus>();

        Assert.Equal(5, nilai.Length);
    }

    [Fact]
    public void InpEpisodeStatus_TidakMemuatInCare()
    {
        // RWI-DEC-054: status InCare sengaja tidak ada. Kehadiran pasien dibaca dari
        // PhysicallyLeftAt, bukan dari status tersendiri.
        Assert.DoesNotContain("InCare", Enum.GetNames<InpEpisodeStatus>());
    }

    [Fact]
    public void InpEpisodeStatus_AngkanyaSesuaiKamusData()
    {
        Assert.Equal(0, (int)InpEpisodeStatus.Draft);
        Assert.Equal(1, (int)InpEpisodeStatus.Admitted);
        Assert.Equal(2, (int)InpEpisodeStatus.DischargePending);
        Assert.Equal(3, (int)InpEpisodeStatus.Closed);
        Assert.Equal(4, (int)InpEpisodeStatus.Cancelled);
    }

    [Fact]
    public void InpDischargeType_MenyisakanAngkaEmpatDanLima()
    {
        // Cara pulang meninggal dan kabur di luar scope revisi ini dan menunggu DEC-INP-007.
        // Angkanya sengaja dikosongkan supaya penambahan kelak tidak menggeser data lama.
        var terpakai = Enum.GetValues<InpDischargeType>().Select(x => (int)x).ToArray();

        Assert.Equal(4, terpakai.Length);
        Assert.DoesNotContain(4, terpakai);
        Assert.DoesNotContain(5, terpakai);
        Assert.Equal(0, (int)InpDischargeType.Unknown);
        Assert.Equal(3, (int)InpDischargeType.Referred);
    }

    [Fact]
    public void EnumPendukung_AngkanyaSesuaiKamusData()
    {
        Assert.Equal(1, (int)InpBedReservationStatus.Active);
        Assert.Equal(4, (int)InpBedReservationStatus.Cancelled);
        Assert.Equal(4, Enum.GetNames<InpBedReservationStatus>().Length);

        // PatientDeparted lahir dari RWI-DEC-055.
        Assert.Equal(4, (int)InpBedPlacementEndReason.PatientDeparted);
        Assert.Equal(4, Enum.GetNames<InpBedPlacementEndReason>().Length);

        Assert.Equal(0, (int)InpFinancialClearanceStatus.Pending);
        Assert.Equal(3, Enum.GetNames<InpFinancialClearanceStatus>().Length);

        // InpIsolationSource lahir dari RWI-DEC-065.
        Assert.Equal(1, (int)InpIsolationSource.AdmissionRecord);
        Assert.Equal(2, (int)InpIsolationSource.ClinicalDecision);
        Assert.Equal(2, Enum.GetNames<InpIsolationSource>().Length);

        Assert.Equal(1, (int)InpStatusChangeActorType.User);
        Assert.Equal(2, (int)InpStatusChangeActorType.System);
        Assert.Equal(2, Enum.GetNames<InpStatusChangeActorType>().Length);
    }

    [Fact]
    public void NilaiBedReservationStatus_TidakAdaYangBernilaiNol()
    {
        // Bawaannya Active = 1. Bila ada nilai 0, baris yang lupa diisi akan terbaca
        // sebagai status yang sah tanpa pernah ditetapkan siapa pun.
        Assert.DoesNotContain(0, Enum.GetValues<InpBedReservationStatus>().Select(x => (int)x));
    }
}
