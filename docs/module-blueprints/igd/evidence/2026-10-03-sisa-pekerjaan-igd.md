# Sisa Pekerjaan Modul IGD — Keadaan 3 Oktober 2026

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` · revision `8` · status `draft` |
| Untuk | Product/Domain Owner IGD (Rizki Gunawan) dan tim pengembang IGD |
| Sumber | Register status task pada [backend-roadmap.md](../roadmap/backend-roadmap.md) dan [frontend-roadmap.md](../roadmap/frontend-roadmap.md); [audit kesiapan `MVP-7`](2026-10-03-kesiapan-mvp-7.md); [decision log](../00-interview-decisions.md); manifest `belum_direncanakan` |
| Source | Backend `rizkiG` `5af6ef3b`; frontend `RizkiV2` `521b18a9a` |
| Sifat | Ringkasan keadaan. Status per task tetap mengikuti register roadmap |

---

## 1. Posisi saat ini

| Ukuran | Angka |
| --- | --- |
| Task yang sudah direncanakan | **61 dari 79 selesai (77%)** — backend 37/47, frontend 24/32 |
| Task belum selesai | 18 — 10 sebagian (🟡), 4 terblokir (⛔), 4 belum dikerjakan |
| `MVP-7` encounter-first | 13/13 task ✅ — **`READY_WITH_CONDITIONS`**: siap UAT, belum siap produksi |
| `MVP-8` penutupan lewat disposisi | 4/6 task ✅ |
| Lingkup blueprint yang **belum direncanakan sama sekali** | Penunjang medis, pemakaian alat, billing IGD; `EPIC IGD-09` pengkajian klinis |

**Mengapa 77% bukan "77% dari seluruh IGD".** Angka itu hanya menghitung task yang sudah punya kartu. Billing IGD,
penunjang medis, dan pemakaian alat belum punya epic, functional requirement, maupun kontrak, sehingga belum bisa
dihitung. Untuk benar-benar 100%, lingkup itu masih harus melewati penggalian kebutuhan, desain, dan perencanaan.

---

## 2. Daftar pekerjaan menurut tahap

Kolom *Bisa mulai sekarang?* menjawab apakah pekerjaan itu menunggu pihak lain.

### Tahap 1 — Menutup `MVP-7` sampai siap produksi

| No | Pekerjaan | Pemilik | Bisa mulai sekarang? |
| ---: | --- | --- | --- |
| 1.1 | Commit dokumen blueprint 3 Oktober; push `RizkiV2` (dua commit **bersamaan**) dan `rizkiG` (C7) | Rizki | Ya |
| 1.2 | ✅ **Selesai 3 Oktober 2026 — `IGD-DEC-181`.** Kirim [permintaan persetujuan Registrasi](../approval-requests/2026-10-03-permintaan-persetujuan-pemilik-registrasi.md) (`IGD-REQ-002`), lalu catat jawabannya (C2, DoD 8) | Rizki → pemegang modul Registrasi | Ya |
| 1.3 | ❌ **Uji 3 Oktober: 9 dari 10 lulus; loket gagal `403` — rute loket sudah diganti ke `/patient-encounters/admin` (`IGD-DEC-182`); build lulus 15.31; tinggal uji ulang `C4-01`.** Beri izin pada peran nyata — `EmergencyVisit : NoShow` untuk perawat triage, `EmergencyEncounterReconciliation` untuk admin data saja — lalu uji dengan akun non-SuperAdmin (C4) | Rizki / admin peran | Ya |
| 1.4 | ✅ **Lunas 3 Oktober 2026 — tiga putaran, hitungan 1, status 4.** Ulangi uji serentak `BE-IGD-055` S6 dengan hitungan baris tersimpan (C5) | Rizki | Ya |
| 1.5 | ✅ **Dev: 0 baris (3 Oktober 2026)** — ulangi per lingkungan sebelum rilis `BE-IGD-053`. Jawab `IGD-OQ-110` (jumlah encounter nonaktif tanpa tanda berakhir pada data lama) — **sebaiknya sebelum** `BE-IGD-053` dirilis ke lingkungan berikutnya | Rizki | Ya |
| 1.6 | Rilis per lingkungan sesuai urutan R3.13.5 + rekonsiliasi K1 + kueri invarian sesudah rilis (C3) | Rizki | Sesudah 1.1 |
| 1.7 | UAT oleh tim terpisah (C1, DoD 10) | Tim UAT | Sesudah 1.3 dan 1.6 |
| 1.8 | Jawab soal tombol *Sekarang* pada pemilih jam waktu tiba (laporan `FE-IGD-036` §10.6) | Rizki | Ya |

### Tahap 2 — Menuntaskan `MVP-8` (penutupan kunjungan lewat disposisi)

| No | Pekerjaan | Pemilik | Bisa mulai sekarang? |
| ---: | --- | --- | --- |
| 2.1 | `FE-IGD-041` 🟡 — kolom PENUTUPAN terdorong keluar layar pada lebar 1440 piksel; perbaiki tata letak, lalu uji U2, U6, U7 | Agent (`build-module-frontend`) | Ya |
| 2.2 | Temuan `BE-IGD-061` S6: observasi berstatus *Dieskalasi* tidak menahan penutupan, sehingga kunjungan bisa selesai sementara observasinya tertinggal dan tidak dapat diselesaikan lagi. Perlu keputusan aturan | Rizki lewat `grill-me` | Ya |
| 2.3 | `BE-IGD-061` S2, S3, S12 — pemicu serah terima dan sikap pesanan | Agent | **Tidak** — tertahan `BE-IGD-039` |
| 2.4 | `verify-module-readiness` untuk `MVP-8` | Agent | Sesudah 2.1–2.3 |
| 2.5 | `IGD-OQ-111` — pencatatan pasien yang memburuk sesudah disposisi dilaksanakan (tidak menahan) | Rizki + Clinical Governance | Ya |

### Tahap 3 — Membuka blocker eksternal (dampak terbesar)

| No | Pekerjaan | Pemilik | Yang terbuka bila selesai |
| ---: | --- | --- | --- |
| 3.1 | `BE-IGD-039` ⛔ — kewenangan unit membandingkan identitas yang salah, dan jalan keluar beralasan `IGD-DEC-092` tidak pernah dibaca kode | **Security/Privacy owner** (belum ditunjuk) | `MVP-6` (`EPIC IGD-08`); uji layar kedatangan dan serah terima `MVP-4`/`MVP-5`; `BE-IGD-035`; `BE-IGD-041` S2/S3; `BE-IGD-061` S2/S3/S12 |
| 3.2 | Isi `MstServiceUnit.OrganizationUnitId` untuk **18 unit** (hari ini 0) | **Master Data** (belum ditunjuk) | Sama dengan 3.1 — keduanya harus selesai bersamaan |

Selama 3.1 dan 3.2 belum selesai, empat task tidak dapat mencapai ✅ dan pasien yang kembali saat kunjungan lamanya
tertahan serah terima tidak dapat dimulai kunjungan barunya (`IGD-DEC-180`).

### Tahap 4 — Backlog yang bisa dikerjakan sekarang tanpa pihak luar

| No | Task | Status | Yang perlu dilakukan | Jenis |
| ---: | --- | --- | --- | --- |
| 4.1 | `BE-IGD-017` | 🟡 | Laporan tracked susulan pemulihan solution | Dokumen |
| 4.2 | `BE-IGD-043` | belum | Laporan susulan pengaturan IGD tersirat (`f76ebaab`) | Dokumen |
| 4.3 | `FE-IGD-025` | belum | Laporan susulan perombakan layar pengkajian dan temuan privasi | Dokumen |
| 4.4 | `FE-IGD-026` | belum | Laporan susulan layar pendaftaran IGD (`c8613d88c`) | Dokumen |
| 4.5 | `BE-IGD-026`, `BE-IGD-031` | 🟡 | Uji langkah mundur migration `20260826090500` di basis data terpisah — butuh rancangan uji sendiri karena ±75 migration modul lain menyusul di belakangnya | Uji |
| 4.6 | `FE-IGD-012` | 🟡 | Tangkapan layar penolakan `409` jalur triase | Uji layar |
| 4.7 | `FE-IGD-013` | 🟡 | Uji simpan lalu muat ulang pengkajian lewat layar | Uji layar |
| 4.8 | `FE-IGD-022` | 🟡 | Uji layar asuhan keperawatan IGD | Uji layar |
| 4.9 | `BE-IGD-041` | 🟡 | Build dan uji API skenario 1 (skenario 2–3 tertahan `BE-IGD-039`) | Uji |
| 4.10 | `BE-IGD-042` | ⛔ | Hanya menunggu **angka** jumlah `EmgVisit` aktif dengan `EncounterType.Outpatient` — pemilik datanya Rizki | Data, lalu kode |
| 4.11 | `FE-IGD-010` | belum | Halaman detail satu kunjungan IGD — **perlu diputuskan dulu apakah masih dibutuhkan** sesudah alur encounter-first | Keputusan, lalu kode |

### Tahap 5 — Keputusan bisnis yang masih terbuka

| ID | Pertanyaan | Menahan |
| --- | --- | --- |
| `IGD-OQ-102`, `IGD-OQ-103` | Sumber roster dokter jaga, kriteria dokter layak, dan tempat penanda override | `EPIC IGD-12` — `BE-IGD-056` ⛔, `FE-IGD-037` ⛔ |
| Temuan `BE-IGD-061` S6 | Apakah observasi *Dieskalasi* menahan penutupan kunjungan | Penyelesaian `MVP-8` |
| `IGD-DEC-100`…`102` | Sikap pesanan, pesanan lab manual, penerimaan per pesanan — masih `draft` | DoD butir 10 `EPIC IGD-07` |
| `IGD-OQ-083` | Tempat menyimpan alasan pembatalan observasi | Bagian `Cancelled` `IGD-DEC-115` |
| `IGD-OQ-089` | Bentuk terstruktur alat jalan napas (OPA, NPA, ETT, LMA, dll.) | Pelaporan alat jalan napas |
| `IGD-OQ-090` | Entri susulan sesudah periode observasi ditutup | Dokumentasi susulan observasi |
| `IGD-OQ-110` | Jumlah encounter nonaktif tanpa tanda berakhir pada data lama | Sebaiknya sebelum rilis `BE-IGD-053` |
| `IGD-OQ-111` | Pasien memburuk sesudah disposisi dilaksanakan | Tidak menahan |
| Ruas kunjungan sesudah Tangani Segera | Tempat layar untuk melengkapi keluhan, cara datang, jenis kasus (`IGD-DEC-143`) | Amendment desain layar |

### Tahap 6 — Lingkup blueprint yang belum direncanakan

| Lingkup | Keadaan | Jalur pengerjaan |
| --- | --- | --- |
| Billing IGD | Batas lingkup ditutup `IGD-DEC-095`…`105`; nol epic, FR, kontrak | `grill-me` → `requirement-completeness-gate` → `design-business-module` → `plan-module-delivery` |
| Penunjang medis | Sama | Sama |
| Pemakaian alat | Sama | Sama |
| `EPIC IGD-09` pengkajian klinis | `OPEN DECISION` — butuh pemilik `ClinicalManagement` dan `PharmacyManagement` | Penunjukan pemilik, lalu `grill-me` |
| Pemesanan radiologi dari IGD | Ditahan `IGD-DEC-111` sampai hasil bacaan radiologi dapat dirilis (`ActAsRadiologist`, pemilik Radiologi) | Menunggu modul Radiologi |
| `POST-MVP` lain | Pembaruan realtime daftar pantau, layar laporan, catatan pemberian obat, status hasil laboratorium | Setelah MVP |

### Tahap 7 — Tata kelola dan kebersihan

| No | Pekerjaan | Pemilik |
| ---: | --- | --- |
| 7.1 | Penunjukan peran yang masih `OPEN`: Security/Privacy, Clinical Governance, Nursing authority, Master Data, pemilik `ClinicalManagement` dan `PharmacyManagement`. Saat terisi, `IGD-DEC-082`, `IGD-DEC-135`, dan butir wajib tinjau klinis (`IGD-DEC-150`) ditinjau ulang | Manajemen |
| 7.2 | Approval blueprint secara keseluruhan (revision `8` masih `draft`; yang `approved` baru irisan per slice) | Rizki |
| 7.3 | Gerbang kelengkapan requirement untuk area IGD selain encounter-first (masih `UNCLASSIFIED`) | Agent (`requirement-completeness-gate`) |
| 7.4 | Segarkan capability map (suplemen 3.2 dicatat pada `0d13f3a8`) dan utang struktur `erd/` → `data/` | Agent |
| 7.5 | Angka kueri audit A/B `BE-IGD-050` (encounter yatim lama) — belum ada sejak 21 September | Rizki |
| 7.6 | Keamanan: tiga `DataProtectionKeys/key-*.xml` masih ter-track di repository backend; kredensial teks biasa pada skrip uji lokal (`test-with-agy/`) | Rizki |

---

## 3. Urutan yang disarankan

1. **Minggu ini:** Tahap 1.1–1.5 dan 1.8 (semuanya di tangan Rizki), bersamaan dengan Tahap 2.1 dan 2.2.
2. **Paralel, eskalasi:** Tahap 3 — satu surat untuk Security/Privacy owner dan Master Data, karena keduanya membuka
   empat task sekaligus.
3. **Sesudah izin peran dan rilis:** UAT `MVP-7` (1.7).
4. **Mengisi waktu tunggu:** backlog Tahap 4 (dokumen dan uji layar, murah dan tanpa pihak luar).
5. **Satu sesi keputusan:** Tahap 5 — terutama `IGD-OQ-102`/`103` yang membuka `EPIC IGD-12`.
6. **Siklus baru:** Tahap 6, mulai dari billing IGD.
