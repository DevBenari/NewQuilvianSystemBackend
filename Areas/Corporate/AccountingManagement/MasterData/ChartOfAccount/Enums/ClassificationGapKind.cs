using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums
{
    public enum ClassificationGapKind
    {
        [Display(Name = "Klasifikasi belum diisi")]
        MissingReportClassification = 1,

        [Display(Name = "Sub-klasifikasi aset lancar belum diisi")]
        MissingCurrentAssetCategory = 2
    }
}
