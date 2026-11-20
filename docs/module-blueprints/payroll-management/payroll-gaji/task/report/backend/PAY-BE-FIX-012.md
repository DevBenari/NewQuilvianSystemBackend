# Laporan Audit dan Desain Backend — `PAY-BE-FIX-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-012` (ad-hoc lanjutan seri `PAY-BE-FIX`); pasangan frontend `PAY-FE-FIX-012`. ID `011` dipakai pekerjaan pihak lain yang berjalan paralel |
| Judul | Penyederhanaan arsitektur Tunjangan Pegawai: Transportasi menjadi jenis tunjangan biasa, Kebijakan Tunjangan per Grade Gaji, dropdown bertingkat, penetapan individual dan massal |
| Slice | Corporate / Payroll Management (`Prl`) dan Human Resource / Master Data / Payroll and Benefit (`Mst`) |
| Roadmap | Tidak ada baris roadmap untuk seri ad-hoc; tidak ada roadmap atau traceability yang diubah (`NOT APPLICABLE`) |
| Klasifikasi | **`EPIC`**. Dua repository, perancangan ulang arsitektur dan database, banyak berkas, jalur uang. Aturan klasifikasi mewajibkan `STOP → DECOMPOSE`: tugas tidak dikerjakan sebagai satu perubahan, tetapi dipecah per fase (bagian 12) |
| Task mode | `CROSS-REPO` |
| Target tulis | Backend: **tidak ada source yang ditulis**. Frontend: satu berkas modal (lihat `PAY-FE-FIX-012`) |
| Commit saat dikerjakan | Backend `d642a9df9de35801ed8332adc0cdcd6127f96353` (`Yasmina`); frontend `31df72b9d907796eb106c638a2a350405c866efa` (`yasmina`). Working tree **tidak bersih**: berisi pekerjaan `PAY-BE/FE-FIX-010` dan pekerjaan pihak lain |
| Model | Claude Sonnet 5.5 |
| Tanggal | 20 November 2026 |
| Status | ⛔ **Inti tugas `BLOCKED_BY_SCHEMA`.** Audit dan desain selesai; satu perbaikan frontend kecil selesai (`PAY-FE-FIX-012`). Tidak ada build, migration, atau uji runtime |

**Backend Governance Preflight.** Area `Corporate`; Module `PayrollManagement` (prefix legacy `Prl`) dan `HumanResource/MasterData/PayrollAndBenefit` (`Mst`). Keberlakuan `TOUCHED LEGACY` untuk perubahan yang ada, `NEW CODE` untuk entity yang dirancang. **Koreksi 20 November 2026:** versi awal laporan ini menyatakan registry tidak memuat baris `Prl` dan entity baru `BLOCKED BY QBE-MOD-002`. **Pernyataan itu keliru.** Registry yang dibaca pemeriksa QBE (`NewQuilvianSystemBackend/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) sudah memuat `Corporate | PayrollManagement / Payroll | BUSINESS DOMAIN / MODULE | Prl | ACTIVE` sejak 2026-11-11 (entri changelog baris 163). Saya semula hanya membaca salinan di repository skills yang tertinggal. Pembuatan entity `Prl*` **tidak** terblokir oleh registry (bagian 3.5).

---

## Pemisahan status pekerjaan

| Kategori | Isi |
| --- | --- |
| **Sudah diimplementasikan** | Perbaikan tombol Batal pada modal "Tambah Tunjangan Pegawai" (frontend). Audit penuh alur tunjangan |
| **Baru berupa desain** | Entity kebijakan dan tarif per grade, tautan penetapan ke kebijakan, batch bulk, API terpadu, rencana penghapusan jalur Transportasi, matriks rutin/insidental/aktivitas, strategi anti-ganda |
| **Belum diverifikasi** | Seluruh perilaku runtime, build, tampilan, jalur impor lembur, hak akses menurut data role sungguhan |
| **Terblokir karena schema** | Kebijakan multi-grade, nested dropdown (dropdown kedua), bulk assignment, pelepasan Transportasi (bagian 11) |
| **Membutuhkan keputusan lanjutan** | Bagian 14 |

---

## 1. Analisis proses bisnis existing

### 1.1 Master Tunjangan dan aturan kelayakan

`MstAllowanceType` adalah sumber jenis tunjangan. `AllowanceTypeController.GetAllowanceTypeOptions` mengembalikan metadata `IsEligibleForWorkforceAssignment`, `IsPolicyBased`, `PolicySource` (`AllowanceTypeController.cs` baris 331-335; DTO `AllowanceTypeDtos.cs` baris 74-78).

`WorkforceAllowanceEligibilityRules` memutuskan apakah satu jenis boleh ditetapkan langsung kepada pegawai. Jenis **layak** hanya bila: aktif, `IsRecurring`, metode `Fixed`/`Percentage`, kategori **bukan** `Shift`/`OnCall`/`Hazard`/`Transport`/`Variable`, tidak terhubung kebijakan Shift/On-call/Hazard aktif, dan komponen payroll-nya aktif serta **tidak** terhubung kebijakan transport aktif. Alasan penolakan yang dikembalikan ke layar:

| Keadaan master | Alasan yang dilihat petugas | Jalur yang benar |
| --- | --- | --- |
| Metode `ManualInput` atau tidak `IsRecurring` | "bersifat input per periode ... lewat Penghasilan Variabel pada Payroll Run" | Payroll Run → Penghasilan Variabel |
| Metode `PolicyBased` atau kategori kebijakan | "dihitung dari kebijakan ... Kelola lewat kebijakannya" | Modul kebijakannya |
| Punya kebijakan aktif terhubung | "agar tidak terhitung ganda" | Modul kebijakannya |

### 1.2 Tunjangan pegawai individual

`PrlWorkforceAllowance` (kunci `WorkforceProfileId` + `AllowanceTypeId`): nominal atau persen, snapshot syarat master (`IsTaxable`, `IsProrated`, `RequiresApproval`, `MaximumAmount`), masa berlaku, dan siklus `Draft → PendingApproval → Approved | Rejected | Inactive`. **Hanya `Approved` yang dihitung mesin payroll.** Model **tidak punya** relasi ke kebijakan mana pun (tidak ada `PolicyId`, tidak ada sumber nominal).

`PayrollWorkforceAllowancesController` (`WorkforceAllowanceAssignment`): `GET` daftar, `GET summary`, `GET {id}`, `GET {id}/amount` (izin `ViewAmount`), `POST`, `PUT`, `POST submit/approve/reject/deactivate`. **Tidak ada endpoint bulk.**

### 1.3 Jalur Transportasi khusus

| Lapisan | Isi |
| --- | --- |
| Kebijakan | `PrlTransportAllowancePolicy`: terikat satu `EmployeeGradeId` (nullable), metode `FixedMonthly` / `PerAttendance` / `PerDay` / `Reimbursement`, batas harian dan bulanan, syarat kehadiran |
| Penetapan | `PrlTransportAllowance` per pegawai |
| Transaksi | `PrlTransportAllowanceTransaction` (kejadian transport) |
| Controller | `WfpTransportAllowanceController` (8 endpoint), `WfpTransportAllowancePolicyController` (9), `WfpTransportAllowanceTransactionController` (8) |
| Kalkulasi | `PayrollCalculationService.BuildTransportAllowanceLines` (baris 661, 1370) menghasilkan komponen `TransportAllowance` |

Tidak ada koleksi tarif per `SalaryGradeId` pada kebijakan ini.

### 1.4 Kalkulasi: sumber tunjangan dan penjaga ganda yang **sudah ada**

`PayrollCalculationService` menyusun penghasilan dari lima sumber yang masing-masing punya kode komponen sendiri:

| Sumber | Kode komponen | Penjaga ganda yang sudah ada (dibaca dari kode) |
| --- | --- | --- |
| Tunjangan Struktur Gaji | `StructureAllowance` | `structureTypeIds` meneruskan jenis yang sudah dihitung dari struktur; penetapan pegawai bertipe sama **dilewati dengan peringatan** (baris 1234-1238) |
| Transportasi khusus | `TransportAllowance` | Penetapan pegawai berkategori `Transport` **dilewati dengan peringatan** (baris 1239-1243) |
| Tunjangan pegawai | `WorkforceAllowance` | Dua penetapan jenis sama yang beririsan di periode **menggagalkan pegawai**, tidak dibayar dua kali (baris 1256-1270); mata uang beda dan jenis nonaktif dilewati |
| Penghasilan variabel | `VariableEarning` | Terikat `PayrollRunEmployeeId`; input manual hanya yang sudah disetujui (baris 54) |
| Lembur dan kehadiran | `Overtime`, `AttendanceAllowance` | Dari input lembur/kehadiran pada baris pegawai run |

**Akibat penting:** karena baris 1239-1243, membuka Transportasi pada form penetapan umum **tidak akan membuatnya terbayar**. Tunjangan itu tetap dilewati. Jadi jalur lama baru boleh dimatikan setelah mesin hitung diubah, dan itu bergantung pada penyimpanan kebijakan yang belum ada.

### 1.5 Tunjangan aktivitas: Shift, On-call, Risiko

`MstShiftAllowancePolicy`, `MstOnCallAllowancePolicy`, `MstHazardAllowancePolicy` memuat tarif dan syarat, **tetapi tidak ada consumer perhitungan Payroll Run**. Pencarian pemakai ketiga model di luar master, DTO, dan controller hanya menemukan `AllowancePolicyCodeService` (pembuatan kode), `PayrollSalaryStructureAllowanceService`, `PayrollWorkforceAllowanceService`, dan `WorkforceAllowanceEligibilityRules` (semuanya hanya **mengeluarkan** jenis berkebijakan dari penetapan langsung). Akibatnya jenis tunjangan yang terhubung kebijakan Shift/On-call/Risiko saat ini **tidak dibayar oleh mesin Payroll Run mana pun**.

Ditemukan juga `PayrollOvertimeAndShiftIngestionService` (jalur "Modern" lama) dengan tarif shift malam tertanam `DefaultNightShiftRate = 50000m`. Ia hanya dipanggil dari `PayrollRunModernController` (baris 195), yang aksi tulisnya diblokir `LegacyModernPayrollGateFilter` pada nilai bawaan, sehingga tidak aktif; tetapi nominal tertanam itu bertentangan dengan prinsip "tidak ada nominal hardcoded" dan dicatat sebagai temuan (bagian 13).

### 1.6 Penghasilan variabel dan lembur

`PayrollVariableInputService` hanya mendukung tiga jenis: Insentif, Bonus, Honor Tugas Tambahan (pesan kesalahan baris 33). Terikat satu baris pegawai pada satu Payroll Run, dengan alur status sendiri. Lembur memiliki model `PrlPayrollOvertimeInput` dan jalur impor lewat `PayrollHrdImportService`; **detail alur lembur tidak saya telusuri sampai tuntas** (`Not Verified`).

### 1.6a Celah anti-ganda yang ditemukan (belum terbukti)

`PayrollSalaryStructureAllowanceService` mengecualikan jenis yang punya kebijakan Shift/On-call/Hazard aktif (baris 318-320) tetapi **tidak** memeriksa kategori `Transport` maupun kebijakan transport. Bila jenis Transport dimasukkan ke Struktur Gaji **dan** pegawai punya `PrlTransportAllowance`, kedua sumber bisa menghasilkan komponen transport pada periode yang sama. Apakah master tidak mencegahnya di lapisan lain **belum diverifikasi**.

### 1.7 Frontend existing

`payroll-workforce-allowance-section.jsx` (tombol "Kelola Transportasi" baris 116, melalui `canManageTransport` dan `openTransport`), `use-payroll-workforce-allowance.js` (`openTransport` baris 328, izin `PAYROLL_TRANSPORT_ALLOWANCE_PERMISSIONS`), `payroll-workforce-allowance-form-modal.jsx` (hanya satu dropdown Jenis; jenis berbasis kebijakan menampilkan informasi dan tombol "Buka Penetapan Transportasi Pegawai"), route `workforce-compensations/[id]/transport-allowance`, menu "Tunjangan Transportasi", hook/service/konstanta/util transport, dan komponen `transport-allowance/` serta `master-data/transport-allowance-policy/`.

---

## 2. Diagram proses bisnis

**Sebelum (sekarang):**

```text
Master Tunjangan (MstAllowanceType)
   ├─ Tetap Fixed/Percentage, rutin ──> Tunjangan Pegawai (PrlWorkforceAllowance) ──> Payroll: WorkforceAllowance
   ├─ Kategori Transport ──> Kebijakan Transport (per EmployeeGradeId)
   │                           └─> Halaman "Kelola Transportasi" ──> PrlTransportAllowance ──> Payroll: TransportAllowance
   ├─ Bonus / Insentif / Honor ──> Payroll Run ──> Penghasilan Variabel ──> Payroll: VariableEarning
   └─ Shift / On-call / Risiko ──> kebijakan aktivitas (TANPA consumer kalkulasi) ──> tidak dibayar
```

**Sesudah (target, `BLOCKED_BY_SCHEMA`):**

```text
Master Tunjangan (satu master per jenis)
   └─ Kebijakan Tunjangan ──> Tarif per SalaryGradeId (berlaku per tanggal)
          └─ Penetapan (individual atau bulk, bukti tarif tersimpan) ──> Payroll: satu resolver tunjangan
   Insidental ──> Payroll Run ──> Penghasilan Variabel (tidak pernah menjadi penetapan berulang)
   Aktivitas  ──> kebijakan tarif + data kejadian terverifikasi ──> Payroll Run (hanya bila consumer sah ada)
   Transport legacy ──> hanya baca histori setelah gerbang cutover (bagian 11)
```

---

## 3. Rancangan arsitektur (DESAIN; belum diimplementasikan)

### 3.1 Prinsip

Satu master per jenis (tanpa "Transportasi Staff/Supervisor/Manager"). Perbedaan nominal hanya pada **tarif per `SalaryGradeId`**, bukan `EmployeeGradeId`. Nominal tidak pernah datang dari frontend. Nilai yang berlaku disalin sebagai **snapshot** ke penetapan (sama seperti snapshot syarat master saat ini), sehingga perubahan kebijakan tidak mengubah penetapan atau payroll final.

### 3.2 Entity usulan

Nama final menunggu keputusan prefix (3.5); `Prl*` dipakai sebagai contoh sesuai entity payroll yang sudah ada.

| Entity | Kolom utama | Aturan |
| --- | --- | --- |
| `PrlAllowancePolicy` | `Id`, `AllowanceTypeId` (FK `MstAllowanceType`), `PolicyCode` (dialokasikan service), `PolicyName`, `CalculationMethod` (hanya `FixedAmount` / `PercentageOfBaseSalary`), `CurrencyCode`, `EffectiveStartDate`, `EffectiveEndDate`, `IsActive`, `PayrollComponentId?`, `LegalEntityId?`, `HospitalSiteId?`, `RevisionNo`, `SupersedesPolicyId?` | Kode unik; hanya satu kebijakan aktif tidak beririsan per kombinasi jenis + lingkup bila bisnis menghendaki; revisi membuat versi baru, tidak menimpa |
| `PrlAllowancePolicyGradeRate` | `Id`, `PolicyId` (FK), `SalaryGradeId` (FK `MstSalaryGrade`), `Amount`, `Percentage`, `MaximumAmount`, `EffectiveStartDate`, `EffectiveEndDate`, `IsActive` | **Tidak boleh ada dua tarif aktif yang beririsan** untuk (`PolicyId`, `SalaryGradeId`): dijaga service dan, bila memungkinkan, constraint pengecualian rentang tanggal pada PostgreSQL |
| `PrlWorkforceAllowance` (tambah kolom) | `AllowancePolicyId?`, `AllowancePolicyGradeRateId?`, `AssignmentSource` (`Direct` / `Policy` / `BulkPolicy`), `BulkBatchId?` | Kolom nullable; penetapan lama tetap `Direct` tanpa kebijakan (tidak perlu migrasi data) |
| `PrlWorkforceAllowanceBulkBatch` dan `...BatchItem` | Kunci idempotency, kebijakan, grade, masa berlaku, mode, status, hasil per pegawai (`Created` / `SkippedDuplicate` / `Rejected` + alasan) | Kunci idempotency unik; satu permintaan yang sama tidak membuat penetapan kedua |

Tarif multi-grade **tidak** boleh disimpan di `Description`, `PolicyName`, atau JSON bebas.

### 3.3 Alternatif yang ditimbang

| Opsi | Penilaian |
| --- | --- |
| **A. Tabel kebijakan umum + tarif per grade (rekomendasi)** | Sesuai tujuan: satu sistem, tanpa jalur khusus. Butuh migration dan keputusan prefix |
| B. Menambah anak `SalaryGradeId` pada `PrlTransportAllowancePolicy` | Tetap bernama transport dan tidak menyatukan jenis lain; mempertahankan jalur khusus |
| C. Memakai `MstSalaryStructureAllowance` | Berlaku per **struktur**, bukan per grade; tidak menghasilkan nominal berbeda per grade |
| D. Menyimpan tarif multi-grade di JSON/Description | Ditolak: melanggar aturan tugas |

### 3.4 Metode transport yang tidak masuk jalur rutin

Kebijakan transport saat ini punya `PerAttendance`, `PerDay`, dan `Reimbursement`. Kebijakan umum hanya mendukung `FixedAmount`/`PercentageOfBaseSalary` (yang dikenali mesin untuk penetapan pegawai). Karena itu **hanya transport `FixedMonthly` yang dapat pindah ke jalur rutin**. Transport berbasis kehadiran, per hari, atau penggantian adalah **kelompok aktivitas** dan masih memerlukan `PrlTransportAllowanceTransaction`; tabel itu harus tetap ada sampai desain aktivitas tersendiri tersedia.

### 3.5 Registry dan prefix (QBE-MOD-002 / QBE-NAM-004)

**Koreksi 20 November 2026.** Teks awal bagian ini menyatakan tidak ada baris `Prl` dan pembuatan entity `BLOCKED`. Itu salah: `Prl` sudah **terdaftar dan `ACTIVE`** pada registry backend sejak 2026-11-11 (persetujuan pemilik repository, dicatat pada entri changelog 2026-11-11), sehingga QBE-MOD-002 dan QBE-NAM-004 **terpenuhi** untuk entity `Prl*` baru. Tiga salinan registry ada: kanonik di `QuilvianEngineeringSkills/agents/rules/...` (tertinggal sejak 2026-09-22 sampai disinkronkan hari ini), mirror Claude (hasil generate), dan salinan backend `docs/engineering/...` yang **dibaca pemeriksa QBE**. Pada 2026-11-20 baris `Prl` yang identik ditambahkan ke salinan kanonik dan mirror atas instruksi pemilik, **tanpa persetujuan baru**. Salinan backend memuat banyak baris lain yang belum ada di kanonik (lihat catatan akhir `PAY-BE-FIX-013`); itu tidak diselaraskan di sini.

### 3.6 Cakupan otomatis per grade (`AutoCoverageByGrade`)

Rekomendasi: **tidak dibangun pada tahap awal.** Bila diperlukan, rancang sebagai aturan cakupan **dievaluasi saat kalkulasi** (tanpa menyalin penetapan berulang). Perilaku yang wajib diputuskan:

| Kejadian | Perilaku yang disarankan |
| --- | --- |
| Pegawai baru bergabung | Ikut cakupan mulai tanggal bergabung, setelah penetapan gajinya efektif |
| Grade berubah | Tarif mengikuti grade efektif per tanggal; prorata menurut aturan existing |
| Pindah unit | Mengikuti kriteria unit pada tanggal efektif |
| Status kontrak berubah | Dievaluasi ulang menurut kriteria jenis hubungan kerja |
| Pegawai keluar | Berhenti pada tanggal keluar |
| Kebijakan berakhir | Berhenti pada tanggal akhir; tidak ada perpanjangan diam-diam |

---

## 4. Rancangan nested dropdown (DESAIN; `BLOCKED_BY_SCHEMA`)

Susunan modal: Pegawai (read-only), Grade Gaji (read-only, dari `WfpSalaryAssignment` utama yang berlaku), Jenis Tunjangan (1), Kebijakan Tunjangan (2), Metode (read-only), Nominal/Persentase (read-only bila berbasis kebijakan), Tanggal Mulai, Tanggal Akhir, Catatan.

| Kejadian | Perilaku |
| --- | --- |
| Jenis berubah | Reset kebijakan dan nominal; tampilkan loading; muat ulang kebijakan menurut `AllowanceTypeId` baru; **batalkan permintaan sebelumnya** (`AbortController`) dan abaikan respons lama agar kebijakan jenis sebelumnya tidak muncul |
| Kebijakan dipilih | Ambil detail dan tarif grade yang cocok; tampilkan nama grade, nominal, metode, tanggal efektif, batas maksimum |
| Tidak ada kebijakan | "Belum tersedia Kebijakan Tunjangan aktif untuk Grade Gaji pegawai ini." dan penyimpanan dinonaktifkan |
| Tanpa penetapan gaji utama | "Penetapan Gaji Utama belum tersedia. Tetapkan Grade Gaji melalui HRD terlebih dahulu." |
| Simpan | Backend menentukan ulang nominal dari kebijakan; nominal kiriman frontend tidak dipercaya |

Tombol Batal tetap berfungsi kecuali saat menyimpan; perbaikan awalnya sudah dikerjakan (`PAY-FE-FIX-012`).

---

## 5. Spesifikasi penetapan individual (DESAIN)

Alur: pilih jenis → pilih kebijakan sesuai grade → pratinjau nominal → simpan. Backend wajib: (1) membaca `SalaryGradeId` dari penetapan gaji utama yang berlaku; (2) mencocokkan kebijakan dengan `AllowanceTypeId`; (3) mencocokkan kebijakan dengan `SalaryGradeId`; (4) memvalidasi tanggal efektif; (5) mata uang; (6) kelayakan pegawai; (7) komponen payroll; (8) mencegah kebijakan ambigu; (9) mencegah penetapan ganda (pola `EnsureNoOverlapAsync` dan `EnsureNotInSalaryStructureAsync` yang sudah ada); (10) mengembalikan nominal dari kebijakan; (11) tidak mempercayai nominal frontend; (12) transaksi dan kunci per pegawai (pola `PRL_WORKFORCE_ALLOWANCE_{id}` yang sudah ada); (13) memeriksa izin; (14) mencatat audit; (15) menjaga histori.

---

## 6. Spesifikasi penetapan massal (DESAIN)

Dua langkah, tanpa efek di langkah pertama:

1. **Pratinjau**: masukan jenis, kebijakan, grade, filter (jenis penerima, unit, hubungan kerja, pencarian), daftar pegawai terpilih. Keluaran per pegawai: eligible atau tidak (alasan), nominal, masa berlaku, konflik duplikasi. Backend memvalidasi ulang **setiap** `WorkforceProfileId`.
2. **Simpan**: membawa kunci idempotency; mengulang validasi; membuat penetapan hanya untuk yang eligible; mengembalikan hasil per pegawai. Permintaan yang sama diulang menghasilkan hasil yang sama tanpa duplikasi.

`SelectedEmployees` (bawaan, hanya yang dicentang) dibedakan tegas dari `AutoCoverageByGrade` (bagian 3.6), yang tidak aktif secara bawaan.

---

## 7. Matriks rutin vs insidental vs aktivitas

| Kelompok | Contoh | Sumber data | Jalur | Consumer kalkulasi | Keadaan sekarang |
| --- | --- | --- | --- | --- | --- |
| Rutin | Jabatan, Makan, Komunikasi, Transport tetap bulanan | Penetapan pegawai (nominal dari kebijakan per grade) | Master → Kebijakan → Penetapan | `WorkforceAllowance` | Jalur penetapan langsung ada; **kebijakan per grade tidak ada**; Transport masih jalur khusus |
| Insidental | Bonus, Insentif, Honor | Keputusan pembayaran per run | Payroll Run → Penghasilan Variabel | `VariableEarning` | **Ada**, 3 jenis; hanya input disetujui yang dihitung |
| Aktivitas | Lembur | Data lembur terverifikasi | Impor HRD → input lembur run | `Overtime` | Ada; alur tidak ditelusuri tuntas (`Not Verified`) |
| Aktivitas | Shift, On-call, Risiko berbasis paparan | Bukti jadwal, penugasan, paparan | Kebijakan aktivitas + kejadian | **Tidak ada** | Kebijakan ada, consumer tidak ada; **tidak boleh diaktifkan** sebelum dibangun terpisah |
| Aktivitas | Transport per kehadiran/hari/penggantian | `PrlTransportAllowanceTransaction` | Kebijakan + transaksi | `TransportAllowance` | Jalur khusus; tetap sampai desain aktivitas |

Tunjangan risiko yang oleh kebijakan rumah sakit didefinisikan sebagai tetap karena penempatan berisiko dapat menjadi rutin; klasifikasinya mengikuti kebijakan aktual dan menunggu keputusan (bagian 14).

---

## 8. Kontrak API

**Dipakai sekarang (tidak berubah):**

#### Corporate / Payroll Management / Workforce Allowance Assignment

Base URL: `api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId}/allowance-assignments` (dibaca dari `PayrollWorkforceAllowancesController.cs` baris 27)

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/`, `/summary`, `/{id}` | Daftar, ringkasan, detail | `WorkforceAllowanceAssignment : Read` |
| `GET` | `/{id}/amount` | Nominal dan persen | `WorkforceAllowanceAssignment : ViewAmount` |
| `POST` / `PUT` | `/`, `/{id}` | Membuat, mengubah | `Create` / `Update` |
| `POST` | `/{id}/submit` · `approve` · `reject` · `deactivate` | Siklus status | `Submit` · `Approve` · `Reject` · `Deactivate` |

#### Corporate / Payroll Management / Transport Allowance (jalur yang akan dipensiunkan bertahap)

`.../workforce-profiles/{workforceProfileId}/transport-allowance` (8 endpoint, `WorkforceTransportAllowance`), `.../transport-allowance-policies` (9, `TransportAllowancePolicy`), `.../transport-allowance-transactions` (8, `TransportAllowanceTransaction`).

**Diusulkan (`Rencana (belum tersedia)`):** opsi kebijakan menurut `allowanceTypeId`, `workforceProfileId`, tanggal efektif; detail kebijakan; pratinjau resolusi nominal; penetapan individual dengan `allowancePolicyId`; pratinjau bulk; simpan bulk dengan kunci idempotency; deteksi duplikasi. Aksi izin baru memakai `AccessTypes` yang sah (`Create`/`Read`/`Update`/`Delete`); `BulkCreate` didaftarkan dengan `AccessType = Create`.

---

## 9. Perubahan backend

**Tidak ada.** Tidak ada source backend yang ditulis pada task ini. Perubahan metadata kelayakan (`Selectable` / `RecurringAssignable` / `PolicyBased` / `EventBased`) sengaja **ditunda**: ia akan didesain bersama penyimpanan kebijakan agar tidak menjadi kode yang belum dipakai, dan alasan penolakan yang sudah ada sudah mengarahkan petugas ke jalur yang benar (bagian 1.1).

## 10. Perubahan frontend

Satu: `payroll-workforce-allowance-form-modal.jsx` (lihat `PAY-FE-FIX-012`). Tombol "Kelola Transportasi", route, hook, service, menu, dan CTA "Buka Penetapan Transportasi Pegawai" **dipertahankan** sampai pengganti siap.

## 11. Penghapusan alur Transportasi khusus (rencana bertahap)

Jalur lama tidak boleh dimatikan sebelum penggantinya mampu menangani fungsinya, atau tunjangan yang ada hilang dari Payroll. Urutan dan gerbangnya:

| Tahap | Isi | Gerbang masuk |
| ---: | --- | --- |
| 1 | Keputusan pemilik: schema, prefix, metode transport yang pindah (bagian 14) | Persetujuan pemilik |
| 2 | Wewenang migration terpisah; tabel kebijakan, tarif, tautan penetapan, batch | Tahap 1 |
| 3 | Backend: CRUD kebijakan, opsi, pratinjau, penetapan individual dan bulk | Tahap 2 |
| 4 | Paritas: setiap `PrlTransportAllowance` `FixedMonthly` aktif punya padanan penetapan baru dengan nominal sama | Tahap 3 + data diverifikasi |
| 5 | Mesin hitung: mencabut pelewatan kategori Transport hanya untuk penetapan berkebijakan **dan** menekan transport lama per pegawai yang sudah pindah | Tahap 4 |
| 6 | Frontend: dropdown bertingkat, lalu hapus tombol, navigasi, CTA, route, hook, service, komponen yatim | Tahap 5 |
| 7 | Pensiun controller transport (hanya baca histori); model tetap `LEGACY_PERSISTED` | Tahap 6 + konsumen histori dinyatakan aman |

**Konsumen yang harus tetap bisa membaca histori** (hasil pencarian): `PayrollCalculationService`, `PayrollRunService`, `PayrollDateNormalizer`, `PayrollWorkforceAllowanceService`, `AllowanceTypeController`, `EmployeeController`, `WorkforceDetailController`, `MstWorkforceProfile`, `PayrollStatusCatalog`, `ApplicationDbContext` dan tiga configuration. **Tidak ada tabel atau data historis yang dihapus.**

## 12. Dekomposisi (aturan EPIC)

| Fase | Isi | Klasifikasi awal | Status |
| --- | --- | --- | --- |
| F0 | Audit dan desain (dokumen ini) | `MEDIUM` | Selesai |
| F1 | Perbaikan modal (Batal) | `LIGHT` | Selesai (`PAY-FE-FIX-012`) |
| F2 | Keputusan pemilik dan wewenang schema | n/a | Menunggu |
| F3 | Kebijakan + tarif per grade (BE) | `HEAVY` | `BLOCKED_BY_SCHEMA` |
| F4 | Penetapan individual berbasis kebijakan + dropdown bertingkat | `HEAVY` | Setelah F3 |
| F5 | Bulk preview dan simpan | `HEAVY` | Setelah F4 |
| F6 | Mesin hitung terpadu, cutover Transport | `EPIC` (pecah lagi) | Setelah F5 + paritas data |
| F7 | Aktivitas: Shift, On-call, Risiko | `HEAVY` per jenis | Perlu desain dan keputusan terpisah |

## 13. Strategi anti-ganda dan pemeliharaan histori

Aturan yang diusulkan: satu kunci sumber (`SourceType`, `SourceId`) per komponen; prioritas **Struktur Gaji > Penetapan pegawai (langsung atau berkebijakan) > Transport lama**, dan Transport lama **hanya** dihitung untuk pegawai yang belum pindah; tidak ada dua sumber untuk jenis dan periode yang sama (konflik menggagalkan pegawai, mengikuti pola yang ada, bukan urutan query). Prioritas final adalah keputusan bisnis (bagian 14). Histori: tidak ada penghapusan; penetapan menyimpan snapshot nominal dan tautan ke tarif yang dipakai; revisi kebijakan membuat versi baru; Payroll Run final dan snapshot-nya tidak pernah disentuh.

**Temuan yang perlu ditindaklanjuti** (tidak diubah di sini): (a) celah struktur gaji tanpa penjaga Transport (1.6a); (b) tarif shift malam tertanam pada jalur lama yang diblokir (1.5).

## 14. Keputusan yang dibutuhkan

| No | Keputusan | Rekomendasi |
| ---: | --- | --- |
| D1 | Izin merancang dan membuat migration untuk Opsi A, serta pemilik registry menetapkan prefix | Setujui Opsi A; ajukan baris registry `Prl`. **Dijawab 20 Nov 2026:** pemilik memilih D1, akan **menjalankan migrasi sendiri**; prefix `Prl` ternyata sudah terdaftar (koreksi pada bagian 3.5) |
| D2 | Hanya transport `FixedMonthly` pindah ke rutin; `PerAttendance`, `PerDay`, `Reimbursement` tetap di jalur aktivitas | Setujui |
| D3 | Prioritas sumber bila satu jenis muncul dari beberapa sumber | Struktur > penetapan pegawai > transport lama |
| D4 | `AutoCoverageByGrade` | Tunda; bila dibangun, dievaluasi saat kalkulasi |
| D5 | Klasifikasi tunjangan risiko (rutin karena penempatan atau berbasis paparan) | Pemilik kebijakan rumah sakit |
| D6 | Apakah kebijakan boleh menimpa `RequiresApproval` master | Tidak; ikuti master |
| D7 | Cara memindahkan data `PrlTransportAllowance` yang ada ke penetapan baru | Skrip idempoten yang dijalankan pemilik, dengan paritas diperiksa |
| D8 | Pembangunan consumer Shift/On-call/Risiko | Fase terpisah per jenis, dengan bukti aktivitas |

## 15. Validasi RBAC dan audit

Endpoint tunjangan pegawai memakai `[AccessPermission("WorkforceAllowanceAssignment", ...)]` per aksi dan memblokir nominal tanpa `ViewAmount` (dibaca pada controller baris 19-21, 54, 95-100). Pola audit mengikuti `LoggerService.AuditAsync` yang sudah dipakai controller penetapan gaji. Seluruh endpoint baru wajib punya `[AccessAction]` dan `[AccessPermission]` yang cocok huruf demi huruf dan tidak memakai `IsInRole`. Hak akses sungguhan per role **tidak diverifikasi**.

## 16. Hasil static review (34 skenario)

Status: **SRP** = Static Review Passed, **SRF** = Static Review Failed, **NV** = Not Verified, **BLOCKED** = `BLOCKED_BY_SCHEMA`. "SRP" hanya berarti kode dibaca; **tidak ada yang dijalankan**.

| No | Skenario | Status | Bukti / catatan |
| ---: | --- | --- | --- |
| 01 | Tombol Kelola Transportasi tidak muncul | BLOCKED | Sengaja dipertahankan: `section.jsx` baris 116; pengganti belum ada |
| 02 | Navigasi operasional Transportasi tidak dipakai | BLOCKED | Sama; route `.../transport-allowance` masih aktif |
| 03 | Master Transportasi dapat dipilih di dropdown Jenis | BLOCKED | Saat ini ditampilkan sebagai "Kebijakan Grade • Transportasi" tetapi penyimpanan diarahkan ke modul transport |
| 04 | Dropdown kedua mengikuti jenis | BLOCKED | Dropdown kedua tidak ada |
| 05 | Dropdown kedua hanya kebijakan sesuai grade | BLOCKED | Tidak ada penyimpanan tarif per grade |
| 06 | Ubah dropdown pertama me-reset kedua | BLOCKED | Idem |
| 07 | Data kebijakan dari Backend | BLOCKED | Idem |
| 08 | Nominal mengikuti `SalaryGradeId` efektif | BLOCKED | Idem |
| 09 | `SalaryGradeId` tidak tertukar `EmployeeGradeId` | NV | Kebijakan transport memetakan grade gaji ke `EmployeeGradeId` (dari temuan tugas; tidak dibaca ulang) |
| 10 | Pesan jelas bila tidak ada kebijakan | BLOCKED | Idem |
| 11 | Tidak ada nominal Rp0 palsu | BLOCKED | Idem |
| 12 | Penetapan individual berhasil (kontrak statis) | NV | Hanya jalur langsung; validasi tidak dibaca penuh |
| 13 | Bulk memvalidasi semua penerima | BLOCKED | Tidak ada endpoint bulk |
| 14 | Bulk tidak membuat duplikasi | BLOCKED | Idem |
| 15 | Grade berbeda tidak mendapat tarif salah | BLOCKED | Idem |
| 16 | Perubahan grade menyesuaikan tarif | BLOCKED | Idem |
| 17 | Penghasilan rutin masuk sesuai masa berlaku | SRP | `BuildWorkforceAllowanceLines`: jendela dan prorata (baris 1250-1295). Filter `Approved` berdasarkan keterangan kode (query pemuatan tidak dibaca) |
| 18 | Bonus hanya periode input dan disetujui | SRP | Input terikat baris run; hanya input manual disetujui yang dihitung (baris 54). Alur status penuh tidak ditelusuri |
| 19 | Insentif hanya periode terkait | SRP | Sama dengan 18 |
| 20 | Honor hanya periode terkait | SRP | Sama dengan 18 |
| 21 | Lembur tidak dihitung dua kali | NV | Jalur impor lembur tidak ditelusuri tuntas |
| 22 | Shift memerlukan bukti aktivitas | NV | Tidak ada consumer perhitungan; tidak boleh diaktifkan |
| 23 | On-call memerlukan bukti penugasan | NV | Sama |
| 24 | Risiko mengikuti klasifikasi benar | NV | Sama; klasifikasi menunggu keputusan D5 |
| 25 | Tunjangan struktur tidak ganda | SRP | `structureTypeIds` (baris 1234-1238) |
| 26 | Transport lama tidak bersamaan dengan sumber baru | SRP (kondisi sekarang) | Sumber baru belum ada; penetapan kategori Transport dilewati (baris 1239-1243). **Catatan:** celah struktur gaji (1.6a) belum diverifikasi |
| 27 | Payroll final utuh | SRP | Tidak ada perubahan source backend |
| 28 | Snapshot dan audit valid | SRP | Tidak ada perubahan |
| 29 | Role tanpa izin tidak dapat menetapkan | SRP (jalur existing) | `[AccessPermission]` per aksi; jalur baru BLOCKED |
| 30 | Nominal tidak bocor tanpa `ViewAmount` | SRP (jalur existing) | Controller baris 19-21, 54, 95-100 |
| 31 | Batal dapat menutup modal saat tidak submitting | SRP | `PAY-FE-FIX-012`: dari source `ConfirmModal` dan 2 berkas tes; tampilan `NOT RUN` |
| 32 | List dan summary direvalidasi setelah simpan | NV | Tidak dibaca tuntas |
| 33 | Tidak ada endpoint orphan rusak | SRP | Tidak ada cleanup yang dilakukan |
| 34 | Tidak ada migration atau perubahan schema | SRP | `git status --short -- Migrations` kosong; tidak ada perubahan snapshot |

## 17. Daftar `BLOCKED_BY_SCHEMA`

Tabel `PrlAllowancePolicy`, `PrlAllowancePolicyGradeRate`, tiga kolom tautan pada `PrlWorkforceAllowance`, dan dua tabel batch bulk (bagian 3.2). Dependensi: keputusan D1 (dijawab), migrasi oleh pemilik, kemudian backend F3. Prefix registry **bukan** lagi dependensi (sudah terdaftar). **Tidak ada migration dibuat, dijalankan, atau diubah.**

## 18. Risiko dan pekerjaan tertunda

| Hal | Isi |
| --- | --- |
| Risiko | Memaksakan jalur baru tanpa memindahkan data akan membuat tunjangan transport terhitung ganda atau hilang |
| Risiko | Celah struktur gaji (1.6a) dan tarif tertanam (1.5) belum ditangani |
| Pekerjaan tertunda | Semua fase F2 s.d. F7 |
| Pihak lain | `PAY-BE-FIX-011` dan perubahan frontend paralel (`PAY-FE-FIX-008` putaran 3, editor workforce) berjalan pada working tree yang sama; tidak disentuh |

## 19. Pernyataan

`dotnet build`, `npm run build`, migration, dan uji runtime **tidak dijalankan**. Tidak ada commit, push, atau deploy. `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.
