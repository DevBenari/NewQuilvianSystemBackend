# Kesiapan `MVP-11` — Form Permintaan Rawat Inap dan transfer pasien ke Rawat Inap (`EPIC IGD-15`) — 18 Oktober 2026

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` · revision **`14`** · status `draft` (irisan Form Permintaan Rawat Inap `approved`: `IGD-DEC-294`, amandemen `IGD-DEC-306`, `318`, `324`) |
| Slice | `MVP-11` = `EPIC IGD-15`: `FR-IGD-109`…`121`, `AT-IGD-221`…`241`; task `BE-IGD-072`, `074`, `077`, `079`, `083`; `FE-IGD-059`, `060`, `061`, `064`, `066`. **Di luar slice:** `FR-IGD-117`, `118` (`OPEN DECISION` `IGD-OQ-127`, `129`); task yang dibatalkan `IGD-DEC-304` (`BE-IGD-073`, `075`, `FE-IGD-062`) |
| **Putusan** | **`READY_WITH_CONDITIONS`** — seluruh jalur milik IGD terbukti dan siap UAT untuk **alur normal** (admisi dulu, lalu antar). **Belum** siap produksi dan belum `DONE`: dua fitur — penutupan saat pasien tiba lebih dulu (`FR-IGD-120`) dan _Tolak_ dari antrean (`FR-IGD-119`) — hanya dapat berjalan bila Rawat Inap memanggil fungsi IGD (`IGD-REQ-003`). Enam syarat ada di bagian 5 |
| Pertanyaan pemilik | _"Apakah transfer pasien IGD harus menunggu Hamzah sebelum dinyatakan selesai?"_ — jawabannya di bagian 1 |
| Skill | `verify-module-readiness` — hanya membaca source dan bukti; tidak mengerjakan perbaikan |
| Wewenang tulis | Berkas ini dan satu baris status pada `MODULE-STATUS.md`, atas perintah Rizki Gunawan 18 Oktober 2026 (_"yaa jalankan"_) |
| Backend diperiksa | `NewQuilvianSystemBackend` `rizkiG` **`6bb18fe6`** + working tree `BE-IGD-083` (5 berkas IGD); DLL build Rizki 18 Oktober 2026 10.51.30, 0 error, 286 warning (baseline). `origin/MHamzah` **`7fff1c1a`** (18 Oktober 06.35) diperiksa: belum memuat pekerjaan `IGD-REQ-003` |
| Frontend diperiksa | `QuilvianSystemFrontendDev` `RizkiV2` **`58ee55d32`**; build produksi `BUILD_ID` `vyZON79AuJIJU924yT7Hf` (Putaran B5, B6, B7) |
| Kontrak | API **`0.19.0`** §12; validation **`0.19.0`** §6, §14 (14.8–14.10); state **`0.15.0`** §9.2, §11 (11.5–11.8); permission/audit **`0.12.0`** §11; integration **`0.10.0`** §9 — `approved` |
| Hash masukan | Kelima kontrak **identik** dengan manifest 0r/0s (`d06fda6e…`, `ceb56822…`, `4e49bcec…`, `c8dadf9a…`, `272fa191…`; isi dinormalkan LF). `00-interview-decisions.md` **berbeda** dari 0s (`705121ea…` lawan `3d873480…`) — hanya karena fakta `IGD-FACT-133`…`136` ditambahkan sesudahnya; nol keputusan baru |
| Keputusan relevan | `IGD-DEC-267`…`294`, `299`, `304`…`309`, `318`…`324`; fakta `IGD-FACT-110`…`136`; pertanyaan terbuka `IGD-OQ-132` (Rawat Inap) |

---

## 1. Putusan dalam bahasa sederhana — dan jawaban untuk pertanyaan pemilik

**Pekerjaan IGD sudah tuntas.** Semua task IGD yang dapat dibuktikan tanpa bantuan modul lain sudah ✅: perawat membuat form,
DPJP menandatangani, pasien masuk Antrean Transfer, petugas admisi meng-_admit_, form tampil _Selesai_, pembatalan sesudah
diproses ditolak, pasien diantar dengan serah terima SBAR, dan kunjungan IGD tertutup sendiri saat pasien dicatat tiba
(Putaran B7). Rollback migration juga terbukti aman.

**Yang belum adalah dua sambungan dari Rawat Inap ke IGD.** Fungsi IGD-nya sudah ada dan sudah diperbaiki, tetapi hanya
berjalan bila Rawat Inap memanggilnya:

| Fitur | Siapa yang memanggil | Akibat bila belum ada |
| --- | --- | --- |
| Penutupan kunjungan saat pasien **tiba lebih dulu**, lalu admisi diproses (`FR-IGD-120`, `AT-IGD-240`) | Transaksi _admit_ Rawat Inap (`IGD-REQ-003` butir 1) | Kunjungan IGD pasien itu **tetap terbuka**. Layar IGD tidak punya tombol _Selesaikan Kunjungan_ (`PATCH /emergency-visits/{id}/complete` hanya lewat API), dan selama kunjungan terbuka, pasien yang sama **tidak dapat didaftarkan lagi ke IGD** (satu pasien satu episode aktif, `BE-IGD-025`) |
| _Tolak_ beralasan dari antrean (`FR-IGD-119`, `AT-IGD-238`, `239`) | Tombol dan endpoint Rawat Inap (`IGD-REQ-003` butir 2) | Petugas admisi tidak dapat menolak dari layarnya. Jalan keluar yang sudah terbukti: petugas admisi menghubungi IGD, lalu perawat IGD membatalkan form beralasan (B5-10) dan pasien keluar dari antrean |

**Jadi:**

1. Untuk menyatakan **`MVP-11` selesai penuh** (`BE-IGD-077` ✅, `DONE`), **ya, perlu menunggu Mas Hamzah** — acceptance
   3 dan 4 `BE-IGD-077` hanya dapat dibuktikan lewat panggilan dari Rawat Inap. Jadwal yang disebut pemilik: Senin
   19 Oktober 2026.
2. **Tidak ada pekerjaan IGD yang menunggu.** Bila pemilik ingin menutup lingkup IGD lebih dulu, itu keputusan pemilik
   (`IGD-DEC` baru): `BE-IGD-077` diterima dengan pengecualian acceptance 3–4 yang dipindah menjadi syarat integrasi milik
   Rawat Inap (`IGD-OQ-132`), ditambah aturan kerja sementara "admisi dulu, antar kemudian". **Tidak disarankan sebelum
   Senin**: jaraknya satu hari, dan risiko kunjungan terbuka yang menahan pendaftaran ulang pasien nyata.

_Contoh._ Tn. Budi diantar ke bangsal pukul 10.30 dan dicatat tiba, lalu admisi selesai pukul 10.45. Hari ini, kunjungan IGD
Tn. Budi tetap terbuka. Bila seminggu kemudian ia datang lagi ke IGD, pendaftarannya ditolak karena episode lamanya masih
berjalan, sampai seseorang menutupnya lewat API. Sesudah panggilan Rawat Inap terpasang, kunjungan itu tertutup sendiri
pukul 10.45.

---

## 2. Cakupan dan denominator

| Yang dihitung | Jumlah | Hasil |
| --- | ---: | --- |
| Functional requirement `FR-IGD-109`…`121` dalam slice | 11 | **9 terbukti** (109–116, 121); **2 sebagian** (119, 120 — sisi IGD terbukti, pemanggil Rawat Inap belum ada). 117 dan 118 di luar slice (`OPEN DECISION`) |
| Skenario uji `AT-IGD-221`…`241` | 21 | **18 tercakup task ✅** (221–237, 241; bukti per skenario di laporan task dan penilaian Putaran B2–B7); `AT-IGD-238` sebagian (tampilan IGD ✅ atas penilaian pemilik `IGD-DEC-319`, pemicunya belum ada); `AT-IGD-239`, `240` belum (butuh Rawat Inap) |
| Task `MVP-11` | 10 | **9 ✅** (`BE-IGD-072`, `074`, `079`, `083`, `FE-IGD-059`, `060`, `061`, `064`, `066`), **1 🟡** (`BE-IGD-077`); 10 laporan tracked ada |
| Butir DoD PRD §12.5 sesudah amandemen §12.7.3 dan §12.8.3 | 15 | **14 terpenuhi** (butir 11 dengan pengecualian pemilik), **1 sebagian** (butir 14) — bagian 4 |
| Grafik dependency R3.18.2 (backend, Mermaid) dan R3.16.1 (frontend, Mermaid) | 2 | Tanda node = tanda kartu = register untuk kesepuluh task. Selisih tercatat: `BE-IGD-078` menunggu `BE-IGD-070` (R3.16) tanpa panah ringkasan; `BE-IGD-079`, `080` tanpa node (bukan task `MVP-11` kecuali `079`) — bagian 6 |

---

## 3. Skor per dimensi

**Kemajuan pengerjaan: 9 dari 10 task ✅ = 90%.**
**Kesiapan ujung-ke-ujung (berbobot): ±84%** — rinciannya di bawah. Kedua angka itu sengaja dipisah.

| Dimensi | Bobot | Skor | Bukti | Gap/blocker |
| --- | ---: | ---: | --- | --- |
| Fondasi — keputusan, kontrak, traceability | 15% | 5/6 (83%) | Lima kontrak `approved`, hash cocok manifest 0r/0s; 11/11 FR dan 21/21 AT terpetakan ke task; arah lintas modul tercatat (`IGD-DEC-323`, `IGD-REQ-003`) | Baris traceability `FR-IGD-109`…`112` masih memuat keadaan lama (bagian 6) |
| Backend | 25% | 4,5/5 (90%) | `…/Services/EmergencyDispositionService.AdmissionRequest.cs` (form, tanda tangan, batal, cetak, `BatalkanFormPermintaanOlehPetugasAdmisiAsync`, `EvaluasiPenutupanKunjunganSusulanAsync`); `…/Services/EmergencyVisitService.cs#TryPromoteToDisposed,TryCloseAfterDispositionAsync`; `…/Services/EmergencyDispositionService.cs#ValidateVisitClosureAsync,CariTindakLanjutTerlaksanaAsync`@`6bb18fe6`+WT; build 0 error/286 warning; QBE Strict `PASS` | `BE-IGD-077` acceptance 3 (respons `409` _Tolak_) dan 4 (penutupan saat dipanggil Rawat Inap) belum terbukti runtime — pemanggilnya belum ada |
| Frontend | 20% | 5/5 (100%) | `FE-IGD-059`, `060`, `061`, `064`, `066` ✅; build produksi `vyZON79AuJIJU924yT7Hf` tanpa lencana `next dev` (B5–B7) | — (header _Informasi Pasien_ tidak dimuat ulang sesudah aksi kepergian, `IGD-FACT-135` — risiko, bukan syarat) |
| Integrasi dan runtime | 20% | 3/6 (50%) | Dev: migration diterapkan dan `Down()` teruji (`IGD-FACT-136`); _admit_ Rawat Inap → form _Selesai_ → kepergian → tutup saat tiba (B6, B7); saringan antrean Rawat Inap (`IGD-DEC-307`) | Panggilan evaluasi sesudah _admit_ dan _Tolak_ belum ada di Rawat Inap (`IGD-OQ-132`); _admit_ oleh akun petugas admisi belum pernah berhasil di uji (B6/B7 lewat SuperAdmin — konfigurasi baru dilengkapi 18 Oktober, `IGD-FACT-134`); konfigurasi per lingkungan belum (C3–C5) |
| Verifikasi | 20% | 14/15 (93%) | DoD bagian 4; Putaran B5, B6, B7 dinilai dari bukti mentah dan log | DoD 14 sebagian; UAT belum (C6) |

Skor berbobot: 0,15×83 + 0,25×90 + 0,20×100 + 0,20×50 + 0,20×93 ≈ **84%**.

---

## 4. Definition of Done (PRD §12.5, §12.7.3, §12.8.3)

| # | Butir | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Form, riwayat, atestasi, salinan beku tersimpan sesuai kamus data | Terpenuhi | `BE-IGD-072`, `074`, `079` ✅ |
| 2 | Tanda tangan menyimpan form dan salinan beku dalam satu transaksi; pasien tampil di antrean | Terpenuhi | B5-04, B5-09 (`IGD-FACT-126`) |
| 3 | Hanya DPJP IGD aktif yang dapat menandatangani; pengalihan memindahkan _Perlu TTD_ | Terpenuhi | B5-13 |
| 4 | _Jalankan_ rawat inap dari IGD selalu ditolak (aturan 23) | Terpenuhi | B4-08 (bukti API), B5-01 |
| 5 | Pembatalan dua jalan sesuai state §11.5 | Terpenuhi | B5-10, B5-12, B6-04 |
| 6 | Petugas admisi memproses pasien dari antrean; tanpa form pasien tidak tampil | Terpenuhi | B6-02, B7-01 (`201`), B6-07 (`404`). Catatan: _admit_ dijalankan SuperAdmin (opsi 2); kriteria IGD tidak bergantung pada siapa yang meng-_admit_ |
| 7 | Tagihan IGD tidak ditautkan — tanpa perubahan Billing | Terpenuhi | Nol berkas Billing; `BE-IGD-075` dibatalkan (`IGD-DEC-304`) |
| 8 | Nol `Program.cs`; nol komentar baru; CRLF tanpa BOM (migration ber-BOM) | Terpenuhi | Laporan task kesepuluh task |
| 9 | Satu migration diterapkan; migration 2 dibatalkan; `Down()` berpenjaga diuji | Terpenuhi | `IGD-FACT-136` — [bukti](2026-10-18-uji-down-migration-be-igd-072.md) |
| 10 | Build backend 0 error; `eslint` dan `npm run build` frontend lulus | Terpenuhi | Build 18 Oktober 2026; build produksi frontend B5–B7 |
| 11 | Hak akses diberikan pengurus Akses Role, bukan penguji | Terpenuhi dengan pengecualian pemilik | Persiapan uji lewat SuperAdmin diizinkan `IGD-DEC-320`, `322`; skenario dijalankan akun peran |
| 12 | Konfirmasi pemilik Rawat Inap atas saringan antrean | Terpenuhi | `IGD-DEC-307` |
| 13 | Serah terima dan penutupan kunjungan pada urutan admisi lalu antar | Terpenuhi | B7-03 (`IGD-FACT-133`) — `AT-IGD-237` |
| 14 | Fungsi tolak admisi dan evaluasi penutupan teruji, mematuhi konkurensi | **Sebagian** | Source dan konkurensi terpetakan (`BE-IGD-077` #6); jalur promosi dan penutupan yang dipakai evaluasi terbukti B7; panggilan dari Rawat Inap belum ada, jadi `AT-IGD-238`…`240` belum terbukti runtime |
| 15 | Layar perawat menampilkan status dan alasan tolak admisi; dapat membuat form baru | Terpenuhi atas penilaian pemilik | `FE-IGD-064` ✅ (`IGD-DEC-319`); pemicunya dari Rawat Inap belum ada |

---

## 5. Syarat (`READY_WITH_CONDITIONS`)

| ID | Syarat | Pemilik | Mitigasi sampai syarat lunas | Menutup |
| --- | --- | --- | --- | --- |
| C1 | Panggilan `EvaluasiPenutupanKunjunganSusulanAsync` di transaksi _admit_ (`IGD-REQ-003` butir 1), lalu Putaran B7 bagian 2 (B7-06, B7-07) | **Muhammad Hamzah** (Rawat Inap); uji: Rizki/Antigravity | Aturan kerja sementara: perawat ruangan baru menekan _Catat Tiba_ sesudah admisi diproses, atau kunjungan yang tertahan ditutup lewat `PATCH /emergency-visits/{id}/complete` oleh pemegang hak `EmergencyVisit : Update`. Kunjungan yang tertahan terbaca lewat saringan _menunggu penutupan_ (`BE-IGD-063`). Keputusan aturan kerja ada pada Rizki | `BE-IGD-077` #4, `FR-IGD-120`, `AT-IGD-240`, DoD 14 |
| C2 | Tombol dan endpoint _Tolak_ beralasan (`IGD-REQ-003` butir 2) | **Muhammad Hamzah** | Petugas admisi menghubungi IGD; perawat IGD membatalkan form beralasan (B5-10) | `BE-IGD-077` #3, `FR-IGD-119`, `AT-IGD-238`, `239` |
| C3 | `BedManagement:UnitAssignments` memuat petugas admisi **dengan** `InpatientEpisode:Create` di setiap lingkungan; _admit_ oleh akun admisi dibuktikan sekali | Rawat Inap (Hamzah) per lingkungan; dev: Rizki (sudah dilengkapi 18 Oktober) | Opsi 2 (SuperAdmin) hanya untuk uji, bukan operasional | `IGD-FACT-127`, `134`; syarat rilis R3.18.5 |
| C4 | Tanda `RequiresAdmissionRequest` pada jenis tindak lanjut rawat inap dan hak Akses Role `EmergencyAdmissionRequest` di setiap lingkungan | Rizki / pengurus Akses Role | Tanpa tanda, jenis itu memakai aturan lama (`IGD-DEC-305`) | Syarat rilis R3.18.5 |
| C4 — **dev ✅ 19 Oktober 2026** | Diperiksa lewat API dengan akun peran (bukan SuperAdmin), backend berjalan di `https://localhost:7184`: (a) `GET …/master-data/emergency-disposition-types` mengembalikan tiga jenis — `RUJUK` `false`, **`RI` `true`**, `PLG` `false`, jadi tanda sudah benar; (b) `GET …/emergency-admission-requests?emergencyVisitId=…` sebagai Perawat IGD `200` dan `GET …/emergency-admission-requests/pending-signature` sebagai Dokter IGD `200`, jadi `EmergencyAdmissionRequest : Read` dan `: Sign` sudah diberikan. `Create`, `Update`, dan `Cancel` perawat terbukti dari Putaran B5–B7 yang membuat, mengubah, dan membatalkan form. **Peringatan untuk lingkungan lain:** kode jenis di dev adalah **`RI`**, sedangkan `EmergencyMasterDataSeeder` mendefinisikan **`RANAP`**; seeder hanya menyisipkan baris baru (`if (existing.Contains(d.Code)) continue;`) dan migration `20261012080102` memberi kolom itu `defaultValue: false`. Jadi di lingkungan yang jenisnya sudah ada sebelum 12 Oktober 2026, tanda itu `false` dan **harus diubah manual** lewat `PUT …/master-data/emergency-disposition-types/{id}` — jangan mencari kode `RANAP`, cari jenis rawat inap yang berlaku di lingkungan itu | Rizki per lingkungan | Sisa C4 hanya di luar dev | Syarat rilis R3.18.5 |
| C5 | Nilai benar `isForInpatient` master kelas "KELAS II" (diubah `true` saat uji B6) | Rawat Inap / Master Data | — | `IGD-REQ-003` butir 4 |
| C6 | ~~UAT dengan pengguna nyata~~ — **dicabut sebagai syarat 19 Oktober 2026 atas keputusan pemilik:** verifikasi lewat agen Antigravity dan pemilik sendiri dianggap memadai untuk modul ini, sehingga UAT oleh tim terpisah tidak lagi menjadi syarat `DONE`. Pencabutan ini **tidak** mengubah putusan `READY_WITH_CONDITIONS`, karena yang menahan adalah C1 dan C2 di Rawat Inap | Rizki | — | Keputusan pemilik 19 Oktober 2026 |

---

## 6. Ketidaksesuaian dokumen dan bukti `STALE`

| Butir | Keadaan | Diarahkan ke |
| --- | --- | --- |
| Traceability `FR-IGD-109`, `110`, `111` | Masih bertuliskan _"build FE belum; TTD dan kirim menunggu `BE-IGD-074`"_, padahal `FE-IGD-059` ✅ (build 14 Oktober) dan `BE-IGD-074` ✅ (18 Oktober) | `manage-module-blueprint` — perbarui menjadi terbukti |
| Traceability `FR-IGD-112` | Masih menyebut _"tersisa uji `Down()`"_ — sudah lunas (`IGD-FACT-136`) | `manage-module-blueprint` |
| Laporan task `BE-IGD-083.md` | Mencatat keadaan saat build (🟡); roadmap sudah ✅ dari Putaran B7. Bukti B7 ada di laporan B7 | `build-module-backend` (pembaruan laporan task) |
| Manifest 0s — hash `00-interview-decisions.md` | Berubah sesudah 0s hanya karena fakta baru | Dicatat; hitung ulang pada amandemen berikutnya |
| Grafik R3.18.2 | `BE-IGD-078` → `BE-IGD-070` (R3.16) tanpa panah ringkasan; `BE-IGD-079`, `080` tanpa node sejak 16 Oktober | `plan-module-delivery` (pass grafik) |

---

## 7. Risiko yang diterima atau dicatat (bukan syarat)

| Risiko | Keterangan |
| --- | --- |
| Lencana basi di layar pemeriksaan | Header _Informasi Pasien_ tidak dimuat ulang sesudah aksi kepergian; status baru terlihat sesudah halaman dimuat ulang (`IGD-FACT-135`). Kandidat task frontend kecil |
| Lingkungan baru dari basis data kosong | EF Core 9 menerapkan seluruh rantai migration dalam satu transaksi; perlu `max_locks_per_transaction` > 64 (`IGD-FACT-136`) |
| Perubahan perilaku non-ranap | Kunjungan non-ranap yang punya kepergian kini tertutup saat _Catat Tiba_ (`IGD-DEC-324`) — disetujui |
| Di luar slice | `FR-IGD-117` (`IGD-OQ-127`), `FR-IGD-118` (`IGD-OQ-129`) belum diputuskan |

---

## 8. Langkah berikutnya

1. **Senin 19 Oktober 2026 — Muhammad Hamzah:** butir 1 dan 2 `IGD-REQ-003` di blueprint dan source Rawat Inap.
2. **Rizki / Antigravity:** Putaran B7 bagian 2 (prompt A.1d, bagian "1 dan 2" atau "2"), dengan akun admisi (C3) — menutup
   `BE-IGD-077` #4. Uji _Tolak_ (B4-11…16) sesudah butir 2.
3. **Agent:** penilaian bukti mentah; `BE-IGD-077` ✅ bila lulus; lalu `verify-module-readiness` ulang — putusan dapat naik
   ke `READY` untuk UAT penuh bila C1–C3 lunas.
4. **Agent (`manage-module-blueprint`):** rapikan baris traceability yang usang (bagian 6).
5. **Bila pemilik memilih menutup lingkup IGD lebih dulu:** catat sebagai `IGD-DEC` baru beserta aturan kerja sementara
   pada C1. Tanpa keputusan itu, `MVP-11` tetap `IN_PROGRESS`.
