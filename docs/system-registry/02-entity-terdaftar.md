# Entity Terdaftar

Diaudit pada backend `f2c5090` dan frontend `847be1fc0`.

Berkas ini menjawab: **apa saja yang sudah dibuat, dan sejauh mana jadinya.**

## Cara membaca tabel

| Kolom | Isi |
| --- | --- |
| **Jenis** | `Master` data induk, `Transaksi` data kejadian, `Sistem` kebutuhan teknis, `Identitas` pengguna dan hak akses |
| **Model** | Kelas entity ditemukan di source |
| **Config** | Ada `IEntityTypeConfiguration<T>` yang mengatur relasi dan index |
| **Migration** | Ada migration yang benar-benar membuat atau mengganti nama tabelnya |
| **API** | Ada controller atau service yang memakainya |
| **Consumer** | Base URL controller-nya benar-benar dipanggil source frontend |
| **Bukti** | Path model ditambah commit SHA yang diaudit |

Tanda `✓` berarti terbukti, `—` berarti tidak ditemukan.

Tingkat kesiapan memakai satu sumbu:

| Tingkat | Artinya bagi developer |
| --- | --- |
| `L1` | Baru berupa kelas terdaftar |
| `L2` | Tabel sudah nyata, tetapi belum ada yang memakainya lewat API |
| `L3` | Sudah bisa dipakai lewat API, belum terbukti sampai ke pengguna |
| `L4` | Terbukti dipakai layar frontend |

## Catatan penting sebelum membaca

**Kolom Config yang bertanda `—` tidak selalu berarti salah.** Sebagian entity dikonfigurasi
lewat atribut `[Table]` pada kelasnya atau lewat `OnModelCreating`, bukan lewat berkas
`IEntityTypeConfiguration` tersendiri. Yang perlu ditinjau adalah entity yang sekaligus tidak
punya configuration dan sudah dipakai API; daftarnya ada pada zona konflik `KF-004`.

**Kolom Consumer memakai perbandingan base URL, bukan penelusuran pemanggilan per fungsi.**
Entity yang dipakai antar modul backend tanpa layar frontend tetap `L3`. Itu bukan cacat;
banyak entity memang tidak pernah disentuh langsung pengguna.

---

### Administrator / MasterData

Jumlah entity: 18

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `MstKioskDevice` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstKioskDevice.cs` @ `f2c5090` |
| `MstIdentityScannerProfile` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstIdentityScannerProfile.cs` @ `f2c5090` |
| `MstCountry` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstCountry.cs` @ `f2c5090` |
| `MstProvince` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstProvince.cs` @ `f2c5090` |
| `MstCity` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstCity.cs` @ `f2c5090` |
| `MstDistrict` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstDistrict.cs` @ `f2c5090` |
| `MstPostalCode` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstPostalCode.cs` @ `f2c5090` |
| `MstBank` | Master | ✓ | — | ✓ | ✓ | ✓ | `L2` | `Areas/Administrator/MasterData/Models/MstBank.cs` @ `f2c5090` |
| `MstNurseStationCluster` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstNurseStationCluster.cs` @ `f2c5090` |
| `MstNurseStationClusterClinic` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstNurseStationClusterClinic.cs` @ `f2c5090` |
| `MstNurseStationClusterStaff` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstNurseStationClusterStaff.cs` @ `f2c5090` |
| `MstQueueDisplayDevice` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstQueueDisplayDevice.cs` @ `f2c5090` |
| `MstQueueVoiceProfile` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstQueueVoiceProfile.cs` @ `f2c5090` |
| `MstMembershipTier` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstMembershipTier.cs` @ `f2c5090` |
| `MstInsuranceProvider` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstInsuranceProvider.cs` @ `f2c5090` |
| `MstCompanyGuarantor` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs` @ `f2c5090` |
| `MstSupplier` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstSupplier.cs` @ `f2c5090` |
| `MstNurseStationClusterStaffClinic` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Administrator/MasterData/Models/MstNurseStationClusterStaffClinic.cs` @ `f2c5090` |

### Corporate / HumanResource/AttendanceManagement

Jumlah entity: 14

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `HrdAttendance` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendance.cs` @ `f2c5090` |
| `HrdAttendanceRawLog` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceRawLog.cs` @ `f2c5090` |
| `HrdAttendanceProcessingRun` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceProcessingRun.cs` @ `f2c5090` |
| `HrdAttendancePeriod` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendancePeriod.cs` @ `f2c5090` |
| `HrdAttendanceSchedulerJob` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceSchedulerJob.cs` @ `f2c5090` |
| `HrdAttendanceDaily` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceDaily.cs` @ `f2c5090` |
| `HrdAttendanceDailySegment` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceDailySegment.cs` @ `f2c5090` |
| `HrdAttendanceException` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceException.cs` @ `f2c5090` |
| `HrdAttendanceCorrectionRequest` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceCorrectionRequest.cs` @ `f2c5090` |
| `HrdAttendanceCorrectionDetail` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceCorrectionDetail.cs` @ `f2c5090` |
| `HrdAttendanceCorrectionApproval` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceCorrectionApproval.cs` @ `f2c5090` |
| `HrdMissingAttendance` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdMissingAttendance.cs` @ `f2c5090` |
| `HrdBusinessTripAttendance` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdBusinessTripAttendance.cs` @ `f2c5090` |
| `HrdRemoteAttendance` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdRemoteAttendance.cs` @ `f2c5090` |

### Corporate / HumanResource/BenefitManagement

Jumlah entity: 9

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `TrxEmployeeBenefitEnrollment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxEmployeeBenefitEnrollment.cs` @ `f2c5090` |
| `TrxEmployeeBenefitDependent` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxEmployeeBenefitDependent.cs` @ `f2c5090` |
| `TrxEmployeeInsuranceEnrollment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxEmployeeInsuranceEnrollment.cs` @ `f2c5090` |
| `TrxBenefitClaim` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxBenefitClaim.cs` @ `f2c5090` |
| `TrxBenefitClaimItem` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxBenefitClaimItem.cs` @ `f2c5090` |
| `TrxBenefitClaimDocument` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxBenefitClaimDocument.cs` @ `f2c5090` |
| `TrxBenefitClaimApproval` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxBenefitClaimApproval.cs` @ `f2c5090` |
| `TrxEmployeeLoan` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxEmployeeLoan.cs` @ `f2c5090` |
| `TrxEmployeeLoanInstallment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxEmployeeLoanInstallment.cs` @ `f2c5090` |

### Corporate / HumanResource/BusinessTravelManagement

Jumlah entity: 13

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `TrxBusinessTravelRequest` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxBusinessTravelRequest.cs` @ `f2c5090` |
| `TrxBusinessTravelParticipant` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxBusinessTravelParticipant.cs` @ `f2c5090` |
| `TrxBusinessTravelApproval` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxBusinessTravelApproval.cs` @ `f2c5090` |
| `TrxTravelItinerary` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxTravelItinerary.cs` @ `f2c5090` |
| `TrxTravelTransportation` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxTravelTransportation.cs` @ `f2c5090` |
| `TrxTravelAccommodation` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxTravelAccommodation.cs` @ `f2c5090` |
| `TrxTravelAdvanceRequest` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxTravelAdvanceRequest.cs` @ `f2c5090` |
| `TrxTravelAdvancePayment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxTravelAdvancePayment.cs` @ `f2c5090` |
| `TrxTravelExpenseClaim` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxTravelExpenseClaim.cs` @ `f2c5090` |
| `TrxTravelExpenseItem` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxTravelExpenseItem.cs` @ `f2c5090` |
| `TrxTravelSettlement` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxTravelSettlement.cs` @ `f2c5090` |
| `TrxTravelDocument` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxTravelDocument.cs` @ `f2c5090` |
| `TrxTravelAttendanceLink` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/BusinessTravelManagement/Models/TrxTravelAttendanceLink.cs` @ `f2c5090` |

### Corporate / HumanResource/CredentialingManagement

Jumlah entity: 18

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `WfpCertification` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/WfpCertification.cs` @ `f2c5090` |
| `WfpCredentialLicense` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/WfpCredentialLicense.cs` @ `f2c5090` |
| `WfpClinicalPrivilege` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/WfpClinicalPrivilege.cs` @ `f2c5090` |
| `WfpComplianceAlert` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/WfpComplianceAlert.cs` @ `f2c5090` |
| `WfpComplianceAlertLog` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/WfpComplianceAlertLog.cs` @ `f2c5090` |
| `TrxCredentialingApplication` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxCredentialingApplication.cs` @ `f2c5090` |
| `TrxCredentialingDocument` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxCredentialingDocument.cs` @ `f2c5090` |
| `TrxCredentialingVerification` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxCredentialingVerification.cs` @ `f2c5090` |
| `TrxCredentialingCommitteeReview` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxCredentialingCommitteeReview.cs` @ `f2c5090` |
| `TrxCredentialingDecision` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxCredentialingDecision.cs` @ `f2c5090` |
| `TrxRecredentialingApplication` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxRecredentialingApplication.cs` @ `f2c5090` |
| `TrxLicenseRenewalRequest` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxLicenseRenewalRequest.cs` @ `f2c5090` |
| `TrxCertificationRenewalRequest` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxCertificationRenewalRequest.cs` @ `f2c5090` |
| `TrxClinicalPrivilegeRequest` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxClinicalPrivilegeRequest.cs` @ `f2c5090` |
| `TrxClinicalPrivilegeAssessment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxClinicalPrivilegeAssessment.cs` @ `f2c5090` |
| `TrxClinicalPrivilegeApproval` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxClinicalPrivilegeApproval.cs` @ `f2c5090` |
| `TrxClinicalPrivilegeSuspension` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxClinicalPrivilegeSuspension.cs` @ `f2c5090` |
| `TrxClinicalPrivilegeRevocation` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxClinicalPrivilegeRevocation.cs` @ `f2c5090` |

### Corporate / HumanResource/EmployeeRelationManagement

Jumlah entity: 8

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `WfpDisciplinaryAction` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/EmployeeRelationManagement/Models/WfpDisciplinaryAction.cs` @ `f2c5090` |
| `TrxEmployeeIncidentReport` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/EmployeeRelationManagement/Models/TrxEmployeeIncidentReport.cs` @ `f2c5090` |
| `TrxEmployeeGrievance` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/EmployeeRelationManagement/Models/TrxEmployeeGrievance.cs` @ `f2c5090` |
| `TrxWorkplaceInvestigation` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/EmployeeRelationManagement/Models/TrxWorkplaceInvestigation.cs` @ `f2c5090` |
| `TrxInvestigationEvidence` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/EmployeeRelationManagement/Models/TrxInvestigationEvidence.cs` @ `f2c5090` |
| `TrxDisciplinaryCase` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/EmployeeRelationManagement/Models/TrxDisciplinaryCase.cs` @ `f2c5090` |
| `TrxDisciplinaryDecision` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/EmployeeRelationManagement/Models/TrxDisciplinaryDecision.cs` @ `f2c5090` |
| `HrdEmployeeRecognition` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/EmployeeRelationManagement/Models/HrdEmployeeRecognition.cs` @ `f2c5090` |

### Corporate / HumanResource/ExpenseManagement

Jumlah entity: 7

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `TrxExpenseClaim` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/ExpenseManagement/Models/TrxExpenseClaim.cs` @ `f2c5090` |
| `TrxExpenseClaimItem` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/ExpenseManagement/Models/TrxExpenseClaimItem.cs` @ `f2c5090` |
| `TrxExpenseReceipt` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/ExpenseManagement/Models/TrxExpenseReceipt.cs` @ `f2c5090` |
| `TrxExpenseApproval` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/ExpenseManagement/Models/TrxExpenseApproval.cs` @ `f2c5090` |
| `TrxExpenseVerification` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/ExpenseManagement/Models/TrxExpenseVerification.cs` @ `f2c5090` |
| `TrxExpensePayment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/ExpenseManagement/Models/TrxExpensePayment.cs` @ `f2c5090` |
| `TrxExpenseReversal` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/ExpenseManagement/Models/TrxExpenseReversal.cs` @ `f2c5090` |

### Corporate / HumanResource/HrServiceManagement

Jumlah entity: 8

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `MstHrServiceCategory` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/HrServiceManagement/Models/MstHrServiceCategory.cs` @ `f2c5090` |
| `MstHrServiceType` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/HrServiceManagement/Models/MstHrServiceType.cs` @ `f2c5090` |
| `MstEmployeeDocumentType` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/HrServiceManagement/Models/MstEmployeeDocumentType.cs` @ `f2c5090` |
| `TrxHrServiceRequest` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/HrServiceManagement/Models/TrxHrServiceRequest.cs` @ `f2c5090` |
| `TrxHrServiceRequestComment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/HrServiceManagement/Models/TrxHrServiceRequestComment.cs` @ `f2c5090` |
| `TrxHrServiceRequestAttachment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/HrServiceManagement/Models/TrxHrServiceRequestAttachment.cs` @ `f2c5090` |
| `TrxEmployeeDocumentRequest` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/HrServiceManagement/Models/TrxEmployeeDocumentRequest.cs` @ `f2c5090` |
| `TrxEmployeeDocumentIssuance` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/HrServiceManagement/Models/TrxEmployeeDocumentIssuance.cs` @ `f2c5090` |

### Corporate / HumanResource/LearningAndDevelopment

Jumlah entity: 13

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `WfpTrainingRecord` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/WfpTrainingRecord.cs` @ `f2c5090` |
| `WfpCompetencyAssessment` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/WfpCompetencyAssessment.cs` @ `f2c5090` |
| `TrxTrainingPlan` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingPlan.cs` @ `f2c5090` |
| `TrxTrainingSession` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingSession.cs` @ `f2c5090` |
| `TrxTrainingParticipant` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingParticipant.cs` @ `f2c5090` |
| `TrxTrainingEnrollmentRequest` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingEnrollmentRequest.cs` @ `f2c5090` |
| `TrxTrainingAttendance` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingAttendance.cs` @ `f2c5090` |
| `TrxTrainingAssessment` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingAssessment.cs` @ `f2c5090` |
| `TrxTrainingResult` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingResult.cs` @ `f2c5090` |
| `TrxTrainingEvaluation` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingEvaluation.cs` @ `f2c5090` |
| `TrxTrainingCertificate` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingCertificate.cs` @ `f2c5090` |
| `TrxIndividualDevelopmentPlan` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxIndividualDevelopmentPlan.cs` @ `f2c5090` |
| `TrxTrainingBudget` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingBudget.cs` @ `f2c5090` |

### Corporate / HumanResource/LeaveManagement

Jumlah entity: 17

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `WfpLeaveBalance` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/WfpLeaveBalance.cs` @ `f2c5090` |
| `TrxLeaveEntitlementPeriod` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveEntitlementPeriod.cs` @ `f2c5090` |
| `WfpLeaveRequest` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/WfpLeaveRequest.cs` @ `f2c5090` |
| `TrxLeaveEntitlement` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveEntitlement.cs` @ `f2c5090` |
| `TrxLeaveAccrualRun` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveAccrualRun.cs` @ `f2c5090` |
| `TrxLeaveAccrual` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveAccrual.cs` @ `f2c5090` |
| `TrxLeaveCarryForwardRun` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveCarryForwardRun.cs` @ `f2c5090` |
| `TrxLeaveCarryForward` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveCarryForward.cs` @ `f2c5090` |
| `TrxLeaveAdjustment` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveAdjustment.cs` @ `f2c5090` |
| `TrxLeaveBalanceTransaction` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveBalanceTransaction.cs` @ `f2c5090` |
| `TrxLeaveRequestApproval` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveRequestApproval.cs` @ `f2c5090` |
| `TrxLeaveRequestAttachment` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveRequestAttachment.cs` @ `f2c5090` |
| `TrxLeaveCancellationRequest` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveCancellationRequest.cs` @ `f2c5090` |
| `TrxLeaveRecall` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveRecall.cs` @ `f2c5090` |
| `TrxCompensatoryLeave` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxCompensatoryLeave.cs` @ `f2c5090` |
| `TrxLeaveExecution` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveExecution.cs` @ `f2c5090` |
| `TrxLeaveAttendanceIntegration` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveAttendanceIntegration.cs` @ `f2c5090` |

### Corporate / HumanResource/LifecycleManagement

Jumlah entity: 21

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `WfpOnboardingChecklist` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/WfpOnboardingChecklist.cs` @ `f2c5090` |
| `WfpOnboardingTask` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/WfpOnboardingTask.cs` @ `f2c5090` |
| `WfpOffboardingChecklist` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/WfpOffboardingChecklist.cs` @ `f2c5090` |
| `WfpOffboardingTask` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/WfpOffboardingTask.cs` @ `f2c5090` |
| `MstOnboardingTemplate` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/MstOnboardingTemplate.cs` @ `f2c5090` |
| `MstOnboardingTemplateTask` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/MstOnboardingTemplateTask.cs` @ `f2c5090` |
| `MstOffboardingTemplate` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/MstOffboardingTemplate.cs` @ `f2c5090` |
| `MstOffboardingTemplateTask` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/MstOffboardingTemplateTask.cs` @ `f2c5090` |
| `TrxEmployeeOnboarding` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxEmployeeOnboarding.cs` @ `f2c5090` |
| `TrxEmployeeOnboardingTask` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxEmployeeOnboardingTask.cs` @ `f2c5090` |
| `TrxProbationReview` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxProbationReview.cs` @ `f2c5090` |
| `TrxEmployeeSeparation` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxEmployeeSeparation.cs` @ `f2c5090` |
| `TrxResignationRequest` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxResignationRequest.cs` @ `f2c5090` |
| `TrxRetirement` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxRetirement.cs` @ `f2c5090` |
| `TrxContractNonRenewal` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxContractNonRenewal.cs` @ `f2c5090` |
| `TrxTermination` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxTermination.cs` @ `f2c5090` |
| `TrxExitClearance` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxExitClearance.cs` @ `f2c5090` |
| `TrxAssetReturn` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxAssetReturn.cs` @ `f2c5090` |
| `TrxAccessRevocation` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxAccessRevocation.cs` @ `f2c5090` |
| `TrxExitInterview` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxExitInterview.cs` @ `f2c5090` |
| `TrxEmploymentCertificateRequest` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxEmploymentCertificateRequest.cs` @ `f2c5090` |

### Corporate / HumanResource/MasterData

Jumlah entity: 87

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `MstLegalEntity` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstLegalEntity.cs` @ `f2c5090` |
| `MstHospitalSite` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstHospitalSite.cs` @ `f2c5090` |
| `MstOrganizationUnit` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstOrganizationUnit.cs` @ `f2c5090` |
| `MstDepartment` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstDepartment.cs` @ `f2c5090` |
| `MstPosition` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstPosition.cs` @ `f2c5090` |
| `MstJobFamily` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstJobFamily.cs` @ `f2c5090` |
| `MstJobLevel` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstJobLevel.cs` @ `f2c5090` |
| `MstEmployeeGrade` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstEmployeeGrade.cs` @ `f2c5090` |
| `MstCostCenter` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstCostCenter.cs` @ `f2c5090` |
| `MstWorkLocation` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstWorkLocation.cs` @ `f2c5090` |
| `MstWorkforceProfile` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstWorkforceProfile.cs` @ `f2c5090` |
| `MstEmployee` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstEmployee.cs` @ `f2c5090` |
| `MstDoctor` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstDoctor.cs` @ `f2c5090` |
| `MstExternalUser` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstExternalUser.cs` @ `f2c5090` |
| `MstWorkforceType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstWorkforceType.cs` @ `f2c5090` |
| `MstEmployeeCategory` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstEmployeeCategory.cs` @ `f2c5090` |
| `MstEmploymentType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstEmploymentType.cs` @ `f2c5090` |
| `MstEmploymentStatus` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstEmploymentStatus.cs` @ `f2c5090` |
| `MstContractType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstContractType.cs` @ `f2c5090` |
| `MstWorkerSource` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstWorkerSource.cs` @ `f2c5090` |
| `MstTerminationReason` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstTerminationReason.cs` @ `f2c5090` |
| `MstTransferReason` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstTransferReason.cs` @ `f2c5090` |
| `MstPromotionReason` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstPromotionReason.cs` @ `f2c5090` |
| `MstWorkSchedule` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstWorkSchedule.cs` @ `f2c5090` |
| `MstShift` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstShift.cs` @ `f2c5090` |
| `MstShiftGroup` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstShiftGroup.cs` @ `f2c5090` |
| `MstShiftPattern` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstShiftPattern.cs` @ `f2c5090` |
| `MstWorkCalendar` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstWorkCalendar.cs` @ `f2c5090` |
| `MstHoliday` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstHoliday.cs` @ `f2c5090` |
| `MstAttendanceDevice` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstAttendanceDevice.cs` @ `f2c5090` |
| `MstAttendanceLocation` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstAttendanceLocation.cs` @ `f2c5090` |
| `MstAttendancePolicy` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstAttendancePolicy.cs` @ `f2c5090` |
| `MstGracePeriodPolicy` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstGracePeriodPolicy.cs` @ `f2c5090` |
| `MstRosterPolicy` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstRosterPolicy.cs` @ `f2c5090` |
| `MstMinimumRestPolicy` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstMinimumRestPolicy.cs` @ `f2c5090` |
| `MstOnCallType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstOnCallType.cs` @ `f2c5090` |
| `MstLeaveType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/LeaveAndOvertime/Models/MstLeaveType.cs` @ `f2c5090` |
| `MstLeavePolicy` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/LeaveAndOvertime/Models/MstLeavePolicy.cs` @ `f2c5090` |
| `MstLeaveEntitlementPolicy` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/LeaveAndOvertime/Models/MstLeaveEntitlementPolicy.cs` @ `f2c5090` |
| `MstLeaveCarryForwardPolicy` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/LeaveAndOvertime/Models/MstLeaveCarryForwardPolicy.cs` @ `f2c5090` |
| `MstLeaveAdjustmentReason` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/MasterData/LeaveAndOvertime/Models/MstLeaveAdjustmentReason.cs` @ `f2c5090` |
| `MstOvertimePolicy` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/LeaveAndOvertime/Models/MstOvertimePolicy.cs` @ `f2c5090` |
| `MstOvertimeRate` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/LeaveAndOvertime/Models/MstOvertimeRate.cs` @ `f2c5090` |
| `MstTravelType` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/TravelAndExpense/Models/MstTravelType.cs` @ `f2c5090` |
| `MstTravelPolicy` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/TravelAndExpense/Models/MstTravelPolicy.cs` @ `f2c5090` |
| `MstTravelClass` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/TravelAndExpense/Models/MstTravelClass.cs` @ `f2c5090` |
| `MstTravelExpenseCategory` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/TravelAndExpense/Models/MstTravelExpenseCategory.cs` @ `f2c5090` |
| `MstTravelAllowanceRate` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/TravelAndExpense/Models/MstTravelAllowanceRate.cs` @ `f2c5090` |
| `MstTravelDestinationZone` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/TravelAndExpense/Models/MstTravelDestinationZone.cs` @ `f2c5090` |
| `MstExpenseCategory` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/TravelAndExpense/Models/MstExpenseCategory.cs` @ `f2c5090` |
| `MstReimbursementPolicy` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/TravelAndExpense/Models/MstReimbursementPolicy.cs` @ `f2c5090` |
| `MstPaymentSettlementMethod` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/MasterData/TravelAndExpense/Models/MstPaymentSettlementMethod.cs` @ `f2c5090` |
| `MstPayrollPeriod` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstPayrollPeriod.cs` @ `f2c5090` |
| `MstPayrollComponent` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstPayrollComponent.cs` @ `f2c5090` |
| `MstPayrollComponentCategory` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstPayrollComponentCategory.cs` @ `f2c5090` |
| `MstSalaryStructure` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstSalaryStructure.cs` @ `f2c5090` |
| `MstSalaryGrade` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstSalaryGrade.cs` @ `f2c5090` |
| `MstAllowanceType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstAllowanceType.cs` @ `f2c5090` |
| `MstDeductionType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstDeductionType.cs` @ `f2c5090` |
| `MstShiftAllowancePolicy` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstShiftAllowancePolicy.cs` @ `f2c5090` |
| `MstOnCallAllowancePolicy` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstOnCallAllowancePolicy.cs` @ `f2c5090` |
| `MstHazardAllowancePolicy` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstHazardAllowancePolicy.cs` @ `f2c5090` |
| `MstBenefitPlan` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstBenefitPlan.cs` @ `f2c5090` |
| `MstBenefitType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstBenefitType.cs` @ `f2c5090` |
| `MstBenefitEligibilityRule` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstBenefitEligibilityRule.cs` @ `f2c5090` |
| `MstCompetency` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstCompetency.cs` @ `f2c5090` |
| `MstPositionCompetencyRequirement` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstPositionCompetencyRequirement.cs` @ `f2c5090` |
| `MstTrainingCatalog` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstTrainingCatalog.cs` @ `f2c5090` |
| `MstTrainingCategory` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstTrainingCategory.cs` @ `f2c5090` |
| `MstCertificationType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstCertificationType.cs` @ `f2c5090` |
| `MstLicenseType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstLicenseType.cs` @ `f2c5090` |
| `MstProfession` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstProfession.cs` @ `f2c5090` |
| `MstSpecialization` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstSpecialization.cs` @ `f2c5090` |
| `MstCredentialingRequirement` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstCredentialingRequirement.cs` @ `f2c5090` |
| `MstClinicalPrivilegeCatalog` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstClinicalPrivilegeCatalog.cs` @ `f2c5090` |
| `MstMandatoryTrainingRule` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstMandatoryTrainingRule.cs` @ `f2c5090` |
| `MstPerformanceCycle` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Performance/Models/MstPerformanceCycle.cs` @ `f2c5090` |
| `MstPerformanceRatingScale` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Performance/Models/MstPerformanceRatingScale.cs` @ `f2c5090` |
| `MstPerformanceTemplate` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Performance/Models/MstPerformanceTemplate.cs` @ `f2c5090` |
| `MstPerformanceTemplateDetail` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Performance/Models/MstPerformanceTemplateDetail.cs` @ `f2c5090` |
| `MstKpiCatalog` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Performance/Models/MstKpiCatalog.cs` @ `f2c5090` |
| `MstApprovalDelegationPolicy` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workflow/Models/MstApprovalDelegationPolicy.cs` @ `f2c5090` |
| `MstRequestReason` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workflow/Models/MstRequestReason.cs` @ `f2c5090` |
| `MstRejectionReason` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workflow/Models/MstRejectionReason.cs` @ `f2c5090` |
| `MstWorkflowDefinition` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workflow/Models/MstWorkflowDefinition.cs` @ `f2c5090` |
| `MstWorkflowStep` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workflow/Models/MstWorkflowStep.cs` @ `f2c5090` |
| `MstApprovalMatrix` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/MasterData/Workflow/Models/MstApprovalMatrix.cs` @ `f2c5090` |

### Corporate / HumanResource/OccupationalHealthManagement

Jumlah entity: 10

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `WfpHealthRecord` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/OccupationalHealthManagement/Models/WfpHealthRecord.cs` @ `f2c5090` |
| `TrxEmployeeMedicalExamination` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/OccupationalHealthManagement/Models/TrxEmployeeMedicalExamination.cs` @ `f2c5090` |
| `TrxEmployeeVaccination` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/OccupationalHealthManagement/Models/TrxEmployeeVaccination.cs` @ `f2c5090` |
| `TrxEmployeeFitnessToWork` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/OccupationalHealthManagement/Models/TrxEmployeeFitnessToWork.cs` @ `f2c5090` |
| `TrxWorkRestriction` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/OccupationalHealthManagement/Models/TrxWorkRestriction.cs` @ `f2c5090` |
| `TrxOccupationalExposure` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/OccupationalHealthManagement/Models/TrxOccupationalExposure.cs` @ `f2c5090` |
| `TrxNeedleStickIncident` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/OccupationalHealthManagement/Models/TrxNeedleStickIncident.cs` @ `f2c5090` |
| `TrxEmployeeInjury` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/OccupationalHealthManagement/Models/TrxEmployeeInjury.cs` @ `f2c5090` |
| `TrxReturnToWorkAssessment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/OccupationalHealthManagement/Models/TrxReturnToWorkAssessment.cs` @ `f2c5090` |
| `TrxEmployeeHealthSurveillance` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/OccupationalHealthManagement/Models/TrxEmployeeHealthSurveillance.cs` @ `f2c5090` |

### Corporate / HumanResource/OvertimeManagement

Jumlah entity: 11

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `TrxOvertimePlan` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/OvertimeManagement/Models/TrxOvertimePlan.cs` @ `f2c5090` |
| `TrxOvertimePlanDetail` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/OvertimeManagement/Models/TrxOvertimePlanDetail.cs` @ `f2c5090` |
| `WfpOvertimeRequest` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/OvertimeManagement/Models/WfpOvertimeRequest.cs` @ `f2c5090` |
| `TrxOvertimeRequestDetail` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/OvertimeManagement/Models/TrxOvertimeRequestDetail.cs` @ `f2c5090` |
| `TrxOvertimeRequestApproval` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/OvertimeManagement/Models/TrxOvertimeRequestApproval.cs` @ `f2c5090` |
| `TrxOvertimeRealization` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/OvertimeManagement/Models/TrxOvertimeRealization.cs` @ `f2c5090` |
| `TrxOvertimeRealizationDetail` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/OvertimeManagement/Models/TrxOvertimeRealizationDetail.cs` @ `f2c5090` |
| `TrxOvertimeVerification` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/OvertimeManagement/Models/TrxOvertimeVerification.cs` @ `f2c5090` |
| `TrxCompensatoryTimeOff` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/OvertimeManagement/Models/TrxCompensatoryTimeOff.cs` @ `f2c5090` |
| `TrxOvertimePeriod` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/OvertimeManagement/Models/TrxOvertimePeriod.cs` @ `f2c5090` |
| `TrxOvertimeSchedulerJob` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/OvertimeManagement/Models/TrxOvertimeSchedulerJob.cs` @ `f2c5090` |

### Corporate / HumanResource/PayrollManagement

Jumlah entity: 19

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `WfpPayroll` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/PayrollManagement/Models/WfpPayroll.cs` @ `f2c5090` |
| `WfpTax` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/PayrollManagement/Models/WfpTax.cs` @ `f2c5090` |
| `WfpInsurance` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/PayrollManagement/Models/WfpInsurance.cs` @ `f2c5090` |
| `WfpTransportAllowancePolicy` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/PayrollManagement/Models/WfpTransportAllowancePolicy.cs` @ `f2c5090` |
| `WfpTransportAllowance` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/PayrollManagement/Models/WfpTransportAllowance.cs` @ `f2c5090` |
| `WfpTransportAllowanceTransaction` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/PayrollManagement/Models/WfpTransportAllowanceTransaction.cs` @ `f2c5090` |
| `TrxPayrollRun` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollRun.cs` @ `f2c5090` |
| `TrxPayrollRunEmployee` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollRunEmployee.cs` @ `f2c5090` |
| `TrxPayrollEmployeeComponent` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollEmployeeComponent.cs` @ `f2c5090` |
| `TrxPayrollAttendanceInput` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollAttendanceInput.cs` @ `f2c5090` |
| `TrxPayrollOvertimeInput` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollOvertimeInput.cs` @ `f2c5090` |
| `TrxPayrollVariableInput` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollVariableInput.cs` @ `f2c5090` |
| `TrxPayrollAdjustment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollAdjustment.cs` @ `f2c5090` |
| `TrxPayrollApproval` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollApproval.cs` @ `f2c5090` |
| `TrxPayrollPayment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollPayment.cs` @ `f2c5090` |
| `TrxPayrollPayslip` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollPayslip.cs` @ `f2c5090` |
| `TrxPayrollReversal` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollReversal.cs` @ `f2c5090` |
| `TrxMedicalServiceFeeCalculation` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxMedicalServiceFeeCalculation.cs` @ `f2c5090` |
| `TrxMedicalServiceFeePayment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxMedicalServiceFeePayment.cs` @ `f2c5090` |

### Corporate / HumanResource/PerformanceManagement

Jumlah entity: 11

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `WfpPerformanceReview` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/PerformanceManagement/Models/WfpPerformanceReview.cs` @ `f2c5090` |
| `WfpPerformanceReviewDetail` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/PerformanceManagement/Models/WfpPerformanceReviewDetail.cs` @ `f2c5090` |
| `TrxPerformanceCycle` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPerformanceCycle.cs` @ `f2c5090` |
| `TrxEmployeeGoal` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxEmployeeGoal.cs` @ `f2c5090` |
| `TrxEmployeeKpiTarget` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxEmployeeKpiTarget.cs` @ `f2c5090` |
| `TrxSelfAssessment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxSelfAssessment.cs` @ `f2c5090` |
| `TrxManagerAssessment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxManagerAssessment.cs` @ `f2c5090` |
| `TrxPeerFeedback` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPeerFeedback.cs` @ `f2c5090` |
| `TrxPerformanceCheckIn` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPerformanceCheckIn.cs` @ `f2c5090` |
| `TrxCalibrationSession` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxCalibrationSession.cs` @ `f2c5090` |
| `TrxPerformanceImprovementPlan` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPerformanceImprovementPlan.cs` @ `f2c5090` |

### Corporate / HumanResource/RecruitmentManagement

Jumlah entity: 20

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `MstRecruitmentSource` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/MstRecruitmentSource.cs` @ `f2c5090` |
| `MstRecruitmentStage` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/MstRecruitmentStage.cs` @ `f2c5090` |
| `MstCandidateStatus` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/MstCandidateStatus.cs` @ `f2c5090` |
| `MstInterviewTemplate` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/MstInterviewTemplate.cs` @ `f2c5090` |
| `MstAssessmentMethod` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/MstAssessmentMethod.cs` @ `f2c5090` |
| `TrxJobRequisition` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxJobRequisition.cs` @ `f2c5090` |
| `TrxJobRequisitionApproval` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxJobRequisitionApproval.cs` @ `f2c5090` |
| `TrxJobVacancy` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxJobVacancy.cs` @ `f2c5090` |
| `TrxCandidate` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxCandidate.cs` @ `f2c5090` |
| `TrxCandidateApplication` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxCandidateApplication.cs` @ `f2c5090` |
| `TrxCandidateDocument` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxCandidateDocument.cs` @ `f2c5090` |
| `TrxCandidateScreening` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxCandidateScreening.cs` @ `f2c5090` |
| `TrxCandidateAssessment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxCandidateAssessment.cs` @ `f2c5090` |
| `TrxCandidateInterview` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxCandidateInterview.cs` @ `f2c5090` |
| `TrxInterviewEvaluation` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxInterviewEvaluation.cs` @ `f2c5090` |
| `TrxReferenceCheck` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxReferenceCheck.cs` @ `f2c5090` |
| `TrxBackgroundCheck` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxBackgroundCheck.cs` @ `f2c5090` |
| `TrxPreEmploymentMedicalCheck` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxPreEmploymentMedicalCheck.cs` @ `f2c5090` |
| `TrxJobOffer` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxJobOffer.cs` @ `f2c5090` |
| `TrxCandidateHiring` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/RecruitmentManagement/Models/TrxCandidateHiring.cs` @ `f2c5090` |

### Corporate / HumanResource/SchedulingManagement

Jumlah entity: 11

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `WfpWorkScheduleAssignment` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/WfpWorkScheduleAssignment.cs` @ `f2c5090` |
| `WfpScheduleChangeRequest` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/WfpScheduleChangeRequest.cs` @ `f2c5090` |
| `WfpShiftSwapRequest` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/WfpShiftSwapRequest.cs` @ `f2c5090` |
| `TrxRosterPeriod` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxRosterPeriod.cs` @ `f2c5090` |
| `TrxRosterAssignment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxRosterAssignment.cs` @ `f2c5090` |
| `TrxRosterApproval` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxRosterApproval.cs` @ `f2c5090` |
| `TrxRosterPublication` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxRosterPublication.cs` @ `f2c5090` |
| `TrxShiftAssignment` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxShiftAssignment.cs` @ `f2c5090` |
| `TrxOnCallAssignment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxOnCallAssignment.cs` @ `f2c5090` |
| `TrxShiftReplacement` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxShiftReplacement.cs` @ `f2c5090` |
| `TrxEmergencyStaffingRequest` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxEmergencyStaffingRequest.cs` @ `f2c5090` |

### Corporate / HumanResource/WorkflowManagement

Jumlah entity: 8

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `TrxWorkflowInstance` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowInstance.cs` @ `f2c5090` |
| `TrxWorkflowStepInstance` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowStepInstance.cs` @ `f2c5090` |
| `TrxApprovalAction` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxApprovalAction.cs` @ `f2c5090` |
| `TrxApprovalDelegation` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxApprovalDelegation.cs` @ `f2c5090` |
| `TrxWorkflowComment` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowComment.cs` @ `f2c5090` |
| `TrxWorkflowAttachment` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowAttachment.cs` @ `f2c5090` |
| `TrxWorkflowStatusHistory` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowStatusHistory.cs` @ `f2c5090` |
| `TrxWorkflowApproverAssignment` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowApproverAssignment.cs` @ `f2c5090` |

### Corporate / HumanResource/WorkforceCore

Jumlah entity: 21

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `WfpOrganizationAssignment` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpOrganizationAssignment.cs` @ `f2c5090` |
| `WfpBankAccount` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpBankAccount.cs` @ `f2c5090` |
| `WfpDocument` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpDocument.cs` @ `f2c5090` |
| `WfpEducation` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpEducation.cs` @ `f2c5090` |
| `WfpEmploymentHistory` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpEmploymentHistory.cs` @ `f2c5090` |
| `WfpContractHistory` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpContractHistory.cs` @ `f2c5090` |
| `WfpEmergencyContact` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpEmergencyContact.cs` @ `f2c5090` |
| `WfpFamilyMember` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpFamilyMember.cs` @ `f2c5090` |
| `WfpDependent` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpDependent.cs` @ `f2c5090` |
| `WfpAddress` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpAddress.cs` @ `f2c5090` |
| `WfpPositionAssignment` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpPositionAssignment.cs` @ `f2c5090` |
| `WfpManagerAssignment` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpManagerAssignment.cs` @ `f2c5090` |
| `WfpSalaryAssignment` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpSalaryAssignment.cs` @ `f2c5090` |
| `TrxEmployeeProfileChangeRequest` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/WorkforceCore/Models/TrxEmployeeProfileChangeRequest.cs` @ `f2c5090` |
| `TrxEmployeeProfileChangeDetail` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/WorkforceCore/Models/TrxEmployeeProfileChangeDetail.cs` @ `f2c5090` |
| `TrxEmployeeProfileChangeVerification` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/Corporate/HumanResource/WorkforceCore/Models/TrxEmployeeProfileChangeVerification.cs` @ `f2c5090` |
| `TrxEmployeeTransfer` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforceCore/Models/TrxEmployeeTransfer.cs` @ `f2c5090` |
| `TrxEmployeePromotion` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforceCore/Models/TrxEmployeePromotion.cs` @ `f2c5090` |
| `TrxEmployeeDemotion` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforceCore/Models/TrxEmployeeDemotion.cs` @ `f2c5090` |
| `TrxEmployeeRotation` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforceCore/Models/TrxEmployeeRotation.cs` @ `f2c5090` |
| `TrxTemporaryAssignment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforceCore/Models/TrxTemporaryAssignment.cs` @ `f2c5090` |

### Corporate / HumanResource/WorkforcePlanning

Jumlah entity: 11

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `MstWorkforceRequirement` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/Corporate/HumanResource/WorkforcePlanning/Models/MstWorkforceRequirement.cs` @ `f2c5090` |
| `MstStaffingStandard` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforcePlanning/Models/MstStaffingStandard.cs` @ `f2c5090` |
| `MstStaffingRatio` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforcePlanning/Models/MstStaffingRatio.cs` @ `f2c5090` |
| `MstShiftSkillRequirement` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforcePlanning/Models/MstShiftSkillRequirement.cs` @ `f2c5090` |
| `MstPositionHeadcountPlan` | Master | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforcePlanning/Models/MstPositionHeadcountPlan.cs` @ `f2c5090` |
| `TrxAnnualManpowerPlan` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforcePlanning/Models/TrxAnnualManpowerPlan.cs` @ `f2c5090` |
| `TrxManpowerPlanDetail` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforcePlanning/Models/TrxManpowerPlanDetail.cs` @ `f2c5090` |
| `TrxHeadcountRequest` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforcePlanning/Models/TrxHeadcountRequest.cs` @ `f2c5090` |
| `TrxStaffingGapAnalysis` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforcePlanning/Models/TrxStaffingGapAnalysis.cs` @ `f2c5090` |
| `TrxDailyStaffingRequirement` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforcePlanning/Models/TrxDailyStaffingRequirement.cs` @ `f2c5090` |
| `TrxWorkforceAllocation` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/Corporate/HumanResource/WorkforcePlanning/Models/TrxWorkforceAllocation.cs` @ `f2c5090` |

### Global-Shared / Shared

Jumlah entity: 12

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `SysAppVersion` | Sistem | ✓ | ✓ | ✓ | — | — | `L2` | `Models/SysAppVersion.cs` @ `f2c5090` |
| `SysAppVersionBuild` | Sistem | ✓ | ✓ | ✓ | — | — | `L2` | `Models/SysAppVersionBuild.cs` @ `f2c5090` |
| `SysApplicationModule` | Sistem | ✓ | ✓ | ✓ | — | — | `L2` | `Models/SysApplicationModule.cs` @ `f2c5090` |
| `SysControllerAccess` | Sistem | ✓ | ✓ | ✓ | — | — | `L2` | `Models/SysControllerAccess.cs` @ `f2c5090` |
| `SysActionAccess` | Sistem | ✓ | ✓ | ✓ | — | — | `L2` | `Models/SysActionAccess.cs` @ `f2c5090` |
| `SysAccessPolicy` | Sistem | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Models/SysAccessPolicy.cs` @ `f2c5090` |
| `ApplicationUserFingerprintCredential` | Identitas | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Models/ApplicationUserFingerprintCredential.cs` @ `f2c5090` |
| `ApplicationUserOrganization` | Identitas | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Models/ApplicationUserOrganization.cs` @ `f2c5090` |
| `MstDisciplinaryActionType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Models/MstDisciplinaryActionType.cs` @ `f2c5090` |
| `MstViolationType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Models/MstViolationType.cs` @ `f2c5090` |
| `MstSanctionType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Models/MstSanctionType.cs` @ `f2c5090` |
| `MstEmployeeRelationCaseType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Models/MstEmployeeRelationCaseType.cs` @ `f2c5090` |

### HealthServices / BillingManagement

Jumlah entity: 35

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `MstPaymentMethod` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/BillingManagement/MasterData/Models/MstPaymentMethod.cs` @ `f2c5090` |
| `MstBillingItemCategory` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/BillingManagement/MasterData/Models/MstBillingItemCategory.cs` @ `f2c5090` |
| `MstAdministrationFeePolicy` | Master | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/MasterData/Models/MstAdministrationFeePolicy.cs` @ `f2c5090` |
| `MstDiscountPolicy` | Master | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/MasterData/Models/MstDiscountPolicy.cs` @ `f2c5090` |
| `MstTaxRule` | Master | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/MasterData/Models/MstTaxRule.cs` @ `f2c5090` |
| `MstRoomChargePolicy` | Master | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/MasterData/Models/MstRoomChargePolicy.cs` @ `f2c5090` |
| `MstRegister` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/BillingManagement/MasterData/Models/MstRegister.cs` @ `f2c5090` |
| `BilInvoice` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoice.cs` @ `f2c5090` |
| `BilInvoiceItem` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoiceItem.cs` @ `f2c5090` |
| `BilCalculationVersion` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilCalculationVersion.cs` @ `f2c5090` |
| `BilDiscountApplication` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilDiscountApplication.cs` @ `f2c5090` |
| `BilChargeReceipt` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilChargeReceipt.cs` @ `f2c5090` |
| `BilNumberSeries` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilNumberSeries.cs` @ `f2c5090` |
| `BilDepositAccount` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilDepositAccount.cs` @ `f2c5090` |
| `BilDepositMovement` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilDepositMovement.cs` @ `f2c5090` |
| `BilSettlement` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilSettlement.cs` @ `f2c5090` |
| `BilTender` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilTender.cs` @ `f2c5090` |
| `BilPaymentAllocation` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilPaymentAllocation.cs` @ `f2c5090` |
| `BilRefundableCredit` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilRefundableCredit.cs` @ `f2c5090` |
| `BilRefundCase` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilRefundCase.cs` @ `f2c5090` |
| `BilRefundLine` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilRefundLine.cs` @ `f2c5090` |
| `BilAdjustment` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilAdjustment.cs` @ `f2c5090` |
| `BilWriteOffCase` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilWriteOffCase.cs` @ `f2c5090` |
| `BilFinalizationRecord` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilFinalizationRecord.cs` @ `f2c5090` |
| `BilArHandoff` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs` @ `f2c5090` |
| `BilApHandoff` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilApHandoff.cs` @ `f2c5090` |
| `BilHandoffAdjustment` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Billing/Models/BilHandoffAdjustment.cs` @ `f2c5090` |
| `BilCashierShift` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShift.cs` @ `f2c5090` |
| `BilCashVarianceReview` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashVarianceReview.cs` @ `f2c5090` |
| `BilCashierShiftHandover` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShiftHandover.cs` @ `f2c5090` |
| `BilCashierShiftCommand` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShiftCommand.cs` @ `f2c5090` |
| `BilFolio` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Operational/Models/BilFolio.cs` @ `f2c5090` |
| `BilChargeLine` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Operational/Models/BilChargeLine.cs` @ `f2c5090` |
| `BilChargeComponent` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Operational/Models/BilChargeComponent.cs` @ `f2c5090` |
| `BilProcessingEffect` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/BillingManagement/Operational/Models/BilProcessingEffect.cs` @ `f2c5090` |

### HealthServices / ClinicalBillingIntegration

Jumlah entity: 1

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `TrxClinicalMilestoneFact` | Transaksi | ✓ | — | ✓ | — | — | `L2` | `Areas/HealthServices/ClinicalBillingIntegration/Models/TrxClinicalMilestoneFact.cs` @ `f2c5090` |

### HealthServices / ClinicalManagement

Jumlah entity: 14

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `TrxPatientAssessment` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs` @ `f2c5090` |
| `TrxDoctorConsultation` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs` @ `f2c5090` |
| `TrxPatientDiagnosis` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientDiagnosis.cs` @ `f2c5090` |
| `TrxPatientProcedure` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs` @ `f2c5090` |
| `TrxPatientAllergy` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAllergy.cs` @ `f2c5090` |
| `TrxNosocomialInfection` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxNosocomialInfection.cs` @ `f2c5090` |
| `TrxPatientMedicalHistory` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientMedicalHistory.cs` @ `f2c5090` |
| `TrxPatientFamilyHistory` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientFamilyHistory.cs` @ `f2c5090` |
| `TrxPatientVitalSign` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientVitalSign.cs` @ `f2c5090` |
| `TrxPatientClinicalDocument` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientClinicalDocument.cs` @ `f2c5090` |
| `TrxPatientConsent` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientConsent.cs` @ `f2c5090` |
| `TrxMedicalCertificate` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxMedicalCertificate.cs` @ `f2c5090` |
| `TrxClinicalNoteAttachment` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxClinicalNoteAttachment.cs` @ `f2c5090` |
| `TrxPatientIntegratedProgressNote` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs` @ `f2c5090` |

### HealthServices / EmergencyInstallationManagement

Jumlah entity: 9

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `TrxEmergencyVisit` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyVisit.cs` @ `f2c5090` |
| `TrxEmergencyTriage` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs` @ `f2c5090` |
| `TrxEmergencyTriageDetail` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriageDetail.cs` @ `f2c5090` |
| `TrxEmergencyResuscitation` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyResuscitation.cs` @ `f2c5090` |
| `TrxEmergencyObservation` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyObservation.cs` @ `f2c5090` |
| `TrxEmergencyObservationDetail` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyObservationDetail.cs` @ `f2c5090` |
| `TrxEmergencyProcedureDetail` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyProcedureDetail.cs` @ `f2c5090` |
| `TrxEmergencyDisposition` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyDisposition.cs` @ `f2c5090` |
| `TrxEmergencyTransfer` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTransfer.cs` @ `f2c5090` |

### HealthServices / InPatientManagement

Jumlah entity: 11

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `InpEpisode` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs` @ `f2c5090` |
| `InpDoctorAssignment` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/InPatientManagement/Models/InpDoctorAssignment.cs` @ `f2c5090` |
| `InpNurseAssignment` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/HealthServices/InPatientManagement/Models/InpNurseAssignment.cs` @ `f2c5090` |
| `InpBedReservation` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/InPatientManagement/Models/InpBedReservation.cs` @ `f2c5090` |
| `InpBedPlacement` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs` @ `f2c5090` |
| `InpDischargeSummary` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/InPatientManagement/Models/InpDischargeSummary.cs` @ `f2c5090` |
| `InpDischargeSummaryRevision` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/InPatientManagement/Models/InpDischargeSummaryRevision.cs` @ `f2c5090` |
| `InpClearanceMark` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/InPatientManagement/Models/InpClearanceMark.cs` @ `f2c5090` |
| `InpFinancialClearance` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/HealthServices/InPatientManagement/Models/InpFinancialClearance.cs` @ `f2c5090` |
| `InpStatusHistory` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/InPatientManagement/Models/InpStatusHistory.cs` @ `f2c5090` |
| `InpCorrectionSession` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/InPatientManagement/Models/InpCorrectionSession.cs` @ `f2c5090` |

### HealthServices / LaboratoryManagement

Jumlah entity: 4

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `LabOrder` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs` @ `f2c5090` |
| `TrxLabSpecimen` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/LaboratoryManagement/Models/TrxLabSpecimen.cs` @ `f2c5090` |
| `TrxLabTransitionHistory` | Transaksi | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/LaboratoryManagement/Models/TrxLabTransitionHistory.cs` @ `f2c5090` |
| `MstLabRejectionReason` | Master | ✓ | — | ✓ | — | — | `L2` | `Areas/HealthServices/LaboratoryManagement/Models/MstLabRejectionReason.cs` @ `f2c5090` |

### HealthServices / MasterData

Jumlah entity: 34

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `MstAgeCategory` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstAgeCategory.cs` @ `f2c5090` |
| `MstServiceUnit` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs` @ `f2c5090` |
| `MstClinic` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstClinic.cs` @ `f2c5090` |
| `MstPatientClass` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstPatientClass.cs` @ `f2c5090` |
| `MstTariffCategory` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstTariffCategory.cs` @ `f2c5090` |
| `MstTariff` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstTariff.cs` @ `f2c5090` |
| `MstRoom` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstRoom.cs` @ `f2c5090` |
| `MstBed` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/MasterData/Models/MstBed.cs` @ `f2c5090` |
| `MstProcedure` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstProcedure.cs` @ `f2c5090` |
| `MstDiagnosisChapter` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDiagnosisChapter.cs` @ `f2c5090` |
| `MstDiagnosis` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDiagnosis.cs` @ `f2c5090` |
| `MstMeasurement` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstMeasurement.cs` @ `f2c5090` |
| `MstMeasurementConversion` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstMeasurementConversion.cs` @ `f2c5090` |
| `MstDrugUnitConversion` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDrugUnitConversion.cs` @ `f2c5090` |
| `MstDrugStorageLocation` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDrugStorageLocation.cs` @ `f2c5090` |
| `MstDrugStockPolicy` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDrugStockPolicy.cs` @ `f2c5090` |
| `MstDrugCategory` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDrugCategory.cs` @ `f2c5090` |
| `MstDrug` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDrug.cs` @ `f2c5090` |
| `MstInsuranceCoverageRule` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstInsuranceCoverageRule.cs` @ `f2c5090` |
| `MstInsuranceTariff` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstInsuranceTariff.cs` @ `f2c5090` |
| `MstDoctorSchedule` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDoctorSchedule.cs` @ `f2c5090` |
| `MstDoctorServiceRule` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDoctorServiceRule.cs` @ `f2c5090` |
| `MstInpatientSetting` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/MasterData/Models/MstInpatientSetting.cs` @ `f2c5090` |
| `MstInpatientClearanceItem` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/MasterData/Models/MstInpatientClearanceItem.cs` @ `f2c5090` |
| `MstDiagnosisDrugRecommendation` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDiagnosisDrugRecommendation.cs` @ `f2c5090` |
| `MstDiagnosisEducationRecommendation` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDiagnosisEducationRecommendation.cs` @ `f2c5090` |
| `MstDiagnosisProcedureRecommendation` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDiagnosisProcedureRecommendation.cs` @ `f2c5090` |
| `MstDrugSupplier` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstDrugSupplier.cs` @ `f2c5090` |
| `MstEmergencyTriageLevel` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstEmergencyTriageLevel.cs` @ `f2c5090` |
| `MstEmergencyTriageIndicator` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstEmergencyTriageIndicator.cs` @ `f2c5090` |
| `MstEmergencyArrivalMode` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstEmergencyArrivalMode.cs` @ `f2c5090` |
| `MstEmergencyCaseType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstEmergencyCaseType.cs` @ `f2c5090` |
| `MstEmergencyDispositionType` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstEmergencyDispositionType.cs` @ `f2c5090` |
| `MstEmergencySetting` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/MasterData/Models/MstEmergencySetting.cs` @ `f2c5090` |

### HealthServices / OperatingRoomManagement

Jumlah entity: 13

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `OprCase` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprCase.cs` @ `f2c5090` |
| `OprCaseProcedure` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprCaseProcedure.cs` @ `f2c5090` |
| `OprSchedule` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprSchedule.cs` @ `f2c5090` |
| `OprTeamMember` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprTeamMember.cs` @ `f2c5090` |
| `OprSafetyChecklist` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprSafetyChecklist.cs` @ `f2c5090` |
| `OprExecutionRecord` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprExecutionRecord.cs` @ `f2c5090` |
| `OprExecutionAddendum` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprExecutionAddendum.cs` @ `f2c5090` |
| `OprAnesthesiaRecord` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprAnesthesiaRecord.cs` @ `f2c5090` |
| `OprMaterialUsage` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprMaterialUsage.cs` @ `f2c5090` |
| `OprRecovery` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprRecovery.cs` @ `f2c5090` |
| `OprHandover` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprHandover.cs` @ `f2c5090` |
| `OprStatusHistory` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprStatusHistory.cs` @ `f2c5090` |
| `OprIntegrationDelivery` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/OperatingRoomManagement/Models/OprIntegrationDelivery.cs` @ `f2c5090` |

### HealthServices / PatientManagement

Jumlah entity: 7

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `MstPatient` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs` @ `f2c5090` |
| `MstPatientMembership` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientMembership.cs` @ `f2c5090` |
| `MstPatientIdentityDocument` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientIdentityDocument.cs` @ `f2c5090` |
| `MstPatientRelationship` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientRelationship.cs` @ `f2c5090` |
| `MstPatientEmergencyContact` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientEmergencyContact.cs` @ `f2c5090` |
| `MstPatientInsurance` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientInsurance.cs` @ `f2c5090` |
| `MstPatientCompanyGuarantor` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientCompanyGuarantor.cs` @ `f2c5090` |

### HealthServices / PharmacyManagement

Jumlah entity: 17

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `TrxPrescription` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs` @ `f2c5090` |
| `TrxPrescriptionItem` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionItem.cs` @ `f2c5090` |
| `TrxPrescriptionCompound` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionCompound.cs` @ `f2c5090` |
| `TrxPrescriptionCompoundItem` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionCompoundItem.cs` @ `f2c5090` |
| `MstPrescriptionTemplate` | Master | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/PharmacyManagement/Models/MstPrescriptionTemplate.cs` @ `f2c5090` |
| `MstPrescriptionTemplateItem` | Master | ✓ | — | ✓ | ✓ | — | `L2` | `Areas/HealthServices/PharmacyManagement/Models/MstPrescriptionTemplateItem.cs` @ `f2c5090` |
| `MstPrescriptionTemplateCompound` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/PharmacyManagement/Models/MstPrescriptionTemplateCompound.cs` @ `f2c5090` |
| `MstPrescriptionTemplateCompoundItem` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/PharmacyManagement/Models/MstPrescriptionTemplateCompoundItem.cs` @ `f2c5090` |
| `MstPrescriptionReviewCriterion` | Master | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/PharmacyManagement/Models/MstPrescriptionReviewCriterion.cs` @ `f2c5090` |
| `TrxPrescriptionReviewItem` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionReviewItem.cs` @ `f2c5090` |
| `TrxPrescriptionReview` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionReview.cs` @ `f2c5090` |
| `TrxPrescriptionPreparation` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionPreparation.cs` @ `f2c5090` |
| `TrxPrescriptionPreparationItem` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionPreparation.cs` @ `f2c5090` |
| `TrxPrescriptionFinalCheck` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionFinalCheck.cs` @ `f2c5090` |
| `TrxPrescriptionFinalCheckItem` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionFinalCheck.cs` @ `f2c5090` |
| `TrxPrescriptionDrugSubstitution` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionDrugSubstitution.cs` @ `f2c5090` |
| `TrxPrescriptionClarification` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionClarification.cs` @ `f2c5090` |

### HealthServices / RegistrationManagement

Jumlah entity: 4

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `TrxKioskScanSession` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/RegistrationManagement/Models/TrxKioskScanSession.cs` @ `f2c5090` |
| `TrxPatientEncounter` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs` @ `f2c5090` |
| `TrxPatientEncounterGuarantor` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs` @ `f2c5090` |
| `TrxQueue` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/HealthServices/RegistrationManagement/Models/TrxQueue.cs` @ `f2c5090` |

