# Radiologi — Existing Capability Map

| Field | Value |
|---|---|
| Blueprint ID | `radiologi` |
| Revision | `1` |
| Status | `draft` |
| Jenis audit | Audit penuh (bukan `impact-scan`) |
| Backend SHA | `64da911` |
| Frontend SHA | `f66ed1885` |
| Decision yang berlaku | `RAD-DEC-001`, `RAD-DEC-002`, `RAD-DEC-003`, `RAD-DEC-004` (`00-interview-decisions.md` revision 4) |
| Decision warisan | `RJ-BIL-GATE-DEC-004`, `RJ-BIL-DEC-014`, `IGD-DEC-099` |
| Contract version yang berlaku | `BIL-INTEGRATION-0.4` (kontrak fakta klinis ke Billing) |
| Tanggal audit | 2026-09-09 |
| Task mode | `AUDIT MODE` untuk source, `MODULE BLUEPRINT MODE` untuk penulisan dokumen |

> **Sifat dokumen ini.** Ini laporan audit **read-only**. Tidak ada satu baris source aplikasi
> pun yang diubah, tidak ada build, dan tidak ada perbaikan kode. Dokumen ini juga **bukan**
> arsitektur target dan **bukan** rencana kerja.
>
> Setiap klaim tentang kode disertai bukti berformat `repository/path#symbol@SHA` supaya bisa
> diperiksa ulang oleh siapa pun.

---

## Batas Audit

**Yang diaudit.** Seluruh kemampuan yang menyentuh perjalanan pemeriksaan radiologi di kedua
repository, dibatasi oleh `RAD-DEC-001` dan `RAD-DEC-002`.

Klaster yang ditelusuri:

| Klaster | Yang dicari |
|---|---|
| Identity/Master Owner | Data induk alat, butir keselamatan, prosedur, tarif |
| Episode/Transaction Owner | Kunjungan pasien, perawatan rawat inap |
| Actor/Workforce | Siapa yang mengerjakan dan mengesahkan |
| Location/Resource | Alat sebagai sumber daya, unit layanan |
| Workflow/Status | Siklus hidup pesanan dan study, riwayat perpindahan |
| Documentation/Record | Tempat hasil radiologi di rekam medis |
| Order/Result | Pesanan, pengambilan citra, hasil bacaan |
| Financial | Penerbitan fakta kelayakan tagih ke Billing |
| Authorization/Audit | Hak akses per endpoint, jejak audit, uji kontrak |
| External Integration | RIS, PACS, DICOM |

**Yang tidak diaudit,** karena berada di luar scope menurut `RAD-DEC-001` dan `RAD-DEC-002`:
perhitungan tarif dan tagihan, stok kontras dan film, kedokteran nuklir, radioterapi,
pemantauan dosis radiasi petugas, serta penyimpanan berkas citra.

---

## Ringkasan Hasil

34 kemampuan diperiksa. Hasilnya:

| Status | Jumlah | Artinya bagi pembaca umum | ID |
|---|---:|---|---|
| `Ready to reuse` | 18 | Sudah ada, terbukti bekerja, tinggal dipakai | `004`–`007`, `009`, `014`–`017`, `019`–`024`, `026`, `032`, `034` |
| `Reuse with adapter` | 3 | Sudah ada tetapi perlu penyesuaian | `018`, `030`, `031` |
| `Extend` | 1 | Sudah ada tetapi perlu ditambah kemampuannya | `008` |
| `Repair` | 3 | Sudah ada tetapi tidak lengkap sehingga tidak bisa dipakai | `001`–`003` |
| `Missing` | 7 | Belum ada sama sekali | `010`–`013`, `025`, `028`, `029` |
| `Conflict` | 2 | Dua sumber saling bertentangan, butuh keputusan manusia | `027`, `033` |
| `Unknown` | 0 | — | — |

Angka `Ready to reuse` yang besar itu jangan dibaca sebagai "modul hampir selesai". Yang sudah
matang adalah **mesin pemeriksaannya**. Yang belum ada justru bagian yang membuat pemeriksaan
itu berguna bagi pasien: hasil bacaan, dan seluruh tampilan.

### Tiga temuan yang paling menentukan

**Pertama — modul ini belum bisa dipakai sama sekali, walau kodenya lengkap.**
Gerbang keselamatan bersifat *fail-closed*: bila tidak ada satu pun aturan keselamatan aktif
untuk sebuah alat, pengambilan citra **ditolak**. Itu perilaku yang benar dan memang diminta
`RJ-BIL-DEC-014`. Masalahnya, **tidak ada satu pun cara untuk memasukkan aturan itu** — tidak
ada endpoint pengelolaan, tidak ada layar, dan tidak ada *seeder*. Akibatnya setiap upaya
pengambilan citra akan ditolak dengan pesan "Aturan keselamatan untuk modalitas ini belum
ditetapkan". Modul ini terkunci dari dalam.

**Kedua — registry kepemilikan bertentangan dengan keputusan yang sudah disetujui dan dengan
kode yang sudah rilis.** `RJ-BIL-DEC-014` (disetujui 28 Agustus 2026) memutuskan menaikkan
modul Radiologi dari `PLANNED` menjadi `ACTIVE`. Kode Billing bahkan menulis bahwa kenaikan itu
sudah terjadi. Tetapi berkas registry — di repository backend **maupun** di suite Skill — masih
tertulis `PLANNED`, dan tidak ada satu pun entri riwayat perubahan untuk Radiologi. Modul
Laboratorium yang diperlakukan dengan pola sama justru punya entri riwayatnya.

**Ketiga — modul IGD masih memberi tahu penggunanya bahwa modul Radiologi belum ada.**
Kode dan teks di layar IGD menyatakan pemesanan radiologi harus ditempuh di luar sistem.
Pernyataan itu benar ketika ditulis 27 Agustus 2026, tetapi modul Radiologi rilis 31 Agustus
2026. Sejak itu pernyataannya menjadi salah dan tidak pernah diperbarui.

---

## Tabel Kontrak Bukti Kemampuan

Singkatan repository: `BE` = `NewQuilvianSystemBackend`, `FE` = `QuilvianSystemFrontendDev`.

### Klaster Identity/Master Owner

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `RAD-CAP-001` | Kelola data induk alat pencitraan | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Models/MstRadModality.cs@64da911`; hanya dibaca lewat `BE/Areas/HealthServices/RadiologyManagement/Controllers/RadStudyController.cs#GetModalities:44@64da911` | `Repair` | Tabel dan endpoint baca sudah ada, tetapi tidak ada endpoint tambah/ubah/nonaktif dan tidak ada seeder | Alat baru hanya bisa didaftarkan lewat database langsung |
| `RAD-CAP-002` | Kelola data induk butir keselamatan | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Models/MstRadSafetyRequirement.cs@64da911`; dibaca lewat `RadStudyController.cs#GetSafetyRequirements:56@64da911` | `Repair` | Sama seperti di atas | Butir keselamatan tidak bisa disesuaikan tanpa rilis, padahal `RJ-BIL-DEC-014` mewajibkan sebaliknya |
| `RAD-CAP-003` | Kelola aturan keselamatan per alat | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Models/MstRadModalitySafetyRule.cs@64da911`; konfigurasi `BE/Repositories/Configurations/HealthServices/RadiologyManagement/MstRadModalitySafetyRuleConfiguration.cs@64da911` | `Repair` | **Tidak ada endpoint pengelolaan sama sekali.** Digabung dengan sifat fail-closed, seluruh pengambilan citra tertolak | **Tertinggi.** Modul tidak dapat menjalankan satu pun pemeriksaan |
| `RAD-CAP-004` | Katalog prosedur dan tarif bersama | MasterData | `BE/Areas/HealthServices/MasterData/Models/MstProcedure.cs@64da911`; dipakai `BE/Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs#ProcedureId@64da911` | `Ready to reuse` | Tidak ada | Rendah |

### Klaster Episode/Transaction Owner

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `RAD-CAP-005` | Menempel pada kunjungan pasien | RegistrationManagement | `BE/Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs#EncounterId@64da911` menuju `TrxPatientEncounter` | `Ready to reuse` | Tidak ada | Rendah |
| `RAD-CAP-006` | Konteks perawatan rawat inap | InPatientManagement | `BE/Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs#InpEpisodeId@64da911`; migration `BE/Migrations/20260903095444_AddRadOrderInpatientContext.cs@64da911` | `Ready to reuse` | Boleh kosong untuk pasien rawat jalan | Rendah |

### Klaster Order/Result

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `RAD-CAP-007` | Siklus hidup pesanan radiologi | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Services/RadOrderService.cs@64da911`; 12 endpoint pada `RadOrderController.cs@64da911` | `Ready to reuse` | Tidak ada | Rendah |
| `RAD-CAP-008` | Dokter menyimpan draf pesanan sebelum dikirim | RadiologyManagement | Status tersedia di `Enums/RadiologyEnums.cs#RadOrderStatus.Draft@64da911`, tetapi pesanan selalu lahir `Requested` di `RadOrderService.cs:226@64da911`; `Draft` hanya muncul sebagai status yang boleh dibatalkan di `RadOrderService.cs:305@64da911` | `Extend` | Tidak ada endpoint yang menghasilkan `Draft` | Sedang. Siklus hidup terkunci `RJ-BIL-GATE-DEC-004` menyebut `Draft → Requested`; kenyataannya `Draft` tidak pernah terjadi |
| `RAD-CAP-009` | Siklus hidup study dan pengambilan citra | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Services/RadStudyService.cs@64da911`; 14 endpoint pada `RadStudyController.cs@64da911` | `Ready to reuse` | Tidak ada | Rendah |
| `RAD-CAP-010` | Hasil bacaan dokter radiolog | RadiologyManagement | Pencarian menyeluruh `*.cs@64da911` tidak menemukan `RadReport` maupun padanannya | `Missing` | Seluruh siklus `Pending → Drafted → Validated → Released` belum ada | **Tertinggi.** Inti Rilis 1 menurut `RAD-DEC-001` |
| `RAD-CAP-011` | Koreksi hasil berversi setelah rilis | RadiologyManagement | Tidak ada; bergantung `RAD-CAP-010` | `Missing` | Seluruh siklus `AmendmentDrafted → AmendmentValidated → AmendmentReleased` belum ada | Tinggi |
| `RAD-CAP-012` | Temuan kritis dan pemberitahuannya | RadiologyManagement | Tidak ada penanda kritis, kotak pemberitahuan, maupun pencatatan kontak pada `*.cs@64da911` | `Missing` | Seluruh mekanisme `RAD-DEC-004` belum ada | Tinggi. Menyangkut keselamatan pasien |
| `RAD-CAP-013` | Daftar kerja petugas radiologi | RadiologyManagement | Tidak ada endpoint daftar kerja; `RadOrderController#GetList@64da911` adalah daftar umum berhalaman, bukan antrian kerja per alat atau per petugas | `Missing` | Perlu endpoint antrian tersendiri | Sedang |

### Klaster Workflow/Status

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `RAD-CAP-014` | Riwayat perpindahan status yang tidak bisa diubah | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Models/RadTransitionHistory.cs@64da911`; ditulis lewat `RadStudyService.cs#AddHistory@64da911` | `Ready to reuse` | Tidak ada | Rendah |
| `RAD-CAP-015` | Gerbang keselamatan yang menolak bila belum diatur | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Services/RadSafetyGateEvaluator.cs#Evaluate@64da911` mengembalikan `PolicyConfigured: false, Cleared: false` ketika tidak ada aturan | `Ready to reuse` | Tidak ada. Logikanya murni dan dapat diuji tanpa database | Rendah sebagai logika, **tetapi lihat `RAD-CAP-003`** — tanpa data aturan, gerbang ini memblokir semuanya |
| `RAD-CAP-016` | Pencegahan dua petugas mengubah data yang sama | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs#Version@64da911` dan `RadStudy.cs#Version@64da911`; dijaga `RadStudyService.cs#SaveWithConcurrencyGuardAsync@64da911` | `Ready to reuse` | Tidak ada | Rendah |
| `RAD-CAP-017` | Pembekuan versi aturan keselamatan saat lolos | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Models/RadStudy.cs#SafetyRuleVersionAtClearance@64da911` | `Ready to reuse` | Tidak ada | Rendah |

### Klaster Documentation/Record

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `RAD-CAP-018` | Tempat hasil radiologi di rekam medis | ClinicalManagement | `BE/Areas/HealthServices/ClinicalManagement/Enums/PatientClinicalDocumentSource.cs#Radiology=5@64da911`; `PatientClinicalDocumentType.cs#RadiologyResult=2@64da911` | `Reuse with adapter` | Slotnya sudah ada, tetapi pencarian menyeluruh menunjukkan **belum ada satu pun kode yang mengisinya** | Sedang. Perlu disambungkan saat `RAD-CAP-010` dibangun |
| `RAD-CAP-019` | Larangan menyalin hasil penunjang ke modul lain | InPatientManagement | Uji arsitektur `BE/Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/InpatientSupportingOrderTests.cs#RawatInapTidakMemilikiSatuPunTabelSalinanHasilPenunjang@64da911` melarang nama tabel `RadResult` dan `RadReportCopy` di luar RadiologyManagement | `Ready to reuse` | Tidak ada | Rendah. Justru melindungi desain hasil bacaan nanti |

### Klaster Financial

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `RAD-CAP-020` | Menerbitkan fakta kelayakan tagih ke Billing | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Services/RadStudyService.cs#EmitChargeEligibilityAsync:930-960@64da911`, dipicu hanya ketika citra dinyatakan dapat dipakai di `RadStudyService.cs#DecideQualityAsync:517@64da911` | `Ready to reuse` | Tidak ada | Rendah |
| `RAD-CAP-021` | Pendaftaran Radiologi sebagai sumber sah di Billing | BillingManagement | `BE/Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs#RadiologySourceContext:42@64da911`; kebijakan siklus di `BillingChargeSourceAdapter.cs#SourcePolicies["RADIOLOGY"]:31@64da911`, contract version `BIL-INTEGRATION-0.4` | `Ready to reuse` | Tidak ada | Rendah |
| `RAD-CAP-022` | Pencegahan pengiriman fakta tagih ganda | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Models/RadStudy.cs#BillingFactSubmitted@64da911`, diisi di `RadStudyService.cs:521@64da911` | `Ready to reuse` | Tidak ada | Rendah |
| `RAD-CAP-023` | Radiologi tidak punya wewenang finansial | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs@64da911` — tidak memuat satu pun kolom `Paid`, `Settlement`, `Void`, `Refund`, atau `Reversal` | `Ready to reuse` | Tidak ada. Invariant ditegakkan dengan cara meniadakan kolomnya | Rendah |

### Klaster Authorization/Audit

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `RAD-CAP-024` | Hak akses per endpoint | RadiologyManagement | 26 atribut `[AccessPermission(...)]` pada `RadOrderController.cs@64da911` dan `RadStudyController.cs@64da911`, mekanisme di `BE/Attributes/AccessPermissionAttribute.cs@64da911` | `Ready to reuse` | Tidak ada | Rendah |
| `RAD-CAP-025` | Uji kontrak hak akses radiologi | RadiologyManagement | **Tidak ada.** Bandingkan `BE/Tests/QuilvianSystemBackend.IntegrationTests.Postgres/Laboratory/LaboratoryAuthorityTests.cs@64da911` dan `BE/Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/MasterData/BloodBankRoleAccessContractTests.cs@64da911` yang dimiliki modul sebanding | `Missing` | Modul Laboratorium dan Bank Darah punya, Radiologi tidak | Sedang. Perubahan hak akses tidak akan ketahuan bila salah |
| `RAD-CAP-026` | Identitas pelaku diambil dari sesi, bukan dari kiriman | RadiologyManagement | `BE/Areas/HealthServices/RadiologyManagement/Services/RadStudyService.cs#GetCurrentUserId:962-973@64da911` menolak tindakan bila identitas tidak dapat ditentukan | `Ready to reuse` | Tidak ada | Rendah |
| `RAD-CAP-027` | Registry kepemilikan modul dan prefix `Rad` | Pemegang tata kelola | `BE/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:22@64da911` tertulis `PLANNED`; salinan di suite Skill `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:22` juga `PLANNED`; sedangkan `BE/Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs:11-13@64da911` menulis registry sudah dinaikkan ke `ACTIVE` | `Conflict` | Lihat bagian Conflict di bawah | **Tertinggi.** Menahan pembuatan entity `Rad*` baru berdasarkan `QBE-MOD-002` |

### Klaster External Integration

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `RAD-CAP-028` | Penyimpanan citra dan pertukaran DICOM | Belum ada pemilik | Tempat penampung ada tetapi tidak dipakai: `BE/Areas/HealthServices/RadiologyManagement/Models/RadStudy.cs#ExternalStudyUid@64da911` dengan keterangan "Tidak dipakai sekarang" | `Missing` | **Disengaja.** `RJ-BIL-GATE-DEC-004` menyatakan integrasi RIS/PACS tidak diaktifkan, dan `RAD-DEC-001` menempatkannya di luar scope | Rendah selama tetap di luar scope |

### Klaster Frontend

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `RAD-CAP-029` | Halaman, service, dan state Radiologi | QuilvianSystemFrontendDev | Pencarian menyeluruh `FE/src@f66ed1885` tidak menemukan satu pun pemanggilan `rad-orders`, `rad-studies`, atau `radiology-management` | `Missing` | Seluruhnya dibangun dari nol | Tinggi karena volumenya besar, tetapi tidak berisiko merusak yang ada |
| `RAD-CAP-030` | Pola frontend penunjang yang sudah terbukti | QuilvianSystemFrontendDev | Modul Laboratorium lengkap di `FE/src/app/health-services/laboratory-management/@f66ed1885` (6 halaman), `FE/src/lib/services/health-services/laboratory-management/@f66ed1885` (7 service), `FE/src/lib/state/slice/health-services/laboratory-management/@f66ed1885` (6 slice) | `Reuse with adapter` | Struktur, penamaan, dan aliran datanya dapat ditiru langsung untuk Radiologi | Rendah. Justru mempercepat |
| `RAD-CAP-031` | Wadah menu Radiologi di sidebar | QuilvianSystemFrontendDev | Kunci `menuRadiologi` sudah terdaftar di `FE/src/components/features/left-sidebar/left-sidebar-menu-handle.jsx:15@f66ed1885` | `Reuse with adapter` | Kuncinya ada, tetapi tidak ada satu pun data menu yang memakainya. Bukan menu mati, melainkan wadah kosong | Rendah |
| `RAD-CAP-032` | Penyaringan hak akses area Health Services | QuilvianSystemFrontendDev | Kata kunci `"radiology"` sudah terdaftar di `FE/src/components/view/administrator/settings/administrator-role-access-view.jsx:61@f66ed1885` | `Ready to reuse` | Tidak ada | Rendah |

### Klaster Lintas Modul

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `RAD-CAP-033` | IGD memesan radiologi lewat sistem | EmergencyInstallationManagement | `BE/Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyOrderKind.cs#RadiologyOrder=4@64da911` masih berketerangan "modul Radiologi belum ada, sehingga pesanannya dibuat di luar sistem"; teks yang sama tampil ke pengguna di `FE/src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-diagnostic-support-tab.jsx:189@f66ed1885`; tidak ada satu pun rujukan `RadOrder` di seluruh folder IGD backend | `Conflict` | Lihat bagian Conflict di bawah | Tinggi. Pengguna IGD diberi tahu hal yang tidak lagi benar |
| `RAD-CAP-034` | Uji perilaku radiologi | RadiologyManagement | `BE/Tests/QuilvianSystemBackend.IntegrationTests.Postgres/Radiology/RadiologySafetyGateTests.cs@64da911` dan `RadiologyStudyLifecycleTests.cs@64da911` | `Ready to reuse` | Tidak ada | Rendah. Catatan: `AGENTS.md` masih menyatakan project test belum terdeteksi — pernyataan itu sudah usang |

---

## Kontrak As-Is

Bagian ini mencatat kontrak yang **benar-benar berlaku sekarang**, bukan yang direncanakan.
Daftar endpoint lengkap bergaya Swagger sudah tercatat pada `00-interview-decisions.md`
bagian `RAD-FACT-004` dan tidak diulang di sini.

### Kontrak fakta klinis ke Billing

| Butir | Nilai as-is | Bukti |
|---|---|---|
| Nama sumber | `Radiology` | `BillingSourceContract.cs:42@64da911` |
| Jenis efek yang sah | `RadiologyCharge` | `BillingSourceContract.cs:43@64da911` |
| Versi kontrak | `BIL-INTEGRATION-0.4` | `BillingChargeSourceAdapter.cs:18@64da911` |
| Satuan fakta | **Satu fakta per study**, bukan per pesanan | `BillingSourceContract.cs:34-41@64da911` |
| Pemicu | Hanya ketika mutu citra dinyatakan dapat dipakai | `RadStudyService.cs#DecideQualityAsync:517@64da911` |
| Isi rekaman | Nomor study, urutan, apakah pengulangan, sebab pengulangan, order tambahan, versi aturan keselamatan | `RadStudyService.cs:945-955@64da911` |

**Contoh nyata.** Tn. B menjalani CT-Scan. Percobaan pertama gerakan pasien membuat citranya
kabur, dinilai tidak dapat dipakai. Percobaan kedua berhasil. Yang terjadi:

1. Study pertama berakhir `QualityRejected`. **Tidak ada** fakta tagih diterbitkan.
2. Study kedua dibuat sebagai pengulangan, dengan `RepeatOfStudyId` menunjuk study pertama dan
   `RepeatCause` bernilai `PatientCondition`.
3. Study kedua berakhir `QualityAccepted`. **Satu** fakta tagih diterbitkan, membawa keterangan
   bahwa ini pengulangan beserta sebabnya.
4. Billing yang memutuskan apakah pengulangan itu ditagihkan kepada pasien. Radiologi tidak
   ikut menentukan.

### Perilaku gerbang keselamatan as-is

| Keadaan | Hasil | Pesan yang muncul |
|---|---|---|
| Tidak ada aturan aktif untuk alat tersebut | **Ditolak** | "Aturan keselamatan untuk modalitas ini belum ditetapkan, sehingga acquisition tidak dapat dijalankan. Hubungi admin Radiologi untuk menetapkan aturannya lebih dulu." |
| Ada aturan, butir wajib belum dijawab | **Ditolak** | "Gerbang keselamatan wajib belum dijawab: `<kode butir>`" |
| Ada aturan, butir wajib dijawab tidak aman | **Ditolak** | "Gerbang keselamatan wajib dinyatakan tidak aman: `<kode butir>`" |
| Butir tidak wajib dijawab tidak aman | **Diloloskan** | — |
| Semua butir wajib `Passed` atau `NotApplicable` | **Diloloskan** | — |

Bukti: `RadSafetyGateEvaluator.cs#Evaluate@64da911` dan `#DescribeBlockage@64da911`.

---

## Conflict yang Butuh Keputusan Manusia

### `RAD-CONF-001` — Registry mencatat `PLANNED`, keputusan dan kode mengatakan `ACTIVE`

| Sumber | Isinya | Bukti |
|---|---|---|
| Keputusan yang disetujui | Menaikkan `RadiologyManagement`/`Rad` dari `PLANNED` ke `ACTIVE`, disetujui Sukma Giri 28 Agustus 2026 | `BE/docs/module-blueprints/rawat-jalan/00-interview-decisions.md#RJ-BIL-DEC-014:104@64da911` |
| Kode aplikasi | Menyatakan kenaikan itu **sudah terjadi** | `BE/Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs:11-13@64da911` |
| Registry di backend | Masih `PLANNED`, tanpa entri riwayat perubahan | `BE/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:22@64da911` |
| Registry di suite Skill | Juga masih `PLANNED` | `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:22` |
| Pembanding | Modul Laboratorium mendapat entri riwayat `PLANNED → ACTIVE` tertanggal 2026-09-02 | `BE/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:104@64da911` |

**Akibatnya sekarang.** Aturan `QBE-MOD-002` menahan pembuatan entity operasional baru untuk
modul berstatus `PLANNED`. Karena hasil bacaan (`RAD-CAP-010`) memerlukan tabel `Rad*` baru,
inti Rilis 1 **tidak dapat dimulai** selama konflik ini terbuka.

**Yang menarik dan perlu dijawab manusia:** delapan tabel `Rad*` yang sudah rilis dibuat
justru **ketika** registry masih `PLANNED`. Perlu diputuskan apakah itu pelanggaran yang harus
dicatat sebagai pengecualian, atau memang registry-nya yang lupa diperbarui.

**Bukan wewenang modul Radiologi.** Perbaikan registry milik pemegang berkas tata kelola.

### `RAD-CONF-002` — IGD masih menyatakan modul Radiologi belum ada

| Sumber | Isinya | Bukti |
|---|---|---|
| Keputusan IGD | Pemesanan radiologi ditunda sampai pemilik `RadiologyManagement` ditunjuk; sementara itu dipesan di luar sistem | `BE/docs/module-blueprints/igd/00-interview-decisions.md#IGD-DEC-099:3549@64da911`, status `draft`, jawaban pengguna 26 Agustus 2026 |
| Kode IGD | `RadiologyOrder = 4` diberi keterangan "modul Radiologi belum ada" | `BE/Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyOrderKind.cs@64da911`, commit `f75ea039` tertanggal 2026-08-27 |
| Teks ke pengguna | "Pemeriksaan radiologi juga belum dapat dipesan lewat sistem — modul Radiologi belum ada" | `FE/.../emergency-assessment-diagnostic-support-tab.jsx:189@f66ed1885` |
| Kenyataan | Modul Radiologi rilis lewat commit `2d855803` tertanggal 2026-08-31, lengkap dengan `POST /rad-orders` | `BE/Migrations/20260828093000_AddRadiologyManagement.cs@64da911` |
| Prasyarat yang sudah gugur | `IGD-DEC-099` menunggu penunjukan pemilik; `RJ-BIL-DEC-014` sudah menunjuk Sukma Giri pada 28 Agustus 2026 | `RJ-BIL-DEC-014@64da911` |

**Urutan kejadiannya:**

| Tanggal | Kejadian |
|---|---|
| 26 Agustus 2026 | Pengguna memutuskan pemesanan radiologi IGD ditunda (`IGD-DEC-099`) |
| 27 Agustus 2026 | Kode IGD ditulis dengan keterangan "modul Radiologi belum ada" |
| 28 Agustus 2026 | Pemilik `RadiologyManagement` ditunjuk (`RJ-BIL-DEC-014`) — prasyarat `IGD-DEC-099` gugur |
| 31 Agustus 2026 | Modul Radiologi rilis, `POST /rad-orders` tersedia |
| 9 September 2026 | Teks IGD masih menyatakan modul belum ada |

**Akibatnya sekarang.** Perawat dan dokter IGD diberi tahu untuk memesan radiologi di luar
sistem, padahal jalurnya sudah ada. Pesanan itu tercatat sebagai pesanan `External` sehingga
tidak menghasilkan study, tidak melewati gerbang keselamatan, dan tidak menerbitkan fakta
tagih.

**Bukan wewenang modul Radiologi.** Perbaikan teks dan penyambungan milik pemilik modul IGD.

---

## Unknown

Tidak ada. Seluruh pertanyaan yang diajukan audit ini dapat dijawab dari source.

Perlu dicatat: audit ini **tidak** memeriksa keadaan database yang sedang berjalan. Apakah
tabel `MstRadModalitySafetyRule` di database pengembangan sudah berisi data atau masih kosong
**tidak dapat dipastikan dari source**, dan pemeriksaannya memerlukan wewenang akses database
yang terpisah. Yang dapat dipastikan dari source hanyalah bahwa tidak ada seeder maupun
endpoint yang mengisinya.

---

## Pemicu Impact Scan

Peta ini menjadi **stale** dan wajib di-scan ulang secara terbatas bila salah satu terjadi:

| Pemicu | Yang harus diperiksa ulang |
|---|---|
| Backend berpindah dari `64da911` | Seluruh klaster yang menyentuh `Areas/HealthServices/RadiologyManagement/` |
| Frontend berpindah dari `f66ed1885` | `RAD-CAP-029` sampai `RAD-CAP-032` |
| `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` berubah | `RAD-CAP-027` dan `RAD-CONF-001` |
| `BillingChargeSourceAdapter.ContractVersion` bergeser dari `BIL-INTEGRATION-0.4` | `RAD-CAP-020` sampai `RAD-CAP-022` |
| Modul IGD menyentuh `EmergencyOrderKind` | `RAD-CAP-033` dan `RAD-CONF-002` |
| Modul Laboratorium mengubah struktur frontend-nya | `RAD-CAP-030` |

---

## Closure Question untuk `/grill-me`

Pertanyaan berikut **tidak dijawab** oleh audit ini. Semuanya memerlukan keputusan manusia dan
harus dibawa ke `Closure pass`.

| ID | Pertanyaan | Pemilik yang dibutuhkan | Memblokir |
|---|---|---|---|
| `RAD-CQ-001` | Delapan tabel `Rad*` sudah rilis padahal registry masih `PLANNED`. Apakah ini dicatat sebagai pengecualian yang disahkan, atau registry yang diperbaiki dan diberi entri riwayat susulan? | Pemegang registry + pemilik modul | `IMPLEMENTATION` seluruh entity `Rad*` baru, termasuk hasil bacaan |
| `RAD-CQ-002` | Siapa yang berwenang mengelola aturan keselamatan per alat, dan apakah perubahannya perlu persetujuan sebelum berlaku? Tanpa jawaban ini `RAD-CAP-003` tidak dapat dirancang, dan tanpa `RAD-CAP-003` modul tidak dapat menjalankan satu pun pemeriksaan | Clinical Governance + pemilik modul | `DESIGN` data induk keselamatan |
| `RAD-CQ-003` | Nilai awal aturan keselamatan diisi dari mana — disiapkan tim sebagai data awal, atau seluruhnya diketik admin rumah sakit sendiri? `RJ-BIL-DEC-014` menyebut baseline standardisasi Indonesia bersifat tidak otoritatif dan wajib diverifikasi terhadap SOP rumah sakit yang sebenarnya | Clinical Governance | `IMPLEMENTATION` data awal |
| `RAD-CQ-004` | Apakah dokter perlu dapat menyimpan draf pesanan sebelum mengirim, atau status `Draft` memang sengaja tidak dipakai dan sebaiknya dihapus dari daftar status? | Pemilik modul | `DESIGN` siklus hidup pesanan |
| `RAD-CQ-005` | Siapa yang memperbaiki `RAD-CONF-002`, dan apakah pesanan radiologi IGD yang sudah terlanjur tercatat sebagai `External` perlu dipindahkan ke jalur resmi atau dibiarkan sebagai riwayat? | Pemilik modul IGD + pemilik modul Radiologi | `DESIGN` titik sentuh IGD |
| `RAD-CQ-006` | Hasil bacaan yang sudah dirilis masuk ke rekam medis lewat `TrxPatientClinicalDocument` sumber `Radiology`, atau dibaca langsung dari modul Radiologi? Uji arsitektur `RAD-CAP-019` melarang penyalinan, tetapi tidak menentukan mana dari dua cara ini yang dipakai | Pemilik modul Rekam Medis + pemilik modul Radiologi | `DESIGN` hasil bacaan |
| `RAD-CQ-007` | Apakah uji kontrak hak akses radiologi (`RAD-CAP-025`) wajib ada sebelum Rilis 1, mengikuti pola Laboratorium dan Bank Darah? | Pemilik modul | `IMPLEMENTATION` — dapat ditunda bila diputuskan demikian |

---

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-09 | Audit penuh pertama pada BE `64da911` dan FE `f66ed1885`. 30 kemampuan diklasifikasikan, 2 conflict dan 7 closure question diterbitkan. | `draft` |
