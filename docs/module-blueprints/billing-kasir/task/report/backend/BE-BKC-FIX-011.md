# Laporan Perubahan Backend — `BE-BKC-FIX-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-011` |
| Judul | Perbaikan migration designer dan penyelarasan snapshot — konfirmasi pencairan Petty Cash Voucher |
| Slice | Perbaikan artefak EF Core (bukan slice fitur baru) untuk migration `20260924080000_AddPettyCashVoucherConfirmationFields` |
| Roadmap | Tidak ada baris roadmap resmi (`backend-roadmap.md`) untuk task ini — migration `20260924080000` sudah ada di source sebelum task ini dimulai, tanpa task ID roadmap yang menyertainya. Dicatat sebagai perbaikan ad-hoc mengikuti pola `BE-BKC-FIX-XXX` yang sudah dipakai modul ini (`BE-BKC-FIX-001`–`010`), dengan Task ID `BE-BKC-FIX-011` ditetapkan dan di-*acc* langsung oleh pemilik modul pada sesi ini (2026-09-25). **Koreksi penomoran**: task ini sempat salah diberi label `BUI-DES-001` pada sesi yang sama — ID tersebut sudah dipakai untuk gerbang approval arsitektur gelombang UI Billing `MVP-30`–`33` (lihat `requirement-traceability.md` § Gelombang `MVP-30` s.d. `MVP-33`) dan tidak berkaitan dengan task ini. Ditulis ulang di sini dengan ID yang benar; berkas lama `task/report/backend/BUI-DES-001.md` dihapus |
| Trace | `PC-DEC-016` (pencabutan gerbang persetujuan, revisi 15 September 2026) — kolom `ConfirmedByUserId`/`ConfirmedByName`/`ConfirmedAt` pada `BilPettyCashVoucher` merekam konfirmasi pencairan oleh Kepala Kasir/Supervisor Kasir |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API/endpoint yang diubah pada task ini |
| Dependency | Tidak ada task lain yang harus lebih dulu selesai; migration dan model sumbernya sudah ada sebelum task ini dimulai |
| Klasifikasi | `LIGHT` — satu repository (skor 0); berkas diperiksa 9–20 (skor 1: model, configuration, migration, snapshot, Designer lama); berkas diubah ≤3 (skor 0); logika bisnis tidak diubah (skor 0); kontrak API tidak disentuh (skor 0); database — penyelarasan artefak migration dengan model yang sudah ada, bukan perancangan schema baru (skor 0); keamanan/auth `NOT APPLICABLE` (skor 0); UI/workflow `NOT APPLICABLE` (skor 0). Total skor 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Migrations/**` (perbaikan berkas migration/designer/snapshot) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | ✅ **SELESAI 25 September 2026.** Seluruh Definition of Done terpenuhi. `dotnet build` dijalankan pengguna sendiri dan **lulus** — dikonfirmasi 25 September 2026 dalam konteks penutupan task `BE-BKC-060`–`063` pada solution backend yang sama (satu solution, satu build) |

---

## 1. Masalah yang diperbaiki

Migration `20260924080000_AddPettyCashVoucherConfirmationFields.cs` sudah ada di source dan menambahkan tiga kolom pada tabel `BilPettyCashVoucher` (`ConfirmedByUserId`, `ConfirmedByName`, `ConfirmedAt`) — kolom ini merekam siapa Kepala Kasir/Supervisor Kasir yang menekan tombol "Cairkan" dan kapan itu terjadi. Namun dua artefak pendampingnya rusak:

1. **`20260924080000_AddPettyCashVoucherConfirmationFields.Designer.cs`** hanya berisi stub kosong (950 byte) — tidak memuat satu pun definisi tabel. Bagi Entity Framework Core, file ini seharusnya memotret **seluruh** model database persis pada titik migration tersebut diterapkan; isi kosong berarti EF akan salah membaca riwayat schema setiap kali tooling migration dijalankan berikutnya (misalnya saat developer lain membuat migration baru, EF bisa salah menghitung perbedaan schema dan menghasilkan migration yang keliru).
2. **`ApplicationDbContextModelSnapshot.cs`** (potret model gabungan seluruh migration) sama sekali tidak mencantumkan ketiga kolom ini — padahal migration-nya sudah diterapkan di source. Snapshot yang tidak sinkron berisiko membuat migration berikutnya mencoba menambahkan ulang kolom yang sebenarnya sudah ada, atau sebaliknya menghapusnya secara tidak sengaja.

Dampak nyata bagi tim: siapa pun yang menjalankan `dotnet ef migrations add <nama-baru>` setelah ini berisiko mendapat migration yang salah (mencoba menambah ulang tiga kolom konfirmasi ini, atau kehilangan kolom lain yang seharusnya tetap ada), karena EF membandingkan model C# saat ini terhadap snapshot yang keliru.

---

## 2. Proses bisnis

Tidak ada proses bisnis baru pada task ini — task ini murni memperbaiki artefak teknis EF Core agar mencerminkan proses bisnis yang **sudah** diimplementasikan sebelumnya (konfirmasi pencairan Petty Cash Voucher oleh Kepala Kasir/Supervisor Kasir, `PC-DEC-016`). Tidak ada perubahan pada `PettyCashVoucherService`, controller, DTO, atau aturan status.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashVoucher.cs`
- `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashVoucherConfiguration.cs`
- `Migrations/20260924080000_AddPettyCashVoucherConfirmationFields.cs`
- `Migrations/20260924080000_AddPettyCashVoucherConfirmationFields.Designer.cs` (versi stub sebelum perbaikan)
- `Migrations/ApplicationDbContextModelSnapshot.cs`
- Contoh pola Designer.cs migration lain yang benar (`20260924054559_AddInpatientBillingIntegrationAndClearanceHandoff.Designer.cs`) sebagai referensi bentuk

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20260924080000_AddPettyCashVoucherConfirmationFields.Designer.cs` | Dibangun ulang dari stub kosong menjadi potret model lengkap per titik migration ini — mencakup properti `ConfirmedAt` (`DateTimeOffset?`), `ConfirmedByName` (`character varying(150)`, nullable), `ConfirmedByUserId` (`Guid?`) pada `BilPettyCashVoucher`, konsisten dengan model dan konfigurasi Fluent API yang sudah ada. Dihasilkan dari `ApplicationDbContextModelSnapshot.cs` yang sudah diperbaiki, dikurangi perubahan yang baru dibawa migration berikutnya (`20260924083000`), sehingga isinya persis mencerminkan state database tepat setelah migration ini diterapkan |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Ditambahkan tiga properti yang sebelumnya hilang pada entity `BilPettyCashVoucher`: `ConfirmedAt`, `ConfirmedByName` (maxlength 150), `ConfirmedByUserId` — tanpa index maupun foreign key, sesuai migration dan konfigurasi asli (kolom ini murni jejak audit, sengaja tidak diberi FK ke tabel user, lihat komentar model) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint atau payload yang berubah |
| Database | Tidak ada perubahan schema baru. Migration `20260924080000` itu sendiri **tidak diubah** isinya (Up/Down tetap seperti semula, sudah benar). Perbaikan murni pada artefak pendamping (`Designer.cs`) dan pada potret gabungan (`ModelSnapshot.cs`) agar konsisten dengan migration yang sudah ada. **Migration ini belum tentu sudah dijalankan (`dotnet ef database update`) ke database manapun** — task ini tidak menyentuh database sama sekali |
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
| Perbandingan properti `BilPettyCashVoucher` pada snapshot terhadap model C# (`BilPettyCashVoucher.cs`) dan konfigurasi (`BilPettyCashVoucherConfiguration.cs`) | Cocok — tipe, nullability, maxlength, tanpa index/FK tambahan | `PASS` | Pembacaan manual berdampingan atas ketiga berkas |
| `git status --short` menunjukkan scope perubahan sesuai task | Sesuai, tidak ada berkas di luar scope | `PASS` | Lihat §7 |

Uji manual: `NOT FEASIBLE` — task ini tidak mengubah perilaku runtime (tidak ada kode aplikasi yang berubah), sehingga tidak ada skenario UI/API yang bisa diuji manual.

**Tidak dijalankan:** `dotnet ef migrations list` dan seluruh eksekusi database (`dotnet ef database update`) — eksekusi migration/database tetap wewenang terpisah yang tidak diberikan pada task ini, terlepas dari `dotnet build` yang sudah lulus.

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`); tidak diminta secara eksplisit pada task ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `Migrations/20260924080000_AddPettyCashVoucherConfirmationFields.Designer.cs` berisi potret model lengkap, bukan stub | Terpenuhi | §3.2; `wc -l` menunjukkan 118.189 baris (setara berkas Designer.cs migration lain, bukan 29 baris seperti stub sebelumnya) |
| `ApplicationDbContextModelSnapshot.cs` mencantumkan `ConfirmedByUserId`, `ConfirmedByName`, `ConfirmedAt` pada `BilPettyCashVoucher` | Terpenuhi | §3.2 |
| Tidak ada perubahan pada Up/Down migration itu sendiri (migration ini sudah benar) | Terpenuhi | Migration `.cs` tidak disentuh, hanya `.Designer.cs` dan `ModelSnapshot.cs` |
| Build backend terverifikasi hijau | Terpenuhi | `dotnet build` dijalankan pengguna sendiri dan lulus, dikonfirmasi 25 September 2026 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` — `dotnet build` sudah dikonfirmasi lulus oleh pengguna 25 September 2026 |
| Masalah yang diketahui | Tidak ada task roadmap resmi yang menaungi migration `20260924080000` maupun task ini; ditandai sebagai ad-hoc dengan Task ID yang ditetapkan pemilik modul. Task ini sempat salah diberi label `BUI-DES-001` (lihat §Metadata baris Roadmap) — dikoreksi pada sesi yang sama sebelum ada pembaca lain yang terpapar salah penomoran itu |
| Risiko tersisa | Migration `20260924080000` sendiri belum dikonfirmasi sudah diterapkan (`dotnet ef database update`) ke database manapun — build lulus hanya membuktikan kompilasi source, bukan eksekusi database. Tetap wewenang terpisah, tidak termasuk scope task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna mengirim instruksi mid-turn untuk tidak menjalankan `dotnet build` secara otomatis — pekerjaan dilanjutkan tanpa build, memakai verifikasi manual/tekstual sebagai gantinya. Build kemudian dijalankan pengguna sendiri dan dikonfirmasi lulus pada sesi berikutnya (25 September 2026). Task ini juga sempat salah diberi Task ID (`BUI-DES-001`, sudah dipakai gerbang arsitektur modul lain) dan dikoreksi ke `BE-BKC-FIX-011` pada sesi yang sama |
| Status Git | `M Migrations/20260924080000_AddPettyCashVoucherConfirmationFields.Designer.cs`, `M Migrations/ApplicationDbContextModelSnapshot.cs` (berbagi berkas dengan `BE-BKC-FIX-012`, lihat laporan tersebut), `M Migrations/20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory.cs`, `?? Migrations/20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory.Designer.cs` |
| Langkah berikutnya | Task ini sudah tuntas. Pertimbangkan menambahkan baris resmi pada `backend-roadmap.md` untuk fitur konfirmasi pencairan (`PC-DEC-016`) bila belum ada, supaya migration ini tidak lagi berstatus ad-hoc di luar penomoran roadmap |
