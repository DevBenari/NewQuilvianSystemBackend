# Laporan Perubahan Backend — `BE-SEC-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-SEC-018` |
| Judul | Closeout eksekusi pemeliharaan otorisasi pada database development |
| Slice | Task terpisah di luar rantai — **dokumentasi/bukti eksekusi**, bukan implementasi |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) — bagian *Task terpisah di luar rantai* |
| Trace | [`BE-SEC-014.md`](BE-SEC-014.md), [`BE-SEC-015.md`](BE-SEC-015.md), [`BE-SEC-016.md`](BE-SEC-016.md), [`BE-SEC-017.md`](BE-SEC-017.md), [`evidence/14`](../../../evidence/14-owner-policy-matrix-and-deployment-preparation.md) |
| Contract version | `NOT APPLICABLE` |
| Dependency | `BE-SEC-017` (`cb7f3fce`) — SHA sumber yang dipakai saat pemeliharaan dijalankan |
| Klasifikasi | `LIGHT` — **nol perubahan source aplikasi, nol perubahan semantik SQL** |
| Task mode | `BACKEND` |
| Target tulis | `docs/module-blueprints/platform-authorization/` saja |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `cb7f3fce62e533ae5233bca43a34f55da88f553e` |
| Branch | `wip/be-sec-003b-verifier-transfer` |
| Database | `QuilvianNewDevAndryZain` (development) |
| Tanggal | 17 September 2026 |
| Status | **Selesai.** Seluruh urutan tulis pemeliharaan otorisasi **SUDAH DIJALANKAN dan DI-COMMIT** |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | Shared Platform (otorisasi) + `Corporate` (HR) |
| Module | `PLATFORM_AUTHORIZATION`, `HUMAN_RESOURCE_MASTER_DATA`, `HUMAN_RESOURCE_SCHEDULING` |
| Keberlakuan | `TOUCHED LEGACY` — **nol berkas source aplikasi berubah pada task ini** |
| Status registry source | Tidak berubah: `1.300 / 340 / 48`, `BE-SEC-003` 24/24 |
| Wewenang database | **Sudah dipakai dan sudah selesai.** Task ini hanya mencatat hasilnya; tidak ada SQL yang dijalankan di sini |
| Pemilihan ID task | `BE-SEC-004`–`BE-SEC-011` dipesan sebagai dekomposisi `BE-SEC-002` (`NOT STARTED`); `BE-SEC-012`–`BE-SEC-017` terpakai. `BE-SEC-018` diverifikasi bebas pada empat sumber: roadmap, laporan tracked, working tree, dan riwayat Git |

---

## 1. Tiga jenis bukti — jangan dicampur

Dokumen ini mencatat tiga kelas bukti yang berbeda derajat kepastiannya. Membedakannya bukan
formalitas: satu-satunya yang membuktikan keadaan **database** adalah kelas ketiga.

| Kelas | Apa yang dibuktikan | Tidak membuktikan |
| --- | --- | --- |
| **S — Source/verifier** | Apa yang dideklarasikan source: `1.300 / 340 / 48`, fallback 69, 24/24, nol naked, nol identitas kanonik ganda | **Tidak** membuktikan apa pun tentang isi database |
| **D — Dry-run** | Apa yang *akan* terjadi bila transaksi di-commit. Dijalankan terhadap database nyata, lalu `ROLLBACK` | **Tidak** membuktikan keadaan akhir; tidak ada baris yang berubah |
| **X — Committed DEV** | Keadaan `QuilvianNewDevAndryZain` yang benar-benar tersimpan sesudah `COMMIT` | Berlaku **hanya** untuk database development ini, bukan staging maupun produksi |

Seluruh angka pada bagian 3–7 di bawah diberi label `S`, `D`, atau `X`.

---

## 2. Backup — prasyarat yang dipenuhi

| Field | Nilai |
| --- | --- |
| Berkas | `dump-QuilvianNewDevAndryZain-202609171439.dump` |
| Dibuat | **Sebelum** operasi tulis mana pun |
| Cakupan | Seluruh database `QuilvianNewDevAndryZain` |

Tanpa berkas ini, seluruh urutan di bawah tidak boleh dijalankan.

---

## 3. Pencabutan pra-seeder Finance `X`

Sasaran: `Finance` × `Manajer Finance` × `WorkSchedule`, action `Update` dan `Delete`.

| Hasil | Nilai |
| --- | --- |
| Policy `Update` | dinonaktifkan |
| Policy `Delete` | dinonaktifkan |
| Efektif `Update`/`Delete` sesudah commit | **0** |
| Baris nonaktif yang dipertahankan | **2** |
| Baris dihapus | **0** — pencabutan memakai `IsActive`, bukan `DELETE` |
| `UpdateBy` | `superadmin` |
| `UpdateDateTime` | tercatat |

**Finance `WorkSchedule.Read` dan `Create` sengaja TIDAK disentuh**, sesuai koreksi lingkup
`BE-SEC-016`. Tidak ada izin Finance lain yang dicabut, diperiksa, maupun dilarang.

Urutannya tidak boleh dibalik: dijalankan **sebelum** seeder, ketika kedua identitas registry masih
tertutup, sehingga pencabutan ini mencegah hak yang belum hidup — bukan mencabut hak yang sedang
dipakai.

---

## 4. Rekonsiliasi `AccessMenuSeeder` `X`

Dijalankan lewat **runner sementara eksternal**. **Aplikasi lengkap TIDAK dinyalakan** — tidak ada
traffic HTTP, tidak ada migration, tidak ada hosted service, dan tidak ada seeder lain yang ikut
berjalan. Ini penting: `DefaultWorkScheduleSeeder` menulis `MstWorkSchedules` dan berjalan tepat
sebelum `AccessMenuSeeder` pada urutan startup normal.

| Metrik | Sebelum `X` | Snapshot source `S` | Sesudah commit `X` | Delta |
| --- | ---: | ---: | ---: | ---: |
| Modules | 48 | 48 | **48** | 0 |
| Resources | 339 | 340 | **340** | **+1** |
| Actions | 1.286 | 1.300 | **1.300** | **+14** |

`+14` action = 10 dari `BE-SEC-012` + 4 dari `BE-SEC-013`. `+1` resource = `WorkScheduleAssignment`.

### 4.1 Penjaga regresi `RoutePath` — terbukti tidak kena

| Metrik | Sebelum `X` | Sesudah `X` |
| --- | ---: | ---: |
| Action aktif dengan `RoutePath` NULL | 1 | **1** |

Angka ini sengaja diukur. `AccessMenuSeeder.ReconcileAsync` menulis `action.RoutePath`, sedangkan
jalur `PermissionRegistryDescriptor.BuildFromAssembly` memberi `RoutePath = null`. Bila runner
memakai jalur assembly, seluruh ~1.300 baris akan ter-NULL — dan authorization verifier **tetap
PASS**, karena ia memang tidak memeriksa kolom itu. Tetap 1 sebelum dan sesudah membuktikan runner
memakai jalur host MVC (`PermissionRegistryDescriptor.Build`) yang benar.

### 4.2 Akibat pada identitas

| Identitas | Hasil `X` |
| --- | --- |
| `WorkSchedule.Update` / `WorkSchedule.Delete` | Baris registry **aktif kembali** (di-upsert pada baris yang sama, bukan baris baru) |
| `Finance` × `Manajer Finance` efektif `Update`/`Delete` | **tetap 0** — seeder tidak pernah menyentuh `SysAccessPolicy`, dan kedua policy sudah dinonaktifkan bagian 3 |
| `WorkScheduleAssignment` `Create`/`Read`/`Update`/`Delete` | **terdaftar dan aktif** |
| Duplikat identitas kanonik action | **0** |
| Duplikat identitas kanonik resource | **0** |

---

## 5. Pemberian hak awal HR `D` `X`

Matriks pemilik yang disetujui, enam resource: `WorkSchedule`, `Shift`, `ShiftGroup`,
`ShiftPattern`, `WorkCalendar`, `WorkScheduleAssignment`.

### 5.1 Dry run sebelum commit `D`

| Metrik | Nilai |
| --- | ---: |
| Total sasaran | **36** |
| Akan disisipkan | **32** |
| Sudah ada | **4** |

Selisih 36 − 32 = 4 adalah bukti idempotensi bekerja: empat kunci alami sudah ada dan masih efektif,
sehingga dipertahankan apa adanya alih-alih digandakan. **36 adalah cakupan akhir, bukan jumlah
`INSERT`** — persis seperti yang dirancang `BE-SEC-016`.

### 5.2 Keadaan akhir `X`

| Pasangan | Hak | Efektif |
| --- | --- | ---: |
| `Human Resource` × `Manajer HR` | Read/Create/Update/Delete × 6 resource | **24** |
| `Human Resource` × `Staff HR` | Read/Create × 6 resource | **12** |
| `Staff HR` `Update`/`Delete` | terlarang | **0** |
| Duplikat kunci alami | — | **0** |

---

## 6. `BE-SEC-003B` Tahap 1 `D` `X`

### 6.1 Dasar dry-run sebelum commit `D`

| Metrik | Nilai |
| --- | ---: |
| Identitas wajib | **24 / 24** |
| Identitas hilang | 0 |
| Baris peta yang masih kurang | **35** |
| Pemegang `PatientAssessment.Complete` | **0** |
| Pemegang `PatientAssessment.Amend` | **0** |
| Duplikat kunci alami | 0 |

`Complete` dan `Amend` bernilai 0 memang diharapkan: keduanya identitas **baru** yang baru terisi
oleh migrasi ini sendiri.

### 6.2 Simulasi `ROLLBACK` lebih dulu `D`

| Metrik | Hasil simulasi |
| --- | ---: |
| Sisa baris peta | **0** |
| Pemegang `Complete` | **1** |
| Pemegang `Amend` | **1** |

Simulasi inilah yang membuktikan urutan `4.1 → 4.2` benar: `Amend` hanya dapat menyambung sesudah
`Complete` terisi. Dijalankan lebih dulu, `Amend` akan menghasilkan nol baris tanpa satu pun error.

### 6.3 Sesudah commit `X`

| Metrik | Nilai |
| --- | ---: |
| Pelestarian historis efektif | **35** |
| `PatientAssessment.Complete` efektif | **1** |
| `PatientAssessment.Amend` efektif | **1** |
| **Total perluasan Tahap 1** | **35 + 1 = 36** |

Sumber kepemilikan `PatientAssessment.Update` pada database ini adalah `Medis` × `Dokter Umum` —
itulah sebabnya `Complete` dan `Amend` masing-masing satu baris, bukan lebih.

---

## 7. `BE-SEC-003B` Tahap 2 `D` `X`

### 7.1 Sasaran yang benar lingkupnya

| Identitas | Jumlah |
| --- | ---: |
| `PatientProcedure.Update` | **1** |
| `DoctorQueue.Update` | **3** |
| **Total** | **4** |

Angka inilah yang sebelumnya tidak terlihat benar, karena dry-run 1.5 salah lingkup — diperbaiki
`BE-SEC-017`. Kontrak `1 + 3 = 4` kini terbukti terhadap database, bukan hanya terhadap keputusan
pemilik.

### 7.2 Simulasi `ROLLBACK` lebih dulu `D`

| Penegasan | Hasil |
| --- | --- |
| Sasaran dibekukan | tepat **4** policy |
| Sasaran yang masih aktif di dalam transaksi | **0** |

### 7.3 Sesudah commit `X`

| Identitas | Aktif | Dinonaktifkan |
| --- | ---: | ---: |
| `PatientProcedure.Update` | **0** | **1** |
| `DoctorQueue.Update` | **0** | **3** |

**Tidak satu pun identitas pensiun yang tidak berhubungan ikut diubah.**

---

## 8. Hasil acceptance akhir database `X`

Query acceptance terakhir terhadap `QuilvianNewDevAndryZain`:

| Metrik | Nilai | Diharapkan |
| --- | ---: | ---: |
| `action_aktif` | **1300** | 1300 ✅ |
| `resource_aktif` | **340** | 340 ✅ |
| `modul_aktif` | **48** | 48 ✅ |
| `legacy_masih_aktif` | **0** | 0 ✅ |
| `legacy_dinonaktifkan` | **4** | 4 ✅ |
| `manager_hr` | **24** | 24 ✅ |
| `staff_hr` | **12** | 12 ✅ |
| `staff_hr_forbidden` | **0** | 0 ✅ |
| `finance_write_efektif` | **0** | 0 ✅ |
| `assessment_complete` | **1** | 1 ✅ |
| `assessment_amend` | **1** | 1 ✅ |
| `duplicate_count` | **0** | 0 ✅ |

**Seluruh nilai cocok.**

---

## 9. Identitas pensiun yang tidak berhubungan — TETAP DI LUAR SCOPE

`BillingItemCategory.*`, `CompanyGuarantor.*`, `KioskScanSession.Cancel`, `Queue.*`, dan identitas
pensiun lain yang muncul pada pembacaan registry:

| Tindakan | Status |
| --- | --- |
| Dihapus / diaktifkan / dinonaktifkan / dimigrasikan | ❌ **tidak satu pun** |
| Dicatat sebagai observasi | ✅ ya — dry-run 1.5c dan [`BE-SEC-017.md`](BE-SEC-017.md) bagian 4 |

`BE-SEC-003B` **tidak menyerap** pembersihan otorisasi yang tidak berhubungan. `BillingItemCategory.*`
milik tim Billing ([`evidence/10`](../../../evidence/10-billing-item-category-ownership-finding.md));
`CompanyGuarantor.*` **belum pernah dianalisis siapa pun**. Keduanya menunggu keputusan pemilik modul
masing-masing, sebagai task tersendiri.

---

## 10. Urutan tulis pemeliharaan: **SELESAI — JANGAN DIULANG BUTA**

> ### ⛔ PERINGATAN OPERASIONAL
>
> Seluruh urutan tulis pemeliharaan otorisasi terhadap `QuilvianNewDevAndryZain` **SUDAH SELESAI dan
> SUDAH DI-COMMIT**. Keempat skrip di bawah **TIDAK BOLEH dijalankan ulang secara buta.**

| Skrip | Keadaan | Bila dijalankan ulang apa adanya |
| --- | --- | --- |
| `be-sec-014-pre-seeder-revoke-finance-workschedule.sql` | **Sudah COMMIT** | Gerbang kardinalitas menemukan 0 sasaran aktif, bukan 2 → **transaksi dibatalkan**. Aman, tetapi sia-sia |
| `AccessMenuSeeder` (runner eksternal) | **Sudah COMMIT** | Idempoten: meng-upsert ke keadaan yang sama. Aman, tetapi tidak ada gunanya tanpa perubahan source |
| `be-sec-014-post-seeder-hr-initial-grants.sql` | **Sudah COMMIT** | 36 kunci alami sudah ada dan efektif → 0 disisipkan, seluruh penegasan lulus. Aman |
| `be-sec-003b-policy-expansion.sql` Tahap 1 | **Sudah COMMIT** | `NOT EXISTS` menyaring seluruhnya → 0 disisipkan. Aman |
| `be-sec-003b-policy-expansion.sql` Tahap 2 | **Sudah COMMIT** | Gerbang 4.3b menemukan 0 sasaran, bukan 4 → **transaksi dibatalkan**. Aman, tetapi sia-sia |

Gerbang kardinalitas pada kedua skrip pencabutan **sengaja** membatalkan diri pada eksekusi kedua.
Itu perilaku yang diinginkan — bukan kegagalan. Jangan melonggarkannya supaya "berhasil".

**Lingkungan selain development belum disentuh.** Keadaan `QuilvianNewDevAndryZain` tidak
membuktikan apa pun tentang staging maupun produksi; keduanya menuntut urutan, backup, dan
persetujuan tersendiri.

---

## 11. Yang TIDAK berubah pada task ini

| Aspek | Status |
| --- | --- |
| Perilaku otorisasi aplikasi | `NONE` — nol berkas `.cs` |
| Semantik SQL deployment | `NONE` — tidak satu skrip pun diubah |
| Registry source | `NONE` — tetap `1.300 / 340 / 48` |
| Akses database pada task ini | `NONE` — hanya pencatatan |

---

## 12. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `dotnet build -c Release --no-incremental` | **0 Error(s)** |
| `tools/authorization-verifier/verify-authorization.sh` | **PASS** |
| `git diff --check` | Bersih |

| Metrik `S` | Diharapkan | Terukur |
| --- | --- | --- |
| Modules | 48 | 48 ✅ |
| Resources | 340 | 340 ✅ |
| Actions | 1.300 | 1.300 ✅ |
| Fallback | 69 | 69 cocok persis ✅ |
| Metadata gaps | 0 | 0 ✅ |
| Naked baru | 0 | 0 ✅ |
| Naked baseline | 0 | 0 ✅ |
| Identitas wajib `BE-SEC-003` | 24/24 | 24/24 ✅ |
| Identitas kanonik ganda | 0 | tidak ganda ✅ |

Source `S` dan database `X` kini **sejajar** pada `1.300 / 340 / 48` — untuk pertama kalinya sejak
`BE-SEC-012`.

---

## 13. Task berikutnya — diidentifikasi, TIDAK dikerjakan

Pelaksanaan ini menyelesaikan **Fase B** `BE-SEC-003` (perluasan `SysAccessPolicy` ke *exact
historical capability set*). `BE-SEC-003` **tetap `PARTIAL`**, karena:

| Bagian `BE-SEC-003` | Keadaan |
| --- | --- |
| Fase A — pemecahan 22 identitas di source | ✅ selesai (`85fcc3fd`) |
| Fase A′ — `DoctorConsultation.WriteSoap`, paritas `IsCancel` | ✅ selesai |
| **Fase B — perluasan `SysAccessPolicy`** | ✅ **selesai pada development oleh pelaksanaan ini** |
| **Fase C — penyempitan audio antrean** | ⛔ **BELUM** |

**Task berikutnya: `BE-SEC-003` Fase C — penyempitan audio antrean.**

Buktinya belum dikerjakan: identitas `QueueVoice.PlayAudio` **tidak ada di source** — `grep`
terhadap seluruh `.cs` hanya menemukan `QueueDisplayRuntimeRead`. Yang dituntut keputusan pemilik:

| Butir | Isi |
| --- | --- |
| Mekanisme | `QueueVoice.PlayAudio` **OR** `QueueDisplayRuntimeRead` — wajib benar-benar OR, bukan AND |
| Dilarang | `AllowAnonymous` pada endpoint audio; menjadikan `QueueDisplayRuntimeRead` permission user dokter/perawat |
| Penerima (`O-1`) | 8 Departemen × Posisi: `Medis` × Dokter Umum/Spesialis/IGD; `Keperawatan` × Perawat Rawat Jalan/Rawat Inap/IGD, Kepala Keperawatan, Kepala Ruangan — mencakup 17 pengguna aktif |

`BE-SEC-004` (registry prefix `Sec` dan skema katalog Business Permission) **tetap terblokir** di
belakang `BE-SEC-003`, sesuai dependency pada roadmap.

**Tidak satu pun dari keduanya dikerjakan pada task ini.**

---

## 14. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build memunculkan 189 warning dokumentasi XML yang sudah ada sebelumnya |
| Masalah yang diketahui | Acceptance criteria `BE-SEC-003` nomor 7, 8, 10 dulu bergantung pada backend test project yang sudah dihapus merge Integration. Sebagiannya kini dijaga authorization verifier (invarian `[2]` fallback 69 = kriteria 8), tetapi pemetaan lengkapnya belum ditinjau ulang |
| Risiko tersisa | **1.** `QuilvianNewDevAndryZain` kini berbeda dari lingkungan lain; urutan yang sama wajib diulang secara sadar di sana, dengan backup dan persetujuan sendiri. **2.** `CompanyGuarantor.*` belum pernah dianalisis. **3.** Rollback (`BE-SEC-003B` bagian 6) belum pernah diuji terhadap database — acceptance criterion nomor 6 masih terbuka |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | **1.** Smoke test akun non-SuperAdmin terhadap enam resource HR dan identitas hasil pemecahan. **2.** Tinjau pemetaan acceptance criteria `BE-SEC-003` ke invarian verifier. **3.** Mulai `BE-SEC-003` Fase C bila pemilik menyetujui. **4.** Commit/push menunggu instruksi terpisah |
