# Laporan Perubahan Backend — `BE-RHB-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-015` |
| Judul | Ringkasan akhir, penutupan episode, pembaca berkas RM |
| Slice | S-E — Reasesmen dan penutupan |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-015` |
| Trace | FR-RHB-050..052, 060, 061; FEAT-026..029; RHB-DEC-019; INT-RHB-06; V-DIS-01/02; UAT-13, 14, 16 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — grup API *Rehab Discharge Summary*; integrasi INT-RHB-06 |
| Dependency | `BE-RHB-012` ✅, `BE-RHB-014` ✅ (2026-11-09); persetujuan pemilik RM `RHB-OQ-012` ✅ (2026-11-06) |
| Klasifikasi | `STANDARD` — endpoint transaksi baru + satu `IMedicalRecordDocumentSource` baru di modul Rehab; tanpa schema baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RehabilitationManagement/{Controllers,Services,DTOs}`, `Program.cs`, dokumen blueprint. Tidak ada berkas modul Rekam Medis yang diubah. |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-09 |
| Status | **✅ Selesai** (2026-11-09). Build owner `PASS` (0 error; 426 warning bawaan, tidak ada di folder Rehab). QBE Strict `PASS`. Uji API: run utama 20/22 (dua FAIL berasal dari susunan skrip, dijelaskan di §5), uji ulang 8/8. Sapuan log: 905 baris log Rehab, 0 memuat data klinis. |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `NEW CODE` |
| Registry | `Rhb` `ACTIVE`. Tidak ada entity baru; `RhbDischargeSummary` dari `BE-RHB-002`. |
| Jenis capability | **Transaksi — sub-proses ter-scope induk, satu per episode.** `GET`/`PUT /by-episode/{episodeId}` dan `POST /by-episode/{episodeId}/sign`. Tidak ada `options`, `PATCH status`, maupun `DELETE`. |
| QBE yang berlaku | QBE-NAM, QBE-SVC-001, QBE-API-001, standar hak akses (`RehabilitationPermissions.DischargeSummary`), aturan transaksi gateway RM |
| Lintas modul | Tulis lewat gateway: `MrcClinicalDocumentIntegrity` jenis 51. **Pembaca berkas RM** `RhbMedicalRecordDocumentSource` didaftarkan sebagai `IMedicalRecordDocumentSource` di `Program.cs` dengan persetujuan pemilik RM (RHB-DEC-019, bukti `RHB-OQ-012`). Pembaca ini hanya membaca, dan tidak menyentuh berkas modul RM. |

## 1. Masalah yang diperbaiki

Episode rehab belum bisa ditutup, dan dokumen rehab yang sudah final belum terlihat di berkas rekam medis
pasien. Penutupan harus memastikan tidak ada catatan yang tertinggal draft. Alasan penutupannya harus jelas,
dan program yang masih berjalan otomatis dinyatakan selesai.

**Contoh.** Episode `RHB-EPS-2026-000004` sudah punya program fisioterapi aktif, dan sebuah sesi masih
berlangsung. dr. Sp.KFR menyusun ringkasan akhir (layanan utama, capaian, kondisi akhir, kontrol 1 bulan)
lalu menandatangani. Permintaannya ditolak dengan daftar "Sesi terapi RHB-SES-2026-000006 — Berlangsung".

Setelah sesi ditandatangani dan reasesmen final, ringkasan ditandatangani. Hasilnya:
- episode *Ditutup* dengan alasan *Selesai secara klinis*;
- program menjadi *Selesai*;
- di berkas rekam medis pasien muncul seluruh dokumen rehab final;
- asesmen yang pernah diberi addendum bertanda *pernah diralat*.

## 2. Proses bisnis

1. **`GET`** mengembalikan ringkasan atau `exists = false`, beserta:
   - daftar catatan episode yang belum final (V-DIS-02);
   - penanda "siap ditutup" dari reasesmen medis (`BE-RHB-014`).
2. **`PUT`** menyimpan draft.
   - Simpan pertama membuat draft: kunjungan wajib (`RHB-VAL-081`), penulis = pengguna, dan didaftarkan ke RM jenis 51.
   - Simpan berikutnya hanya oleh penulis, dengan versi; kunjungan tidak dapat diganti.
   - Tanggal tindak lanjut tidak boleh lampau.
3. **`POST /sign`** (hanya penulis):
   - V-DIS-01: alasan penutupan, layanan utama, capaian, dan kondisi akhir wajib; tindak lanjut selain "tanpa" wajib instruksi; rujuk balik/ke fasilitas lain wajib tujuan (422 `RHB-VAL-061`);
   - episode yang belum diasesmen hanya boleh ditutup karena meninggal, menolak, pindah, atau putus layanan (422 `RHB-VAL-061`);
   - V-DIS-02: masih ada asesmen, edukasi, program, sesi, atau reasesmen yang draft → 409 `RHB-VAL-060` beserta daftarnya;
   - dalam satu transaksi: ringkasan *Final*, episode *Closed* (alasan, waktu, penutup), program *Active* → *Completed*, riwayat, lalu kunci RM.
4. **Sesudah ditutup**, semua pembuatan dokumen baru dijawab 409 `RHB-VAL-023`. Penjaga ini sudah ada di setiap service sejak task sebelumnya dan kini terbukti di runtime.
5. **Berkas RM** (INT-RHB-06) membaca dokumen final jenis 47–52:
   - asesmen, edukasi, sesi, reasesmen, dan ringkasan final;
   - program yang pernah diaktifkan, termasuk versi lama.

   Pasien diperiksa lewat episode, dan rincian untuk pasien lain dijawab `null` (404 di RM). Penanda *pernah diralat* diambil dari jumlah addendum.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Controllers/RehabDischargeSummaryController.cs` | Baru | 3 endpoint, Resource `RehabDischargeSummary`, aksi `Read`/`Update`/`Sign` |
| `Services/RhbDischargeService.cs` | Baru | Aturan §2 butir 1–3 |
| `Services/RhbMedicalRecordDocumentSource.cs` | Baru | `IMedicalRecordDocumentSource` untuk jenis 47–52: daftar (`ReadAsync`) dan rincian berpasangan label–nilai (`ReadDetailAsync`) |
| `DTOs/RehabDischargeSummaryDtos.cs` | Baru | `SaveRehabDischargeSummaryRequest`, `RehabDischargeSummaryResponse`, `RehabPendingDocument` |
| `Program.cs` | Diperbarui | `AddScoped<RhbDischargeService>()`, `AddScoped<IMedicalRecordDocumentSource, RhbMedicalRecordDocumentSource>()` |

### 3.2 Dampak kontrak, database, dan keamanan

| Hal | Isi |
| --- | --- |
| Schema / migration | Tidak ada |
| Hak akses | Controller baru dengan Read, Update, dan Sign. Tidak ada nama peran di kode. Berkas RM tetap dijaga butir `MedicalRecord : Read` milik RM. |
| Privasi | Pembaca RM hanya mengembalikan dokumen final milik pasien yang diminta |
| Logger | id, status episode, status rekam, alasan penutupan (kode). Lihat sapuan §5. |

### 3.3 Delta terhadap kontrak dan desain

| Delta | Rincian |
| --- | --- |
| `PUT` dijawab 201 saat membuat | Simpan pertama membuat dokumen, jadi jawabannya 201; simpan berikutnya 200 |
| `GET` memuat `pendingDocuments` dan `readyToCloseSignals` | Kontrak menyebut "beserta daftar prasyarat yang belum final". Penanda "siap ditutup" dari reasesmen medis ditambahkan sebagai bahan isian. |
| Program pernah aktif = final | Untuk berkas RM, program final adalah yang punya `ActivatedAt` (aktivasi = tanda tangan), termasuk versi *Superseded*/*Stopped*/*Completed* |
| Waktu final edukasi | Model edukasi tidak menyimpan waktu tanda tangan; diambil dari `SignedAt` baris keutuhan RM |
| Nama kelas service | Kartu menyebut `RhbDischargeService`, dan nama itu yang dipakai (arsitektur menyebut `RhbDischargeSummaryService`) |

## 4. Validasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| Build owner 2026-11-09 (`dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false`) | `PASS`, exit 0, **0 error**, 426 warning bawaan (tidak ada di folder Rehab), kompilasi penuh 3m46s. Sumber tidak disunting selama build. |
| QBE Strict | `PASS`. Controller tanpa `ApplicationDbContext`; pasangan `[AccessAction]`/`[AccessPermission]` identik; grep `IsInRole`/`UserType`/nama peran = 0. |
| **Sapuan log statis (UAT-16)** | Payload logger di **13 controller** Rehab hanya berisi id, nomor, kode status/keputusan/persetujuan/alasan, versi, atau jumlah. Tidak ada `LoggerService`/`ILogger` di service Rehab. |
| Review diff | 4 berkas baru, 1 berkas lama diubah (`Program.cs`) |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 5. Uji API manual

**Lingkungan.**
- Backend Debug di `https://localhost:7184`, DB dev `QuilvianNewDevSukma`, 2026-11-09.
- EP4 (`RHB-EPS-2026-000004`) dibangun lengkap dari *Menunggu asesmen medis* lewat API modul ini: asesmen medis → asesmen profesi → tujuan → program aktif → sesi.
- Akun uji berprofesi Dokter Umum; ringkasan akhir oleh SuperAdmin.

**Run utama (20/22)**

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Controller akses `RehabDischargeSummary` (Read/Update/Sign) terdaftar; akun uji login; EP4 sampai *Terapi berjalan* | PASS ×4 |
| 2 | `GET` sebelum ada ringkasan → `exists = false`, daftar belum final memuat sesi *Berlangsung*, aksi `update` | PASS |
| 3 | Buat tanpa kunjungan → 422 `081`; `PUT` pertama → 201, keutuhan *Draf* | PASS ×2 |
| 4 | V-DIS-01: belum lengkap → 422 `061` (alasan, capaian, kondisi); *Kontrol* tanpa instruksi → 422 `061` | PASS ×2 |
| 5 | Versi basi → 409 `010`; bukan penulis → 403 `022` | PASS ×2 |
| 6 | **AC2 / UAT-14**: sesi berlangsung → 409 `RHB-VAL-060` + daftar `[Sesi terapi RHB-SES-2026-000006 — Berlangsung]` | PASS |
| 7 | Setelah sesi dan reasesmen final → daftar belum final kosong | PASS |
| 8 | **AC1 / UAT-13**: tanda tangan → *Final*, keutuhan *Ditandatangani*; EP4 *Ditutup* (*Selesai secara klinis*); program aktif → *Selesai*; riwayat `open → sign-medical-assessment → activate-program → sign-discharge-summary` | PASS ×2 |
| 9 | **AC3**: edukasi, tujuan, reasesmen, dan ringkasan pada episode ditutup → 409 `RHB-VAL-023` (4/4) | PASS |
| 10 | EP2 (belum diasesmen): aturan alasan penutupan | **FAIL** (salah susun skrip): draft memakai *Kontrol* tanpa instruksi, sehingga 422 datang dari V-DIS-01, bukan aturan alasan. Penutupan EP2 dengan *Pasien menolak* → 200 *Ditutup* tetap berhasil. Aturan alasan diuji ulang. |
| 11 | EP1 dengan draft reasesmen residu | **FAIL** (salah susun skrip): penyebab yang sama, V-DIS-01 menolak lebih dulu. Diuji ulang. |
| 12 | **AC5**: berkas RM pasien A memuat keenam jenis dokumen rehab final (asesmen 4, program 6, sesi 5, reasesmen 5, ringkasan 2, edukasi 5) | PASS |
| 13 | **AC5**: asesmen medis EP1 yang diberi addendum (`BE-RHB-009`) bertanda pernah diralat | PASS |
| 14 | **AC4**: rincian ringkasan EP4 untuk pasien pemilik → 200 dengan bagian (Episode, Alasan penutupan, Layanan utama, …); untuk pasien B → **404** | PASS |
| 15 | Ringkasan draft (EP1) tidak terbaca di berkas RM → 404 | PASS |

**Uji ulang (8/8)**

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | **V-DIS-02**: EP1 lengkap, tetapi ada 2 draft reasesmen residu (`BE-RHB-014` §6) → 409 `060`, daftar `[Reasesmen: Draf, Reasesmen: Draf]`; EP1 tetap *Terapi berjalan* | PASS ×2 |
| 2 | Episode baru `RHB-EPS-2026-000005` dari rujukan eksternal → *Menunggu asesmen medis* | PASS |
| 3 | Belum diasesmen + *Selesai secara klinis* / *Dihentikan* / *Dirujuk* → 422 `061` (`closureReason`) | PASS ×3 |
| 4 | V-DIS-01: rujuk ke fasilitas lain tanpa tujuan → 422 `061` (`followUpTargetText`) | PASS |
| 5 | Belum diasesmen + *Pindah fasilitas* → 200 *Ditutup* | PASS |

**AC6 / UAT-16 — sapuan log runtime (seluruh modul).**
- Berkas log aplikasi `Logs/quilvian-backend-20261106.json` dan `…20261109.json` (seluruh uji Rehab BE-RHB-007..015) memuat **905** baris log Rehab (651 + 254).
- Pencarian penanda klinis dari skrip uji (teks riwayat/temuan/kesimpulan, ukuran, kondisi, nama penerima edukasi, nama pasien ANISA PRAMESTI, alasan klinis) menemukan **0** baris.
- Baris log aplikasi berisi modul, aksi, pesan tetap, path, dan identitas pengguna saja.

## 6. Bukti database (kueri READ ONLY)

| Data | Hasil |
| --- | --- |
| Episode + ringkasan + keutuhan jenis 51 | EP2 *Closed* (alasan 7 *Pasien menolak*); EP4 *Closed* (alasan 1); EP5 *Closed* (alasan 4 *Pindah fasilitas*, tindak lanjut 4). Ketiga ringkasan *Final* dengan keutuhan *Signed* oleh penulis. EP1 tetap *Active* dengan ringkasan *Draf*. |
| Program EP4 | 1 program *Completed* lewat penutupan (riwayat `discharge`) |
| `RhbStatusHistory` | `sign-discharge-summary` ×3, beserta alasan penutupan dalam bentuk label |
| Keutuhan RM jenis 47–52 | Semua dokumen rehab terdaftar: *Signed* untuk final, *Cancelled* untuk yang dibatalkan, *Draft* hanya untuk 2 reasesmen residu dan ringkasan EP1 |

**Pembersihan.**
- 0 dari 27 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pasangan uji: 0 baris;
- tidak ada layanan uji yang aktif;
- backend dihentikan.

Episode EP2, EP4, dan EP5 sekarang tertutup sebagai data uji. EP1 masih terbuka karena residu draft.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-13 | Terpenuhi | Run utama #8, §6. Variasi: reasesmen bertanda *Lanjutkan*, sehingga program diselesaikan oleh penutupan. Efek reasesmen *Selesai* (program → *Completed*) memakai cabang `BE-RHB-014` dan belum dijalankan di runtime. |
| (2) UAT-14 (409 + daftar) | Terpenuhi | Run utama #6, uji ulang #1 |
| (3) Dokumen baru pada episode `Closed` → 409 `RHB-VAL-023` | Terpenuhi | Run utama #9 |
| (4) `ReadDetailAsync` pasien lain → `null` | Terpenuhi | Run utama #14 (404 dari RM) |
| (5) Berkas RM menampilkan dokumen final rehab beserta addendumnya | Terpenuhi | Run utama #12–13, #15 |
| (6) UAT-16: log bebas data klinis (sapuan seluruh modul) | Terpenuhi | §4 (statis) dan §5 (runtime, 905 baris) |
| DoD: build owner, laporan | Terpenuhi | §4, berkas ini |

## 8. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| Celah balapan penutupan | V-DIS-02 diperiksa sebelum transaksi penutupan. Draft yang dibuat tepat di antara pemeriksaan dan penyimpanan (milidetik) tidak tertolak, dan sesudahnya tertahan oleh `RHB-VAL-023`. Dampaknya kecil karena draft itu tidak bisa diubah lagi; bila perlu, tambahkan kunci episode bersama di semua pembuat dokumen. |
| EP1 terbuka | Ditahan 2 draft reasesmen residu yang tidak bisa dibatalkan (penulisnya akun uji nonaktif). Perlu pembersihan manual DB bila EP1 ingin ditutup. |
| Billing | Sesi final tetap belum tertagih sampai `BE-RHB-013` (menunggu `RHB-OQ-015`) |
| Status backend modul | Seluruh task backend selesai kecuali `BE-RHB-013` (⛔ `RHB-OQ-015`) |
| Task berikutnya | Frontend: `FE-RHB-005` dan `FE-RHB-006` 🟡; `FE-RHB-011` menunggu `FE-RHB-008` |
