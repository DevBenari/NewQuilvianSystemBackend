# Laporan Perubahan Backend — `BE-BKC-FIX-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-012` |
| Judul | Perbaikan migration designer, penyelarasan snapshot, dan koreksi panjang kolom `RefundCategory` — memo diskon dokter dan kategori refund |
| Slice | Perbaikan artefak EF Core (bukan slice fitur baru) untuk migration `20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory` |
| Roadmap | Tidak ada baris roadmap resmi (`backend-roadmap.md`) untuk task ini — migration `20260924083000` sudah ada di source sebelum task ini dimulai, tanpa task ID roadmap yang menyertainya. Dicatat sebagai perbaikan ad-hoc mengikuti pola `BE-BKC-FIX-XXX` yang sudah dipakai modul ini (`BE-BKC-FIX-001`–`010`), dengan Task ID `BE-BKC-FIX-012` ditetapkan dan di-*acc* langsung oleh pemilik modul pada sesi ini (2026-09-25). **Koreksi penomoran**: task ini sempat salah diberi label `BUI-DES-002` pada sesi yang sama — ID tersebut sudah dipakai untuk gerbang approval arsitektur gelombang UI Billing `MVP-30`–`33` (lihat `requirement-traceability.md` § Gelombang `MVP-30` s.d. `MVP-33`) dan tidak berkaitan dengan task ini. Ditulis ulang di sini dengan ID yang benar; berkas lama `task/report/backend/BUI-DES-002.md` dihapus |
| Trace | Terkait rumpun Diskon (`BKC-DEC-007`–`012`, task `BE-BKC-007`) untuk kolom `DoctorDiscountMemoFile`, dan rumpun Refund (`DEC-032`,`033`, task `BE-BKC-013`) untuk `RefundCategory`/`SelectedBillingItemIdsJson`/nullable `RefundableCreditId` — dokumen kontrak `api-contract.md`/`validation-matrix.md` tidak diperiksa ulang pada task ini karena scope-nya murni artefak migration, bukan endpoint |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API/endpoint yang diubah pada task ini |
| Dependency | Tidak ada task lain yang harus lebih dulu selesai; migration dan model sumbernya sudah ada sebelum task ini dimulai |
| Klasifikasi | `LIGHT` — satu repository (skor 0); berkas diperiksa 9–20 (skor 1: dua model, dua configuration, migration, snapshot); berkas diubah ≤3 (skor 0: migration `.cs` diperbaiki, `Designer.cs` baru, snapshot); logika bisnis tidak diubah (skor 0); kontrak API tidak disentuh (skor 0); database — penyelarasan artefak migration dengan model yang sudah ada plus satu koreksi maxlength (skor 0, bukan perancangan schema baru); keamanan/auth `NOT APPLICABLE` (skor 0); UI/workflow `NOT APPLICABLE` (skor 0). Total skor 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Migrations/**` (perbaikan berkas migration/designer/snapshot) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | ✅ **SELESAI 25 September 2026.** Seluruh Definition of Done terpenuhi. `dotnet build` dijalankan pengguna sendiri dan **lulus** — dikonfirmasi 25 September 2026 dalam konteks penutupan task `BE-BKC-060`–`063` pada solution backend yang sama (satu solution, satu build) |

---

## 1. Masalah yang diperbaiki

Migration `20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory.cs` sudah ada di source dan mengubah dua tabel: menambah `DoctorDiscountMemoFile` pada `BilDiscountApplication` (lampiran memo persetujuan diskon dokter), serta pada `BilRefundCase` — mengubah `RefundableCreditId` menjadi boleh kosong, menambah `RefundCategory` (kategori refund: `BILLING` atau `DEPOSITO`), dan menambah `SelectedBillingItemIdsJson` (daftar item tagihan terpilih untuk refund). Ditemukan tiga masalah:

1. **`Designer.cs` untuk migration ini tidak ada sama sekali** — seharusnya setiap migration EF Core punya potret model per titik migration tersebut, tapi berkas ini belum pernah dibuat.
2. **`ApplicationDbContextModelSnapshot.cs` tidak sinkron** — keempat perubahan kolom di atas sama sekali tidak tercermin di snapshot gabungan, padahal migration-nya sudah ada di source.
3. **Bug nyata pada migration itu sendiri**: kolom `RefundCategory` di-scaffold dengan panjang `character varying(20)` / `maxLength: 20`, padahal model `BilRefundCase.cs` (`[Required, MaxLength(30)]`) dan konfigurasi Fluent API-nya (`BilRefundCaseConfiguration.cs`, `.HasMaxLength(30)`) sama-sama menetapkan panjang 30. Contoh konkret dampaknya: nilai kategori `"DEPOSITO"` (8 karakter) sendiri masih muat di kedua ukuran, tapi bila kelak ditambahkan nilai kategori baru sepanjang 21–30 karakter sesuai kontrak model (`[MaxLength(30)]` mengizinkannya), migration yang membuat kolom database hanya `varchar(20)` akan menolaknya saat runtime — padahal validasi C# di layer aplikasi sudah meloloskannya karena mengacu ke `MaxLength(30)`. Ini adalah cacat kontrak model-vs-database klasik yang baru muncul saat data produksi menyentuh batas itu.

Dampak nyata bagi tim: migration berikutnya yang dibuat lewat `dotnet ef migrations add` berisiko salah menghitung perbedaan schema (menambah ulang kolom yang sudah ada, atau mencoba "memperbaiki" `RefundCategory` ke ukuran 30 sebagai perubahan yang tidak disengaja) karena EF membandingkan terhadap snapshot dan Designer.cs yang keliru/hilang.

---

## 2. Proses bisnis

Tidak ada proses bisnis baru pada task ini — task ini murni memperbaiki artefak teknis EF Core dan satu cacat panjang kolom, agar konsisten dengan proses bisnis yang **sudah** diimplementasikan sebelumnya:

- **Memo diskon dokter**: saat diskon jenis `DOCTOR` diajukan dan disetujui, lampiran bukti (`DoctorDiscountMemoFile`) dapat disertakan pada `BilDiscountApplication` — kolom ini nullable karena tidak semua jenis diskon (`PROMO_TOTAL`, `PROMO_ITEM`) memerlukan memo.
- **Kategori refund**: setiap `BilRefundCase` kini eksplisit membawa `RefundCategory` (`BILLING` untuk refund tagihan biasa, `DEPOSITO` untuk refund uang jaminan/deposit), dengan `SelectedBillingItemIdsJson` menyimpan item tagihan spesifik yang dipilih untuk refund kategori `BILLING`. `RefundableCreditId` menjadi boleh kosong karena refund kategori `BILLING` tidak selalu berasal dari kredit refundable yang sudah ada (`BilRefundableCredit`) — berbeda dengan refund kategori `DEPOSITO` yang selalu mengacu ke satu kredit tertentu.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/BillingManagement/Billing/Models/BilDiscountApplication.cs`
- `Areas/HealthServices/BillingManagement/Billing/Models/BilRefundCase.cs`
- `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilDiscountApplicationConfiguration.cs`
- `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilRefundCaseConfiguration.cs`
- `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingRefundDtos.cs` (memastikan nilai kategori yang dipakai, `"BILLING"`/`"DEPOSITO"`, konsisten dengan default migration)
- `Migrations/20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory.cs`
- `Migrations/ApplicationDbContextModelSnapshot.cs`
- Contoh pola Designer.cs migration lain yang benar (`20260924054559_AddInpatientBillingIntegrationAndClearanceHandoff.Designer.cs`) sebagai referensi bentuk

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory.cs` | Kolom `RefundCategory` diperbaiki dari `character varying(20)`/`maxLength: 20` menjadi `character varying(30)`/`maxLength: 30`, menyamakan dengan model `BilRefundCase.cs` dan `BilRefundCaseConfiguration.cs`. Bagian lain (Up/Down untuk `DoctorDiscountMemoFile`, `RefundableCreditId`, `SelectedBillingItemIdsJson`) sudah benar dan tidak diubah |
| `Migrations/20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory.Designer.cs` | Dibuat baru — memotret model lengkap tepat setelah migration ini diterapkan (migration terakhir saat ini), sehingga isinya identik dengan `ApplicationDbContextModelSnapshot.cs` yang sudah diperbaiki, hanya berbeda pada header kelas (`[Migration(...)]`, `BuildTargetModel`) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Ditambahkan `DoctorDiscountMemoFile` (`character varying(500)`, nullable) pada `BilDiscountApplication`; pada `BilRefundCase` — `RefundableCreditId` diubah jadi `Guid?` beserta relasi FK ke `BilRefundableCredit` yang tidak lagi wajib (`.IsRequired()` dihapus), ditambahkan `RefundCategory` (`character varying(30)`, wajib, default `"BILLING"`) dan `SelectedBillingItemIdsJson` (`text`, nullable) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint atau payload yang berubah pada task ini. Kontrak DTO (`BillingRefundDtos.cs`) sudah lebih dulu memakai `RefundCategory` sebelum task ini; tidak disentuh |
| Database | Satu koreksi schema: panjang kolom `RefundCategory` naik dari rencana 20 menjadi 30 karakter — perubahan ini ada di dalam migration yang **belum tentu sudah dijalankan** ke database manapun, sehingga koreksi ini tidak berdampak pada data yang sudah ada. Task ini tidak menjalankan `dotnet ef database update` maupun perintah database apa pun |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint apa pun.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Lulus | `PASS` | Dijalankan pengguna sendiri (bukan agent, sesuai instruksi "jangan lakukan build backend secara automatis" yang tetap berlaku sepanjang sesi), dikonfirmasi 25 September 2026 — satu solution yang sama dengan penutupan `BE-BKC-060`–`063` |
| Pemeriksaan brace balance `Designer.cs` hasil generate | 1723 `{` = 1723 `}` | `PASS` | Perintah `grep -o '{' \| wc -l` vs `grep -o '}' \| wc -l` pada berkas hasil |
| Pemeriksaan encoding/line ending (tanpa BOM, CRLF) konsisten dengan `Designer.cs` migration lain | Sesuai | `PASS` | `file` dan `xxd` atas 3 byte pertama, dibandingkan `20260924054559_...Designer.cs` |
| Diff `Designer.cs` migration ini terhadap `Designer.cs` migration `20260924080000` (`BE-BKC-FIX-011`) | Hanya berisi 4 perubahan yang memang dibawa migration ini — `DoctorDiscountMemoFile`, `RefundCategory`, `SelectedBillingItemIdsJson`, nullability `RefundableCreditId` — tidak ada perubahan liar lain | `PASS` | `diff` kedua berkas, diperiksa manual |
| Perbandingan properti `BilDiscountApplication`/`BilRefundCase` pada snapshot terhadap model C# dan konfigurasi Fluent API | Cocok — tipe, nullability, maxlength, default value, FK | `PASS` | Pembacaan manual berdampingan atas seluruh berkas terkait |
| Koreksi `RefundCategory` maxlength 20→30 pada migration disamakan dengan model | Selesai | `PASS` | §3.2 |
| `git status --short` menunjukkan scope perubahan sesuai task | Sesuai, tidak ada berkas di luar scope | `PASS` | Lihat §7 |

Uji manual: `NOT FEASIBLE` — task ini tidak mengubah perilaku runtime (tidak ada kode aplikasi yang berubah), sehingga tidak ada skenario UI/API yang bisa diuji manual.

**Tidak dijalankan:** `dotnet ef migrations list` dan seluruh eksekusi database (`dotnet ef database update`) — eksekusi migration/database tetap wewenang terpisah yang tidak diberikan pada task ini, terlepas dari `dotnet build` yang sudah lulus.

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`); tidak diminta secara eksplisit pada task ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `Migrations/20260924083000_...Designer.cs` dibuat dan memuat potret model lengkap | Terpenuhi | §3.2; berkas baru 118.201 baris |
| `ApplicationDbContextModelSnapshot.cs` mencantumkan keempat perubahan migration ini | Terpenuhi | §3.2 |
| Panjang kolom `RefundCategory` pada migration disamakan dengan model (`MaxLength(30)`) | Terpenuhi | §3.2, §1 butir 3 |
| Tidak ada perubahan pada bagian migration yang sudah benar (`DoctorDiscountMemoFile`, `RefundableCreditId`, `SelectedBillingItemIdsJson`) | Terpenuhi | Hanya blok `RefundCategory` yang diubah pada berkas `.cs` migration |
| Build backend terverifikasi hijau | Terpenuhi | `dotnet build` dijalankan pengguna sendiri dan lulus, dikonfirmasi 25 September 2026 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` — `dotnet build` sudah dikonfirmasi lulus oleh pengguna 25 September 2026. Bila migration `20260924083000` **sudah pernah dijalankan** ke database manapun sebelum koreksi maxlength ini, kolom fisik `RefundCategory` di database itu masih `varchar(20)` sampai migration baru (mis. `AlterColumn`) dibuat dan dijalankan — koreksi pada task ini baru berlaku untuk migration yang belum diterapkan |
| Masalah yang diketahui | Tidak ada task roadmap resmi yang menaungi migration `20260924083000` maupun task ini; ditandai sebagai ad-hoc dengan Task ID yang ditetapkan pemilik modul. Task ini sempat salah diberi label `BUI-DES-002` (lihat §Metadata baris Roadmap) — dikoreksi pada sesi yang sama sebelum ada pembaca lain yang terpapar salah penomoran itu |
| Risiko tersisa | Perlu dipastikan apakah migration ini sudah diterapkan ke database dev/staging manapun — bila sudah, koreksi maxlength di task ini perlu migration susulan (`AlterColumn`) yang belum dibuat pada task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna mengirim instruksi mid-turn untuk tidak menjalankan `dotnet build` secara otomatis — pekerjaan dilanjutkan tanpa build, memakai verifikasi manual/tekstual sebagai gantinya. Build kemudian dijalankan pengguna sendiri dan dikonfirmasi lulus pada sesi berikutnya (25 September 2026). Task ini juga sempat salah diberi Task ID (`BUI-DES-002`, sudah dipakai gerbang arsitektur modul lain) dan dikoreksi ke `BE-BKC-FIX-012` pada sesi yang sama |
| Status Git | `M Migrations/20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory.cs`, `?? Migrations/20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory.Designer.cs`, `M Migrations/ApplicationDbContextModelSnapshot.cs` (berbagi berkas dengan `BE-BKC-FIX-011`, lihat laporan tersebut), `M Migrations/20260924080000_AddPettyCashVoucherConfirmationFields.Designer.cs` |
| Langkah berikutnya | Task ini sudah tuntas. Pastikan juga apakah migration `20260924083000` sudah diterapkan ke database manapun — bila sudah, rencanakan migration susulan untuk `RefundCategory` ke `varchar(30)`. Pertimbangkan menambahkan baris resmi pada `backend-roadmap.md` untuk perluasan memo diskon dokter dan kategori refund bila belum ada |
