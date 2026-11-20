# Laporan Perbaikan Frontend — `PAY-FE-FIX-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-010` (ad-hoc lanjutan seri `PAY-FE-FIX-001..009`) |
| Judul | Kolom "Persetujuan" penetapan gaji: penetapan yang sah otomatis tampil "Disetujui Otomatis", bukan "Belum Disetujui" |
| Slice | Corporate / Payroll Management, bagian Riwayat Penetapan Gaji (detail Profil Penggajian) dan Rekap Penetapan Gaji |
| Roadmap | Tidak ada baris roadmap untuk seri ad-hoc `PAY-FE-FIX`; tidak ada roadmap atau traceability yang diubah (`NOT APPLICABLE`) |
| Trace | Keputusan "peniadaan approval" 19 November 2026; `PAY-FE-FIX-008`; kontrak `PAY-BE-FIX-010` |
| Contract version | Respons `PAY-BE-FIX-010` (field additive), dibaca dari source backend pada commit `d642a9df` ditambah perubahan kerja yang belum di-commit |
| Wewenang UI | Perubahan tampilan kolom dan filter pada layar yang sudah ada; tanpa layar, route, atau komponen baru |
| Dependency | `PAY-BE-FIX-010` (source selesai, **belum dibuild**). Tanpa backend itu layar memakai pemetaan lama (lihat bagian 4) |
| Klasifikasi | `MEDIUM` — skor 5 (satu repository: 0; berkas diperiksa 9–20: 1; berkas diubah 4–8: 1; logika sedang: 1; kontrak memakai yang sudah ada: 1; UI dua layar: 1; database 0; keamanan berkaitan: 0) |
| Task mode | `CROSS-REPO`, tahap frontend setelah backend |
| Target tulis | `QuilvianSystemFrontendDev` ditambah berkas laporan ini di repository backend |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `31df72b9d907796eb106c638a2a350405c866efa` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `d642a9df9de35801ed8332adc0cdcd6127f96353` ditambah perubahan kerja `PAY-BE-FIX-010` |
| Tanggal | 20 November 2026 |
| Status | 🟡 **Source selesai dan logika teruji; tampilan belum dilihat.** `VISUAL CHECK: NOT FEASIBLE` karena server dev dan build dilarang |

---

## 1. Keadaan yang ditemukan di awal

Pada halaman `/payroll/workforce-compensations/<WorkforceProfileId>`, tabel **Riwayat Penetapan Gaji** menampilkan badge **"Belum Disetujui"** dengan keterangan **"Belum disyaratkan Payroll"** untuk penetapan yang aktif, utama, dan sedang berlaku. Dua kalimat itu saling bertentangan.

Penyebab, dengan bukti:

- `getSalaryApprovalKey` (`payroll-compensation-utils.js`) hanya mengenal tiga keadaan dan menurunkannya dari fakta persetujuan manual: `approved`, `pendingapproval`, dan `notapproved` (bila `approvalRequired` false dan tidak ada persetujuan). Tidak ada keadaan "sah otomatis menurut kebijakan".
- `renderApprovalCell` (`payroll-compensation-table-columns.jsx`) memetakan `notapproved` ke label "Belum Disetujui" dan menulis "Belum disyaratkan Payroll" di bawahnya. Kolom yang sama dipakai Rekap Penetapan Gaji.
- Backend belum mengirim status kelayakan Payroll; `PAY-BE-FIX-010` menambahkannya (`AuthorizationStatus`, `OperationalStatusCode`, `IsPayrollUsable`).
- Filter "Persetujuan" pada Rekap memakai `isApproved`, sehingga penetapan yang sah otomatis ikut terhitung sebagai "Menunggu persetujuan".

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna.** Petugas Payroll membuka Profil Penggajian pegawai atau Rekap Penetapan Gaji untuk memastikan gaji pegawai akan dihitung.

**Langkah.**

1. Petugas membuka `Payroll > Penghasilan Pegawai > Penetapan Gaji`, atau membuka detail Profil Penggajian lalu bagian Riwayat Penetapan Gaji.
2. Setiap baris menampilkan badge dan keterangan di kolom **Persetujuan**, terpisah dari kolom **Status Aktif**.
3. Di Rekap, petugas dapat menyaring menurut **Persetujuan** (asal wewenang) dan **Status Berlaku** (keadaan operasional), dua hal yang sekarang terpisah.

**Aturan tampilan** (badge ditentukan satu fungsi, `getSalaryApprovalKey`, dari status Backend, bukan dari `IsActive`):

| Urutan | Syarat dari Backend | Badge | Keterangan di bawah badge |
| ---: | --- | --- | --- |
| 1 | `OperationalStatusCode = Inactive` | **Nonaktif** | "Tidak dipakai Payroll." + asal persetujuan |
| 2 | `OperationalStatusCode = Expired` | **Berakhir** | "Masa berlaku telah berakhir." + asal persetujuan |
| 3 | `AuthorizationStatus = PendingApproval` | **Menunggu Persetujuan** | "Belum dapat digunakan Payroll." |
| 4 | `OperationalStatusCode = Scheduled` | **Terjadwal** | "Belum mulai berlaku." + asal persetujuan |
| 5 | `OperationalStatusCode = NotPrimary` | **Bukan Penetapan Utama** | "Payroll hanya memakai penetapan utama." + asal persetujuan |
| 6 | `ManualApproved` dan `Current` | **Disetujui** | Tanggal persetujuan dan "oleh {nama}" bila tersedia |
| 7 | `AutoByPolicy` dan `Current` | **Disetujui Otomatis** | "Berlaku tanpa persetujuan manual sesuai kebijakan Payroll." |

"Asal persetujuan" pada keterangan berbunyi "Persetujuan manual tidak disyaratkan." (kebijakan) atau "Disetujui manual {tanggal} oleh {nama}" (bila persetujuan manual benar-benar tercatat). "Disetujui Otomatis" **tidak pernah** diberi tanggal atau nama pengesah, karena tidak ada orang yang menyetujui.

**Contoh.** Penetapan pada screenshot (Gaji Pokok Dokter Tetap, 30 Juni 2026 sampai sekarang, Utama, Aktif, `ApprovedAt` kosong, kebijakan tidak mensyaratkan) kini tampil **"Disetujui Otomatis"** dengan keterangan "Berlaku tanpa persetujuan manual sesuai kebijakan Payroll."

**Jalur tidak normal.**

| Kejadian | Yang dilihat petugas |
| --- | --- |
| Backend belum memuat `PAY-BE-FIX-010` (field status tidak dikirim) | Layar memakai pemetaan lama: "Belum Disetujui" bila approval tidak disyaratkan, "Menunggu Persetujuan" bila disyaratkan. Tidak ada tebakan dari `IsActive` |
| `AuthorizationStatus` bernilai tak dikenal | Diperlakukan seperti Backend lama, bukan ditebak |
| Tanpa izin `ViewAmount` | Kolom Gaji Pokok tetap "terselubung"; kolom Persetujuan tidak memuat nominal |
| Filter menghasilkan data kosong atau gagal dimuat | Keadaan kosong dan galat bawaan `DataTable`/`InformationAlert` (tidak diubah) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`payroll-compensation-table-columns.jsx`, `payroll-compensation-utils.js`, `payroll-constants.js`, `payroll-salary-assignment-view.jsx`, `payroll-compensation-detail-view.jsx`, `use-payroll-salary-assignments.js`, `payroll-compensation-service.js`, `payroll-common-utils.js` / `api-payload-utils.js` (`buildQueryParams`), `status-badge.jsx`, tes `workforce-salary-assignment.test.mjs`, serta kontrak backend pada laporan `PAY-BE-FIX-010`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-constants.js` | Opsi filter Persetujuan diganti ke status otorisasi; opsi filter Status Berlaku baru; konfigurasi badge ditambah `autobypolicy`, `scheduled`, `expired`, `inactive`, `notprimary` (kunci lama dipertahankan untuk Backend lama); teks keterangan |
| `src/utils/payroll/payroll-compensation-utils.js` | Normalisasi field baru pada riwayat dan rekap (null bila tidak dikirim); `getSalaryApprovalKey` diperluas (satu tempat keputusan); `buildSalaryApprovalNotes` baru (murni, `formatDate` disuntikkan) |
| `src/components/view/payroll/compensation/payroll-compensation-table-columns.jsx` | `renderApprovalCell` memakai kunci dan keterangan baru; dipakai Riwayat dan Rekap sekaligus |
| `src/lib/hooks/payroll/use-payroll-salary-assignments.js` | State filter `authorizationStatus` dan `operationalStatus` menggantikan `isApproved`; dikirim ke backend lewat service yang sudah ada |
| `src/components/view/payroll/compensation/payroll-salary-assignment-view.jsx` | Filter Persetujuan memakai status otorisasi; filter Status Berlaku ditambah; teks informasi menjelaskan "Disetujui Otomatis" |
| `tests/unit/payroll-salary-approval-status.test.mjs` | **Baru.** 15 tes logika murni dan kontrak (opsional menurut kebijakan tes frontend) |

Tidak ada route, hook, slice, service, atau CSS yang ditambah. Service `PayrollCompensationService` tidak diubah: `buildQueryParams` sudah membuang nilai kosong dan meneruskan parameter baru.

### 3.3 Kepatuhan arsitektur frontend

Alur `view → hook → service → InstanceAxios` dipertahankan. Keputusan tampilan berada di util murni (tanpa React, tanpa request); view hanya merender. Konstanta statis berada di `payroll-constants.js`. Tidak ada formula atau keputusan kelayakan baru di frontend: frontend membaca `AuthorizationStatus` dan `OperationalStatusCode` dari backend dan hanya memetakan keduanya ke label.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/payroll/compensation/payroll-compensation-table-columns.jsx` itu sendiri (kolom Persetujuan versi sebelumnya) dan `payroll-salary-assignment-view.jsx`.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Badge kolom Persetujuan | `StatusBadge` (`config` per kunci) | `REUSE` | `config[statusKey]` sudah memberi label dan kelas; hanya kunci baru yang didaftarkan |
| Keterangan di bawah badge | `styles.cellStack` (`payroll-run.module.css`) dan `tableStyles.supportingCell` | `REUSE` | Kelas yang sama sudah dipakai `renderApprovalCell`; tidak ada CSS baru |
| Filter Persetujuan | `FilterSelect` | `REUSE` | Hanya isi opsi dan nilai yang berubah |
| Filter Status Berlaku | `FilterSelect` | `REUSE` | Pola sama dengan filter lain di `DataFilter` |
| Teks informasi | `InformationAlert` | `REUSE` | Hanya teks |

`UI GATE: PASS — 5 elemen REUSE, 0 EXTEND, 0 COMPOSE, 0 WRAP, 0 NEW`. Karena tidak ada CSS baru, `payroll-run.module.css` yang sedang diubah pihak lain **tidak disentuh**.

Hasil pemeriksa dan grep anti-regresi, apa adanya:

```text
$ node ../QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs \
    src/components/view/payroll/compensation/payroll-compensation-table-columns.jsx \
    src/components/view/payroll/compensation/payroll-salary-assignment-view.jsx
OK — 2 berkas diperiksa, 0 temuan.

$ grep (button mentah, utility tipografi Bootstrap, dialog manual, input mentah/UUID, prop tidak sah, class Tailwind)
  pada kedua berkas JSX
(kosong)
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/payroll/workforce-compensations/<id>` (Riwayat Penetapan Gaji) | — | `VISUAL CHECK: NOT FEASIBLE` |
| `/payroll/...` Rekap Penetapan Gaji | — | `VISUAL CHECK: NOT FEASIBLE` |

`VISUAL CHECK: NOT FEASIBLE` — server development dan build dilarang pada task ini, dan data pegawai pada screenshot tidak dapat dibaca dari sini. Karena itu status task paling tinggi `🟡`. Risiko yang belum terlihat: baris keterangan yang bertambah (dua baris) membuat sel lebih tinggi; kolom `minWidth: 170` belum dilihat pada layar sempit.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Keadaan memuat bawaan `DataTable` (tidak diubah) |
| Kosong | "Data penetapan gaji tidak ditemukan." (tidak diubah) |
| Gagal | `InformationAlert` merah dengan pesan dari backend (tidak diubah) |
| Tanpa hak akses | `AccessDeniedGate` pada Rekap (tidak diubah) |
| Backend belum diperbarui | Pemetaan lama; tidak ada status yang dikarang |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Payroll Management / Workforce Compensation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `api/v1/corporate/payroll-management/workforce-compensations/{workforceProfileId}` | Riwayat Penetapan Gaji pada detail Profil Penggajian (`SalaryHistory`) | `PayrollCompensation : ReadDetail` |

#### Corporate / Human Resource / Workforce Core / Cross-Employee Administration

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `api/v1/corporate/human-resource/workforce-core/salary-assignments` | Rekap Penetapan Gaji, kini dengan query `authorizationStatus` dan `operationalStatus` | `WfpSalaryAssignment : ReadAll` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Tes baru `payroll-salary-approval-status.test.mjs` | **15 lulus, 0 gagal, 0 dilewati** | `PASS` | Dieksekusi; mengimpor util dan konstanta (bukan pencocokan teks), ditambah satu tes kontrak yang membaca source backend |
| Tes lama `workforce-salary-assignment.test.mjs` | 23 lulus, **1 gagal** | `EXISTING / ENVIRONMENT ISSUE` | Lihat di bawah |
| `check-css-usage.mjs` pada 2 JSX | 0 temuan | `PASS` | Keluaran pada bagian 3.4 |
| Grep anti-regresi pada 2 JSX | Kosong | `PASS` | Bagian 3.4 |
| ESLint pada 6 berkas yang diubah/dibuat | 0 error, 1 warning | `PASS` | Warning `react-hooks/set-state-in-effect` pada `use-payroll-salary-assignments.js` baris 39 (`setLoading(true)` di effect), baris yang tidak saya ubah |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dilarang pada task ini |
| Verifikasi tampilan di peramban | Tidak dilakukan | `NOT RUN` | `VISUAL CHECK: NOT FEASIBLE` |

`AUTOMATED TEST: node --import ./QuilvianSystemFrontendDev/tests/helpers/register.mjs --test <2 berkas> — FAIL (38 dari 39 lulus; 1 gagal bukan akibat task ini)`

**Tes yang gagal.** `workforce-salary-assignment.test.mjs`, tes "BE: WfpSalaryAssignmentController dan DTO mendukung rentang gaji…", asersi `ValidateSalaryAssignmentAsync(request.SalaryStructureId, request.SalaryGradeId, request.BaseSalary`. String itu ada **satu kali pada `HEAD`** dan **nol kali pada kerja sekarang**, karena `PAY-BE-FIX-011` (pekerjaan pihak lain yang berjalan paralel pada controller yang sama) mengganti argumen menjadi `baseSalary` hasil penentuan server (`WfpSalaryAssignmentController.cs` baris 680). Bagian `ValidateRequestAsync` tidak disentuh task ini. Kegagalan ini perlu ditangani pemilik `PAY-BE-FIX-011` (menyesuaikan asersi tes teks itu); saya tidak mengubah tes itu.

Uji manual: `NOT FEASIBLE`. Untuk filter: daftar opsi, nilai terpilih, dan teks sudah ditentukan dan dites sebagai konstanta; **efek ke request/hasil, reset, filter gabungan, dan paginasi tidak dicoba di layar**. Dari kode: setiap perubahan filter mengembalikan halaman ke 1 (`updateFilter`), `handleReset` mengembalikan `DEFAULT_FILTERS`, dan `buildQueryParams` membuang nilai kosong.

**Tidak dijalankan:** build, verifikasi tampilan, dan pembacaan data `WorkforceProfileId 33793d1d-c34f-43e5-9940-c727f754e45c`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Penetapan sah otomatis tidak lagi tampil "Belum Disetujui" | Terpenuhi pada logika | UAT-01 pada tes baru; `getSalaryApprovalKey` |
| Tidak memalsukan persetujuan manual (tanpa tanggal/pengesah untuk "Disetujui Otomatis") | Terpenuhi | Tes UAT-01; `buildSalaryApprovalNotes` |
| Kondisi 2: disyaratkan dan belum tercatat → "Menunggu Persetujuan" / "Belum dapat digunakan Payroll." | Terpenuhi | Tes UAT-02 |
| Kondisi 3: persetujuan manual → "Disetujui" + tanggal + nama | Terpenuhi | Tes UAT-03 |
| Kondisi 4 dan 5: Nonaktif, Terjadwal, Berakhir terpisah dari asal persetujuan | Terpenuhi | Tes UAT-04, UAT-05, tes "berakhir" |
| Filter Persetujuan selaras, `isApproved` tidak diubah maknanya, ada filter status operasional | Terpenuhi pada source | Konstanta dan hook; backend tidak diubah untuk `isApproved` |
| Pagination dan filter lama dipertahankan | Terpenuhi pada source | `updateFilter`, `setPageSize`, filter `isPrimary` dan `isActive` tidak diubah |
| ViewAmount dipertahankan | Terpenuhi | Kolom Gaji Pokok dan `amountText` tidak diubah |
| Ringkasan sinkron dengan daftar | Terpenuhi | Rekap hanya menampilkan total dari daftar; tidak ada hitungan terpisah |
| Tampilan sesuai dan tidak merusak tata letak | **Belum terpenuhi** | `VISUAL CHECK: NOT FEASIBLE` |
| Build berhasil | **Belum terpenuhi** | `NOT RUN` |

**Skenario UAT frontend.** UAT-01 s.d. UAT-05 dan UAT-09 terbukti pada logika (tes dieksekusi). UAT-06 (data lama tanpa `ApprovedAt` terbaca otomatis tanpa histori palsu) terbukti pada logika dan bergantung pada backend untuk nilai `AuthorizationStatus`. UAT-08 (konsistensi riwayat/rekap/detail) terpenuhi karena satu fungsi dipakai bersama; konsistensi dengan Readiness bergantung pada backend. UAT-10 s.d. UAT-12 adalah tanggung jawab backend. UAT-07 mengikuti aturan siklus hidup backend yang tidak diubah. **Semua ini belum dilihat di layar.**

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Rekap memakai tiga filter yang berkaitan (Persetujuan, Status Berlaku, serta Jenis Penetapan dan Status yang sudah ada). Sebagian beririsan (mis. Nonaktif); dibiarkan karena menghapus filter lama mengubah perilaku yang sudah dipakai |
| Masalah yang diketahui | Tes lama `workforce-salary-assignment.test.mjs` (1 asersi) gagal akibat `PAY-BE-FIX-011` pihak lain, bukan task ini |
| Dependency backend | `PAY-BE-FIX-010` belum dibuild. Bila backend belum dideploy bersama frontend, layar memakai pemetaan lama (badge "Belum Disetujui" kembali muncul) sehingga keduanya sebaiknya dirilis bersamaan |
| Perubahan sampingan | `NONE` oleh task ini. **Dicatat:** `salary-structure-allowance-table-columns.jsx` dan `payroll-run.module.css` berubah oleh pihak lain (Putaran 3 `PAY-FE-FIX-008`) sebelum dan selama task ini; tidak disentuh |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah. Tidak ada stage, commit, push, pull, merge, atau rebase |
| Langkah berikutnya | Pemilik menjalankan build backend dan frontend, membuka dua layar ini dengan data pegawai contoh, dan menyimpan screenshot; lalu memperbaiki asersi tes lama yang terdampak `PAY-BE-FIX-011` |

```text
$ git status --short   (repo frontend, branch yasmina, HEAD 31df72b9)
 M src/components/view/payroll/compensation/payroll-compensation-table-columns.jsx   <- task ini
 M src/components/view/payroll/compensation/payroll-salary-assignment-view.jsx       <- task ini
 M src/components/view/payroll/master-data/salary-structure/detail/salary-structure-allowance-table-columns.jsx   <- pihak lain
 M src/lib/constants/payroll/payroll-constants.js                                    <- task ini
 M src/lib/hooks/payroll/use-payroll-salary-assignments.js                           <- task ini
 M src/style/payroll/payroll-run.module.css                                          <- pihak lain
 M src/utils/payroll/payroll-compensation-utils.js                                   <- task ini
?? tests/unit/payroll-salary-approval-status.test.mjs                                <- task ini
```
