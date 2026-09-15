# Laporan Perubahan Backend — `BE-BKC-056`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-056` |
| **Judul** | Pembersihan endpoint dan hak akses persetujuan |
| **Slice** | `MVP-21` — eksekusi gelombang 3 (sejajar `BE-BKC-057`, sesudah `BE-BKC-055`) |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md`, kartu `BE-BKC-056` |
| **Trace** | `FR-BKC-088`; `PC-DES-021`, `PC-DEC-024` |
| **Contract version** | `BIL-API-1.1` (penghapusan `POST /vouchers/{id}/approve`/`.../reject`); `BIL-PERMISSION-0.9` — `approved` 15 September 2026 |
| **Dependency** | `BE-BKC-055` — ✅ selesai (kedua method service sudah hilang, lihat `BE-BKC-055.md`); `{PC-OQ-007}` — ✅ **DITUTUP** 15 September 2026 (bagian 2) |
| **Klasifikasi** | `LIGHT` — sebagian besar scope task ini (penghapusan action controller) **sudah dikerjakan lebih dulu** pada `BE-BKC-055` karena keterpaksaan kompilasi; sisa scope task ini murni verifikasi peran dan pembersihan baris database warisan, bukan penulisan source baru |
| **Task mode** | `BACKEND` — repository `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Target tulis** | Laporan task ini, baris status pada roadmap serta `requirement-traceability.md`; **nol source baru** — source-nya sudah ditulis `BE-BKC-055` |
| **Model** | Claude Sonnet 5 |
| **Commit backend saat dikerjakan** | `22441de9e0733e75e2ffb46e2cb8ce58da57166f` (branch `Yasmina`) |
| **Tanggal** | 15 September 2026 |
| **Status** | ✅ **SELESAI 15 September 2026.** Kedua endpoint (`approve`/`reject`) dan atribut `[AccessAction]`/`[AccessPermission]`-nya sudah hilang dari `PettyCashVouchersController` (ditarik maju `BE-BKC-055`, diverifikasi ulang lewat `grep` sesi ini — nol sisa referensi). `PC-OQ-007` (pemeriksaan Departemen × Posisi yang hanya memegang `Approve`/`Reject`) **dijalankan pengguna sendiri**: 0 baris ditemukan — tidak ada peran yang kehilangan seluruh aksesnya. SQL pembersihan baris `SysActionAccess`/`SysAccessPolicy` warisan **dijalankan pengguna**: 2 baris diperbarui. Kelima permission baru (`PettyCashVoucher : Return`/`Reverse` dari `BE-BKC-057`; `PettyCashBudget : Create`/`Activate`/`Close` dari `BE-BKC-054`) sudah terdaftar via `AccessMenuSeeder` berbasis pemindaian atribut, tanpa perlu source tambahan pada task ini |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` (registry) → `PettyCash / Vouchers` |
| **Owner / Prefix Registry** | Prefix `Bil` — sudah terdaftar |
| **Keberlakuan** | `TOUCHED LEGACY` — penghapusan dua endpoint dan dua butir hak akses warisan dari aggregate yang sudah ada |
| **QBE ID yang berlaku** | `QBE-PERM-001` (pembersihan permission mengikuti pola baku `AccessMenuSeeder`) |
| **Pengecualian / Temuan** | Nol source baru ditulis pada task ini — seluruh penghapusan controller **sudah dilakukan** `BE-BKC-055` karena controller tidak dapat dikompilasi memanggil method service yang sudah dihapus lebih dulu di sana. Task ini murni menuntaskan dua syarat non-source yang tersisa: verifikasi peran (`PC-OQ-007`) dan pembersihan baris database warisan |

---

## 1. Masalah yang diperbaiki

Setelah `PC-DEC-016` mencabut gerbang persetujuan (`BE-BKC-055`), dua butir hak akses lama (`PettyCashVoucher : Approve`, `Reject`) menjadi mati — tidak ada endpoint lagi yang memakainya. Risiko konkretnya: bila ada Departemen × Posisi yang **hanya** memegang salah satu dari kedua butir itu tanpa butir Petty Cash lain, peran itu akan kehilangan **seluruh** aksesnya ke modul ini secara diam-diam begitu butir itu dihapus — layarnya hanya menjadi kosong tanpa pesan apa pun. `PC-OQ-007` adalah pertanyaan yang menahan task ini sampai pemeriksaan itu dijalankan dan dilaporkan.

---

## 2. Proses bisnis

**Pelaku.** Pemilik arsitektur backend (menjawab `PC-OQ-007`), dijalankan lewat query SQL langsung ke database oleh pengguna (governance Keselamatan Database).

**Langkah yang terjadi sesi ini.**

1. Query pemeriksaan Departemen × Posisi yang **hanya** memegang `PettyCashVoucher : Approve` atau `Reject` dan tidak memegang butir Petty Cash lain dijalankan pengguna → **0 baris ditemukan**. Tidak ada peran yang akan kehilangan seluruh aksesnya.
2. SQL pembersihan baris `SysActionAccess`/`SysAccessPolicy` warisan `Approve`/`Reject` dijalankan pengguna → **2 baris diperbarui**.
3. Dengan `PC-OQ-007` tertutup dan controller sudah bersih (`BE-BKC-055`), tidak ada lagi pekerjaan source yang tersisa untuk task ini.

**Hasil akhir.** Endpoint Setujui dan Tolak beserta kedua butir hak aksesnya hilang dari sistem, tanpa meninggalkan peran yang kehilangan seluruh aksesnya diam-diam — dibuktikan lewat hasil query 0 baris sebelum pembersihan dijalankan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / Dokumen | Tujuan pemeriksaan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` | Kartu task `BE-BKC-056`: scope, blocker `PC-OQ-007`, DoD |
| `docs/module-blueprints/billing-kasir/contracts/permission-audit-matrix.md` | Baris permission `Approve`/`Reject` yang harus hilang, dan lima permission baru yang harus ada |
| `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashVouchersController.cs` | **Verifikasi ulang** (bukan penulisan) — konfirmasi nol sisa action/atribut `Approve`/`Reject`; konfirmasi lima permission baru (`Return`, `Reverse`, dan tiga permission `PettyCashBudget` pada controller lain) sudah terdaftar via atribut |
| `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashBudgetController.cs` | Konfirmasi `Create`/`Activate`/`Close` (`BE-BKC-054`) sudah memakai `[AccessAction]`/`[AccessPermission]` yang benar |
| `AccessMenuSeeder` (pola pendaftaran) | Konfirmasi pendaftaran permission berbasis pemindaian atribut saat startup, bukan seed data manual — sehingga kelima permission baru otomatis terdaftar tanpa source tambahan |
| `git status --short`, `git log` | State Git saat ini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | **Tidak ada berkas source diubah pada task ini.** Penghapusan action `Approve`/`Reject` sudah dilakukan `BE-BKC-055` (lihat `BE-BKC-055.md` bagian 3.2). Task ini murni verifikasi ulang dan tindakan database non-source |

Tindakan database yang dijalankan **pengguna** (bukan agent, sesuai governance Keselamatan Database): query pemeriksaan peran (0 baris) dan `UPDATE` pembersihan `SysActionAccess`/`SysAccessPolicy` (2 baris).

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Kedua endpoint (`approve`/`reject`) sudah tidak ada — breaking change yang sudah terdaftar dan dieksekusi lewat `BE-BKC-055` |
| Database | Dua baris `SysActionAccess`/`SysAccessPolicy` warisan diperbarui oleh pengguna. **Bukan migration** — ini baris data operasional (role-access matrix), bukan skema |
| Keamanan/Auth | `PC-OQ-007` tertutup dengan bukti 0 peran yang hanya bergantung pada `Approve`/`Reject`. Tidak ada peran yang kehilangan akses tanpa diketahui |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini menghapus endpoint, tidak menambah. Kedua endpoint yang hilang (`POST /vouchers/{id}/approve`, `.../reject`) didokumentasikan penghapusannya pada `BE-BKC-055.md` bagian 4 (tidak lagi dicantumkan).

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `grep` ulang `PettyCashVouchersController.cs` untuk sisa `Approve`/`Reject` | Nol kecocokan | `PASS` | Pencarian sesi ini — daftar action yang ada: `Read` ×4, `Create`, `Cancel`, `Disburse`, `AttachProof`, `Return`, `Reverse` (baris 24–130) |
| Query SQL `PC-OQ-007` — Departemen × Posisi yang hanya memegang `Approve`/`Reject` | 0 baris | `PASS` | Dijalankan pengguna, hasil discreenshot ke sesi ini |
| SQL pembersihan `SysActionAccess`/`SysAccessPolicy` warisan | 2 baris diperbarui | `PASS` | Dijalankan pengguna, hasil discreenshot ke sesi ini |
| Konfirmasi kelima permission baru terdaftar via atribut | `PettyCashVoucher : Return`/`Reverse` (`PettyCashVouchersController.cs` baris 117–130); `PettyCashBudget : Create`/`Activate`/`Close` (`PettyCashBudgetController.cs`, `BE-BKC-054`) | `PASS` | Pembacaan kode sesi ini |
| `dotnet build` | Berhasil | `PASS` | Dijalankan pengguna, dikonfirmasi "Udah sya build dan lakukan migration" — mencakup source `BE-BKC-055` yang menjadi dasar task ini |

Uji manual: `NOT APPLICABLE` — tidak ada UI baru; verifikasi bersifat database dan konfigurasi.

**Tidak dijalankan oleh agent:** kedua query SQL dan `dotnet build` — seluruhnya dijalankan pengguna sesuai governance Keselamatan Database dan instruksi eksplisit.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `BIL-AT-102`, `BIL-AT-103` — kedua endpoint menghasilkan `404` | **Terpetakan ke source, belum diuji request HTTP sungguhan** | Kedua action sudah hilang dari controller — routing ASP.NET Core akan menghasilkan `404` untuk path yang tidak terdaftar, tetapi belum diverifikasi lewat request nyata pada sesi ini |
| DoD: kedua endpoint menghasilkan `404` | **Terpetakan ke source**, sama seperti di atas | — |
| DoD: lima butir hak akses baru terdaftar | **Terpenuhi** | Bagian 5 |
| DoD: laporan pemeriksaan peran dilampirkan | **Terpenuhi** | Bagian 5 — 0 baris |
| DoD: tidak ada peran yang kehilangan seluruh akses tanpa diberitahu | **Terpenuhi** | Dibuktikan lewat hasil query 0 baris **sebelum** pembersihan dijalankan |
| DoD: `dotnet build` lulus | **Terpenuhi** | Dikonfirmasi pengguna |
| DoD: `git status --short` dilaporkan | **Terpenuhi** | Bagian 7 |

Task ini ditandai `✅` pada roadmap. Satu butir (`BIL-AT-102`/`103` sebagai request HTTP sungguhan menghasilkan `404`) belum diuji langsung lewat request nyata — risikonya sangat rendah karena ASP.NET Core routing menghasilkan `404` otomatis untuk path yang tidak terdaftar pada controller, dan ini adalah perilaku framework yang tidak memerlukan logika kustom untuk gagal dengan benar.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` — `PC-OQ-007` tertutup bersih dengan 0 baris temuan |
| Risiko tersisa | `BIL-AT-102`/`103` belum diuji sebagai request HTTP sungguhan — risiko rendah, perilaku framework baku |
| Perubahan sampingan | `NONE` pada task ini sendiri — perubahan source sudah dicatat sebagai bagian `BE-BKC-055` di laporan itu, bukan disembunyikan di sini |
| Interupsi | `NONE` pada task ini — `PC-OQ-007` dan pembersihannya dijalankan pengguna secara langsung tanpa hambatan |
| Status Git | Tidak ada source baru dari task ini. Perubahan database (2 baris `SysActionAccess`/`SysAccessPolicy`) tidak tercermin di `git status` — itu data operasional, bukan berkas |
| Langkah berikutnya | Uji `BIL-AT-102`/`103` sebagai request HTTP sungguhan begitu environment tersedia; tidak ada pekerjaan backend lain tersisa untuk task ini |
