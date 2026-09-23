# Laporan Perubahan Backend — `BE-SEC-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-SEC-017` |
| Judul | Koreksi lingkup dry-run `BE-SEC-003B` bagian 1.5 |
| Slice | Task terpisah di luar rantai — pemeliharaan skrip penerapan |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) — bagian *Task terpisah di luar rantai* |
| Trace | [`BE-SEC-015.md`](BE-SEC-015.md) (pengerasan Tahap 2), [`evidence/14`](../../../evidence/14-owner-policy-matrix-and-deployment-preparation.md) |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API yang disentuh |
| Dependency | `BE-SEC-015` (`5ab741ca`), `BE-SEC-016` (`a2c133c4`) — keduanya selesai dan sudah di-commit |
| Klasifikasi | `LIGHT` — dua skrip database, koreksi query baca-saja; nol perubahan source aplikasi |
| Task mode | `BACKEND` |
| Target tulis | `Migrations/scripts/`, `docs/module-blueprints/platform-authorization/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a2c133c43f3e2dbcad113ed015a5b8abe24e60fc` |
| Tanggal | 17 September 2026 |
| Status | **Selesai.** Koreksi siap; tidak ada SQL yang dijalankan pada task ini |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | Shared Platform (otorisasi) |
| Module | `PLATFORM_AUTHORIZATION` |
| Submodule | `SysAccessPolicy` — dry-run skrip penerapan `BE-SEC-003B` |
| Pemilik/prefix pada registry | `Sys` — tidak ada perubahan registry |
| Keberlakuan | `TOUCHED LEGACY` — **nol berkas source aplikasi berubah** |
| Status registry | Tidak berubah. Verifier tetap `1.300 / 340 / 48`, `BE-SEC-003` 24/24 |
| QBE ID yang berlaku | Tidak ada. Tidak ada entity, rename, migration, maupun eksekusi database |
| Wewenang database | **TIDAK DIPAKAI pada task ini.** `BE-SEC-003B` Tahap 1 dan Tahap 2 tetap `PAUSED` |
| Pemilihan ID task | `BE-SEC-004`–`BE-SEC-011` dipesan sebagai dekomposisi `BE-SEC-002` (`NOT STARTED`); `BE-SEC-012`–`BE-SEC-016` terpakai. `BE-SEC-017` diverifikasi bebas pada empat sumber: roadmap, laporan tracked, working tree, dan riwayat Git |

---

## 1. Bagaimana cacat ini ditemukan

**Lewat dry-run baca-saja yang benar-benar dijalankan terhadap database development
`QuilvianNewDevAndryZain`, bukan lewat pembacaan kode.** Inilah eksekusi pertama skrip
`BE-SEC-003B` terhadap database nyata.

Hasil yang diperoleh pemilik:

| Bagian | Hasil | Penilaian |
| --- | --- | --- |
| 1.0 | 24 identitas, seluruhnya `ok` | ✅ sesuai |
| 1.0b | `identitas_siap` = 24, `identitas_wajib` = 24, `belum_siap` = 0 | ✅ sesuai |
| 1.4 | Sasaran pelestarian Tahap 1 = **35** | ✅ sesuai (35 + 1 `Amend` = 36) |
| 2.1 | Pemegang `PatientAssessment.Complete` saat ini = **0** | ✅ diharapkan — `Complete` baru terisi 4.1 |
| 2.2 | Sumber `PatientAssessment.Update` = `Medis` × `Dokter Umum` | ✅ sesuai |
| 3.1 | `sisa_baris_belum_dibuat` = **35** | ✅ konsisten dengan 1.4 |
| 3.2 | Duplikat kunci alami = **0 baris** | ✅ sesuai |
| **1.5** | Mengembalikan identitas pensiun yang **tidak ada hubungannya** dengan `BE-SEC-003B` — `BillingItemCategory.*`, `CompanyGuarantor.*`, dan lainnya, bercampur dengan `DoctorQueue.Update` | ⛔ **SALAH LINGKUP** |

Seluruh bagian lain terbukti benar terhadap database nyata. Hanya 1.5 yang keliru.

---

## 2. Akar masalah

Predikat 1.5 sebelum koreksi, identik pada kedua varian:

```sql
FROM be_sec_003b_pemegang h
JOIN be_sec_003b_identitas i ON i.resource = h.resource AND i.action = h.action
WHERE i.action_is_delete
```

Satu-satunya penyaring adalah `i.action_is_delete`. Artinya **secara efektif** query itu berbunyi:

> seluruh policy hidup yang menunjuk baris registry mana pun yang sudah dihapus

bukan yang dimaksud:

> policy yang menunjuk dua identitas pensiun milik `BE-SEC-003B`

Registry memuat banyak identitas pensiun dari pekerjaan lain — `BillingItemCategory.*`,
`CompanyGuarantor.*`, dan seterusnya — dan seluruhnya ikut terjaring.

### 2.1 Yang TIDAK rusak

Penting dibedakan, karena konsekuensinya jauh berbeda:

| Bagian | Lingkup | Keadaan |
| --- | --- | --- |
| 4.3a (sasaran Tahap 2) | dibatasi `IN (('PatientProcedure','Update'), ('DoctorQueue','Update'))` | ✅ **sudah benar sejak `BE-SEC-015`** |
| 4.3b (gerbang kardinalitas) | menuntut tepat 4 = 1 + 3 | ✅ sudah benar |
| 1.5 (pratinjau) | tanpa pembatasan identitas | ⛔ **inilah yang keliru** |

**Tahap 2 tidak pernah akan menyentuh identitas yang tidak berhubungan.** Cacatnya murni pada
pratinjau. Tetapi dampaknya tetap serius: operator yang menjalankan dry-run melihat daftar jauh
lebih panjang dari empat baris, dan wajar menyimpulkan kontrak `1 + 3 = 4` sudah tidak berlaku —
padahal gerbang 4.3b justru akan membatalkan transaksi bila jumlahnya bukan 4. Pratinjau yang
menyesatkan mengikis kepercayaan pada gerbang yang sebetulnya benar.

---

## 3. Koreksi

### 3.1 Bagian 1.5a — rincian sasaran

Predikatnya kini **identik dengan 4.3a**, sehingga daftar yang ditinjau operator adalah persis
baris yang akan dinonaktifkan Tahap 2:

```sql
FROM public."SysAccessPolicy" p
JOIN be_sec_003b_identitas i ON i.action_access_id = p."ActionAccessId"
WHERE i.action_is_delete
  AND (i.resource, i.action) IN (('PatientProcedure','Update'), ('DoctorQueue','Update'))
  AND p."IsActive"
```

Identitas bisnis, bukan GUID. Nama departemen dan posisi ditampilkan lewat `LEFT JOIN` supaya
keluarannya dapat dibaca manusia tanpa mengubah himpunan barisnya.

### 3.2 Bagian 1.5b — ringkasan kontrak, dibaca apa adanya

Menghitung `PatientProcedure.Update`, `DoctorQueue.Update`, dan totalnya langsung dari database,
lalu membandingkannya dengan kontrak pemilik:

| Kolom | Arti |
| --- | --- |
| `patientprocedure_update` | jumlah sebenarnya, wajib 1 |
| `doctorqueue_update` | jumlah sebenarnya, wajib 3 |
| `total` | jumlah sebenarnya, wajib 4 |
| `status` | `cocok` bila 1 + 3 = 4, selain itu `BEDA — JANGAN jalankan Tahap 2` |

**Angka-angkanya tidak dipaksakan.** Query melaporkan keadaan sebenarnya; bila berbeda dari
kontrak, statusnya berbunyi `BEDA` dan operator diminta melapor kepada pemilik. Tidak ada
`LIMIT`, tidak ada penyaring yang membuat hasilnya seolah-olah 4.

### 3.3 Bagian 1.5c — observasi di luar scope

Identitas pensiun **lain** yang masih dipegang policy hidup kini ditampilkan terpisah, diringkas
per identitas, dan diberi label eksplisit:

```
'DI LUAR SCOPE BE-SEC-003B — tidak disentuh'
```

Tujuannya satu: keberadaannya tercatat sehingga tidak lagi disalahartikan sebagai sasaran Tahap 2.
Bagian ini **tidak** membuatnya menjadi sasaran, dan tidak menyarankan tindakan apa pun atasnya.

---

## 4. Identitas pensiun yang tidak berhubungan

`BillingItemCategory.*`, `CompanyGuarantor.*`, dan identitas pensiun lain yang muncul pada dry-run:

| Tindakan | Status |
| --- | --- |
| Dihapus | ❌ tidak |
| Diaktifkan kembali | ❌ tidak |
| Dinonaktifkan | ❌ tidak |
| Dimigrasikan | ❌ tidak |
| Diubah dengan cara apa pun | ❌ tidak |
| Dicatat sebagai observasi di luar scope | ✅ ya, pada 1.5c dan laporan ini |

**`BE-SEC-003B` tidak menyerap pembersihan otorisasi yang tidak berhubungan.** `BillingItemCategory.*`
sudah tercatat sebagai milik tim Billing pada [`evidence/10`](../../../evidence/10-billing-item-category-ownership-finding.md)
dan [`evidence/14`](../../../evidence/14-owner-policy-matrix-and-deployment-preparation.md) bagian G.
`CompanyGuarantor.*` belum pernah dianalisis dan **tidak** dianalisis di sini. Keduanya menunggu
keputusan pemilik modul masing-masing, sebagai task tersendiri.

---

## 5. Verifikasi

### 5.1 Parity psql ↔ DBeaver

| Pemeriksaan | Hasil |
| --- | --- |
| Bagian 1.5 sesudah koreksi | **Identik** pada kedua varian — 44 baris ternormalisasi, diff kosong |
| Predikat 1.5a vs 4.3a | **Identik persis**, pada kedua varian |
| Predikat `action_is_delete` tanpa pembatasan identitas | **Nihil** — keempat kemunculan pada tiap berkas kini dibatasi (`IN` pada 1.5a/1.5b/4.3a, `NOT IN` pada 1.5c) |
| Perbedaan semantik bisnis lain | **Nihil** — satu-satunya selisih tetap tiga blok gerbang operator milik varian DBeaver |

### 5.2 Build dan verifier

| Perintah | Hasil |
| --- | --- |
| `dotnet build -c Release --no-incremental` | **0 Error(s)** |
| `tools/authorization-verifier/verify-authorization.sh` | **PASS** |
| `git diff --check` | Bersih, exit 0 |

| Metrik | Diharapkan | Terukur |
| --- | --- | --- |
| Actions | 1.300 | 1.300 ✅ |
| Resources | 340 | 340 ✅ |
| Modules | 48 | 48 ✅ |
| Metadata gaps | 0 | 0 ✅ |
| Fallback | 69 persis | 69 cocok persis ✅ |
| `BE-SEC-003` | 24/24 | 24/24 ✅ |
| Naked baru | 0 | 0 ✅ |
| Naked baseline | 0 | 0 ✅ |
| Identitas kanonik ganda | 0 | tidak ganda ✅ |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Cacat dikonfirmasi pada kedua varian | Terpenuhi | Bagian 2 |
| 2. Akar masalah dinyatakan tepat | Terpenuhi | Predikat hanya `i.action_is_delete` |
| 3. 1.5 dibatasi dua identitas lewat kunci bisnis | Terpenuhi | Bagian 3.1 |
| 4. Tidak ada GUID yang di-*hardcode* | Terpenuhi | Identitas dinyatakan sebagai `(resource, action)` |
| 5. Ringkasan 1 + 3 = 4 tersedia dan baca-saja | Terpenuhi | Bagian 3.2 |
| 6. Angka tidak dipaksakan agar tampak benar | Terpenuhi | Status `BEDA` bila berbeda |
| 7. Koreksi diterapkan pada kedua varian | Terpenuhi | Bagian 5.1 |
| 8. Predikat 1.5 = predikat Tahap 2 | Terpenuhi | Bagian 5.1 |
| 9. Identitas pensiun lain tidak diubah | Terpenuhi | Bagian 4 |
| 10. Tidak ada perubahan data policy | Terpenuhi | Seluruh koreksi pada query baca-saja |
| 11. ID task berikutnya diverifikasi | Terpenuhi | Preflight |
| 12. Penemuan lewat dry-run DB nyata didokumentasikan | Terpenuhi | Bagian 1 |
| 13. Build dan verifier dijalankan ulang | Terpenuhi | Bagian 5.2 |
| 14. Tidak ada SQL yang dijalankan pada task ini | Terpenuhi | Bagian 5 |

**Butir Definition of Done yang belum terpenuhi — disebut apa adanya:**

| Butir | Keadaan |
| --- | --- |
| 1.5 hasil koreksi dijalankan terhadap database | **Belum.** Task ini tidak menjalankan SQL. Angka 1 + 3 = 4 masih berasal dari kontrak pemilik dan belum dikonfirmasi ulang lewat query yang sudah benar |
| Penerapan `BE-SEC-003B` | **Belum.** Tahap 1 dan Tahap 2 tetap `PAUSED` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build memunculkan 189 warning dokumentasi XML yang sudah ada sebelumnya |
| Masalah yang diketahui | Kontrak Tahap 2 `1 + 3 = 4` belum pernah dikonfirmasi oleh query yang lingkupnya benar. Dry-run sebelumnya memakai 1.5 yang salah lingkup, sehingga angka 4 belum terbukti terhadap database |
| Risiko tersisa | **1.** Bila 1.5b hasil koreksi ternyata melaporkan selain 1 + 3 = 4, Tahap 2 tidak boleh dijalankan dan pemilik wajib memutuskan ulang — gerbang 4.3b memang membatalkan transaksi, tetapi lebih baik diketahui sebelum jendela pemeliharaan dibuka. **2.** `CompanyGuarantor.*` belum pernah dianalisis siapa pun |
| Perubahan sampingan | `NONE` — tidak ada identitas pensiun lain yang disentuh |
| Interupsi | `NONE` |
| Langkah berikutnya | **1.** Jalankan ulang bagian 1.5 yang sudah dikoreksi terhadap `QuilvianNewDevAndryZain` dan konfirmasi 1 + 3 = 4. **2.** Serahkan keluaran 1.5c kepada pemilik modul Billing dan pemilik `CompanyGuarantor` sebagai observasi terpisah. **3.** Baru lanjutkan urutan penerapan. **4.** Commit/push menunggu instruksi terpisah |
