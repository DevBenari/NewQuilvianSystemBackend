# Laporan Perubahan Backend — `BE-BKC-FIX-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-001` (ad-hoc bug fix — **bukan** task roadmap bernomor `BE-BKC-0xx`; task ID ini dibuat sendiri untuk menjaga jejak laporan tetap tracked, mengikuti konvensi lokasi laporan yang sama) |
| Judul | Perbaikan filter scope tarif (`serviceUnitId`/`clinicId`/`patientClassId`) pada `TariffController` |
| Slice | Ditemukan saat verifikasi manual `FE-BKC-014` (entri manual katalog tarif, `BKC-DEC-061`) — bukan bagian scope aslinya, tapi memblokir fitur itu berfungsi |
| Roadmap | `NOT APPLICABLE` — tidak ada baris roadmap untuk perbaikan ini. Dipicu oleh `FE-BKC-014` (`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` baris 193–207) |
| Trace | `BKC-DEC-061` (konteks encounter dipakai memfilter katalog tarif) — perbaikan ini yang membuat filter itu benar-benar bisa menghasilkan data |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan endpoint/DTO/response shape. Murni perbaikan logika query internal `ApplyStandardFilter` |
| Dependency | Tidak ada — perbaikan berdiri sendiri pada method yang sudah ada |
| Klasifikasi | `LIGHT` — skor 1 (repo 0, berkas diperiksa 0 [≤8, hanya 1 berkas disentuh dan sudah dibaca penuh saat `FE-BKC-014`], berkas diubah 0 [1 berkas], logika bisnis 1 [Sedang — mengubah semantik filter, bukan perilaku baru], kontrak API 0, database 0, keamanan 0, UI/workflow 0) |
| Task mode | `BACKEND` — bug fix ad-hoc, otorisasi eksplisit pengguna lewat pertanyaan konfirmasi langsung ("Perbaiki backend sekarang (task terpisah, Rekomendasi)") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/MasterData/Controllers/TariffController.cs`, `docs/module-blueprints/billing-kasir/task/report/backend/` |
| Model | Claude Sonnet 5 |
| Governance Preflight | Area `HealthServices`; Module/pemilik `Administrator/HealthServices — Master/Reference`; prefix `Mst` (registry, Lifecycle `ACTIVE`). Tidak ada entity/module baru. Keberlakuan `TOUCHED LEGACY` — mengubah method query yang sudah lama berjalan pada controller master data yang sudah ada |
| QBE ID berlaku | `QBE-VAL-001` (invarian query diperbaiki supaya konsisten dengan semantik domain yang sudah berlaku di modul lain) |
| Commit backend saat dikerjakan | `fec3579` |
| Tanggal | 3 September 2026 |
| Status | Source diperbaiki (3 baris kondisi filter). **Build/restart backend belum dijalankan oleh sesi ini** — proses `QuilvianSystemBackend.exe` yang sedang berjalan (PID 10976) adalah binary lama yang di-build manual pengguna sebelumnya, bukan `dotnet watch`, sehingga TIDAK otomatis memuat perbaikan ini. Pengguna perlu build + restart backend, lalu perbaikan ini bisa diverifikasi ulang lewat langkah yang sama persis dengan yang menemukannya (lihat § 5) |

---

## 1. Masalah yang diperbaiki

Ditemukan saat verifikasi manual `FE-BKC-014` (dropdown "Tarif Layanan" baru pada form "Buat Invoice
Manual"): dropdown itu **selalu kosong**, tidak peduli kata kunci pencarian apa pun yang diketik,
untuk kunjungan pasien manapun.

Ditelusuri lewat panggilan API langsung (terautentikasi, dari sesi browser yang sama) ke endpoint
`GET tariffs/options`:

- Tanpa filter unit layanan/klinik/kelas pasien, pencarian `search=a` mengembalikan **351.749**
  tarif.
- Dengan `serviceUnitId` milik kunjungan pasien sungguhan (`AGNES YULIANI RAJA GUK GUK`, kunjungan
  `ENC-RSMMC-00170`) ditambahkan — persis seperti yang dikirim `FE-BKC-014` sesuai desainnya
  (`BKC-DEC-061`) — hasilnya **0** tarif.

Akar masalahnya: kolom `ServiceUnitId`/`ClinicId`/`PatientClassId` pada `MstTariff` bersifat
opsional (`Guid?`) — kosong (`null`) berarti "tarif ini berlaku untuk **semua** unit
layanan/klinik/kelas pasien" (tarif global), bukan "tidak berlaku di mana pun". Ini sudah
diterapkan dengan benar di tempat lain (`InsuranceCoverageService.FindDrugTariffAsync`/
`FindProcedureTariffAsync`, dipakai mesin resolusi coverage), tapi filter pada
`TariffController.ApplyStandardFilter` memakai **strict equality** — hanya mencocokkan tarif yang
`ServiceUnitId`-nya PERSIS sama dengan yang dikirim, mengabaikan kemungkinan tarif itu global.
Karena hampir seluruh data tarif di database development memang berscope `null` (tarif umum, bukan
tarif khusus satu unit), filter yang salah ini secara efektif menyaring habis semua data begitu
konteks kunjungan (yang selalu punya `ServiceUnitId` terisi) ikut dikirim.

---

## 2. Proses bisnis

**Tujuan**: memastikan pencarian tarif berdasarkan konteks kunjungan (unit layanan/klinik/kelas
pasien) benar-benar mengembalikan tarif yang relevan — baik yang secara eksplisit dikhususkan untuk
konteks itu, maupun tarif umum yang berlaku di mana saja.

**Pelaku**: tidak ada aktor manusia langsung pada perbaikan ini — dampaknya otomatis berlaku untuk
setiap pemanggilan `GET tariffs` maupun `GET tariffs/options` yang menyertakan filter
`serviceUnitId`/`clinicId`/`patientClassId`.

**Langkah** (di dalam `ApplyStandardFilter`):

1. **[TIDAK BERUBAH]** Filter `tariffCategoryId`, `procedureId`, `drugId` tetap strict equality —
   ketiganya adalah identitas tarif (tarif ini "untuk" kategori/procedure/drug tertentu), bukan
   scope kewilayahan, jadi tarif yang tidak terhubung ke procedure/drug tertentu memang seharusnya
   tidak ikut muncul saat memfilter procedure/drug tertentu.
2. **[BERUBAH]** Filter `serviceUnitId`: sebelumnya `x.ServiceUnitId == value`, sekarang
   `!x.ServiceUnitId.HasValue || x.ServiceUnitId == value` — tarif global (null) maupun tarif yang
   eksplisit untuk unit itu sama-sama muncul.
3. **[BERUBAH]** Filter `clinicId` dan `patientClassId`: pola yang sama persis.

**Aturan yang berlaku**: null pada field scope tarif = "berlaku di mana saja", konsisten dengan
`InsuranceCoverageService` yang sudah lebih dulu menerapkan pola ini untuk kalkulasi coverage.

**Status yang dihasilkan**: tidak ada — murni perbaikan hasil query baca.

**Jalur tidak normal**: tidak berubah.

**Hasil akhir**: `GET tariffs/options?serviceUnitId=<milik kunjungan>` sekarang mengembalikan tarif
global **dan** tarif yang eksplisit untuk unit itu, bukan kosong.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`Areas/HealthServices/MasterData/Controllers/TariffController.cs` (sudah dibaca penuh saat
`FE-BKC-014`, termasuk `ApplyStandardFilter`, `GetTariffs`, `GetTariffOptions`);
`Areas/HealthServices/MasterData/Models/MstTariff.cs` (nullability `ServiceUnitId`/`ClinicId`/
`PatientClassId`); `Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs`
(pola null-is-universal yang sudah ada sebagai referensi, `FindDrugTariffAsync`/
`FindProcedureTariffAsync`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Controllers/TariffController.cs` | `ApplyStandardFilter`: filter `serviceUnitId`/`clinicId`/`patientClassId` diubah dari strict equality menjadi null-is-universal (`!x.Field.HasValue \|\| x.Field == value`), dipakai bersama oleh `GetTariffs` dan `GetTariffOptions` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — request/response shape `GET tariffs` dan `GET tariffs/options` tidak berubah sama sekali, hanya HASIL query untuk kombinasi filter yang sama yang berubah (bertambah, tidak pernah berkurang, karena tarif yang sebelumnya cocok strict-equality tetap cocok pada kondisi baru) |
| Database | `NOT APPLICABLE` — tidak ada model/migration/schema yang disentuh |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru atau berubah kontraknya. Endpoint yang perilakunya (hasil query) berubah:

#### Health Services / Master Data / Tariff

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar tarif berpaging — filter `serviceUnitId`/`clinicId`/`patientClassId` kini juga menyertakan tarif global | `Tariff : Read` (tidak berubah) |
| `GET` | `/options` | Pilihan tarif untuk dropdown/picker — filter yang sama, dipakai `FE-BKC-014` | `Tariff : Read` (tidak berubah) |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Sengaja tidak dijalankan** | `NOT RUN` | Instruksi eksplisit pengguna berlaku sepanjang sesi ini |
| Reproduksi bug SEBELUM perbaikan (bukti akar masalah) | `serviceUnitId` encounter sungguhan → 0 hasil; tanpa filter → 351.749 hasil | `CONFIRMED` (bug nyata, bukan dugaan) | Panggilan `fetch` terautentikasi dari sesi browser Playwright yang sudah login sebagai `superadmin@admin.com`, langsung ke `https://localhost:7184/api/v1/health-services/master-data/tariffs/options` |
| Reproduksi ulang SETELAH perbaikan (proses backend yang sedang berjalan) | **Belum bisa** — proses `QuilvianSystemBackend.exe` (PID 10976) yang melayani `https://localhost:7184` adalah binary lama, tidak otomatis memuat source yang baru diubah | `NOT RUN` | Perlu pengguna build ulang + restart proses backend terlebih dahulu |

Uji manual: `PARTIAL` — akar masalah dan efeknya pada `FE-BKC-014` sudah dibuktikan langsung
(bukan dugaan dari membaca kode saja), tapi perbaikannya sendiri belum bisa diklik-coba ulang
sampai backend di-build ulang dan proses yang berjalan di-restart.

**Tidak dijalankan:** `dotnet build`/`dotnet test` (keputusan sadar pengguna); restart proses
backend (di luar wewenang task ini — proses yang sedang berjalan milik pengguna, bukan sesuatu yang
dibuat/dikendalikan sesi ini).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Filter `serviceUnitId`/`clinicId`/`patientClassId` mengikuti semantik null-is-universal, konsisten dengan `InsuranceCoverageService` | Terpenuhi (source) | Diff `ApplyStandardFilter` § 3.2 — pola persis sama dengan `FindDrugTariffAsync`/`FindProcedureTariffAsync` |
| `tariffCategoryId`/`procedureId`/`drugId` TIDAK ikut berubah (bukan field scope, strict equality tetap benar) | Terpenuhi | Diff § 3.2 hanya menyentuh 3 dari 6 kondisi filter |
| Bug yang memblokir `FE-BKC-014` teratasi | **Belum diverifikasi ulang** — akar masalah dan perbaikannya sudah benar secara logis (mirroring pola yang sudah terbukti benar di modul lain), tapi belum dibuktikan lewat request nyata terhadap proses backend yang sudah memuat perbaikan ini | Lihat § 5 dan § 7 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` — perbaikan ini sengaja dipersempit hanya pada 3 field scope; tidak menyentuh field identitas (`tariffCategoryId`/`procedureId`/`drugId`) karena semantiknya memang berbeda (lihat § 2 langkah 1) |
| Risiko tersisa | Perbaikan ini juga mengubah hasil `GET tariffs` (bukan cuma `/options`) — endpoint yang sama dipakai layar master data tarif untuk admin. Efeknya seharusnya murni menambah baris yang sebelumnya salah tersaring (lihat § 3.3), tapi belum diverifikasi lewat klik-coba layar itu secara spesifik |
| Perubahan sampingan | `NONE` |
| Interupsi | Task ini murni lahir dari temuan tak terduga saat verifikasi manual `FE-BKC-014` (bukan interupsi teknis) — dikonfirmasi eksplisit ke pengguna lewat pertanyaan sebelum implementasi dimulai (lihat riwayat percakapan; pengguna memilih "Perbaiki backend sekarang") |
| Status Git | `On branch Yasmina, up to date with 'origin/Yasmina'`. File ini menambah 1 modified file baru (`Areas/HealthServices/MasterData/Controllers/TariffController.cs`) di atas working tree `BE-BKC-018`–`021` yang sudah ada sebelumnya pada sesi yang sama — lihat `BE-BKC-021.md` § 7 untuk daftar lengkap sebelumnya. Ditambah laporan ini sendiri (untracked) |
| Langkah berikutnya | 1) **Pengguna build ulang dan restart proses backend** (`QuilvianSystemBackend.exe`, PID 10976 saat ini berjalan dari binary lama). 2) Setelah restart, verifikasi ulang dengan langkah yang sama persis dengan yang menemukan bug ini: buka form "Buat Invoice Manual", pilih kunjungan pasien mana pun, buka dropdown "Tarif Layanan", ketik satu huruf apa pun — harus muncul hasil (bukan kosong). 3) Lanjutkan verifikasi `FE-BKC-014` yang sempat terhenti (pilih tarif, cek harga terisi otomatis, submit, cek invoice tersimpan) — lihat laporan `FE-BKC-014.md` untuk detail lengkap begitu ditulis |
